using Identity.Application.Contracts;

namespace Identity.Application.Users.Queries;

public interface IListUsersQueryHandler
{
    Task<PagedResult<UserListItemDto>> HandleAsync(ListUsersQuery query, CancellationToken cancellationToken);
}
