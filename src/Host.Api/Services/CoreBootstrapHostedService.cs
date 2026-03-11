using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Authorization;
using Infrastructure.Persistence.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace Host.Api.Services;

public sealed class CoreBootstrapHostedService(
    IServiceProvider serviceProvider,
    ILogger<CoreBootstrapHostedService> logger,
    IConfiguration configuration,
    IHostEnvironment hostEnvironment) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (hostEnvironment.IsEnvironment("Testing"))
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var businessDbContext = scope.ServiceProvider.GetRequiredService<BusinessDbContext>();
        var logDbContext = scope.ServiceProvider.GetRequiredService<LogDbContext>();

        await EnsureDatabaseAsync(logDbContext, cancellationToken);
        await EnsureDatabaseAsync(businessDbContext, cancellationToken);
        await EnsureAdminSeedAsync(businessDbContext, cancellationToken);

        logger.LogInformation("Core bootstrap tamamlandi.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static async Task EnsureDatabaseAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        var hasMigrations = dbContext.Database.GetMigrations().Any();

        if (hasMigrations)
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
            return;
        }

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }

    private async Task EnsureAdminSeedAsync(BusinessDbContext businessDbContext, CancellationToken cancellationToken)
    {
        var adminUserCode = configuration["BootstrapAdmin:UserCode"] ?? "CORE_ADMIN";
        var adminUsername = configuration["BootstrapAdmin:Username"] ?? "core.admin";
        var adminEmail = configuration["BootstrapAdmin:Email"] ?? "core.admin@local";
        var adminPassword = configuration["BootstrapAdmin:Password"] ?? "CoreAdmin@12345";

        var role = await businessDbContext.Roles.FirstOrDefaultAsync(x => !x.IsDeleted && x.Code == "SYS_ADMIN", cancellationToken);
        if (role is null)
        {
            role = new Role
            {
                Code = "SYS_ADMIN",
                Name = "System Administrator",
                Description = "Bootstrap admin role",
                IsSystemRole = true
            };

            businessDbContext.Roles.Add(role);
            await businessDbContext.SaveChangesAsync(cancellationToken);
        }

        var user = await businessDbContext.Users.FirstOrDefaultAsync(x => !x.IsDeleted && x.UserCode == adminUserCode, cancellationToken);
        if (user is null)
        {
            user = new User
            {
                UserCode = adminUserCode,
                Username = adminUsername,
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                IsActive = true,
                MustChangePassword = true,
                PasswordExpiresAt = DateTime.UtcNow.AddDays(90)
            };

            businessDbContext.Users.Add(user);
            await businessDbContext.SaveChangesAsync(cancellationToken);
        }

        var hasAssignment = await businessDbContext.UserRoles.AnyAsync(
            x => !x.IsDeleted && x.UserId == user.Id && x.RoleId == role.Id,
            cancellationToken);

        if (!hasAssignment)
        {
            businessDbContext.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            });

            await businessDbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await businessDbContext.UserModulePermissions.AnyAsync(x => !x.IsDeleted && x.UserId == user.Id && x.ModuleId == 1, cancellationToken))
        {
            businessDbContext.UserModulePermissions.Add(new UserModulePermission
            {
                UserId = user.Id,
                ModuleId = 1,
                AuthorizationLevel = 1
            });
        }

        if (!await businessDbContext.UserSubModulePermissions.AnyAsync(x => !x.IsDeleted && x.UserId == user.Id && x.SubModuleId == 1, cancellationToken))
        {
            businessDbContext.UserSubModulePermissions.Add(new UserSubModulePermission
            {
                UserId = user.Id,
                SubModuleId = 1,
                AuthorizationLevel = 2
            });
        }

        foreach (var pageId in new[] { 1, 2, 3, 4 })
        {
            if (await businessDbContext.UserPagePermissions.AnyAsync(
                    x => !x.IsDeleted && x.UserId == user.Id && x.SubModulePageId == pageId,
                    cancellationToken))
            {
                continue;
            }

            businessDbContext.UserPagePermissions.Add(new UserPagePermission
            {
                UserId = user.Id,
                SubModulePageId = pageId,
                AuthorizationLevel = 3
            });
        }

        if (!await businessDbContext.UserCompanyPermissions.AnyAsync(x => !x.IsDeleted && x.UserId == user.Id && x.CompanyId == 1, cancellationToken))
        {
            businessDbContext.UserCompanyPermissions.Add(new UserCompanyPermission
            {
                UserId = user.Id,
                CompanyId = 1,
                AuthorizationLevel = 4
            });
        }

        await businessDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Bootstrap admin dogrulandi. UserCode={UserCode}", adminUserCode);
    }
}
