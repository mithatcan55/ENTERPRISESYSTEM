using Host.Api.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Host.Api.DependencyInjection;

/// <summary>
/// Host katmanına ait servis kayıtlarını merkezi hale getirir.
/// Scrutor ile assembly scan kullanarak tekrarlı AddScoped satırlarını azaltır.
/// </summary>
public static class HostServiceCollectionExtensions
{
    private sealed class HostAssemblyMarker;

    /// <summary>
    /// Host katmanı core servislerini ve cross-cutting bileşenlerini kaydeder.
    /// </summary>
    public static IServiceCollection AddHostCoreServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.Scan(scan => scan
            .FromAssemblyOf<HostAssemblyMarker>()
            .AddClasses(classes => classes
                .InNamespaces("Host.Api.Services", "Host.Api.Authorization.Services")
                .Where(type => type.Name.EndsWith("Service", StringComparison.Ordinal)
                            || type.Name.EndsWith("Accessor", StringComparison.Ordinal)
                            || type.Name.EndsWith("Context", StringComparison.Ordinal)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
