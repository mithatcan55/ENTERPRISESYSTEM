using Application.Security;
using Authorization.Application.Security;
using Authorization.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Authorization.Infrastructure.Security;

public sealed class PermissionAuthorizationHandler(IPermissionAuthorizationService permissionAuthorizationService)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var userIdRaw = context.User.FindFirst(SecurityClaimTypes.UserId)?.Value
                        ?? context.User.FindFirst(SecurityClaimTypes.Subject)?.Value;

        if (!int.TryParse(userIdRaw, out var userId))
        {
            return;
        }

        var cancellationToken = (context.Resource as HttpContext)?.RequestAborted ?? CancellationToken.None;
        var isAllowed = await permissionAuthorizationService.IsAllowedAsync(
            requirement.PermissionCode,
            userId,
            cancellationToken);

        if (isAllowed)
        {
            context.Succeed(requirement);
        }
    }
}
