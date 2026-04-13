using Application.Security;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Host.Api.Services;

/// <summary>
/// Claim set'i üzerinden kullanıcı ve şirket kimliğini çözer.
/// </summary>
public sealed class CurrentUserContext(
    IHttpContextAccessor httpContextAccessor,
    IdentityDbContext identityDbContext) : ICurrentUserContext
{
    private const string DbRoleCacheKey = "__current_user_db_roles";

    public bool TryGetUserId(out int userId)
    {
        userId = 0;
        var user = httpContextAccessor.HttpContext?.User;

        var raw = user?.FindFirst("user_id")?.Value
                  ?? user?.FindFirst("sub")?.Value
                  ?? user?.FindFirst("uid")?.Value;

        return int.TryParse(raw, out userId);
    }

    public bool TryGetSessionId(out int sessionId)
    {
        sessionId = 0;
        var user = httpContextAccessor.HttpContext?.User;
        var raw = user?.FindFirst(SecurityClaimTypes.SessionId)?.Value;
        return int.TryParse(raw, out sessionId);
    }

    public bool TryGetCompanyId(out int companyId)
    {
        companyId = 0;
        var user = httpContextAccessor.HttpContext?.User;

        var raw = user?.FindFirst("company_id")?.Value
                  ?? user?.FindFirst("companyId")?.Value
                  ?? user?.FindFirst("tenant_company_id")?.Value;

        return int.TryParse(raw, out companyId);
    }

    public bool TryGetUserCode(out string userCode)
    {
        userCode = string.Empty;
        var user = httpContextAccessor.HttpContext?.User;

        var raw = user?.FindFirst("user_code")?.Value
                  ?? user?.FindFirst("usercode")?.Value
                  ?? user?.FindFirst("employee_code")?.Value;

        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        userCode = raw;
        return true;
    }

    public bool TryGetUsername(out string username)
    {
        username = string.Empty;
        var user = httpContextAccessor.HttpContext?.User;

        var raw = user?.FindFirst("preferred_username")?.Value
                  ?? user?.FindFirst("username")?.Value
                  ?? user?.Identity?.Name;

        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        username = raw;
        return true;
    }

    public bool TryGetActorIdentity(out string actorIdentity)
    {
        actorIdentity = string.Empty;

        if (TryGetUserCode(out var userCode))
        {
            actorIdentity = userCode;
            return true;
        }

        if (TryGetUsername(out var username))
        {
            actorIdentity = username;
            return true;
        }

        if (TryGetUserId(out var userId))
        {
            actorIdentity = userId.ToString();
            return true;
        }

        return false;
    }

    public bool IsInRole(string roleCode)
    {
        if (string.IsNullOrWhiteSpace(roleCode))
        {
            return false;
        }

        if (!TryGetUserId(out var userId))
        {
            return false;
        }

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return false;
        }

        if (httpContext.Items.TryGetValue(DbRoleCacheKey, out var cached) && cached is HashSet<string> cachedRoles)
        {
            return cachedRoles.Contains(roleCode);
        }

        var dbRoles = (
                from user in identityDbContext.Users.AsNoTracking()
                join userRole in identityDbContext.UserRoles.AsNoTracking() on user.Id equals userRole.UserId
                join role in identityDbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                where user.Id == userId
                      && user.IsActive
                      && !user.IsDeleted
                      && !userRole.IsDeleted
                      && !role.IsDeleted
                select role.Code)
            .Distinct()
            .ToList();

        var dbRoleSet = new HashSet<string>(dbRoles, StringComparer.OrdinalIgnoreCase);
        httpContext.Items[DbRoleCacheKey] = dbRoleSet;

        return dbRoleSet.Contains(roleCode);
    }
}
