namespace Identity.Application.Roles.Commands;

public interface IAssignRoleCommandHandler
{
    Task HandleAsync(int userId, int roleId, CancellationToken cancellationToken);
}
