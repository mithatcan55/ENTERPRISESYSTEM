using Application.Security;
using Authorization.Application.Security;
using Authorization.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Authorization.Infrastructure.Security;

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

        var httpContext = context.Resource as HttpContext;
        var endpointAttribute = httpContext?.GetEndpoint()?.Metadata.GetMetadata<TCodeAuthorizeAttribute>();
        var cancellationToken = httpContext?.RequestAborted ?? CancellationToken.None;
        var contextValues = ResolveContextValues(httpContext);
        var requiredActionCode = requirement.RequiredActionCode
            ?? endpointAttribute?.ActionCode
            ?? InferActionCode(httpContext);
        var denyOnUnsatisfiedConditions = endpointAttribute?.DenyOnUnsatisfiedConditions
            ?? requirement.DenyOnUnsatisfiedConditions;

        // Handler burada sadece HTTP tarafindan gelen veriyi toplar.
        // Gercek yetki karari merkezi servis olan TCodeAuthorizationService'te verilir.
        var result = await tCodeAuthorizationService.AuthorizeAsync(
            requirement.TransactionCode,
            userId,
            companyId,
            contextValues,
            requiredActionCode,
            denyOnUnsatisfiedConditions,
            cancellationToken);

        if (result.IsAllowed)
        {
            context.Succeed(requirement);
        }
    }

    private static IReadOnlyDictionary<string, string?> ResolveContextValues(HttpContext? httpContext)
    {
        if (httpContext is null)
        {
            return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }

        // Condition bazli yetki kontrolleri icin hem query hem route degerleri birlikte okunur.
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var queryValue in httpContext.Request.Query)
        {
            values[queryValue.Key] = queryValue.Value.ToString();
        }

        foreach (var routeValue in httpContext.Request.RouteValues)
        {
            if (routeValue.Value is not null)
            {
                values[routeValue.Key] = routeValue.Value.ToString();
            }
        }

        return values;
    }

    private static string? InferActionCode(HttpContext? httpContext)
    {
        if (httpContext is null)
        {
            return null;
        }

        // Attribute action code vermediyse HTTP semantiginden makul bir action uretmeye calisiyoruz.
        var method = httpContext.Request.Method.ToUpperInvariant();
        return method switch
        {
            "GET" => "READ",
            "POST" => InferPostActionFromRoute(httpContext),
            "PUT" or "PATCH" => "UPDATE",
            "DELETE" => "DELETE",
            _ => null
        };
    }

    private static string InferPostActionFromRoute(HttpContext httpContext)
    {
        var segments = httpContext.Request.Path.Value?
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? [];

        var lastSegment = segments.LastOrDefault();
        if (string.IsNullOrWhiteSpace(lastSegment))
        {
            return "CREATE";
        }

        return lastSegment.ToUpperInvariant() switch
        {
            "DEACTIVATE" => "DEACTIVATE",
            "REACTIVATE" => "REACTIVATE",
            "REVOKE" => "REVOKE",
            "ASSIGN" => "ASSIGN",
            _ => "CREATE"
        };
    }
}
