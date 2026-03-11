using System.Security.Claims;

namespace Application.Security;

public static class SecurityClaimTypes
{
    public const string Subject = "sub";
    public const string UserId = "user_id";
    public const string UserCode = "user_code";
    public const string Username = "username";
    public const string SessionId = "session_id";
    public const string SessionKey = "session_key";
    public const string CompanyId = "company_id";
    public const string Role = "role";
    public const string Permission = "permission";

    public const string NameIdentifier = ClaimTypes.NameIdentifier;
    public const string Name = ClaimTypes.Name;
    public const string RoleClaim = ClaimTypes.Role;
}
