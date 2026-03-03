using System.Security.Claims;
using System.Text.Encodings.Web;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Host.Api.Security;

public sealed class SessionAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    BusinessDbContext businessDbContext) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "SessionBearer";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var authHeader = authHeaderValues.ToString();
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Authorization header formatı geçersiz.");
        }

        var sessionKey = authHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(sessionKey))
        {
            return AuthenticateResult.Fail("Session token boş olamaz.");
        }

        var now = DateTime.UtcNow;

        var session = await businessDbContext.UserSessions
            .FirstOrDefaultAsync(x => x.SessionKey == sessionKey && !x.IsDeleted, Context.RequestAborted);

        if (session is null)
        {
            return AuthenticateResult.Fail("Session bulunamadı.");
        }

        if (session.IsRevoked)
        {
            return AuthenticateResult.Fail("Session revoke edilmiş.");
        }

        if (session.ExpiresAt <= now)
        {
            return AuthenticateResult.Fail("Session süresi dolmuş.");
        }

        var user = await businessDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == session.UserId && !x.IsDeleted, Context.RequestAborted);

        if (user is null)
        {
            return AuthenticateResult.Fail("Session kullanıcısı bulunamadı.");
        }

        if (!user.IsActive)
        {
            return AuthenticateResult.Fail("Kullanıcı pasif durumda.");
        }

        if (user.PasswordExpiresAt.HasValue && user.PasswordExpiresAt.Value <= now)
        {
            return AuthenticateResult.Fail("Şifre süresi dolmuş.");
        }

        if (!session.LastSeenAt.HasValue || (now - session.LastSeenAt.Value).TotalSeconds >= 30)
        {
            session.LastSeenAt = now;
            await businessDbContext.SaveChangesAsync(Context.RequestAborted);
        }

        var companyId = await businessDbContext.UserCompanyPermissions
            .AsNoTracking()
            .Where(x => x.UserId == user.Id && !x.IsDeleted)
            .OrderBy(x => x.Id)
            .Select(x => (int?)x.CompanyId)
            .FirstOrDefaultAsync(Context.RequestAborted);

        var roles = await (
                from userRole in businessDbContext.UserRoles.AsNoTracking()
                join role in businessDbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                where userRole.UserId == user.Id && !userRole.IsDeleted && !role.IsDeleted
                select role.Code)
            .ToListAsync(Context.RequestAborted);

        var permissions = await businessDbContext.UserPageActionPermissions
            .AsNoTracking()
            .Where(x => x.UserId == user.Id && !x.IsDeleted && x.IsAllowed)
            .Select(x => x.ActionCode)
            .Distinct()
            .ToListAsync(Context.RequestAborted);

        var claims = new List<Claim>
        {
            new(SecurityClaimTypes.Subject, user.Id.ToString()),
            new(SecurityClaimTypes.UserId, user.Id.ToString()),
            new(SecurityClaimTypes.UserCode, user.UserCode),
            new(SecurityClaimTypes.Username, user.Username),
            new(SecurityClaimTypes.NameIdentifier, user.Id.ToString()),
            new(SecurityClaimTypes.Name, user.Username),
            new(SecurityClaimTypes.SessionId, session.Id.ToString()),
            new(SecurityClaimTypes.SessionKey, session.SessionKey)
        };

        if (companyId.HasValue)
        {
            claims.Add(new Claim(SecurityClaimTypes.CompanyId, companyId.Value.ToString()));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(SecurityClaimTypes.RoleClaim, role));
            claims.Add(new Claim(SecurityClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim(SecurityClaimTypes.Permission, permission));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return AuthenticateResult.Success(ticket);
    }
}
