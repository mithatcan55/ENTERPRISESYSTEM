namespace Identity.Application.Contracts;

public sealed class LoginRequest
{
    public string Identifier { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed record LoginResponseDto(
    int UserId,
    string UserCode,
    string Username,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    string TokenType,
    bool MustChangePassword,
    DateTime? PasswordExpiresAt,
    EffectiveAuthorizationSummaryDto EffectiveAuthorization);

public sealed record RefreshTokenResponseDto(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    string TokenType);

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed record EffectiveAuthorizationSummaryDto(
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> TransactionCodes,
    IReadOnlyList<string> Permissions);

public sealed class ChangePasswordRequest
{
    public int UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class RevokeSessionRequest
{
    public string? Reason { get; set; }
}

public sealed record SessionListItemDto(
    int Id,
    int UserId,
    string SessionKey,
    DateTime StartedAt,
    DateTime ExpiresAt,
    DateTime? LastSeenAt,
    bool IsRevoked,
    DateTime? RevokedAt,
    string? RevokedBy,
    string? ClientIpAddress,
    string? UserAgent);
