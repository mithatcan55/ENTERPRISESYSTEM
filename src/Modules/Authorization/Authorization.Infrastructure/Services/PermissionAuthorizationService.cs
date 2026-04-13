using Authorization.Application.Services;

namespace Authorization.Infrastructure.Services;

/// <summary>
/// Permission kontrolünü tek bir serviste toplar.
/// Böylece MVC authorization handler ve request pipeline pre-check
/// aynı kural setini kullanır.
/// </summary>
public sealed class PermissionAuthorizationService(
    IEffectivePermissionService effectivePermissionService) : IPermissionAuthorizationService
{
    public async Task<bool> IsAllowedAsync(string permissionCode, int userId, CancellationToken cancellationToken)
    {
        // Permission tarafinin source of truth'u claim degil veritabanidir.
        return await effectivePermissionService.HasPermissionAsync(userId, permissionCode, cancellationToken);
    }
}
