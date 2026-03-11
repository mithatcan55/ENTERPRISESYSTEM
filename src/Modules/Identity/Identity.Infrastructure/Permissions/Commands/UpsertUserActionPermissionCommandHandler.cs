using Application.Exceptions;
using Identity.Application.Contracts;
using Identity.Application.Permissions.Commands;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Permissions.Commands;

public sealed class UpsertUserActionPermissionCommandHandler(BusinessDbContext businessDbContext) : IUpsertUserActionPermissionCommandHandler
{
    public async Task<UserActionPermissionDto> HandleAsync(UpsertUserActionPermissionRequest request, CancellationToken cancellationToken)
    {
        // Upsert kullanmamizin sebebi ayni endpoint ile hem olusturma hem guncelleme yapabilmek.
        if (request.UserId <= 0)
        {
            throw new ValidationAppException(
                "Permission dogrulamasi basarisiz.",
                new Dictionary<string, string[]>
                {
                    ["userId"] = ["Gecerli bir userId zorunludur."]
                });
        }

        if (string.IsNullOrWhiteSpace(request.ActionCode))
        {
            throw new ValidationAppException(
                "Permission dogrulamasi basarisiz.",
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
            throw new NotFoundAppException($"Kullanici bulunamadi. userId={request.UserId}");
        }

        var page = await ResolvePageAsync(request.SubModulePageId, request.TransactionCode, cancellationToken);
        var normalizedActionCode = request.ActionCode.Trim().ToUpperInvariant();

        // Eski kayit varsa guncellenir, yoksa yeni kayit acilir.
        // Soft-delete edilmis bir kayit varsa tekrar aktif hale de getirilebilir.
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

    private async Task<SubModulePage> ResolvePageAsync(int? subModulePageId, string? transactionCode, CancellationToken cancellationToken)
    {
        // API esnekligi icin cagirani tek bir anahtara zorlamiyoruz.
        // UI ister page id ile, ister T-Code ile ayni sayfayi hedefleyebilir.
        if (subModulePageId.HasValue)
        {
            var pageById = await businessDbContext.SubModulePages
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == subModulePageId.Value && !x.IsDeleted, cancellationToken);

            if (pageById is null)
            {
                throw new NotFoundAppException($"Sayfa bulunamadi. subModulePageId={subModulePageId.Value}");
            }

            return pageById;
        }

        if (string.IsNullOrWhiteSpace(transactionCode))
        {
            throw new ValidationAppException(
                "Permission dogrulamasi basarisiz.",
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
            throw new NotFoundAppException($"Sayfa bulunamadi. transactionCode={normalizedTCode}");
        }

        return pageByTCode;
    }
}
