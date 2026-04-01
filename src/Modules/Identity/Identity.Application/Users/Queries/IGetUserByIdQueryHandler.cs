using Identity.Application.Contracts;

namespace Identity.Application.Users.Queries;

public interface IGetUserByIdQueryHandler
{
    Task<UserDetailDto> HandleAsync(int userId, CancellationToken cancellationToken);
}
