namespace Identity.Application.Contracts;

public sealed record UserListItemDto(
    int Id,
    string UserCode,
    string Username,
    string? FirstName,
    string? LastName,
    string DisplayName,
    string Email,
    bool IsActive,
    bool MustChangePassword,
    DateTime? PasswordExpiresAt,
    DateTime CreatedAt,
    string? CreatedBy,
    string? ModifiedBy,
    DateTime? ModifiedAt,
    bool IsDeleted,
    DateTime? DeletedAt,
    string? DeletedBy,
    string? ProfileImageUrl,
    int RoleCount,
    string? PrimaryRoleName);

public sealed record UserDetailDto(
    int Id,
    string UserCode,
    string Username,
    string? FirstName,
    string? LastName,
    string DisplayName,
    string Email,
    bool IsActive,
    bool MustChangePassword,
    DateTime? PasswordExpiresAt,
    DateTime CreatedAt,
    string? CreatedBy,
    string? ModifiedBy,
    DateTime? ModifiedAt,
    bool IsDeleted,
    DateTime? DeletedAt,
    string? DeletedBy,
    string? ProfileImageUrl);

public sealed record CreatedUserDto(
    int Id,
    string UserCode,
    string Username,
    string? FirstName,
    string? LastName,
    string Email,
    bool MustChangePassword,
    DateTime? PasswordExpiresAt);

public sealed class UpdateUserRequest
{
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? ProfileImageUrl { get; set; }
    public bool MustChangePassword { get; set; }
    // PasswordExpiresAt is system-managed: set automatically on password change
}

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
