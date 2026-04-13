namespace Authorization.Application.Services;

public interface IEffectivePermissionService
{
    Task<IReadOnlyList<string>> ResolveEffectivePermissionsAsync(int userId, CancellationToken cancellationToken);
    Task<bool> HasPermissionAsync(int userId, string permissionCode, CancellationToken cancellationToken);
}
