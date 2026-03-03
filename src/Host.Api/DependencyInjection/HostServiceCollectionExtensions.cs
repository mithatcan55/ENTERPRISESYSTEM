using Host.Api.Exceptions;
using Host.Api.Middleware;
using Microsoft.AspNetCore.Mvc;
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
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var correlationId = context.HttpContext.Items[CorrelationIdMiddleware.CorrelationItemKey]?.ToString()
                                    ?? context.HttpContext.TraceIdentifier;

                var validationProblem = new ValidationProblemDetails(context.ModelState)
                {
                    Title = "Doğrulama hatası.",
                    Detail = "Gönderilen veri doğrulama kurallarını sağlamıyor.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = context.HttpContext.Request.Path,
                    Type = "https://httpstatuses.com/400"
                };

                validationProblem.Extensions["errorCode"] = "validation_error";
                validationProblem.Extensions["correlationId"] = correlationId;

                return new BadRequestObjectResult(validationProblem);
            };
        });

        services.Scan(scan => scan
            .FromAssemblyOf<HostAssemblyMarker>()
            .AddClasses(classes => classes
                .InNamespaces("Host.Api.Services", "Host.Api.Authorization.Services", "Host.Api.Identity.Services")
                .Where(type => type.Name.EndsWith("Service", StringComparison.Ordinal)
                            || type.Name.EndsWith("Accessor", StringComparison.Ordinal)
                            || type.Name.EndsWith("Context", StringComparison.Ordinal)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
