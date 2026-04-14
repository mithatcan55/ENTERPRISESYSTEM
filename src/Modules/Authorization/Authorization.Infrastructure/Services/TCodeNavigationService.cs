using Authorization.Application.Contracts;
using Authorization.Application.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Infrastructure.Services;

public sealed class TCodeNavigationService(AuthorizationDbContext authorizationDbContext) : ITCodeNavigationService
{
    private static readonly AliasDefinition[] Aliases =
    [
        new("USRT01", "Users Create", "/users?mode=create", "SYS01"),
        new("USRT02", "Users Edit", "/users?mode=edit", "SYS02"),
    ];

    public async Task<IReadOnlyList<TCodeNavigationItemDto>> SearchAsync(
        int userId,
        string? query,
        int take,
        CancellationToken cancellationToken)
    {
        var requestedTake = Math.Clamp(take, 1, 100);
        var normalizedQuery = query?.Trim();
        var loweredQuery = normalizedQuery?.ToLowerInvariant();

        var baseItems = await (
                from pagePermission in authorizationDbContext.UserPagePermissions.AsNoTracking()
                join page in authorizationDbContext.SubModulePages.AsNoTracking() on pagePermission.SubModulePageId equals page.Id
                where pagePermission.UserId == userId
                      && !pagePermission.IsDeleted
                      && !page.IsDeleted
                      && !string.IsNullOrWhiteSpace(page.RouteLink)
                select new TCodeNavigationItemDto(
                    page.TransactionCode,
                    page.Name,
                    page.RouteLink!,
                    null))
            .Distinct()
            .ToListAsync(cancellationToken);

        var aliasItems = Aliases
            .Where(alias => baseItems.Any(x => x.TransactionCode.Equals(alias.SourceTransactionCode, StringComparison.OrdinalIgnoreCase)))
            .Select(alias => new TCodeNavigationItemDto(alias.Code, alias.Name, alias.RouteLink, alias.SourceTransactionCode))
            .ToList();

        var allItems = baseItems
            .Concat(aliasItems)
            .Where(item => string.IsNullOrWhiteSpace(loweredQuery)
                           || item.TransactionCode.ToLowerInvariant().Contains(loweredQuery, StringComparison.Ordinal)
                           || item.Name.ToLowerInvariant().Contains(loweredQuery, StringComparison.Ordinal))
            .OrderBy(item => item.TransactionCode)
            .ThenBy(item => item.Name)
            .Take(requestedTake)
            .ToList();

        return allItems;
    }

    public async Task<TCodeNavigationItemDto?> ResolveAsync(
        int userId,
        string transactionCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(transactionCode))
        {
            return null;
        }

        var normalizedCode = transactionCode.Trim().ToUpperInvariant();
        var alias = Aliases.FirstOrDefault(x => x.Code == normalizedCode);

        if (alias is not null)
        {
            var hasSourceAccess = await (
                    from pagePermission in authorizationDbContext.UserPagePermissions.AsNoTracking()
                    join page in authorizationDbContext.SubModulePages.AsNoTracking() on pagePermission.SubModulePageId equals page.Id
                    where pagePermission.UserId == userId
                          && !pagePermission.IsDeleted
                          && !page.IsDeleted
                          && page.TransactionCode == alias.SourceTransactionCode
                    select page.Id)
                .AnyAsync(cancellationToken);

            if (hasSourceAccess)
            {
                return new TCodeNavigationItemDto(alias.Code, alias.Name, alias.RouteLink, alias.SourceTransactionCode);
            }
        }

        var directItem = await (
                from pagePermission in authorizationDbContext.UserPagePermissions.AsNoTracking()
                join page in authorizationDbContext.SubModulePages.AsNoTracking() on pagePermission.SubModulePageId equals page.Id
                where pagePermission.UserId == userId
                      && !pagePermission.IsDeleted
                      && !page.IsDeleted
                      && page.TransactionCode == normalizedCode
                      && !string.IsNullOrWhiteSpace(page.RouteLink)
                select new TCodeNavigationItemDto(
                    page.TransactionCode,
                    page.Name,
                    page.RouteLink!,
                    null))
            .FirstOrDefaultAsync(cancellationToken);

        return directItem;
    }

    private sealed record AliasDefinition(
        string Code,
        string Name,
        string RouteLink,
        string SourceTransactionCode);
}
