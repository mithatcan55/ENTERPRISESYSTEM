using Application.Exceptions;
using Identity.Application.Contracts;
using Identity.Application.Users.Commands;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Users.Commands;

public sealed class GrantUserPermissionsCommandHandler(
    IdentityDbContext identityDbContext,
    AuthorizationDbContext authorizationDbContext) : IGrantUserPermissionsCommandHandler
{
    public async Task HandleAsync(int userId, GrantUserPermissionsRequest request, CancellationToken cancellationToken)
    {
        var userExists = await identityDbContext.Users
            .AsNoTracking()
            .AnyAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);

        if (!userExists)
            throw new NotFoundAppException($"Kullanici bulunamadi. userId={userId}");

        // ── Level 1: Modules ────────────────────────────────────────
        if (request.ModuleIds is { Count: > 0 })
        {
            var existing = await authorizationDbContext.UserModulePermissions
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            var existingIds = existing.Select(x => x.ModuleId).ToHashSet();

            foreach (var moduleId in request.ModuleIds.Where(id => !existingIds.Contains(id)))
            {
                authorizationDbContext.UserModulePermissions.Add(new UserModulePermission
                {
                    UserId = userId,
                    ModuleId = moduleId,
                    AuthorizationLevel = 1
                });
            }

            if (request.RevokeOthers)
            {
                var toRevoke = existing.Where(x => !request.ModuleIds.Contains(x.ModuleId));
                foreach (var perm in toRevoke)
                    perm.IsDeleted = true;
            }
        }

        // ── Level 2: SubModules ─────────────────────────────────────
        if (request.SubModuleIds is { Count: > 0 })
        {
            var existing = await authorizationDbContext.UserSubModulePermissions
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            var existingIds = existing.Select(x => x.SubModuleId).ToHashSet();

            foreach (var subModuleId in request.SubModuleIds.Where(id => !existingIds.Contains(id)))
            {
                authorizationDbContext.UserSubModulePermissions.Add(new UserSubModulePermission
                {
                    UserId = userId,
                    SubModuleId = subModuleId,
                    AuthorizationLevel = 2
                });
            }

            if (request.RevokeOthers)
            {
                var toRevoke = existing.Where(x => !request.SubModuleIds.Contains(x.SubModuleId));
                foreach (var perm in toRevoke)
                    perm.IsDeleted = true;
            }
        }

        // ── Level 3: Pages ──────────────────────────────────────────
        if (request.PageIds is { Count: > 0 })
        {
            var existing = await authorizationDbContext.UserPagePermissions
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            var existingIds = existing.Select(x => x.SubModulePageId).ToHashSet();

            foreach (var pageId in request.PageIds.Where(id => !existingIds.Contains(id)))
            {
                authorizationDbContext.UserPagePermissions.Add(new UserPagePermission
                {
                    UserId = userId,
                    SubModulePageId = pageId,
                    AuthorizationLevel = 3
                });
            }

            if (request.RevokeOthers)
            {
                var toRevoke = existing.Where(x => !request.PageIds.Contains(x.SubModulePageId));
                foreach (var perm in toRevoke)
                    perm.IsDeleted = true;
            }
        }

        // ── Level 4: Companies ──────────────────────────────────────
        if (request.CompanyIds is { Count: > 0 })
        {
            var existing = await authorizationDbContext.UserCompanyPermissions
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            var existingIds = existing.Select(x => x.CompanyId).ToHashSet();

            foreach (var companyId in request.CompanyIds.Where(id => !existingIds.Contains(id)))
            {
                authorizationDbContext.UserCompanyPermissions.Add(new UserCompanyPermission
                {
                    UserId = userId,
                    CompanyId = companyId,
                    AuthorizationLevel = 4
                });
            }

            if (request.RevokeOthers)
            {
                var toRevoke = existing.Where(x => !request.CompanyIds.Contains(x.CompanyId));
                foreach (var perm in toRevoke)
                    perm.IsDeleted = true;
            }
        }

        await authorizationDbContext.SaveChangesAsync(cancellationToken);
    }
}
