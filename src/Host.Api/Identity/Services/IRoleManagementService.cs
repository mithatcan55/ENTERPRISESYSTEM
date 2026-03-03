using Host.Api.Identity.Contracts;

namespace Host.Api.Identity.Services;

public interface IRoleManagementService
{
    Task<IReadOnlyList<RoleListItemDto>> ListRolesAsync(CancellationToken cancellationToken);
    Task<RoleListItemDto> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken);
    Task AssignRoleAsync(int userId, int roleId, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserRoleItemDto>> ListUserRolesAsync(int userId, CancellationToken cancellationToken);
}
