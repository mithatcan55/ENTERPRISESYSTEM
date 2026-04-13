using Application.Exceptions;
using Identity.Application.Roles.Commands;
using Identity.Infrastructure.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

        var existingAssignment = await identityDbContext.UserRoles
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId, cancellationToken);

        if (existingAssignment is not null)
        {
            if (!existingAssignment.IsDeleted)
            {
                return;
            }

            existingAssignment.IsDeleted = false;
            existingAssignment.DeletedAt = null;
            existingAssignment.DeletedBy = null;
            await identityDbContext.SaveChangesAsync(cancellationToken);
            await identityDbContext.RevokeAllSessionsAndRefreshTokensAsync(userId, "critical_change:role_change", cancellationToken);
            return;
        }

        identityDbContext.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = roleId
        });

        try
        {
            await identityDbContext.SaveChangesAsync(cancellationToken);
            await identityDbContext.RevokeAllSessionsAndRefreshTokensAsync(userId, "critical_change:role_change", cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Idempotent assign: another request inserted the same role concurrently.
        }
    }
}
