using Host.Api.Middleware;
using Host.Api.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Host.Api.Exceptions;

/// <summary>
/// Uygulamadaki yakalanmamış hataları tek noktada yönetir.
/// - Hata detayını system_logs tablosuna kaydeder.
/// - API tüketicisine RFC7807 ProblemDetails döner.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    LogDbContext logDbContext,
    ICurrentUserContext currentUserContext,
    IProblemDetailsService problemDetailsService,
    IHostEnvironment hostEnvironment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception caught by global exception handler.");

        var correlationId = httpContext.Items[CorrelationIdMiddleware.CorrelationItemKey]?.ToString()
                            ?? httpContext.TraceIdentifier;

        var actorIdentity = currentUserContext.TryGetActorIdentity(out var resolvedActor)
            ? resolvedActor
            : "anonymous";

        var username = currentUserContext.TryGetUsername(out var resolvedUsername)
            ? resolvedUsername
            : actorIdentity;

        var systemLog = new SystemLog
        {
            Timestamp = DateTimeOffset.UtcNow,
            TimeZone = TimeZoneInfo.Local.Id,
            Level = "Error",
            Category = "UnhandledException",
            Source = nameof(GlobalExceptionHandler),
            Message = exception.Message,
            MessageTemplate = "Unhandled exception occurred while processing HTTP request.",
            Exception = exception.Message,
            StackTrace = exception.ToString(),
            UserId = actorIdentity,
            Username = username,
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
            CorrelationId = correlationId,
            RequestId = httpContext.TraceIdentifier,
            HttpMethod = httpContext.Request.Method,
            HttpPath = httpContext.Request.Path,
            QueryString = httpContext.Request.QueryString.HasValue ? httpContext.Request.QueryString.Value : null,
            HttpStatusCode = StatusCodes.Status500InternalServerError,
            MachineName = Environment.MachineName,
            Environment = hostEnvironment.EnvironmentName,
            ApplicationName = hostEnvironment.ApplicationName,
            ApplicationVersion = typeof(GlobalExceptionHandler).Assembly.GetName().Version?.ToString(),
            ProcessId = Environment.ProcessId,
            ThreadId = Environment.CurrentManagedThreadId
        };

        logDbContext.SystemLogs.Add(systemLog);
        await logDbContext.SaveChangesAsync(cancellationToken);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problem = new ProblemDetails
        {
            Title = "Beklenmeyen bir hata oluştu.",
            Status = StatusCodes.Status500InternalServerError,
            Detail = hostEnvironment.IsDevelopment() ? exception.Message : "İstek işlenirken bir hata oluştu.",
            Instance = httpContext.Request.Path
        };

        problem.Extensions["correlationId"] = correlationId;

        var handled = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });

        return handled;
    }
}
