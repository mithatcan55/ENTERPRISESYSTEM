using Application.Exceptions;
using Identity.Application.Roles.Commands;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Roles.Commands;

public sealed class DeleteRoleCommandHandler(BusinessDbContext businessDbContext) : IDeleteRoleCommandHandler
{
    public async Task HandleAsync(int roleId, CancellationToken cancellationToken)
    {
        var role = await businessDbContext.Roles
            .FirstOrDefaultAsync(x => x.Id == roleId && !x.IsDeleted, cancellationToken);

        if (role is null)
        {
            throw new NotFoundAppException($"Role bulunamadi. roleId={roleId}");
        }

        if (role.IsSystemRole)
        {
            throw new ForbiddenAppException("Sistem role kayitlari silinemez.");
        }

        var hasActiveAssignments = await businessDbContext.UserRoles
            .AsNoTracking()
            .AnyAsync(x => x.RoleId == roleId && !x.IsDeleted, cancellationToken);

        if (hasActiveAssignments)
        {
            throw new ValidationAppException(
                "Role silme dogrulamasi basarisiz.",
                new Dictionary<string, string[]>
                {
                    ["roleId"] = ["Role aktif kullanici atamasi icerdigi icin silinemez. Once atamalari kaldirin."]
                });
        }

        role.IsDeleted = true;
        role.DeletedAt = DateTime.UtcNow;
        await businessDbContext.SaveChangesAsync(cancellationToken);
    }
}
