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
            .Select(p => new PermissionLookupItemDto(
                p.Id,
                p.TransactionCode,
                p.Code,
                p.Name,
                $"{ResolveResource(p.TransactionCode)}.{ResolveAction(p.Code)}",
                $"{p.TransactionCode}.{p.Code}",
                p.TransactionCode))
            .ToListAsync(cancellationToken);

        return new UserLookupsDto(roles, permissions);
    }

    private static string ResolveResource(string? transactionCode)
    {
        return (transactionCode ?? string.Empty).Trim().ToUpperInvariant() switch
        {
            "SYS01" or "SYS02" or "SYS03" or "SYS04" => "Users",
            "SYS05" => "Roles",
            "SYS06" => "Permissions",
            "SYS07" => "Localization",
            _ => "General"
        };
    }

    private static string ResolveAction(string? pageCode)
    {
        var normalized = (pageCode ?? string.Empty).Trim().ToUpperInvariant();
        if (normalized.Contains("CREATE", StringComparison.Ordinal))
        {
            return "Create";
        }

        if (normalized.Contains("DELETE", StringComparison.Ordinal))
        {
            return "Delete";
        }

        if (normalized.Contains("UPDATE", StringComparison.Ordinal)
            || normalized.Contains("EDIT", StringComparison.Ordinal)
            || normalized.Contains("ROLES", StringComparison.Ordinal)
            || normalized.Contains("PERMISSIONS", StringComparison.Ordinal)
            || normalized.Contains("MANAGE", StringComparison.Ordinal))
        {
            return "Update";
        }

        return "View";
    }
}
