using Identity.Application.Contracts;
using Identity.Application.Roles.Queries;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Roles.Queries;

public sealed class ListUserRolesQueryHandler(IdentityDbContext identityDbContext) : IListUserRolesQueryHandler
{
    public async Task<IReadOnlyList<UserRoleItemDto>> HandleAsync(int userId, CancellationToken cancellationToken)
    {
        return await (
                from userRole in identityDbContext.UserRoles.AsNoTracking()
                join role in identityDbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                where !userRole.IsDeleted && !role.IsDeleted && userRole.UserId == userId
                orderby role.Name
                select new UserRoleItemDto(role.Id, role.Code, role.Name, role.IsSystemRole))
            .ToListAsync(cancellationToken);
    }
}
