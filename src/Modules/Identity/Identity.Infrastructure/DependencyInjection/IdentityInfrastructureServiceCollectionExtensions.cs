using Identity.Application.Services;
using Identity.Application.Permissions.Commands;
using Identity.Application.Permissions.Queries;
using Identity.Application.Roles.Commands;
using Identity.Application.Roles.Queries;
using Identity.Application.Users.Commands;
using Identity.Application.Users.Queries;
using Identity.Infrastructure.Permissions.Commands;
using Identity.Infrastructure.Permissions.PreChecks;
using Identity.Infrastructure.Permissions.Queries;
using Identity.Infrastructure.Roles.Commands;
using Identity.Infrastructure.Roles.Queries;
using Identity.Infrastructure.Services;
using Identity.Infrastructure.Users.Commands;
using Identity.Infrastructure.Users.PreChecks;
using Identity.Infrastructure.Users.Queries;
using Application.Pipeline;
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
        services.AddScoped<IListRolesQueryHandler, ListRolesQueryHandler>();
        services.AddScoped<IListUserRolesQueryHandler, ListUserRolesQueryHandler>();
        services.AddScoped<ICreateRoleCommandHandler, CreateRoleCommandHandler>();
        services.AddScoped<IAssignRoleCommandHandler, AssignRoleCommandHandler>();
        services.AddScoped<IUnassignRoleCommandHandler, UnassignRoleCommandHandler>();
        services.AddScoped<IDeleteRoleCommandHandler, DeleteRoleCommandHandler>();
        services.AddScoped<IListUserActionPermissionsQueryHandler, ListUserActionPermissionsQueryHandler>();
        services.AddScoped<IUpsertUserActionPermissionCommandHandler, UpsertUserActionPermissionCommandHandler>();
        services.AddScoped<IDeleteUserActionPermissionCommandHandler, DeleteUserActionPermissionCommandHandler>();
        services.AddScoped<IRequestValidator<CreateUserCommand>, CreateUserCommandValidator>();
        services.AddScoped<IRequestValidator<UpdateUserCommand>, UpdateUserCommandValidator>();
        services.AddScoped<IRequestValidator<CreateRoleCommand>, CreateRoleCommandValidator>();
        services.AddScoped<IRequestValidator<UpsertUserActionPermissionCommand>, UpsertUserActionPermissionCommandValidator>();
        services.AddScoped<IRequestPreCheck<ListUsersQuery>, TCodeProtectedRequestPreCheck<ListUsersQuery>>();
        services.AddScoped<IRequestPreCheck<CreateUserCommand>, TCodeProtectedRequestPreCheck<CreateUserCommand>>();
        services.AddScoped<IRequestPreCheck<UpdateUserCommand>, TCodeProtectedRequestPreCheck<UpdateUserCommand>>();
        services.AddScoped<IRequestPreCheck<DeactivateUserCommand>, TCodeProtectedRequestPreCheck<DeactivateUserCommand>>();
        services.AddScoped<IRequestPreCheck<ReactivateUserCommand>, TCodeProtectedRequestPreCheck<ReactivateUserCommand>>();
        services.AddScoped<IRequestPreCheck<DeleteUserCommand>, TCodeProtectedRequestPreCheck<DeleteUserCommand>>();
        services.AddScoped<IRequestPreCheck<ListUserActionPermissionsQuery>, PermissionProtectedRequestPreCheck<ListUserActionPermissionsQuery>>();
        services.AddScoped<IRequestPreCheck<UpsertUserActionPermissionCommand>, PermissionProtectedRequestPreCheck<UpsertUserActionPermissionCommand>>();
        services.AddScoped<IRequestPreCheck<DeleteUserActionPermissionCommand>, PermissionProtectedRequestPreCheck<DeleteUserActionPermissionCommand>>();
        services.AddScoped<IPasswordPolicyService, PasswordPolicyService>();

        return services;
    }
}
