using Identity.Application.Contracts;

namespace Identity.Application.Users.Commands;

public interface IUpdateUserCommandHandler
{
    Task<UserListItemDto> HandleAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken);
}
