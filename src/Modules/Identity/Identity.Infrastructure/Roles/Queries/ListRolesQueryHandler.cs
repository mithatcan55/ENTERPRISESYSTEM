using Identity.Application.Contracts;
using Identity.Application.Roles.Queries;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Roles.Queries;

public sealed class ListRolesQueryHandler(BusinessDbContext businessDbContext) : IListRolesQueryHandler
{
    public async Task<IReadOnlyList<RoleListItemDto>> HandleAsync(CancellationToken cancellationToken)
    {
        return await businessDbContext.Roles
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => new RoleListItemDto(x.Id, x.Code, x.Name, x.Description, x.IsSystemRole, x.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
