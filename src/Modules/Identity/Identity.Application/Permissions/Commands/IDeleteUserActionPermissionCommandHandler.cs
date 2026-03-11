namespace Identity.Application.Permissions.Commands;

public interface IDeleteUserActionPermissionCommandHandler
{
    Task HandleAsync(int permissionId, CancellationToken cancellationToken);
}
