using Identity.Application.Services;
using Identity.Application.Users.Commands;
using Identity.Application.Users.Queries;
using Identity.Infrastructure.Services;
using Identity.Infrastructure.Users.Commands;
using Identity.Infrastructure.Users.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure.DependencyInjection;

public static class IdentityInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructureModule(this IServiceCollection services)
    {
        services.AddScoped<IAuthLifecycleService, AuthLifecycleService>();
        services.AddScoped<IListUsersQueryHandler, ListUsersQueryHandler>();
        services.AddScoped<ICreateUserCommandHandler, CreateUserCommandHandler>();
        services.AddScoped<IUpdateUserCommandHandler, UpdateUserCommandHandler>();
        services.AddScoped<IDeactivateUserCommandHandler, DeactivateUserCommandHandler>();
        services.AddScoped<IReactivateUserCommandHandler, ReactivateUserCommandHandler>();
        services.AddScoped<IDeleteUserCommandHandler, DeleteUserCommandHandler>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();
        services.AddScoped<IUserPermissionService, UserPermissionService>();
        services.AddScoped<IPasswordPolicyService, PasswordPolicyService>();

        return services;
    }
}
