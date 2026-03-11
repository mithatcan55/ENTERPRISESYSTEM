using System.Reflection;
using Authorization.Application.Security;
using Authorization.Presentation.Controllers;
using Identity.Presentation.Controllers;
using Integrations.Presentation.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace UnitTests;

public sealed class AuthorizationGuardTests
{
    private static readonly HashSet<string> AnonymousAllowList = new(StringComparer.OrdinalIgnoreCase)
    {
        "POST /api/auth/login",
        "POST /api/auth/refresh"
    };

    private static readonly HashSet<string> MutatingSelfServiceAllowList = new(StringComparer.OrdinalIgnoreCase)
    {
        "POST /api/auth/change-password",
        "POST /api/sessions/{sessionId:int}/revoke"
    };

    [Fact]
    public void All_Controller_Endpoints_Must_Follow_Security_Guard_Rules()
    {
        var controllerAssemblies = new[]
        {
            typeof(AuthController).Assembly,
            typeof(TCodeController).Assembly,
            typeof(OutboxController).Assembly
        };

        var controllerTypes = controllerAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false }
                           && typeof(ControllerBase).IsAssignableFrom(type)
                           && type.Namespace is not null
                           && type.Namespace.EndsWith(".Controllers", StringComparison.Ordinal))
            .ToArray();

        var violations = new List<string>();

        foreach (var controllerType in controllerTypes)
        {
            var classAuthorize = controllerType.GetCustomAttributes<AuthorizeAttribute>(true).ToArray();
            var classAllowAnonymous = controllerType.GetCustomAttributes<AllowAnonymousAttribute>(true).Any();
            var classRoute = controllerType.GetCustomAttribute<RouteAttribute>(true)?.Template;

            var methods = controllerType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(method => method.GetCustomAttributes<HttpMethodAttribute>(true).Any())
                .ToArray();

            foreach (var method in methods)
            {
                var httpAttributes = method.GetCustomAttributes<HttpMethodAttribute>(true).ToArray();
                var methodAuthorize = method.GetCustomAttributes<AuthorizeAttribute>(true).ToArray();
                var methodAllowAnonymous = method.GetCustomAttributes<AllowAnonymousAttribute>(true).Any();
                var methodTCode = method.GetCustomAttributes<TCodeAuthorizeAttribute>(true).ToArray();
                var methodPermission = method.GetCustomAttributes<PermissionAuthorizeAttribute>(true).ToArray();

                var hasAllowAnonymous = methodAllowAnonymous || classAllowAnonymous;
                var effectiveAuthorize = methodAuthorize.Concat(classAuthorize).ToArray();
                var hasAuthorize = effectiveAuthorize.Any();
                var hasRolePolicy = effectiveAuthorize.Any(x => !string.IsNullOrWhiteSpace(x.Roles));
                var hasTCode = methodTCode.Any() || controllerType.GetCustomAttributes<TCodeAuthorizeAttribute>(true).Any();
                var hasPermission = methodPermission.Any() || controllerType.GetCustomAttributes<PermissionAuthorizeAttribute>(true).Any();

                foreach (var httpAttribute in httpAttributes)
                {
                    var verbs = httpAttribute.HttpMethods is not null && httpAttribute.HttpMethods.Any()
                        ? httpAttribute.HttpMethods
                        : new[] { "GET" };

                    var route = BuildRoute(classRoute, httpAttribute.Template);

                    foreach (var verb in verbs)
                    {
                        var endpointKey = $"{verb.ToUpperInvariant()} {route}";

                        if (hasAllowAnonymous)
                        {
                            if (!AnonymousAllowList.Contains(endpointKey))
                            {
                                violations.Add(LogViolation("ANON_ENDPOINT_NOT_ALLOWLISTED", endpointKey,
                                    "AllowAnonymous yalnızca tanımlı endpoint listesinde olabilir."));
                            }

                            continue;
                        }

                        if (!hasAuthorize)
                        {
                            violations.Add(LogViolation("MISSING_AUTHORIZE", endpointKey,
                                "Endpoint üzerinde [Authorize] yok."));
                        }

                        var isMutating = verb.Equals("POST", StringComparison.OrdinalIgnoreCase)
                                         || verb.Equals("PUT", StringComparison.OrdinalIgnoreCase)
                                         || verb.Equals("PATCH", StringComparison.OrdinalIgnoreCase)
                                         || verb.Equals("DELETE", StringComparison.OrdinalIgnoreCase);

                        if (isMutating && !hasTCode && !hasRolePolicy && !hasPermission && !MutatingSelfServiceAllowList.Contains(endpointKey))
                        {
                            violations.Add(LogViolation("MISSING_ENFORCEMENT", endpointKey,
                            "Yazma işlemlerinde en az bir enforcement gerekir: role veya T-Code veya Permission."));
                        }
                    }
                }
            }
        }

        if (violations.Count > 0)
        {
            var header = "SECURITY_GUARD_FAIL: Authorization/TCode kuralları ihlal edildi.";
            var body = string.Join(Environment.NewLine, violations);
            Assert.Fail($"{header}{Environment.NewLine}{body}");
        }
    }

    private static string BuildRoute(string? classRoute, string? methodRoute)
    {
        var left = NormalizeRoutePart(classRoute);
        var right = NormalizeRoutePart(methodRoute);

        if (string.IsNullOrWhiteSpace(left) && string.IsNullOrWhiteSpace(right))
        {
            return "/";
        }

        if (string.IsNullOrWhiteSpace(left))
        {
            return right;
        }

        if (string.IsNullOrWhiteSpace(right))
        {
            return left;
        }

        return $"{left}/{right.TrimStart('/')}";
    }

    private static string NormalizeRoutePart(string? route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            return string.Empty;
        }

        var normalized = route.Trim();
        normalized = normalized.Trim('/');

        return string.IsNullOrWhiteSpace(normalized)
            ? string.Empty
            : $"/{normalized}";
    }

    private static string LogViolation(string code, string endpoint, string reason)
    {
        return $"[SECURITY_GUARD] code={code}; endpoint={endpoint}; reason={reason}";
    }
}
