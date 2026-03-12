using Application.Exceptions;
using Identity.Application.Contracts;
using Identity.Application.Permissions.Queries;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Permissions.Queries;

public sealed class ListUserActionPermissionsQueryHandler(BusinessDbContext businessDbContext) : IListUserActionPermissionsQueryHandler
{
    public async Task<IReadOnlyList<UserActionPermissionDto>> HandleAsync(UserActionPermissionQueryRequest request, CancellationToken cancellationToken)
    {
        // Query handler yazma mantigina degil, veriyi en uygun sekilde okuma ve sekillendirme mantigina odaklanir.
        if (request.UserId <= 0)
        {
            throw new ValidationAppException(
                "Permission sorgu dogrulamasi basarisiz.",
                new Dictionary<string, string[]>
                {
                    ["userId"] = ["Gecerli bir userId zorunludur."]
                });
        }

        var query =
            from permission in businessDbContext.UserPageActionPermissions.AsNoTracking()
            join page in businessDbContext.SubModulePages.AsNoTracking() on permission.SubModulePageId equals page.Id
            where permission.UserId == request.UserId
                  && !permission.IsDeleted
                  && !page.IsDeleted
            select new { permission, page };

        // Bu sorgu hem page id hem transaction code ile daraltma destekler.
        // Bu esneklik operasyon ekranlari ve admin UI icin faydalidir.
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
            // Query tarafinda projection'i en sonda yaparak hem SQL tarafini temiz tutuyor
            // hem de API'nin ihtiyac duydugu DTO'ya dogrudan cikiyoruz.
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
}
