using Integrations.Application.Services;
using Integrations.Infrastructure.Services;
using Application.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Integrations.Infrastructure.DependencyInjection;

public static class IntegrationsInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddIntegrationsInfrastructureModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IExternalOutboxService, ExternalOutboxService>();
        services.AddScoped<IExternalDataGateway, ExternalDataGateway>();
        services.AddScoped<IEmailDeliveryService, EmailDeliveryService>();
        services.AddScoped<IExcelReportComposerService, ExcelReportComposerService>();
        services.AddScoped<Identity.Application.Services.IIdentityNotificationService, IdentityNotificationService>();
        services.AddScoped<INotificationChannel, EmailNotificationChannel>();

        services.AddOptions<Integrations.Application.Configuration.ExternalServicesOptions>()
            .BindConfiguration("ExternalServices")
            .ValidateDataAnnotations();

        services.AddHttpClient("reference-api", (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<Integrations.Application.Configuration.ExternalServicesOptions>>().Value;
            client.BaseAddress = new Uri(options.ReferenceApi.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.ReferenceApi.TimeoutSeconds <= 0 ? 5 : options.ReferenceApi.TimeoutSeconds);
        });

        // CaniasGateway HttpClient — ERP verisi icin proxy
        services.AddScoped<ICaniasGatewayClient, CaniasGatewayClient>();
        services.AddSingleton<IExcelExporter, DynamicExcelExporter>();
        services.AddHttpClient("canias-gateway", (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<Integrations.Application.Configuration.ExternalServicesOptions>>().Value;
            client.BaseAddress = new Uri(options.CaniasGateway.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.CaniasGateway.TimeoutSeconds <= 0 ? 30 : options.CaniasGateway.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHostedService<ExternalOutboxDispatcherService>();

        return services;
    }
}
