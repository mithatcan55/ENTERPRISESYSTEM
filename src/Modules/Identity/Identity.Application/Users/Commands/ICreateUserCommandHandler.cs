using Identity.Application.Contracts;

namespace Identity.Application.Users.Commands;

public interface ICreateUserCommandHandler
{
    Task<CreatedUserDto> HandleAsync(CreateUserRequest request, CancellationToken cancellationToken);
}
