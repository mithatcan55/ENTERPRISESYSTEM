using System.ComponentModel.DataAnnotations;

namespace Identity.Application.Contracts;

public sealed class LoginRequest
{
    public string Identifier { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed record LoginResponseDto(
    int UserId,
    string UserCode,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    string TokenType,
    bool MustChangePassword,
    DateTime? PasswordExpiresAt,
    bool IsPasswordExpiringSoon,
    int? DaysUntilPasswordExpiry,
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
    [Required]
    [MinLength(3)]
    public string Reason { get; set; } = string.Empty;
}

public enum SessionRevokeScope
{
    Current = 1,
    Selected = 2,
    All = 3,
    AllExceptCurrent = 4
}

public sealed class RevokeBulkSessionsRequest
{
    public SessionRevokeScope Scope { get; set; } = SessionRevokeScope.Current;
    public List<int>? SessionIds { get; set; }
    public int? UserId { get; set; }
    [Required]
    [MinLength(3)]
    public string Reason { get; set; } = string.Empty;
}

public sealed record RevokeBulkSessionsResponse(
    SessionRevokeScope Scope,
    int RequestedCount,
    int RevokedCount,
    IReadOnlyList<int> RevokedSessionIds);

public sealed record SessionListItemDto(
    int Id,
    int UserId,
    string UserCode,
    string SessionKey,
    DateTime StartedAt,
    DateTime ExpiresAt,
    DateTime? LastSeenAt,
    bool IsRevoked,
    DateTime? RevokedAt,
    string? RevokedBy,
    string? ClientIpAddress,
    string? UserAgent);
