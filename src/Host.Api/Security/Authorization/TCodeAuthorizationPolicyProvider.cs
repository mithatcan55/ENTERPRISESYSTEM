using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Host.Api.Security.Authorization;

public sealed class TCodeAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider = new(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(TCodeAuthorizeAttribute.PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var transactionCode = policyName[TCodeAuthorizeAttribute.PolicyPrefix.Length..].Trim();
            if (string.IsNullOrWhiteSpace(transactionCode))
            {
                return Task.FromResult<AuthorizationPolicy?>(null);
            }

            var policy = new AuthorizationPolicyBuilder(SessionAuthenticationHandler.SchemeName)
                .RequireAuthenticatedUser()
                .AddRequirements(new TCodeRequirement(transactionCode))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
