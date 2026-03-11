using Identity.Application.Contracts;

namespace Identity.Application.Roles.Queries;

public interface IListUserRolesQueryHandler
{
    Task<IReadOnlyList<UserRoleItemDto>> HandleAsync(int userId, CancellationToken cancellationToken);
}
