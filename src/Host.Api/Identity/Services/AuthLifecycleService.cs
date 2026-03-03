using System.Text.Json;
using Host.Api.Exceptions;
using Host.Api.Identity.Contracts;
using Host.Api.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace Host.Api.Identity.Services;

public sealed class AuthLifecycleService(
    BusinessDbContext businessDbContext,
    LogDbContext logDbContext,
    IHttpContextAccessor httpContextAccessor,
    ICurrentUserContext currentUserContext,
    IPasswordPolicyService passwordPolicyService) : IAuthLifecycleService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationAppException(
                "Login doğrulaması başarısız.",
                new Dictionary<string, string[]>
                {
                    ["identifier"] = ["Identifier zorunludur."],
                    ["password"] = ["Password zorunludur."]
                });
        }

        var identifier = request.Identifier.Trim();
        var normalizedEmail = identifier.ToLowerInvariant();
        var normalizedCode = identifier.ToUpperInvariant();

        var user = await businessDbContext.Users
            .FirstOrDefaultAsync(x => !x.IsDeleted &&
                                      (x.UserCode == normalizedCode
                                       || x.Username == identifier
                                       || x.Email == normalizedEmail),
                                 cancellationToken);

        if (user is null)
        {
            await LogSecurityEventAsync("Login", false, "Kullanıcı bulunamadı.", identifier, null, cancellationToken);
            throw new ForbiddenAppException("Geçersiz kullanıcı adı veya şifre.");
        }

        if (!user.IsActive)
        {
            await LogSecurityEventAsync("Login", false, "Kullanıcı pasif.", user.UserCode, user.Id, cancellationToken);
            throw new ForbiddenAppException("Kullanıcı pasif durumda.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            await LogSecurityEventAsync("Login", false, "Şifre doğrulaması başarısız.", user.UserCode, user.Id, cancellationToken);
            throw new ForbiddenAppException("Geçersiz kullanıcı adı veya şifre.");
        }

        if (user.PasswordExpiresAt.HasValue && user.PasswordExpiresAt.Value <= DateTime.UtcNow)
        {
            await LogSecurityEventAsync("Login", false, "Şifre süresi dolmuş.", user.UserCode, user.Id, cancellationToken);
            throw new ForbiddenAppException("Şifre süresi dolmuş. Şifre değişikliği zorunlu.");
        }

        var now = DateTime.UtcNow;
        var session = new UserSession
        {
            UserId = user.Id,
            SessionKey = Guid.NewGuid().ToString("N"),
            StartedAt = now,
            ExpiresAt = now.AddHours(12),
            LastSeenAt = now,
            IsRevoked = false,
            ClientIpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString()
        };

        businessDbContext.UserSessions.Add(session);
        await businessDbContext.SaveChangesAsync(cancellationToken);

        var roles = await (
                from userRole in businessDbContext.UserRoles.AsNoTracking()
                join role in businessDbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                where userRole.UserId == user.Id && !userRole.IsDeleted && !role.IsDeleted
                select role.Code)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var transactionCodes = await (
                from userPagePermission in businessDbContext.UserPagePermissions.AsNoTracking()
                join page in businessDbContext.SubModulePages.AsNoTracking()
                    on userPagePermission.SubModulePageId equals page.Id
                where userPagePermission.UserId == user.Id
                      && !userPagePermission.IsDeleted
                      && !page.IsDeleted
                select page.TransactionCode)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var permissions = await businessDbContext.UserPageActionPermissions
            .AsNoTracking()
            .Where(x => x.UserId == user.Id && !x.IsDeleted && x.IsAllowed)
            .Select(x => x.ActionCode)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var effectiveAuthorization = new EffectiveAuthorizationSummary(roles, transactionCodes, permissions);

        await LogSecurityEventAsync("Login", true, null, user.UserCode, user.Id, cancellationToken,
            new { session.SessionKey, session.ExpiresAt, user.MustChangePassword, effectiveAuthorization });

        return new LoginResponse(
            user.Id,
            user.UserCode,
            user.Username,
            session.SessionKey,
            session.ExpiresAt,
            user.MustChangePassword,
                user.PasswordExpiresAt,
                effectiveAuthorization);
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (request.UserId <= 0 || string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new ValidationAppException(
                "Şifre değişim doğrulaması başarısız.",
                new Dictionary<string, string[]>
                {
                    ["request"] = ["userId, currentPassword ve newPassword zorunludur."]
                });
        }

        if (!currentUserContext.TryGetUserId(out var authenticatedUserId))
        {
            throw new ForbiddenAppException("Kimliği doğrulanmış kullanıcı bilgisi çözümlenemedi.");
        }

        if (authenticatedUserId != request.UserId)
        {
            throw new ForbiddenAppException("Sadece kendi şifrenizi değiştirebilirsiniz.");
        }

        var user = await businessDbContext.Users
            .FirstOrDefaultAsync(x => x.Id == request.UserId && !x.IsDeleted, cancellationToken);

        if (user is null)
        {
            throw new NotFoundAppException($"Kullanıcı bulunamadı. userId={request.UserId}");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            await LogSecurityEventAsync("ChangePassword", false, "Mevcut şifre yanlış.", user.UserCode, user.Id, cancellationToken);
            throw new ForbiddenAppException("Mevcut şifre doğrulanamadı.");
        }

        try
        {
            await passwordPolicyService.EnforcePasswordChangePolicyOrThrowAsync(user, request.NewPassword, cancellationToken);
        }
        catch (ValidationAppException)
        {
            await LogSecurityEventAsync("ChangePassword", false, "Yeni şifre policy doğrulamasını geçemedi.", user.UserCode, user.Id, cancellationToken);
            throw;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.MustChangePassword = false;
        user.PasswordExpiresAt = DateTime.UtcNow.AddDays(90);

        await businessDbContext.SaveChangesAsync(cancellationToken);
        await passwordPolicyService.RecordPasswordHistoryAsync(user.Id, user.PasswordHash, cancellationToken);

        await LogSecurityEventAsync("ChangePassword", true, null, user.UserCode, user.Id, cancellationToken,
            new { user.PasswordExpiresAt });
    }

    public async Task<IReadOnlyList<SessionListItemDto>> ListSessionsAsync(int userId, bool onlyActive, CancellationToken cancellationToken)
    {
        var query = businessDbContext.UserSessions
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
        var session = await businessDbContext.UserSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId && !x.IsDeleted, cancellationToken);

        if (session is null)
        {
            throw new NotFoundAppException($"Session bulunamadı. sessionId={sessionId}");
        }

        if (session.IsRevoked)
        {
            return;
        }

        if (!currentUserContext.TryGetUserId(out var actorUserId))
        {
            throw new ForbiddenAppException("Kimliği doğrulanmış kullanıcı bilgisi çözümlenemedi.");
        }

        var principal = httpContextAccessor.HttpContext?.User;
        var isPrivilegedActor = principal?.IsInRole("SYS_ADMIN") == true || principal?.IsInRole("SYS_OPERATOR") == true;

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

            throw new ForbiddenAppException("Sadece kendi session kaydınızı revoke edebilirsiniz.");
        }

        var actor = currentUserContext.TryGetActorIdentity(out var actorIdentity)
            ? actorIdentity
            : "system";

        session.IsRevoked = true;
        session.RevokedAt = DateTime.UtcNow;
        session.RevokedBy = actor;

        await businessDbContext.SaveChangesAsync(cancellationToken);

        await LogSecurityEventAsync("RevokeSession", true, reason, actor, session.UserId, cancellationToken,
            new { session.Id, session.SessionKey, session.RevokedAt, session.RevokedBy, reason });
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
        var httpContext = httpContextAccessor.HttpContext;
        var payload = additional is null ? null : JsonSerializer.Serialize(additional);

        var log = new SecurityEventLog
        {
            Timestamp = DateTimeOffset.UtcNow,
            EventType = "AuthLifecycle",
            Severity = isSuccessful ? "Information" : "Warning",
            UserId = resource,
            Username = currentUserContext.TryGetUsername(out var username) ? username : resource,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
            Resource = resource,
            Action = action,
            IsSuccessful = isSuccessful,
            FailureReason = failureReason,
            AdditionalData = JsonSerializer.Serialize(new
            {
                NumericUserId = numericUserId,
                Payload = payload
            })
        };

        logDbContext.SecurityEventLogs.Add(log);
        await logDbContext.SaveChangesAsync(cancellationToken);
    }
}
