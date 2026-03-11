using Identity.Application.Contracts;

namespace Identity.Application.Permissions.Queries;

public interface IListUserActionPermissionsQueryHandler
{
    Task<IReadOnlyList<UserActionPermissionDto>> HandleAsync(UserActionPermissionQueryRequest request, CancellationToken cancellationToken);
}
