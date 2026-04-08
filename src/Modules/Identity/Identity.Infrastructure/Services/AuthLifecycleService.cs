using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Exceptions;
using Application.Observability;
using Identity.Application.Configuration;
using Identity.Application.Contracts;
using Identity.Application.Services;
using Infrastructure.Observability;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Services;

public sealed class AuthLifecycleService(
    IdentityDbContext identityDbContext,
    AuthorizationDbContext authorizationDbContext,
    IOperationalEventPublisher operationalEventPublisher,
    IIdentityRequestContext identityRequestContext,
    IPasswordPolicyService passwordPolicyService,
    IJwtAccessTokenService jwtAccessTokenService,
    IOptions<JwtTokenOptions> jwtOptions) : IAuthLifecycleService
{
    private readonly JwtTokenOptions _jwtOptions = jwtOptions.Value;

    public async Task<LoginResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        // Login akisi burada sadece sifre kontrolu degildir.
        // Session acma, yetki ozetini toplama ve token uretme ayni operasyonun parcasi olarak ilerler.
        if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationAppException(
                "Login doğrulaması başarısız.",
                new Dictionary<string, string[]>
                {
                    ["identifier"] = ["Identifier zorunludur."],
                    ["password"] = ["Password zorunludur."]
                },
                errorCode: "auth_login_validation_failed");
        }

        var identifier = request.Identifier.Trim();
        var normalizedEmail = identifier.ToLowerInvariant();
        var normalizedCode = identifier.ToUpperInvariant();

        var user = await identityDbContext.Users
            .FirstOrDefaultAsync(x => !x.IsDeleted &&
                                      (x.UserCode == normalizedCode || x.Email == normalizedEmail),
                cancellationToken);

        if (user is null)
        {
            await LogSecurityEventAsync("Login", false, "Kullanıcı bulunamadı.", identifier, null, cancellationToken);
            throw new ForbiddenAppException("Geçersiz kullanıcı adı veya şifre.", errorCode: "auth_invalid_credentials");
        }

        if (!user.IsActive)
        {
            await LogSecurityEventAsync("Login", false, "Kullanıcı pasif.", user.UserCode, user.Id, cancellationToken);
            throw new ForbiddenAppException("Kullanıcı pasif durumda.", errorCode: "auth_user_inactive");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            await LogSecurityEventAsync("Login", false, "Şifre doğrulaması başarısız.", user.UserCode, user.Id, cancellationToken);
            throw new ForbiddenAppException("Geçersiz kullanıcı adı veya şifre.", errorCode: "auth_invalid_credentials");
        }

        if (user.PasswordExpiresAt.HasValue && user.PasswordExpiresAt.Value <= DateTime.UtcNow)
        {
            await LogSecurityEventAsync("Login", false, "Şifre süresi dolmuş.", user.UserCode, user.Id, cancellationToken);
            throw new ForbiddenAppException("Şifre süresi dolmuş. Şifre değişikliği zorunlu.", errorCode: "auth_password_expired");
        }

        var now = DateTime.UtcNow;
        var session = new UserSession
        {
            UserId = user.Id,
            SessionKey = Guid.NewGuid().ToString("N"),
            StartedAt = now,
            ExpiresAt = now.AddDays(_jwtOptions.RefreshTokenDays <= 0 ? 7 : _jwtOptions.RefreshTokenDays),
            LastSeenAt = now,
            IsRevoked = false,
            ClientIpAddress = identityRequestContext.RemoteIpAddress,
            UserAgent = identityRequestContext.UserAgent
        };

        identityDbContext.UserSessions.Add(session);
        await identityDbContext.SaveChangesAsync(cancellationToken);

        var roles = await (
                from userRole in identityDbContext.UserRoles.AsNoTracking()
                join role in identityDbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                where userRole.UserId == user.Id && !userRole.IsDeleted && !role.IsDeleted
                select role.Code)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var transactionCodes = await (
                from userPagePermission in authorizationDbContext.UserPagePermissions.AsNoTracking()
                join page in authorizationDbContext.SubModulePages.AsNoTracking() on userPagePermission.SubModulePageId equals page.Id
                where userPagePermission.UserId == user.Id && !userPagePermission.IsDeleted && !page.IsDeleted
                select page.TransactionCode)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var permissions = await authorizationDbContext.UserPageActionPermissions
            .AsNoTracking()
            .Where(x => x.UserId == user.Id && !x.IsDeleted && x.IsAllowed)
            .Select(x => x.ActionCode)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var companyId = await authorizationDbContext.UserCompanyPermissions
            .AsNoTracking()
            .Where(x => x.UserId == user.Id && !x.IsDeleted)
            .OrderBy(x => x.Id)
            .Select(x => (int?)x.CompanyId)
            .FirstOrDefaultAsync(cancellationToken);

        var accessToken = jwtAccessTokenService.CreateToken(new AccessTokenRequest(
            user.Id,
            user.UserCode,
            companyId,
            session.Id,
            roles,
            permissions));

        var (refreshTokenValue, refreshTokenHash, refreshTokenExpiresAt) = CreateRefreshToken();

        identityDbContext.UserRefreshTokens.Add(new UserRefreshToken
        {
            UserId = user.Id,
            UserSessionId = session.Id,
            TokenHash = refreshTokenHash,
            TokenId = accessToken.TokenId,
            ExpiresAt = refreshTokenExpiresAt,
            CreatedByIp = identityRequestContext.RemoteIpAddress,
            UserAgent = identityRequestContext.UserAgent
        });

        await identityDbContext.SaveChangesAsync(cancellationToken);

        var effectiveAuthorization = new EffectiveAuthorizationSummaryDto(roles, transactionCodes, permissions);

        await LogSecurityEventAsync("Login", true, null, user.UserCode, user.Id, cancellationToken,
            new { session.Id, accessToken.ExpiresAtUtc, refreshTokenExpiresAt, effectiveAuthorization });

        var daysUntilExpiry = user.PasswordExpiresAt.HasValue
            ? (int?)(user.PasswordExpiresAt.Value - DateTime.UtcNow).TotalDays
            : null;
        var expiringSoon = daysUntilExpiry.HasValue
            && daysUntilExpiry.Value >= 0
            && daysUntilExpiry.Value <= 14; // ExpiryWarningDays default

        return new LoginResponseDto(
            user.Id,
            user.UserCode,
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            refreshTokenValue,
            refreshTokenExpiresAt,
            "Bearer",
            user.MustChangePassword,
            user.PasswordExpiresAt,
            expiringSoon,
            daysUntilExpiry,
            effectiveAuthorization);
    }

    public async Task<RefreshTokenResponseDto> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        // Refresh akisinin ana ilkesi rotation'dir:
        // eski token kullanilir, sonra hemen revoke edilip yerine yenisi acilir.
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new ValidationAppException(
                "Refresh token doğrulaması başarısız.",
                new Dictionary<string, string[]>
                {
                    ["refreshToken"] = ["RefreshToken zorunludur."]
                },
                errorCode: "refresh_token_validation_failed");
        }

        var hashedIncomingToken = HashToken(request.RefreshToken.Trim());

        var refreshToken = await identityDbContext.UserRefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == hashedIncomingToken && !x.IsDeleted, cancellationToken);

        if (refreshToken is null)
        {
            throw new ForbiddenAppException("Refresh token geçersiz.", errorCode: "refresh_token_invalid");
        }

        var now = DateTime.UtcNow;

        if (refreshToken.IsRevoked || refreshToken.IsUsed)
        {
            await HandleRefreshTokenReuseAsync(refreshToken, cancellationToken);
            throw new ForbiddenAppException("Refresh token tekrar kullanımı tespit edildi.", errorCode: "refresh_token_reused");
        }

        if (refreshToken.ExpiresAt <= now)
        {
            throw new ForbiddenAppException("Refresh token süresi dolmuş.", errorCode: "refresh_token_expired");
        }

        var session = await identityDbContext.UserSessions
            .FirstOrDefaultAsync(x => x.Id == refreshToken.UserSessionId && !x.IsDeleted, cancellationToken);

        if (session is null || session.IsRevoked || session.ExpiresAt <= now)
        {
            throw new ForbiddenAppException("Session geçersiz veya süresi dolmuş.", errorCode: "session_invalid_or_expired");
        }

        var user = await identityDbContext.Users
            .FirstOrDefaultAsync(x => x.Id == refreshToken.UserId && !x.IsDeleted, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new ForbiddenAppException("Kullanıcı aktif değil.", errorCode: "auth_user_inactive");
        }

        if (user.PasswordExpiresAt.HasValue && user.PasswordExpiresAt.Value <= now)
        {
            throw new ForbiddenAppException("Şifre süresi dolmuş.", errorCode: "auth_password_expired");
        }

        var roles = await (
                from userRole in identityDbContext.UserRoles.AsNoTracking()
                join role in identityDbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                where userRole.UserId == user.Id && !userRole.IsDeleted && !role.IsDeleted
                select role.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        var permissions = await authorizationDbContext.UserPageActionPermissions
            .AsNoTracking()
            .Where(x => x.UserId == user.Id && !x.IsDeleted && x.IsAllowed)
            .Select(x => x.ActionCode)
            .Distinct()
            .ToListAsync(cancellationToken);

        var companyId = await authorizationDbContext.UserCompanyPermissions
            .AsNoTracking()
            .Where(x => x.UserId == user.Id && !x.IsDeleted)
            .OrderBy(x => x.Id)
            .Select(x => (int?)x.CompanyId)
            .FirstOrDefaultAsync(cancellationToken);

        var accessToken = jwtAccessTokenService.CreateToken(new AccessTokenRequest(
            user.Id,
            user.UserCode,
            companyId,
            session.Id,
            roles,
            permissions));

        var (newRefreshTokenValue, newRefreshTokenHash, newRefreshTokenExpiresAt) = CreateRefreshToken();

        refreshToken.IsUsed = true;
        refreshToken.UsedAt = now;
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = now;
        refreshToken.RevokedBy = "rotation";
        refreshToken.ReplacedByTokenHash = newRefreshTokenHash;

        identityDbContext.UserRefreshTokens.Add(new UserRefreshToken
        {
            UserId = user.Id,
            UserSessionId = session.Id,
            TokenHash = newRefreshTokenHash,
            TokenId = accessToken.TokenId,
            ExpiresAt = newRefreshTokenExpiresAt,
            CreatedByIp = identityRequestContext.RemoteIpAddress,
            UserAgent = identityRequestContext.UserAgent
        });

        session.LastSeenAt = now;

        await identityDbContext.SaveChangesAsync(cancellationToken);

        await LogSecurityEventAsync("RefreshToken", true, null, user.UserCode, user.Id, cancellationToken,
            new { session.Id, accessToken.ExpiresAtUtc, newRefreshTokenExpiresAt });

        return new RefreshTokenResponseDto(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            newRefreshTokenValue,
            newRefreshTokenExpiresAt,
            "Bearer");
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        // "Sadece kendi hesabini degistirebilirsin" kurali burada servis seviyesinde enforce edilir.
        if (request.UserId <= 0 || string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new ValidationAppException(
                "Şifre değişim doğrulaması başarısız.",
                new Dictionary<string, string[]>
                {
                    ["request"] = ["userId, currentPassword ve newPassword zorunludur."]
                },
                errorCode: "password_change_validation_failed");
        }

        if (!identityRequestContext.TryGetUserId(out var authenticatedUserId))
        {
            throw new ForbiddenAppException("Kimliği doğrulanmış kullanıcı bilgisi çözümlenemedi.", errorCode: "identity_context_missing");
        }

        if (authenticatedUserId != request.UserId)
        {
            throw new ForbiddenAppException("Sadece kendi şifrenizi değiştirebilirsiniz.", errorCode: "password_change_not_allowed");
        }

        var user = await identityDbContext.Users
            .FirstOrDefaultAsync(x => x.Id == request.UserId && !x.IsDeleted, cancellationToken);

        if (user is null)
        {
            throw new NotFoundAppException($"Kullanıcı bulunamadı. userId={request.UserId}", errorCode: "user_not_found");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            await LogSecurityEventAsync("ChangePassword", false, "Mevcut şifre yanlış.", user.UserCode, user.Id, cancellationToken);
            throw new ForbiddenAppException("Mevcut şifre doğrulanamadı.", errorCode: "password_current_invalid");
        }

        await passwordPolicyService.EnforcePasswordChangePolicyOrThrowAsync(
            user.Id,
            user.UserCode,
            user.Email,
            user.PasswordHash,
            request.NewPassword,
            cancellationToken);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.MustChangePassword = false;
        user.PasswordExpiresAt = DateTime.UtcNow.AddDays(90);

        await identityDbContext.SaveChangesAsync(cancellationToken);
        await passwordPolicyService.RecordPasswordHistoryAsync(user.Id, user.PasswordHash, cancellationToken);

        await LogSecurityEventAsync("ChangePassword", true, null, user.UserCode, user.Id, cancellationToken,
            new { user.PasswordExpiresAt });
    }

    public async Task<IReadOnlyList<SessionListItemDto>> ListSessionsAsync(int userId, bool onlyActive, CancellationToken cancellationToken)
    {
        var query = identityDbContext.UserSessions
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsDeleted);

        if (onlyActive)
        {
            var now = DateTime.UtcNow;
            query = query.Where(x => !x.IsRevoked && x.ExpiresAt > now);
        }

        return await query
            .OrderByDescending(x => x.StartedAt)
            .Select(x => new SessionListItemDto(
                x.Id,
                x.UserId,
                x.SessionKey,
                x.StartedAt,
                x.ExpiresAt,
                x.LastSeenAt,
                x.IsRevoked,
                x.RevokedAt,
                x.RevokedBy,
                x.ClientIpAddress,
                x.UserAgent))
            .ToListAsync(cancellationToken);
    }

    public async Task RevokeSessionAsync(int sessionId, string? reason, CancellationToken cancellationToken)
    {
        // Session revoke davranisi self-service ve privileged actor senaryolarini ayni yerde toplar.
        var session = await identityDbContext.UserSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId && !x.IsDeleted, cancellationToken);

        if (session is null)
        {
            throw new NotFoundAppException($"Session bulunamadı. sessionId={sessionId}", errorCode: "session_not_found");
        }

        if (session.IsRevoked)
        {
            return;
        }

        if (!identityRequestContext.TryGetUserId(out var actorUserId))
        {
            throw new ForbiddenAppException("Kimliği doğrulanmış kullanıcı bilgisi çözümlenemedi.", errorCode: "identity_context_missing");
        }

        var isPrivilegedActor = identityRequestContext.IsInRole("SYS_ADMIN")
                                || identityRequestContext.IsInRole("SYS_OPERATOR");

        if (!isPrivilegedActor && session.UserId != actorUserId)
        {
            await LogSecurityEventAsync(
                "RevokeSession",
                false,
                "Başka kullanıcının session kaydını revoke etmeye yetki yok.",
                actorUserId.ToString(),
                actorUserId,
                cancellationToken,
                new { TargetSessionId = session.Id, TargetUserId = session.UserId });

            throw new ForbiddenAppException("Sadece kendi session kaydınızı revoke edebilirsiniz.", errorCode: "session_revoke_not_allowed");
        }

        var actor = identityRequestContext.TryGetActorIdentity(out var actorIdentity)
            ? actorIdentity
            : "system";

        session.IsRevoked = true;
        session.RevokedAt = DateTime.UtcNow;
        session.RevokedBy = actor;

        var refreshTokens = await identityDbContext.UserRefreshTokens
            .Where(x => x.UserSessionId == session.Id && !x.IsDeleted && !x.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedBy = actor;
        }

        await identityDbContext.SaveChangesAsync(cancellationToken);

        await LogSecurityEventAsync("RevokeSession", true, reason, actor, session.UserId, cancellationToken,
            new { session.Id, session.SessionKey, session.RevokedAt, session.RevokedBy, reason });
    }

    private async Task HandleRefreshTokenReuseAsync(UserRefreshToken refreshToken, CancellationToken cancellationToken)
    {
        // Reuse tespiti savunmaci bir guvenlik olayidir; ayni session'a bagli aktif tokenlar topluca revoke edilir.
        refreshToken.ReuseDetectedAt = DateTime.UtcNow;

        var linkedSession = await identityDbContext.UserSessions
            .FirstOrDefaultAsync(x => x.Id == refreshToken.UserSessionId && !x.IsDeleted, cancellationToken);

        if (linkedSession is not null && !linkedSession.IsRevoked)
        {
            linkedSession.IsRevoked = true;
            linkedSession.RevokedAt = DateTime.UtcNow;
            linkedSession.RevokedBy = "refresh_reuse_detected";
        }

        var activeTokens = await identityDbContext.UserRefreshTokens
            .Where(x => x.UserSessionId == refreshToken.UserSessionId && !x.IsDeleted && !x.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedBy = "refresh_reuse_detected";
        }

        await identityDbContext.SaveChangesAsync(cancellationToken);

        await LogSecurityEventAsync(
            "RefreshTokenReuseDetected",
            false,
            "Refresh token tekrar kullanımı tespit edildi.",
            refreshToken.UserId.ToString(),
            refreshToken.UserId,
            cancellationToken,
            new { refreshToken.UserSessionId, refreshToken.TokenId });
    }

    private (string TokenValue, string TokenHash, DateTime ExpiresAt) CreateRefreshToken()
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var days = _jwtOptions.RefreshTokenDays <= 0 ? 7 : _jwtOptions.RefreshTokenDays;
        var expiresAt = DateTime.UtcNow.AddDays(days);
        return (raw, HashToken(raw), expiresAt);
    }

    private static string HashToken(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    private async Task LogSecurityEventAsync(
        string action,
        bool isSuccessful,
        string? failureReason,
        string resource,
        int? numericUserId,
        CancellationToken cancellationToken,
        object? additional = null)
    {
        var payload = additional is null ? null : JsonSerializer.Serialize(additional);

        // Auth tarafi loglari dogrudan tabloya yazmak yerine event backbone'a gonderir.
        var operationalEvent = new OperationalEvent
        {
            EventName = isSuccessful ? "AuthLifecycleCompleted" : "AuthLifecycleFailed",
            Severity = isSuccessful ? "Information" : "Warning",
            Category = "Authentication",
            Source = nameof(AuthLifecycleService),
            Message = $"{action} {(isSuccessful ? "completed" : "failed")} for {resource}",
            IsSuccessful = isSuccessful,
            FailureReason = failureReason,
            UserId = resource,
            Username = identityRequestContext.TryGetUsername(out var username) ? username : resource,
            IpAddress = identityRequestContext.RemoteIpAddress,
            UserAgent = identityRequestContext.UserAgent,
            Resource = resource,
            Action = action,
            OperationName = action,
            Properties = new Dictionary<string, object?>
            {
                ["numericUserId"] = numericUserId,
                ["payload"] = payload
            }
        };

        await operationalEventPublisher.PublishAsync(operationalEvent, cancellationToken);
    }
}
