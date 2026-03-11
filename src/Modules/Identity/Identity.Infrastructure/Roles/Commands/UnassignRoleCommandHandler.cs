using Application.Exceptions;
using Identity.Application.Roles.Commands;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Roles.Commands;

public sealed class UnassignRoleCommandHandler(BusinessDbContext businessDbContext) : IUnassignRoleCommandHandler
{
    public async Task HandleAsync(int userId, int roleId, CancellationToken cancellationToken)
    {
        var userRole = await businessDbContext.UserRoles
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId && !x.IsDeleted, cancellationToken);

        if (userRole is null)
        {
            throw new NotFoundAppException($"Kullanici-role atamasi bulunamadi. userId={userId}, roleId={roleId}");
        }

        userRole.IsDeleted = true;
        userRole.DeletedAt = DateTime.UtcNow;
        await businessDbContext.SaveChangesAsync(cancellationToken);
    }
}
