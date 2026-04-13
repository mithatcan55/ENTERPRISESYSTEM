using Authorization.Application.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Authorization.Infrastructure.Services;

public sealed class EffectivePermissionService(
    IdentityDbContext identityDbContext,
    AuthorizationDbContext authorizationDbContext) : IEffectivePermissionService
{
    private static readonly Dictionary<string, IReadOnlyList<string>> RolePermissionMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["SYS_ADMIN"] =
            [
                "Users.View", "Users.Create", "Users.Update", "Users.Delete",
                "Roles.View", "Roles.Create", "Roles.Update", "Roles.Delete",
                "Permissions.View", "Permissions.Create", "Permissions.Update", "Permissions.Delete",
            ]
        };

    public async Task<IReadOnlyList<string>> ResolveEffectivePermissionsAsync(int userId, CancellationToken cancellationToken)
    {
        if (userId <= 0)
        {
            return [];
        }

        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var roleCodes = await (
                from userRole in identityDbContext.UserRoles.AsNoTracking()
                join role in identityDbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                where userRole.UserId == userId && !userRole.IsDeleted && !role.IsDeleted
                select role.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var roleCode in roleCodes)
        {
            if (!RolePermissionMap.TryGetValue(roleCode, out var mappedPermissions))
            {
                continue;
            }

            foreach (var mapped in mappedPermissions)
            {
                permissions.Add(mapped);
            }
        }

        var directPermissions = await (
                from userAction in authorizationDbContext.UserPageActionPermissions.AsNoTracking()
                join page in authorizationDbContext.SubModulePages.AsNoTracking() on userAction.SubModulePageId equals page.Id
                where userAction.UserId == userId
                      && !userAction.IsDeleted
                      && userAction.IsAllowed
                      && !page.IsDeleted
                select new
                {
                    page.TransactionCode,
                    userAction.ActionCode
                })
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var direct in directPermissions)
        {
            var permissionNames = MapPermissionNames(direct.TransactionCode, direct.ActionCode);
            foreach (var permissionName in permissionNames)
            {
                permissions.Add(permissionName);
            }
        }

        return permissions
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionCode, CancellationToken cancellationToken)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(permissionCode))
        {
            return false;
        }

        var normalizedTarget = NormalizePermission(permissionCode);
        var permissions = await ResolveEffectivePermissionsAsync(userId, cancellationToken);
        return permissions.Any(x => string.Equals(x, normalizedTarget, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<string> MapPermissionNames(string? transactionCode, string? actionCode)
    {
        var resource = ResolveResource(transactionCode);
        var normalizedAction = (actionCode ?? string.Empty).Trim().ToUpperInvariant();

        return normalizedAction switch
        {
            "READ" or "VIEW" => [$"{resource}.View"],
            "CREATE" => [$"{resource}.Create"],
            "UPDATE" or "ACTIVATE" or "DEACTIVATE" or "REACTIVATE" => [$"{resource}.Update"],
            "DELETE" => [$"{resource}.Delete"],
            "MANAGE" or "ALL" => [$"{resource}.View", $"{resource}.Create", $"{resource}.Update", $"{resource}.Delete"],
            _ => [$"{resource}.{ToPascal(actionCode)}"]
        };
    }

    private static string ResolveResource(string? transactionCode)
    {
        return (transactionCode ?? string.Empty).Trim().ToUpperInvariant() switch
        {
            "SYS01" or "SYS02" or "SYS03" or "SYS04" => "Users",
            "SYS05" => "Roles",
            "SYS06" => "Permissions",
            "SYS07" => "Localization",
            _ => "General"
        };
    }

    private static string NormalizePermission(string permissionCode)
    {
        var trimmed = permissionCode.Trim();
        if (trimmed.Contains(':', StringComparison.Ordinal))
        {
            var parts = trimmed.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                var mapped = MapPermissionNames(parts[0], parts[1]).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(mapped))
                {
                    return mapped;
                }
            }
        }

        if (trimmed.Contains('_', StringComparison.Ordinal) && !trimmed.Contains('.', StringComparison.Ordinal))
        {
            // Legacy UPPER_SNAKE formati (örn: PERMISSIONS_READ)
            var parts = trimmed.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length >= 2)
            {
                var resource = ToPascal(string.Join("_", parts.Take(parts.Length - 1)));
                var action = ToPascal(parts[^1]);
                return $"{resource}.{action}";
            }
        }

        return ToPascalDot(trimmed);
    }

    private static string ToPascalDot(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if (!value.Contains('.', StringComparison.Ordinal))
        {
            return ToPascal(value);
        }

        var parts = value.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ToPascal)
            .ToList();

        return string.Join('.', parts);
    }

    private static string ToPascal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var tokens = value
            .Split(['_', '-', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        if (tokens.Count == 0)
        {
            return string.Empty;
        }

        return string.Concat(tokens.Select(token =>
        {
            if (token.Length == 1)
            {
                return token.ToUpperInvariant();
            }

            return char.ToUpperInvariant(token[0]) + token[1..].ToLowerInvariant();
        }));
    }
}
