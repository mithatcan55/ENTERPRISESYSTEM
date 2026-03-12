using Identity.Application.Contracts;
using Identity.Application.Roles.Queries;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Roles.Queries;

public sealed class ListRolesQueryHandler(IdentityDbContext identityDbContext) : IListRolesQueryHandler
{
    public async Task<IReadOnlyList<RoleListItemDto>> HandleAsync(CancellationToken cancellationToken)
    {
        // Role listeleme gibi basit okuma akislari CQRS tarafinda query handler ile net bir sorumluluk alir.
        return await identityDbContext.Roles
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            // Role adina gore siralamak operasyon ekraninda taramayi kolaylastirir.
            .OrderBy(x => x.Name)
            .Select(x => new RoleListItemDto(x.Id, x.Code, x.Name, x.Description, x.IsSystemRole, x.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
