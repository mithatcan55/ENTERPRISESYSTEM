using Host.Api.Exceptions;
using Host.Api.Identity.Contracts;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace Host.Api.Identity.Services;

public sealed class RoleManagementService(BusinessDbContext businessDbContext) : IRoleManagementService
{
    public async Task<IReadOnlyList<RoleListItemDto>> ListRolesAsync(CancellationToken cancellationToken)
    {
        return await businessDbContext.Roles
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => new RoleListItemDto(x.Id, x.Code, x.Name, x.Description, x.IsSystemRole, x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<RoleListItemDto> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationAppException(
                "Role oluşturma doğrulaması başarısız.",
                new Dictionary<string, string[]>
                {
                    ["code"] = ["Code zorunludur."],
                    ["name"] = ["Name zorunludur."]
                });
        }

        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();

        var exists = await businessDbContext.Roles
            .AsNoTracking()
            .AnyAsync(x => !x.IsDeleted && (x.Code == code || x.Name == name), cancellationToken);

        if (exists)
        {
            throw new ValidationAppException(
                "Role benzersizlik kontrolü başarısız.",
                new Dictionary<string, string[]>
                {
                    ["role"] = ["Aynı code veya name ile role zaten mevcut."]
                });
        }

        var role = new Role
        {
            Code = code,
            Name = name,
            Description = request.Description?.Trim(),
            IsSystemRole = false
        };

        businessDbContext.Roles.Add(role);
        await businessDbContext.SaveChangesAsync(cancellationToken);

        return new RoleListItemDto(role.Id, role.Code, role.Name, role.Description, role.IsSystemRole, role.CreatedAt);
    }

    public async Task AssignRoleAsync(int userId, int roleId, CancellationToken cancellationToken)
    {
        var userExists = await businessDbContext.Users
            .AsNoTracking()
            .AnyAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);

        if (!userExists)
        {
            throw new NotFoundAppException($"Kullanıcı bulunamadı. userId={userId}");
        }

        var role = await businessDbContext.Roles
            .FirstOrDefaultAsync(x => x.Id == roleId && !x.IsDeleted, cancellationToken);

        if (role is null)
        {
            throw new NotFoundAppException($"Role bulunamadı. roleId={roleId}");
        }

        var alreadyAssigned = await businessDbContext.UserRoles
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.RoleId == roleId && !x.IsDeleted, cancellationToken);

        if (alreadyAssigned)
        {
            return;
        }

        businessDbContext.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = roleId
        });

        await businessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserRoleItemDto>> ListUserRolesAsync(int userId, CancellationToken cancellationToken)
    {
        return await (
                from userRole in businessDbContext.UserRoles.AsNoTracking()
                join role in businessDbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                where !userRole.IsDeleted && !role.IsDeleted && userRole.UserId == userId
                orderby role.Name
                select new UserRoleItemDto(role.Id, role.Code, role.Name, role.IsSystemRole))
            .ToListAsync(cancellationToken);
    }
}
