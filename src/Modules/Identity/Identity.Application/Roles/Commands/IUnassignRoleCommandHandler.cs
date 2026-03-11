namespace Identity.Application.Roles.Commands;

public interface IUnassignRoleCommandHandler
{
    Task HandleAsync(int userId, int roleId, CancellationToken cancellationToken);
}
