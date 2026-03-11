using Identity.Application.Contracts;

namespace Identity.Application.Permissions.Commands;

public interface IUpsertUserActionPermissionCommandHandler
{
    Task<UserActionPermissionDto> HandleAsync(UpsertUserActionPermissionRequest request, CancellationToken cancellationToken);
}
