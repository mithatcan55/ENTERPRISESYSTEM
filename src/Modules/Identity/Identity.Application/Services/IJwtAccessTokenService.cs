namespace Identity.Application.Services;

public sealed record AccessTokenRequest(
    int UserId,
    string UserCode,
    int? CompanyId,
    int SessionId,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);

public sealed record AccessTokenResult(string Token, DateTime ExpiresAtUtc, string TokenId);

public interface IJwtAccessTokenService
{
    AccessTokenResult CreateToken(AccessTokenRequest request);
}
