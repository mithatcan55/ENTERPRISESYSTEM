using Application.Security;
using Authorization.Application.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Infrastructure.Services;

/// <summary>
/// Permission kontrolünü tek bir serviste toplar.
/// Böylece MVC authorization handler ve request pipeline pre-check
/// aynı kural setini kullanır.
/// </summary>
public sealed class PermissionAuthorizationService(
    AuthorizationDbContext authorizationDbContext,
    IHttpContextAccessor httpContextAccessor) : IPermissionAuthorizationService
{
    public async Task<bool> IsAllowedAsync(string permissionCode, int userId, CancellationToken cancellationToken)
    {
        var normalizedPermission = permissionCode.Trim().ToUpperInvariant();
        var principal = httpContextAccessor.HttpContext?.User;

        // Ilk tercih claim uzerinden hizli izin cozumlemektir.
        // Token icinde permission varsa gereksiz DB sorgusu calistirmayiz.
        var hasPermissionClaim = principal?.Claims
            .Where(x => string.Equals(x.Type, SecurityClaimTypes.Permission, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Value)
            .Any(x => string.Equals(x, normalizedPermission, StringComparison.OrdinalIgnoreCase)) == true;

        if (hasPermissionClaim)
        {
            return true;
        }

        // Claim yoksa veritabani ikinci dogrulama kaynagi olur.
        // Bu yaklasim hem performans hem guncel yetki verisi arasinda dengeli bir cozum sunar.
        return await authorizationDbContext.UserPageActionPermissions
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == userId
                     && !x.IsDeleted
                     && x.IsAllowed
                     && x.ActionCode == normalizedPermission,
                cancellationToken);
    }
}
