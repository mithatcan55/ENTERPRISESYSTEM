namespace Identity.Application.Contracts;

public sealed record UserListItemDto(
    int Id,
    string UserCode,
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
    IReadOnlyList<UserRoleDto> Roles,
    IReadOnlyList<UserDirectPermissionDto> DirectPermissions);

public sealed record UserRoleDto(int RoleId, string RoleCode, string RoleName);

public sealed record UserDirectPermissionDto(
    int Id,
    int SubModulePageId,
    string TransactionCode,
    string ActionCode,
    bool IsAllowed);

public sealed record CreatedUserDto(
    int Id,
    string UserCode,
    string? FirstName,
    string? LastName,
    string Email,
    bool MustChangePassword,
    DateTime? PasswordExpiresAt);

public sealed class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? ProfileImageUrl { get; set; }
    public bool MustChangePassword { get; set; }
    public List<int>? RoleIds { get; set; }
    public List<int>? PermissionIds { get; set; }
}

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record LookupItemDto(int Id, string Name);

public sealed record PermissionLookupItemDto(
    int Id,
    string TransactionCode,
    string ActionCode,
    string DisplayName);

public sealed record UserLookupsDto(
    IReadOnlyList<LookupItemDto> Roles,
    IReadOnlyList<PermissionLookupItemDto> Permissions);
