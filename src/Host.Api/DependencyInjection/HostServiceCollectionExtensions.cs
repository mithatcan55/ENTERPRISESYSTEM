using Application.Exceptions;
using Application.Security;
using Host.Api.Configuration;
using Host.Api.Exceptions;
using Host.Api.Localization;
using Host.Api.Middleware;
using Host.Api.Observability;
using Host.Api.Security;
using Host.Api.Services;
using Identity.Application.Configuration;
using Infrastructure.Observability;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace Host.Api.DependencyInjection;

public static class HostServiceCollectionExtensions
{
    private sealed class HostAssemblyMarker;

    public static IServiceCollection AddHostCoreServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        StartupSecurityValidator.EnsureNoPlaceholderSecrets(configuration, environment);

        services.AddHttpContextAccessor();
        services.AddSingleton<IApiTextLocalizer, ApiTextLocalizer>();
        services.AddSingleton<ISensitiveDataRedactor, SensitiveDataRedactor>();
        services.AddScoped<OperationLoggingFilter>();

        services.AddOptions<PersistenceBootstrapOptions>()
            .BindConfiguration(PersistenceBootstrapOptions.SectionName);

        services.AddOptions<SensitiveDataLoggingOptions>()
            .BindConfiguration(SensitiveDataLoggingOptions.SectionName);

        var jwtSection = configuration.GetSection(JwtTokenOptions.SectionName);
        var jwtOptions = jwtSection.Get<JwtTokenOptions>() ?? new JwtTokenOptions();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var userIdRaw = context.Principal?.FindFirst(SecurityClaimTypes.UserId)?.Value
                                    ?? context.Principal?.FindFirst(SecurityClaimTypes.Subject)?.Value;
                    var sessionIdRaw = context.Principal?.FindFirst(SecurityClaimTypes.SessionId)?.Value;

                    if (!int.TryParse(userIdRaw, out var userId) || !int.TryParse(sessionIdRaw, out var sessionId))
                    {
                        context.Fail("invalid_token_claims");
                        return;
                    }

                    var identityDbContext = context.HttpContext.RequestServices.GetRequiredService<IdentityDbContext>();
                    var now = DateTime.UtcNow;

                    var session = await identityDbContext.UserSessions
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId && !x.IsDeleted, context.HttpContext.RequestAborted);

                    if (session is null || session.IsRevoked || session.ExpiresAt <= now)
                    {
                        context.Fail("session_invalid_or_expired");
                        return;
                    }

                    var user = await identityDbContext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, context.HttpContext.RequestAborted);

                    if (user is null || !user.IsActive)
                    {
                        context.Fail("user_inactive");
                        return;
                    }

                    if (user.PasswordExpiresAt.HasValue && user.PasswordExpiresAt.Value <= now)
                    {
                        context.Fail("password_expired");
                    }
                }
            };
        });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();
        });

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
                var localizer = context.HttpContext.RequestServices.GetRequiredService<IApiTextLocalizer>();
                var correlationId = context.HttpContext.Items[CorrelationIdMiddleware.CorrelationItemKey]?.ToString()
                                    ?? context.HttpContext.TraceIdentifier;

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var payload = new
                {
                    type = "https://httpstatuses.com/429",
                    title = localizer.Get("rate_limited_title"),
                    status = StatusCodes.Status429TooManyRequests,
                    detail = localizer.Get("rate_limited_detail"),
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
                var localizer = context.HttpContext.RequestServices.GetRequiredService<IApiTextLocalizer>();
                var correlationId = context.HttpContext.Items[CorrelationIdMiddleware.CorrelationItemKey]?.ToString()
                                    ?? context.HttpContext.TraceIdentifier;

                var validationProblem = new ValidationProblemDetails(context.ModelState)
                {
                    Title = localizer.Get("validation_title"),
                    Detail = localizer.Get("validation_detail"),
                    Status = StatusCodes.Status400BadRequest,
                    Instance = context.HttpContext.Request.Path,
                    Type = "https://httpstatuses.com/400"
                };

                validationProblem.Extensions["errorCode"] = "validation_error";
                validationProblem.Extensions["correlationId"] = correlationId;

                return new BadRequestObjectResult(validationProblem);
            };
        });

        services.Configure<MvcOptions>(options =>
        {
            options.Filters.AddService<OperationLoggingFilter>();
        });

        services.Scan(scan => scan
            .FromAssemblyOf<HostAssemblyMarker>()
            .AddClasses(classes => classes
                .InNamespaces("Host.Api.Services", "Host.Api.Security")
                .Where(type => type.Name.EndsWith("Service", StringComparison.Ordinal)
                            || type.Name.EndsWith("Accessor", StringComparison.Ordinal)
                            || type.Name.EndsWith("Context", StringComparison.Ordinal)
                            || type.Name.EndsWith("Gateway", StringComparison.Ordinal))
                .Where(type => !typeof(IHostedService).IsAssignableFrom(type)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddOptions<PasswordPolicyOptions>()
            .BindConfiguration(PasswordPolicyOptions.SectionName)
            .ValidateDataAnnotations();

        services.AddOptions<JwtTokenOptions>()
            .BindConfiguration(JwtTokenOptions.SectionName)
            .Validate(options => !string.IsNullOrWhiteSpace(options.SigningKey) && options.SigningKey.Length >= 32,
                "Jwt:SigningKey en az 32 karakter olmalıdır.")
            .ValidateDataAnnotations();

        services.AddHostedService<CoreBootstrapHostedService>();

        return services;
    }
}
