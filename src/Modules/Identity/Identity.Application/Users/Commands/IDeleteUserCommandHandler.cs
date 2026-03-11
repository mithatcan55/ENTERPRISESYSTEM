namespace Identity.Application.Users.Commands;

public interface IDeleteUserCommandHandler
{
    Task HandleAsync(int userId, CancellationToken cancellationToken);
}
