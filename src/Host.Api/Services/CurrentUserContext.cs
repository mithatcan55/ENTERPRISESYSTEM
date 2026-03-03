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
}
