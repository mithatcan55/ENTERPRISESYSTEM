using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Authorization;
using Infrastructure.Persistence.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence.Auditing;

namespace Host.Api.Services;

public sealed class CoreBootstrapHostedService(
    IServiceProvider serviceProvider,
    ILogger<CoreBootstrapHostedService> logger,
    IConfiguration configuration,
    IHostEnvironment hostEnvironment) : IHostedService
{
    private sealed class SystemAuditActorAccessor : IAuditActorAccessor
    {
        public string GetActorId() => "system";
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (hostEnvironment.IsEnvironment("Testing"))
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var logDbContext = scope.ServiceProvider.GetRequiredService<LogDbContext>();
        var identityDbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var authorizationDbContext = scope.ServiceProvider.GetRequiredService<AuthorizationDbContext>();
        var integrationsDbContext = scope.ServiceProvider.GetRequiredService<IntegrationsDbContext>();
        var reportsDbContext = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();

        await EnsureDatabaseAsync(logDbContext, cancellationToken);
        await using var businessDbContext = CreateLegacyBusinessDbContext();
        await EnsureDatabaseAsync(businessDbContext, cancellationToken);

        await EnsureDatabaseAsync(identityDbContext, cancellationToken);
        await EnsureDatabaseAsync(authorizationDbContext, cancellationToken);
        await EnsureDatabaseAsync(integrationsDbContext, cancellationToken);
        await EnsureDatabaseAsync(reportsDbContext, cancellationToken);
        await EnsureAuthorizationSeedAsync(authorizationDbContext, cancellationToken);
        await EnsureAdminSeedAsync(identityDbContext, authorizationDbContext, cancellationToken);

        logger.LogInformation("Core bootstrap tamamlandi.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private BusinessDbContext CreateLegacyBusinessDbContext()
    {
        var connectionString = configuration.GetConnectionString("BusinessDb")
                               ?? throw new InvalidOperationException("BusinessDb connection string tanimli degil.");

        var options = new DbContextOptionsBuilder<BusinessDbContext>()
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", PersistenceSchemaNames.Business);
            })
            .Options;

        return new BusinessDbContext(options, new SystemAuditActorAccessor());
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

    private static async Task EnsureAuthorizationSeedAsync(AuthorizationDbContext authorizationDbContext, CancellationToken cancellationToken)
    {
        var module = await authorizationDbContext.Modules.FirstOrDefaultAsync(x => !x.IsDeleted && x.Code == "SYS", cancellationToken);
        if (module is null)
        {
            module = new Module
            {
                Name = "System",
                Code = "SYS",
                Description = "Sistem yonetimi ana modulu",
                CompanyId = 1,
                RouteLink = "/system"
            };

            authorizationDbContext.Modules.Add(module);
            await authorizationDbContext.SaveChangesAsync(cancellationToken);
        }

        var subModule = await authorizationDbContext.SubModules.FirstOrDefaultAsync(x => !x.IsDeleted && x.Code == "SYS_USER", cancellationToken);
        if (subModule is null)
        {
            subModule = new SubModule
            {
                ModuleId = module.Id,
                Name = "UserManagement",
                Code = "SYS_USER",
                Description = "Kullanici islemleri",
                RouteLink = "/system/users"
            };

            authorizationDbContext.SubModules.Add(subModule);
            await authorizationDbContext.SaveChangesAsync(cancellationToken);
        }

        var requiredPages = new[]
        {
            new { Name = "Create User", Code = "USER_CREATE", TransactionCode = "SYS01", RouteLink = "/system/users/create" },
            new { Name = "Update User", Code = "USER_UPDATE", TransactionCode = "SYS02", RouteLink = "/system/users/update" },
            new { Name = "View User", Code = "USER_VIEW", TransactionCode = "SYS03", RouteLink = "/system/users/view" },
            new { Name = "User Report", Code = "USER_REPORT", TransactionCode = "SYS04", RouteLink = "/system/users/report" }
        };

        foreach (var pageSeed in requiredPages)
        {
            var page = await authorizationDbContext.SubModulePages
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.TransactionCode == pageSeed.TransactionCode, cancellationToken);

            if (page is not null)
            {
                continue;
            }

            authorizationDbContext.SubModulePages.Add(new SubModulePage
            {
                SubModuleId = subModule.Id,
                Name = pageSeed.Name,
                Code = pageSeed.Code,
                TransactionCode = pageSeed.TransactionCode,
                RouteLink = pageSeed.RouteLink
            });
        }

        await authorizationDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureAdminSeedAsync(IdentityDbContext identityDbContext, AuthorizationDbContext authorizationDbContext, CancellationToken cancellationToken)
    {
        var adminUserCode = configuration["BootstrapAdmin:UserCode"] ?? "CORE_ADMIN";
        var adminUsername = configuration["BootstrapAdmin:Username"] ?? "core.admin";
        var adminEmail = configuration["BootstrapAdmin:Email"] ?? "core.admin@local";
        var adminPassword = configuration["BootstrapAdmin:Password"] ?? "CoreAdmin@12345";

        var role = await identityDbContext.Roles.FirstOrDefaultAsync(x => !x.IsDeleted && x.Code == "SYS_ADMIN", cancellationToken);
        if (role is null)
        {
            role = new Role
            {
                Code = "SYS_ADMIN",
                Name = "System Administrator",
                Description = "Bootstrap admin role",
                IsSystemRole = true
            };

            identityDbContext.Roles.Add(role);
            await identityDbContext.SaveChangesAsync(cancellationToken);
        }

        var user = await identityDbContext.Users.FirstOrDefaultAsync(x => !x.IsDeleted && x.UserCode == adminUserCode, cancellationToken);
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

            identityDbContext.Users.Add(user);
            await identityDbContext.SaveChangesAsync(cancellationToken);
        }

        var hasAssignment = await identityDbContext.UserRoles.AnyAsync(
            x => !x.IsDeleted && x.UserId == user.Id && x.RoleId == role.Id,
            cancellationToken);

        if (!hasAssignment)
        {
            identityDbContext.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            });

            await identityDbContext.SaveChangesAsync(cancellationToken);
        }

        var systemModule = await authorizationDbContext.Modules
            .AsNoTracking()
            .FirstAsync(x => !x.IsDeleted && x.Code == "SYS", cancellationToken);

        var systemSubModule = await authorizationDbContext.SubModules
            .AsNoTracking()
            .FirstAsync(x => !x.IsDeleted && x.Code == "SYS_USER", cancellationToken);

        if (!await authorizationDbContext.UserModulePermissions.AnyAsync(x => !x.IsDeleted && x.UserId == user.Id && x.ModuleId == systemModule.Id, cancellationToken))
        {
            authorizationDbContext.UserModulePermissions.Add(new UserModulePermission
            {
                UserId = user.Id,
                ModuleId = systemModule.Id,
                AuthorizationLevel = 1
            });
        }

        if (!await authorizationDbContext.UserSubModulePermissions.AnyAsync(x => !x.IsDeleted && x.UserId == user.Id && x.SubModuleId == systemSubModule.Id, cancellationToken))
        {
            authorizationDbContext.UserSubModulePermissions.Add(new UserSubModulePermission
            {
                UserId = user.Id,
                SubModuleId = systemSubModule.Id,
                AuthorizationLevel = 2
            });
        }

        var pageIds = await authorizationDbContext.SubModulePages
            .AsNoTracking()
            .Where(x => !x.IsDeleted && (x.TransactionCode == "SYS01" || x.TransactionCode == "SYS02" || x.TransactionCode == "SYS03" || x.TransactionCode == "SYS04"))
            .OrderBy(x => x.TransactionCode)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var pageId in pageIds)
        {
            if (await authorizationDbContext.UserPagePermissions.AnyAsync(
                    x => !x.IsDeleted && x.UserId == user.Id && x.SubModulePageId == pageId,
                    cancellationToken))
            {
                continue;
            }

            authorizationDbContext.UserPagePermissions.Add(new UserPagePermission
            {
                UserId = user.Id,
                SubModulePageId = pageId,
                AuthorizationLevel = 3
            });
        }

        if (!await authorizationDbContext.UserCompanyPermissions.AnyAsync(x => !x.IsDeleted && x.UserId == user.Id && x.CompanyId == 1, cancellationToken))
        {
            authorizationDbContext.UserCompanyPermissions.Add(new UserCompanyPermission
            {
                UserId = user.Id,
                CompanyId = 1,
                AuthorizationLevel = 4
            });
        }

        await authorizationDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Bootstrap admin dogrulandi. UserCode={UserCode}", adminUserCode);
    }
}
