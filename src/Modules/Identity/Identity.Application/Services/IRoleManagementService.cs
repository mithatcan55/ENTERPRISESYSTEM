using Identity.Application.Contracts;

namespace Identity.Application.Services;

public interface IRoleManagementService
{
    Task<IReadOnlyList<RoleListItemDto>> ListRolesAsync(CancellationToken cancellationToken);
    Task<RoleListItemDto> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken);
    Task AssignRoleAsync(int userId, int roleId, CancellationToken cancellationToken);
    Task UnassignRoleAsync(int userId, int roleId, CancellationToken cancellationToken);
    Task DeleteRoleAsync(int roleId, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserRoleItemDto>> ListUserRolesAsync(int userId, CancellationToken cancellationToken);
}
