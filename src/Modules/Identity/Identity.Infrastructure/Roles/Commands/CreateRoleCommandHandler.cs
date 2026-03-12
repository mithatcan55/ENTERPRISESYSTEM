using Application.Exceptions;
using Identity.Application.Contracts;
using Identity.Application.Roles.Commands;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Roles.Commands;

public sealed class CreateRoleCommandHandler(IdentityDbContext identityDbContext) : ICreateRoleCommandHandler
{
    public async Task<RoleListItemDto> HandleAsync(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        // Role olusturma akisini yalın tutuyoruz:
        // request kontrolu, benzersizlik kontrolu, kaydetme.
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

        // Role'ler hem kodla hem adla kullanildigi icin iki alan da benzersiz kabul ediliyor.
        var exists = await identityDbContext.Roles
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

        identityDbContext.Roles.Add(role);
        await identityDbContext.SaveChangesAsync(cancellationToken);

        // DTO donmemizin nedeni presentation katmanini EF entity detaylarindan bagimsiz tutmak.
        return new RoleListItemDto(role.Id, role.Code, role.Name, role.Description, role.IsSystemRole, role.CreatedAt);
    }
}
