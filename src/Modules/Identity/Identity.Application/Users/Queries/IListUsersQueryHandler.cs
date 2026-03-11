using Identity.Application.Contracts;

namespace Identity.Application.Users.Queries;

public interface IListUsersQueryHandler
{
    Task<IReadOnlyList<UserListItemDto>> HandleAsync(CancellationToken cancellationToken);
}
