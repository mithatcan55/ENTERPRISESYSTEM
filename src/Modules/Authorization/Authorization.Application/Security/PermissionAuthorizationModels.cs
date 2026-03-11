using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;

namespace Authorization.Application.Security;

public sealed class PermissionRequirement(string permissionCode) : IAuthorizationRequirement
{
    public string PermissionCode { get; } = permissionCode;
}

public sealed class PermissionAuthorizeAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";

    public PermissionAuthorizeAttribute([DisallowNull] string permissionCode)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
        {
            throw new ArgumentException("Permission code bos olamaz.", nameof(permissionCode));
        }

        Policy = $"{PolicyPrefix}{permissionCode.Trim().ToUpperInvariant()}";
    }
}
