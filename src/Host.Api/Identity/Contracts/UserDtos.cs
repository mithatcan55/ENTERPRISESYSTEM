namespace Host.Api.Identity.Contracts;

public sealed record UserListItemDto(
    int Id,
    string UserCode,
    string Username,
    string Email,
    bool IsActive,
    bool MustChangePassword,
    DateTime? PasswordExpiresAt,
    DateTime CreatedAt);

public sealed record CreatedUserDto(
    int Id,
    string UserCode,
    string Username,
    string Email,
    bool MustChangePassword,
    DateTime? PasswordExpiresAt);
