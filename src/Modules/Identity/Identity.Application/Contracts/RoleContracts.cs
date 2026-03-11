namespace Identity.Application.Contracts;

public sealed class CreateRoleRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed record RoleListItemDto(
    int Id,
    string Code,
    string Name,
    string? Description,
    bool IsSystemRole,
    DateTime CreatedAt);

public sealed record UserRoleItemDto(
    int RoleId,
    string RoleCode,
    string RoleName,
    bool IsSystemRole);
