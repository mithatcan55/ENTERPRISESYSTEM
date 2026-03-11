namespace Authorization.Application.Services;

public interface IPermissionAuthorizationService
{
    Task<bool> IsAllowedAsync(string permissionCode, int userId, CancellationToken cancellationToken);
}
