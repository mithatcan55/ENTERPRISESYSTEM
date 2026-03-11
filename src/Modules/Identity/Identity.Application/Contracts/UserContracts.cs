namespace Identity.Application.Contracts;

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

public sealed class UpdateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
