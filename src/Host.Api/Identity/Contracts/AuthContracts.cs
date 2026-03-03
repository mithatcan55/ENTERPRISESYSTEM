namespace Host.Api.Identity.Contracts;

public sealed class LoginRequest
{
    public string Identifier { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed record LoginResponse(
    int UserId,
    string UserCode,
    string Username,
    string SessionKey,
    DateTime SessionExpiresAt,
    bool MustChangePassword,
    DateTime? PasswordExpiresAt);

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
