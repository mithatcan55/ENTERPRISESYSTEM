using Authorization.Application.Services;
using Authorization.Application.Security;
using Authorization.Infrastructure.Security;
using Authorization.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Infrastructure.DependencyInjection;

public static class AuthorizationInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorizationInfrastructureModule(this IServiceCollection services)
    {
        services.AddScoped<ITCodeAuthorizationService, TCodeAuthorizationService>();
        services.AddScoped<IEffectivePermissionService, EffectivePermissionService>();
        services.AddScoped<IPermissionAuthorizationService, PermissionAuthorizationService>();
        services.AddScoped<IAuthorizationFieldPolicyAdminService, AuthorizationFieldPolicyAdminService>();
        services.AddScoped<IAuthorizationFieldPolicyEvaluator, AuthorizationFieldPolicyEvaluator>();
        services.AddSingleton<IAuthorizationPolicyProvider, TCodeAuthorizationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, TCodeAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }
}
