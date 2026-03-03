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
        var (statusCode, title, detail, errorCode, errors) = MapException(exception, hostEnvironment.IsDevelopment());

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception caught by global exception handler.");
        }
        else
        {
            logger.LogWarning(exception, "Handled application exception caught by global exception handler.");
        }

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
            Level = statusCode >= 500 ? "Error" : "Warning",
            Category = statusCode >= 500 ? "UnhandledException" : "HandledAppException",
            Source = nameof(GlobalExceptionHandler),
            Message = exception.Message,
            MessageTemplate = "Exception occurred while processing HTTP request.",
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
            HttpStatusCode = statusCode,
            MachineName = Environment.MachineName,
            Environment = hostEnvironment.EnvironmentName,
            ApplicationName = hostEnvironment.ApplicationName,
            ApplicationVersion = typeof(GlobalExceptionHandler).Assembly.GetName().Version?.ToString(),
            ProcessId = Environment.ProcessId,
            ThreadId = Environment.CurrentManagedThreadId
        };

        logDbContext.SystemLogs.Add(systemLog);
        await logDbContext.SaveChangesAsync(cancellationToken);

        httpContext.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problem.Type = $"https://httpstatuses.com/{statusCode}";
        problem.Extensions["errorCode"] = errorCode;
        problem.Extensions["correlationId"] = correlationId;
        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        var handled = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });

        return handled;
    }

    private static (int StatusCode, string Title, string Detail, string ErrorCode, IReadOnlyDictionary<string, string[]>? Errors)
        MapException(Exception exception, bool isDevelopment)
    {
        if (exception is AppException appException)
        {
            var title = appException.StatusCode switch
            {
                StatusCodes.Status400BadRequest => "Doğrulama hatası.",
                StatusCodes.Status403Forbidden => "Bu işlem için yetkiniz yok.",
                StatusCodes.Status404NotFound => "Kayıt bulunamadı.",
                _ => "İstek işlenemedi."
            };

            var detail = appException.Detail
                         ?? (isDevelopment ? appException.Message : "İstek işlenirken bir uygulama hatası oluştu.");

            return (appException.StatusCode, title, detail, appException.ErrorCode, appException.Errors);
        }

        var defaultDetail = isDevelopment
            ? exception.Message
            : "İstek işlenirken beklenmeyen bir hata oluştu.";

        return (
            StatusCodes.Status500InternalServerError,
            "Beklenmeyen bir hata oluştu.",
            defaultDetail,
            "internal_error",
            null);
    }
}
