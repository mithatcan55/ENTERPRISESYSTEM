using Application.Exceptions;
using Identity.Application.Contracts;
using Identity.Application.Roles.Commands;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Roles.Commands;

public sealed class CreateRoleCommandHandler(BusinessDbContext businessDbContext) : ICreateRoleCommandHandler
{
    public async Task<RoleListItemDto> HandleAsync(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationAppException(
                "Role olusturma dogrulamasi basarisiz.",
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
                "Role benzersizlik kontrolu basarisiz.",
                new Dictionary<string, string[]>
                {
                    ["role"] = ["Ayni code veya name ile role zaten mevcut."]
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
}
