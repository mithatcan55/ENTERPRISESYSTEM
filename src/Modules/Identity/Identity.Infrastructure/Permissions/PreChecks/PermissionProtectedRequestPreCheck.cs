using Application.Exceptions;
using Application.Pipeline;
using Application.Security;
using Authorization.Application.Services;

namespace Identity.Infrastructure.Permissions.PreChecks;

/// <summary>
/// Permission tabanli request'ler icin ikinci savunma hattidir.
/// Attribute bazli kontrolun yanina pipeline seviyesinde ortak bir check ekler.
/// </summary>
public sealed class PermissionProtectedRequestPreCheck<TRequest>(
    IPermissionAuthorizationService permissionAuthorizationService,
    ICurrentUserContext currentUserContext) : IRequestPreCheck<TRequest>
    where TRequest : IPermissionProtectedRequest
{
    public async Task CheckAsync(TRequest request, CancellationToken cancellationToken)
    {
        if (!currentUserContext.TryGetUserId(out var userId))
        {
            throw new ForbiddenAppException("Kimligi dogrulanmis kullanici baglami cozumlenemedi.");
        }

        var isAllowed = await permissionAuthorizationService.IsAllowedAsync(
            request.PermissionCode,
            userId,
            cancellationToken);

        if (!isAllowed)
        {
            throw new ForbiddenAppException(
                $"Permission yetki kontrolu basarisiz. permissionCode={request.PermissionCode}",
                errorCode: "permission_request_precheck_failed");
        }
    }
}
