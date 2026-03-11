using Integrations.Application.Services;
using Integrations.Infrastructure.Services;
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

        services.AddOptions<Integrations.Application.Configuration.ExternalServicesOptions>()
            .BindConfiguration("ExternalServices")
            .ValidateDataAnnotations();

        services.AddHttpClient("reference-api", (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<Integrations.Application.Configuration.ExternalServicesOptions>>().Value;
            client.BaseAddress = new Uri(options.ReferenceApi.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.ReferenceApi.TimeoutSeconds <= 0 ? 5 : options.ReferenceApi.TimeoutSeconds);
        });

        services.AddHostedService<ExternalOutboxDispatcherService>();

        return services;
    }
}
