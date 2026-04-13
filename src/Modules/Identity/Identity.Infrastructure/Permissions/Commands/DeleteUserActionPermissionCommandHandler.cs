using Application.Exceptions;
using Identity.Application.Permissions.Commands;
using Identity.Infrastructure.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Permissions.Commands;

public sealed class DeleteUserActionPermissionCommandHandler(
    AuthorizationDbContext authorizationDbContext,
    IdentityDbContext identityDbContext) : IDeleteUserActionPermissionCommandHandler
{
    public async Task HandleAsync(int permissionId, CancellationToken cancellationToken)
    {
        if (permissionId <= 0)
        {
            throw new ValidationAppException(
                "Permission silme dogrulamasi basarisiz.",
                new Dictionary<string, string[]>
                {
                    ["permissionId"] = ["Gecerli bir permissionId zorunludur."]
                });
        }

        var permission = await authorizationDbContext.UserPageActionPermissions
            .FirstOrDefaultAsync(x => x.Id == permissionId && !x.IsDeleted, cancellationToken);

        if (permission is null)
        {
            throw new NotFoundAppException(
                $"Permission bulunamadi. permissionId={permissionId}",
                errorCode: "permission_not_found");
        }

        permission.IsDeleted = true;
        permission.DeletedAt = DateTime.UtcNow;
        await authorizationDbContext.SaveChangesAsync(cancellationToken);
        await identityDbContext.RevokeAllSessionsAndRefreshTokensAsync(permission.UserId, "critical_change:permission_change", cancellationToken);
    }
}
