using Identity.Application.Contracts;

namespace Identity.Application.Users.Commands;

public interface IGrantUserPermissionsCommandHandler
{
    Task HandleAsync(int userId, GrantUserPermissionsRequest request, CancellationToken cancellationToken);
}
