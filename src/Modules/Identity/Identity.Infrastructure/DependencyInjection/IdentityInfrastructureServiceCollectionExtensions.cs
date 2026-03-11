using Identity.Application.Services;
using Identity.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure.DependencyInjection;

public static class IdentityInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructureModule(this IServiceCollection services)
    {
        services.AddScoped<IAuthLifecycleService, AuthLifecycleService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();
        services.AddScoped<IUserPermissionService, UserPermissionService>();
        services.AddScoped<IPasswordPolicyService, PasswordPolicyService>();

        return services;
    }
}
