using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Authorization;
using Infrastructure.Persistence.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence.Auditing;
using Infrastructure.Observability;
using Infrastructure.Persistence.Entities;
using System.Diagnostics;

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
        var logEventWriter = scope.ServiceProvider.GetRequiredService<ILogEventWriter>();
        logger.LogInformation("Core bootstrap baslatiliyor.");
        await WriteLifecycleLogAsync(
            logEventWriter,
            "Information",
            "Core bootstrap baslatiliyor.",
            "BootstrapStart",
            cancellationToken,
            contextName: null,
            durationMs: null,
            exception: null);
        var logDbContext = scope.ServiceProvider.GetRequiredService<LogDbContext>();
        var identityDbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var authorizationDbContext = scope.ServiceProvider.GetRequiredService<AuthorizationDbContext>();
        var integrationsDbContext = scope.ServiceProvider.GetRequiredService<IntegrationsDbContext>();
        var reportsDbContext = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
        var approvalsDbContext = scope.ServiceProvider.GetRequiredService<ApprovalsDbContext>();
        var documentsDbContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();

        try
        {
            await EnsureDatabaseAsync(logDbContext, logger, logEventWriter, cancellationToken);
            await using var businessDbContext = CreateLegacyBusinessDbContext();
            await EnsureDatabaseAsync(businessDbContext, logger, logEventWriter, cancellationToken);

            await EnsureDatabaseAsync(identityDbContext, logger, logEventWriter, cancellationToken);
            await EnsureDatabaseAsync(authorizationDbContext, logger, logEventWriter, cancellationToken);
            await EnsureDatabaseAsync(integrationsDbContext, logger, logEventWriter, cancellationToken);
            await EnsureDatabaseAsync(reportsDbContext, logger, logEventWriter, cancellationToken);
            await EnsureDatabaseAsync(approvalsDbContext, logger, logEventWriter, cancellationToken);
            await EnsureDatabaseAsync(documentsDbContext, logger, logEventWriter, cancellationToken);

            logger.LogInformation("Authorization seed kontrolu baslatiliyor.");
            await WriteLifecycleLogAsync(
                logEventWriter,
                "Information",
                "Authorization seed kontrolu baslatiliyor.",
                "AuthorizationSeedStart",
                cancellationToken,
                contextName: null,
                durationMs: null,
                exception: null);
            await EnsureAuthorizationSeedAsync(authorizationDbContext, cancellationToken);
            logger.LogInformation("Authorization seed kontrolu tamamlandi.");
            await WriteLifecycleLogAsync(
                logEventWriter,
                "Information",
                "Authorization seed kontrolu tamamlandi.",
                "AuthorizationSeedCompleted",
                cancellationToken,
                contextName: null,
                durationMs: null,
                exception: null);

            logger.LogInformation("Bootstrap admin kontrolu baslatiliyor.");
            await WriteLifecycleLogAsync(
                logEventWriter,
                "Information",
                "Bootstrap admin kontrolu baslatiliyor.",
                "BootstrapAdminSeedStart",
                cancellationToken,
                contextName: null,
                durationMs: null,
                exception: null);
            await EnsureAdminSeedAsync(identityDbContext, authorizationDbContext, cancellationToken);
            logger.LogInformation("Bootstrap admin kontrolu tamamlandi.");
            await WriteLifecycleLogAsync(
                logEventWriter,
                "Information",
                "Bootstrap admin kontrolu tamamlandi.",
                "BootstrapAdminSeedCompleted",
                cancellationToken,
                contextName: null,
                durationMs: null,
                exception: null);

            logger.LogInformation("Core bootstrap tamamlandi.");
            await WriteLifecycleLogAsync(
                logEventWriter,
                "Information",
                "Core bootstrap tamamlandi.",
                "BootstrapCompleted",
                cancellationToken,
                contextName: null,
                durationMs: null,
                exception: null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Core bootstrap basarisiz oldu.");
            await WriteLifecycleLogAsync(logEventWriter, "Error", "Core bootstrap basarisiz oldu.", "BootstrapFailed", cancellationToken, exception: ex);
            throw;
        }
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

    private static async Task EnsureDatabaseAsync(
        DbContext dbContext,
        ILogger logger,
        ILogEventWriter logEventWriter,
        CancellationToken cancellationToken)
    {
        var contextName = dbContext.GetType().Name;
        var stopwatch = Stopwatch.StartNew();
        logger.LogInformation("Database hazirlaniyor. Context={ContextName}", contextName);
        await WriteLifecycleLogAsync(
            logEventWriter,
            "Information",
            $"Database hazirlaniyor. Context={contextName}",
            "DatabaseBootstrapStarted",
            cancellationToken,
            contextName,
            stopwatch.ElapsedMilliseconds);

        try
        {
            var hasMigrations = dbContext.Database.GetMigrations().Any();

            if (hasMigrations)
            {
                logger.LogInformation("Migration uygulanacak. Context={ContextName}", contextName);
                await dbContext.Database.MigrateAsync(cancellationToken);
                logger.LogInformation(
                    "Migration tamamlandi. Context={ContextName}; DurationMs={DurationMs}",
                    contextName,
                    stopwatch.ElapsedMilliseconds);
                await WriteLifecycleLogAsync(
                    logEventWriter,
                    "Information",
                    $"Migration tamamlandi. Context={contextName}",
                    "DatabaseMigrationCompleted",
                    cancellationToken,
                    contextName,
                    stopwatch.ElapsedMilliseconds);
                return;
            }

            logger.LogInformation("Migration yok, EnsureCreated calisacak. Context={ContextName}", contextName);
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            logger.LogInformation(
                "EnsureCreated tamamlandi. Context={ContextName}; DurationMs={DurationMs}",
                contextName,
                stopwatch.ElapsedMilliseconds);
            await WriteLifecycleLogAsync(
                logEventWriter,
                "Information",
                $"EnsureCreated tamamlandi. Context={contextName}",
                "DatabaseEnsureCreatedCompleted",
                cancellationToken,
                contextName,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            await WriteLifecycleLogAsync(
                logEventWriter,
                "Error",
                $"Database hazirlama basarisiz oldu. Context={contextName}",
                "DatabaseBootstrapFailed",
                cancellationToken,
                contextName,
                stopwatch.ElapsedMilliseconds,
                ex);
            throw;
        }
    }

    private static Task WriteLifecycleLogAsync(
        ILogEventWriter logEventWriter,
        string level,
        string message,
        string operationName,
        CancellationToken cancellationToken,
        string? contextName = null,
        long? durationMs = null,
        Exception? exception = null)
    {
        var properties = new Dictionary<string, object?>
        {
            ["OperationName"] = operationName
        };

        if (!string.IsNullOrWhiteSpace(contextName))
        {
            properties["ContextName"] = contextName;
        }

        if (durationMs.HasValue)
        {
            properties["DurationMs"] = durationMs.Value;
        }

        var log = new SystemLog
        {
            Timestamp = DateTimeOffset.UtcNow,
            TimeZone = TimeZoneInfo.Local.Id,
            Level = level,
            Category = "StartupLifecycle",
            Source = nameof(CoreBootstrapHostedService),
            Message = message,
            MessageTemplate = message,
            Exception = exception?.Message,
            StackTrace = exception?.ToString(),
            OperationName = operationName,
            DurationMs = durationMs,
            MachineName = Environment.MachineName,
            Environment = AppContext.GetData("ENVIRONMENT")?.ToString(),
            ApplicationName = typeof(Program).Assembly.GetName().Name,
            ApplicationVersion = typeof(Program).Assembly.GetName().Version?.ToString(),
            ProcessId = Environment.ProcessId,
            ThreadId = Environment.CurrentManagedThreadId,
            Properties = System.Text.Json.JsonSerializer.Serialize(properties)
        };

        return logEventWriter.WriteSystemAsync(log, cancellationToken);
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
            new { Name = "User Report", Code = "USER_REPORT", TransactionCode = "SYS04", RouteLink = "/system/users/report" },
            new { Name = "User Roles", Code = "USER_ROLES", TransactionCode = "SYS05", RouteLink = "/system/users/roles" },
            new { Name = "User Permissions", Code = "USER_PERMISSIONS", TransactionCode = "SYS06", RouteLink = "/system/users/permissions" }
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
        else if (hostEnvironment.IsDevelopment())
        {
            // Development ortaminda bootstrap admin kullanicisinin sifresi config ile uyumlu tutulur.
            // Boylece local reset veya secret degisikliginden sonra eski hash nedeniyle login kilitlenmez.
            var passwordNeedsRefresh = !BCrypt.Net.BCrypt.Verify(adminPassword, user.PasswordHash);
            var identityNeedsRefresh =
                !string.Equals(user.Username, adminUsername, StringComparison.Ordinal) ||
                !string.Equals(user.Email, adminEmail, StringComparison.OrdinalIgnoreCase);

            if (passwordNeedsRefresh || identityNeedsRefresh)
            {
                user.Username = adminUsername;
                user.Email = adminEmail;
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
                user.IsActive = true;
                user.MustChangePassword = true;
                user.PasswordExpiresAt = DateTime.UtcNow.AddDays(90);

                await identityDbContext.SaveChangesAsync(cancellationToken);
            }
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
            .Where(x => !x.IsDeleted && (x.TransactionCode == "SYS01" || x.TransactionCode == "SYS02" || x.TransactionCode == "SYS03" || x.TransactionCode == "SYS04" || x.TransactionCode == "SYS05" || x.TransactionCode == "SYS06"))
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

        // Action permission seed: her T-Code icin admin kullanicisinin gerekli aksiyonlara sahip olmasi saglanir.
        var requiredActionsByCodes = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["SYS01"] = ["CREATE", "UPDATE", "DELETE", "DEACTIVATE", "REACTIVATE"],
            ["SYS02"] = ["UPDATE"],
            ["SYS03"] = ["READ", "VIEW"],
            ["SYS04"] = ["READ"],
            ["SYS05"] = ["MANAGE"],
            ["SYS06"] = ["MANAGE", "PERMISSIONS_READ"],
        };

        var actionPages = await authorizationDbContext.SubModulePages
            .AsNoTracking()
            .Where(x => !x.IsDeleted && new[] { "SYS01", "SYS02", "SYS03", "SYS04", "SYS05", "SYS06" }.Contains(x.TransactionCode))
            .Select(x => new { x.Id, x.TransactionCode })
            .ToListAsync(cancellationToken);

        foreach (var page in actionPages)
        {
            if (!requiredActionsByCodes.TryGetValue(page.TransactionCode, out var actions))
                continue;

            var existingActions = await authorizationDbContext.UserPageActionPermissions
                .Where(x => x.UserId == user.Id && x.SubModulePageId == page.Id && !x.IsDeleted)
                .Select(x => x.ActionCode)
                .ToListAsync(cancellationToken);

            foreach (var actionCode in actions)
            {
                if (existingActions.Any(a => string.Equals(a, actionCode, StringComparison.OrdinalIgnoreCase)))
                    continue;

                authorizationDbContext.UserPageActionPermissions.Add(new UserPageActionPermission
                {
                    UserId = user.Id,
                    SubModulePageId = page.Id,
                    ActionCode = actionCode,
                    IsAllowed = true
                });
            }
        }

        await authorizationDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Bootstrap admin dogrulandi. UserCode={UserCode}", adminUserCode);
    }
}
