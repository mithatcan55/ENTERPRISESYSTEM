using Host.Api.Authorization.Services;
using Host.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Host.Api.Security.Authorization;

public sealed class TCodeAuthorizationHandler(ITCodeAuthorizationService tCodeAuthorizationService)
    : AuthorizationHandler<TCodeRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TCodeRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var userIdRaw = context.User.FindFirst(SecurityClaimTypes.UserId)?.Value
                ?? context.User.FindFirst(SecurityClaimTypes.Subject)?.Value;
        var companyIdRaw = context.User.FindFirst(SecurityClaimTypes.CompanyId)?.Value;

        if (!int.TryParse(userIdRaw, out var userId) || !int.TryParse(companyIdRaw, out var companyId))
        {
            return;
        }

        var cancellationToken = (context.Resource as HttpContext)?.RequestAborted ?? CancellationToken.None;

        var result = await tCodeAuthorizationService.AuthorizeAsync(
            requirement.TransactionCode,
            userId,
            companyId,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase),
            cancellationToken);

        if (result.IsAllowed)
        {
            context.Succeed(requirement);
        }
    }
}
