namespace Identity.Application.Users.Commands;

public interface IReactivateUserCommandHandler
{
    Task HandleAsync(int userId, CancellationToken cancellationToken);
}
