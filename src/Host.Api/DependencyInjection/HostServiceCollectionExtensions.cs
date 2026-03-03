using Host.Api.Exceptions;
using Host.Api.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Threading.RateLimiting;

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
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "global",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 240,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy("auth-strict", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "auth",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.OnRejected = async (context, token) =>
            {
                var correlationId = context.HttpContext.Items[CorrelationIdMiddleware.CorrelationItemKey]?.ToString()
                                    ?? context.HttpContext.TraceIdentifier;

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var payload = new
                {
                    type = "https://httpstatuses.com/429",
                    title = "Çok fazla istek gönderildi.",
                    status = StatusCodes.Status429TooManyRequests,
                    detail = "Lütfen bir süre sonra tekrar deneyin.",
                    errorCode = "rate_limited",
                    correlationId
                };

                await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(payload), token);
            };
        });
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
                .InNamespaces("Host.Api.Services", "Host.Api.Authorization.Services", "Host.Api.Identity.Services", "Host.Api.Operations.Services")
                .Where(type => type.Name.EndsWith("Service", StringComparison.Ordinal)
                            || type.Name.EndsWith("Accessor", StringComparison.Ordinal)
                            || type.Name.EndsWith("Context", StringComparison.Ordinal)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
