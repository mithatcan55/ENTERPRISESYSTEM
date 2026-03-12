using Application.Exceptions;
using Identity.Application.Roles.Commands;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Roles.Commands;

public sealed class AssignRoleCommandHandler(IdentityDbContext identityDbContext) : IAssignRoleCommandHandler
{
    public async Task HandleAsync(int userId, int roleId, CancellationToken cancellationToken)
    {
        var userExists = await identityDbContext.Users
            .AsNoTracking()
            .AnyAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);

        if (!userExists)
        {
            throw new NotFoundAppException($"Kullanici bulunamadi. userId={userId}");
        }

        var role = await identityDbContext.Roles
            .FirstOrDefaultAsync(x => x.Id == roleId && !x.IsDeleted, cancellationToken);

        if (role is null)
        {
            throw new NotFoundAppException($"Role bulunamadi. roleId={roleId}");
        }

        var alreadyAssigned = await identityDbContext.UserRoles
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.RoleId == roleId && !x.IsDeleted, cancellationToken);

        if (alreadyAssigned)
        {
            return;
        }

        identityDbContext.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = roleId
        });

        await identityDbContext.SaveChangesAsync(cancellationToken);
    }
}
