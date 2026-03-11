using Identity.Application.Contracts;

namespace Identity.Application.Roles.Queries;

public interface IListRolesQueryHandler
{
    Task<IReadOnlyList<RoleListItemDto>> HandleAsync(CancellationToken cancellationToken);
}
