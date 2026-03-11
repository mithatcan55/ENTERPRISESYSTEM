namespace Identity.Application.Users.Commands;

public interface IDeactivateUserCommandHandler
{
    Task HandleAsync(int userId, CancellationToken cancellationToken);
}
