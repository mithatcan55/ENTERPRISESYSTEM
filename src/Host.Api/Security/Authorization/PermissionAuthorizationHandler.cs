using Host.Api.Security;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Host.Api.Security.Authorization;

public sealed class PermissionAuthorizationHandler(BusinessDbContext businessDbContext)
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
        var normalizedPermission = requirement.PermissionCode.Trim().ToUpperInvariant();

        var isAllowed = await businessDbContext.UserPageActionPermissions
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == userId
                     && !x.IsDeleted
                     && x.IsAllowed
                     && x.ActionCode == normalizedPermission,
                cancellationToken);

        if (isAllowed)
        {
            context.Succeed(requirement);
        }
    }
}
