using Application.Exceptions;
using Identity.Application.Contracts;
using Identity.Application.Users.Queries;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Users.Queries;

public sealed class GetUserPermissionSummaryQueryHandler(
    IdentityDbContext identityDbContext,
    AuthorizationDbContext authorizationDbContext) : IGetUserPermissionSummaryQueryHandler
{
    public async Task<UserPermissionSummaryDto> HandleAsync(int userId, CancellationToken cancellationToken)
    {
        var userExists = await identityDbContext.Users
            .AsNoTracking()
            .AnyAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);

        if (!userExists)
            throw new NotFoundAppException($"Kullanici bulunamadi. userId={userId}");

        // ── Level 1: Module permissions ─────────────────────────────
        var userModuleIds = await authorizationDbContext.UserModulePermissions
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .Select(x => x.ModuleId)
            .ToListAsync(cancellationToken);

        var allModules = await authorizationDbContext.Modules
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        // ── Level 2: SubModule permissions ──────────────────────────
        var userSubModuleIds = await authorizationDbContext.UserSubModulePermissions
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .Select(x => x.SubModuleId)
            .ToListAsync(cancellationToken);

        var allSubModules = await authorizationDbContext.SubModules
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        // ── Level 3: Page permissions ───────────────────────────────
        var userPageIds = await authorizationDbContext.UserPagePermissions
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .Select(x => x.SubModulePageId)
            .ToListAsync(cancellationToken);

        var allPages = await authorizationDbContext.SubModulePages
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.TransactionCode)
            .ToListAsync(cancellationToken);

        // ── Level 4: Company permissions ────────────────────────────
        var companies = await authorizationDbContext.UserCompanyPermissions
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .Select(x => new UserCompanyAccessDto(x.CompanyId, true))
            .ToListAsync(cancellationToken);

        // ── Level 5: Action permissions ─────────────────────────────
        // Join action permissions with pages to get TransactionCode
        var actionPerms = await (
            from ap in authorizationDbContext.UserPageActionPermissions.AsNoTracking()
            join p in authorizationDbContext.SubModulePages.AsNoTracking() on ap.SubModulePageId equals p.Id
            where ap.UserId == userId && !ap.IsDeleted
            select new UserActionPermissionDto(
                ap.Id, ap.UserId, ap.SubModulePageId,
                p.TransactionCode, ap.ActionCode, ap.IsAllowed,
                ap.CreatedAt, ap.ModifiedAt)
        ).ToListAsync(cancellationToken);

        var actionsByPage = actionPerms
            .Where(a => a.IsAllowed)
            .GroupBy(a => a.SubModulePageId)
            .ToDictionary(g => g.Key, g => g.Select(a => a.ActionCode).ToList());

        // ── Build hierarchy ─────────────────────────────────────────
        var moduleAccessDtos = allModules.Select(m =>
        {
            var subModulesForModule = allSubModules.Where(sm => sm.ModuleId == m.Id).ToList();

            var subModuleAccessDtos = subModulesForModule.Select(sm =>
            {
                var pagesForSub = allPages.Where(p => p.SubModuleId == sm.Id).ToList();

                var pageAccessDtos = pagesForSub.Select(p => new UserPageAccessDto(
                    p.Id,
                    p.Code,
                    p.Name,
                    p.TransactionCode,
                    userPageIds.Contains(p.Id),
                    actionsByPage.TryGetValue(p.Id, out var actions) ? actions : []
                )).ToList();

                return new UserSubModuleAccessDto(
                    sm.Id,
                    sm.Code,
                    sm.Name,
                    userSubModuleIds.Contains(sm.Id),
                    pageAccessDtos);
            }).ToList();

            return new UserModuleAccessDto(
                m.Id,
                m.Code,
                m.Name,
                userModuleIds.Contains(m.Id),
                subModuleAccessDtos);
        }).ToList();

        return new UserPermissionSummaryDto(userId, moduleAccessDtos, companies, actionPerms);
    }
}
