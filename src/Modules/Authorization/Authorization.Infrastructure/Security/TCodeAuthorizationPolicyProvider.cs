using Authorization.Application.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Authorization.Infrastructure.Security;

public sealed class TCodeAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider = new(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(TCodeAuthorizeAttribute.PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var transactionCode = policyName[TCodeAuthorizeAttribute.PolicyPrefix.Length..].Trim();
            if (string.IsNullOrWhiteSpace(transactionCode))
            {
                return Task.FromResult<AuthorizationPolicy?>(null);
            }

            var policy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .AddRequirements(new TCodeRequirement(transactionCode))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        if (policyName.StartsWith(PermissionAuthorizeAttribute.PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissionCode = policyName[PermissionAuthorizeAttribute.PolicyPrefix.Length..].Trim();
            if (string.IsNullOrWhiteSpace(permissionCode))
            {
                return Task.FromResult<AuthorizationPolicy?>(null);
            }

            var policy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permissionCode))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
