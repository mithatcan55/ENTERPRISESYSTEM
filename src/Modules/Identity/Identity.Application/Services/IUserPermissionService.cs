using Identity.Application.Contracts;

namespace Identity.Application.Services;

public interface IUserPermissionService
{
    Task<UserActionPermissionDto> UpsertActionPermissionAsync(UpsertUserActionPermissionRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserActionPermissionDto>> ListActionPermissionsAsync(UserActionPermissionQueryRequest request, CancellationToken cancellationToken);
    Task DeleteActionPermissionAsync(int permissionId, CancellationToken cancellationToken);
}
