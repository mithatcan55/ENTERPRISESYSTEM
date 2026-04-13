using Application.Exceptions;
using Identity.Application.Roles.Commands;
using Identity.Infrastructure.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Roles.Commands;

public sealed class UnassignRoleCommandHandler(IdentityDbContext identityDbContext) : IUnassignRoleCommandHandler
{
    public async Task HandleAsync(int userId, int roleId, CancellationToken cancellationToken)
    {
        var userRole = await identityDbContext.UserRoles
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId && !x.IsDeleted, cancellationToken);

        if (userRole is null)
        {
            throw new NotFoundAppException($"Kullanici-role atamasi bulunamadi. userId={userId}, roleId={roleId}");
        }

        userRole.IsDeleted = true;
        userRole.DeletedAt = DateTime.UtcNow;
        await identityDbContext.SaveChangesAsync(cancellationToken);
        await identityDbContext.RevokeAllSessionsAndRefreshTokensAsync(userId, "critical_change:role_change", cancellationToken);
    }
}
