using Application.Security;

namespace Host.Api.Services;

/// <summary>
/// Claim set'i üzerinden kullanıcı ve şirket kimliğini çözer.
/// </summary>
public sealed class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public bool TryGetUserId(out int userId)
    {
        userId = 0;
        var user = httpContextAccessor.HttpContext?.User;

        var raw = user?.FindFirst("user_id")?.Value
                  ?? user?.FindFirst("sub")?.Value
                  ?? user?.FindFirst("uid")?.Value;

        return int.TryParse(raw, out userId);
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

        var user = httpContextAccessor.HttpContext?.User;
        return user?.IsInRole(roleCode) == true
               || user?.Claims.Any(x =>
                   (string.Equals(x.Type, SecurityClaimTypes.Role, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(x.Type, SecurityClaimTypes.RoleClaim, StringComparison.OrdinalIgnoreCase))
                   && string.Equals(x.Value, roleCode, StringComparison.OrdinalIgnoreCase)) == true;
    }
}
