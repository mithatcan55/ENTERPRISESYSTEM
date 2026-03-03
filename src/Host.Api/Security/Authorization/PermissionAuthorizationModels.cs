using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;

namespace Host.Api.Security.Authorization;

public sealed class PermissionRequirement(string permissionCode) : IAuthorizationRequirement
{
    public string PermissionCode { get; } = permissionCode;
}

/// <summary>
/// Dinamik Permission policy kullanımı için attribute.
/// Örn: [PermissionAuthorize("SESSION_REVOKE")]
/// </summary>
public sealed class PermissionAuthorizeAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";

    public PermissionAuthorizeAttribute([DisallowNull] string permissionCode)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
        {
            throw new ArgumentException("Permission code boş olamaz.", nameof(permissionCode));
        }

        Policy = $"{PolicyPrefix}{permissionCode.Trim().ToUpperInvariant()}";
    }
}
