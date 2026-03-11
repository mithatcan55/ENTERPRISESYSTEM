namespace Identity.Application.Roles.Commands;

public interface IDeleteRoleCommandHandler
{
    Task HandleAsync(int roleId, CancellationToken cancellationToken);
}
