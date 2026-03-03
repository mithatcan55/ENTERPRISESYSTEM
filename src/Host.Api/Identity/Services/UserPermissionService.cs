using Host.Api.Exceptions;
using Host.Api.Identity.Contracts;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Host.Api.Identity.Services;

public sealed class UserPermissionService(BusinessDbContext businessDbContext) : IUserPermissionService
{
    public async Task<UserActionPermissionDto> UpsertActionPermissionAsync(UpsertUserActionPermissionRequest request, CancellationToken cancellationToken)
    {
        if (request.UserId <= 0)
        {
            throw new ValidationAppException(
                "Permission doğrulaması başarısız.",
                new Dictionary<string, string[]>
                {
                    ["userId"] = ["Geçerli bir userId zorunludur."]
                });
        }

        if (string.IsNullOrWhiteSpace(request.ActionCode))
        {
            throw new ValidationAppException(
                "Permission doğrulaması başarısız.",
                new Dictionary<string, string[]>
                {
                    ["actionCode"] = ["ActionCode zorunludur."]
                });
        }

        var userExists = await businessDbContext.Users
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.UserId && !x.IsDeleted, cancellationToken);

        if (!userExists)
        {
            throw new NotFoundAppException($"Kullanıcı bulunamadı. userId={request.UserId}");
        }

        var page = await ResolvePageAsync(request.SubModulePageId, request.TransactionCode, cancellationToken);

        var normalizedActionCode = request.ActionCode.Trim().ToUpperInvariant();

        var permission = await businessDbContext.UserPageActionPermissions
            .FirstOrDefaultAsync(
                x => x.UserId == request.UserId
                     && x.SubModulePageId == page.Id
                     && x.ActionCode == normalizedActionCode,
                cancellationToken);

        if (permission is null)
        {
            permission = new UserPageActionPermission
            {
                UserId = request.UserId,
                SubModulePageId = page.Id,
                ActionCode = normalizedActionCode,
                IsAllowed = request.IsAllowed
            };

            businessDbContext.UserPageActionPermissions.Add(permission);
        }
        else
        {
            permission.IsAllowed = request.IsAllowed;
            permission.IsDeleted = false;
            permission.DeletedAt = null;
            permission.DeletedBy = null;
        }

        await businessDbContext.SaveChangesAsync(cancellationToken);

        return new UserActionPermissionDto(
            permission.Id,
            permission.UserId,
            permission.SubModulePageId,
            page.TransactionCode,
            permission.ActionCode,
            permission.IsAllowed,
            permission.CreatedAt,
            permission.ModifiedAt);
    }

    public async Task<IReadOnlyList<UserActionPermissionDto>> ListActionPermissionsAsync(UserActionPermissionQueryRequest request, CancellationToken cancellationToken)
    {
        if (request.UserId <= 0)
        {
            throw new ValidationAppException(
                "Permission sorgu doğrulaması başarısız.",
                new Dictionary<string, string[]>
                {
                    ["userId"] = ["Geçerli bir userId zorunludur."]
                });
        }

        var query =
            from permission in businessDbContext.UserPageActionPermissions.AsNoTracking()
            join page in businessDbContext.SubModulePages.AsNoTracking() on permission.SubModulePageId equals page.Id
            where permission.UserId == request.UserId
                  && !permission.IsDeleted
                  && !page.IsDeleted
            select new { permission, page };

        if (request.SubModulePageId.HasValue)
        {
            query = query.Where(x => x.permission.SubModulePageId == request.SubModulePageId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.TransactionCode))
        {
            var normalizedTCode = request.TransactionCode.Trim().ToUpperInvariant();
            query = query.Where(x => x.page.TransactionCode == normalizedTCode);
        }

        return await query
            .OrderBy(x => x.page.TransactionCode)
            .ThenBy(x => x.permission.ActionCode)
            .Select(x => new UserActionPermissionDto(
                x.permission.Id,
                x.permission.UserId,
                x.permission.SubModulePageId,
                x.page.TransactionCode,
                x.permission.ActionCode,
                x.permission.IsAllowed,
                x.permission.CreatedAt,
                x.permission.ModifiedAt))
            .ToListAsync(cancellationToken);
    }

    private async Task<SubModulePage> ResolvePageAsync(int? subModulePageId, string? transactionCode, CancellationToken cancellationToken)
    {
        if (subModulePageId.HasValue)
        {
            var pageById = await businessDbContext.SubModulePages
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == subModulePageId.Value && !x.IsDeleted, cancellationToken);

            if (pageById is null)
            {
                throw new NotFoundAppException($"Sayfa bulunamadı. subModulePageId={subModulePageId.Value}");
            }

            return pageById;
        }

        if (string.IsNullOrWhiteSpace(transactionCode))
        {
            throw new ValidationAppException(
                "Permission doğrulaması başarısız.",
                new Dictionary<string, string[]>
                {
                    ["subModulePageId"] = ["SubModulePageId veya TransactionCode zorunludur."],
                    ["transactionCode"] = ["SubModulePageId veya TransactionCode zorunludur."]
                });
        }

        var normalizedTCode = transactionCode.Trim().ToUpperInvariant();

        var pageByTCode = await businessDbContext.SubModulePages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TransactionCode == normalizedTCode && !x.IsDeleted, cancellationToken);

        if (pageByTCode is null)
        {
            throw new NotFoundAppException($"Sayfa bulunamadı. transactionCode={normalizedTCode}");
        }

        return pageByTCode;
    }
}
