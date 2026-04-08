using Identity.Application.Contracts;
using Identity.Application.Users.Queries;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Users.Queries;

public sealed class GetUserLookupsQueryHandler(
    IdentityDbContext identityDbContext,
    AuthorizationDbContext authorizationDbContext) : IGetUserLookupsQueryHandler
{
    public async Task<UserLookupsDto> HandleAsync(CancellationToken cancellationToken)
    {
        var roles = await identityDbContext.Roles
            .AsNoTracking()
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.Name)
            .Select(r => new LookupItemDto(r.Id, r.Name))
            .ToListAsync(cancellationToken);

        var permissions = await authorizationDbContext.SubModulePages
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.TransactionCode)
            .Select(p => new PermissionLookupItemDto(p.Id, p.TransactionCode, p.Code, p.Name))
            .ToListAsync(cancellationToken);

        return new UserLookupsDto(roles, permissions);
    }
}
