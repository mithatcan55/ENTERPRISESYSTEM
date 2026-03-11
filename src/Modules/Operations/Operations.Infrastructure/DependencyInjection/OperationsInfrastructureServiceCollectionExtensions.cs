using Microsoft.Extensions.DependencyInjection;
using Operations.Application.Services;
using Operations.Infrastructure.Services;

namespace Operations.Infrastructure.DependencyInjection;

public static class OperationsInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddOperationsInfrastructureModule(this IServiceCollection services)
    {
        services.AddScoped<IOperationsLogQueryService, OperationsLogQueryService>();
        services.AddScoped<IAuditDashboardService, AuditDashboardService>();
        return services;
    }
}
