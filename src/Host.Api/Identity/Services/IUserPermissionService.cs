using Host.Api.Identity.Contracts;

namespace Host.Api.Identity.Services;

public interface IUserPermissionService
{
    Task<UserActionPermissionDto> UpsertActionPermissionAsync(UpsertUserActionPermissionRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserActionPermissionDto>> ListActionPermissionsAsync(UserActionPermissionQueryRequest request, CancellationToken cancellationToken);
}
