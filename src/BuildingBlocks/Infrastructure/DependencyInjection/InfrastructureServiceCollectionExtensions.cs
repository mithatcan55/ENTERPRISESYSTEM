using Infrastructure.Logging;
using Infrastructure.Observability;
using Infrastructure.Pipeline;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

/// <summary>
/// Infrastructure katmanına ait servis kayıtlarını tek noktada toplar.
/// Bu sayede Host tarafında bağlantı ve DbContext detayları dağılmaz.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// BusinessDb ve LogDb context kayıtlarını yapar.
    /// </summary>
    public static IServiceCollection AddInfrastructurePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ILogEventWriter, LogEventWriter>();
        services.AddScoped<Application.Observability.IOperationalEventPublisher, OperationalEventPublisher>();
        services.AddScoped<Application.Pipeline.IRequestExecutionPipeline, RequestExecutionPipeline>();
        services.AddScoped<Application.Observability.INotificationChannel, WebhookNotificationChannel>();
        services.AddScoped<DatabaseCommandLoggingInterceptor>();
        services.AddScoped<EntityChangeLoggingInterceptor>();

        services.AddOptions<ObservabilityRoutingOptions>()
            .BindConfiguration(ObservabilityRoutingOptions.SectionName);

        services.AddOptions<WebhookNotificationOptions>()
            .BindConfiguration(WebhookNotificationOptions.SectionName);

        services.AddOptions<EmailNotificationOptions>()
            .BindConfiguration(EmailNotificationOptions.SectionName)
            .ValidateDataAnnotations();

        services.AddHttpClient("observability-webhook", (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<WebhookNotificationOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.Url))
            {
                client.BaseAddress = new Uri(options.Url);
            }
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddDbContext<LogDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString("LogDb");
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", PersistenceSchemaNames.Logs);
            });
        });

        services.AddDbContext<AuthorizationDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("BusinessDb");
            options.UseNpgsql(connectionString);
            options.AddInterceptors(
                sp.GetRequiredService<DatabaseCommandLoggingInterceptor>(),
                sp.GetRequiredService<EntityChangeLoggingInterceptor>());
        });

        services.AddDbContext<IdentityDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("BusinessDb");
            options.UseNpgsql(connectionString);
            options.AddInterceptors(
                sp.GetRequiredService<DatabaseCommandLoggingInterceptor>(),
                sp.GetRequiredService<EntityChangeLoggingInterceptor>());
        });

        services.AddDbContext<IntegrationsDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("BusinessDb");
            options.UseNpgsql(connectionString);
            options.AddInterceptors(
                sp.GetRequiredService<DatabaseCommandLoggingInterceptor>(),
                sp.GetRequiredService<EntityChangeLoggingInterceptor>());
        });

        return services;
    }
}
