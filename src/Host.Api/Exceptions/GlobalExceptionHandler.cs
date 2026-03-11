using Host.Api.Localization;
using Host.Api.Middleware;
using Application.Exceptions;
using Application.Observability;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Host.Api.Exceptions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IOperationalEventPublisher operationalEventPublisher,
    IProblemDetailsService problemDetailsService,
    IHostEnvironment hostEnvironment,
    IApiTextLocalizer localizer) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, detail, errorCode, errors) = MapException(exception, hostEnvironment.IsDevelopment(), localizer);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Global exception handler processed an unhandled error.");
        }
        else
        {
            logger.LogWarning(exception, "Global exception handler processed an application error.");
        }

        var correlationId = httpContext.Items[CorrelationIdMiddleware.CorrelationItemKey]?.ToString()
                            ?? httpContext.TraceIdentifier;

        var principal = httpContext.User;
        var actorIdentity = principal?.FindFirst("user_code")?.Value
                            ?? principal?.FindFirst("usercode")?.Value
                            ?? principal?.FindFirst("preferred_username")?.Value
                            ?? principal?.FindFirst("username")?.Value
                            ?? principal?.FindFirst("sub")?.Value
                            ?? principal?.FindFirst("user_id")?.Value
                            ?? "anonymous";

        var username = principal?.FindFirst("preferred_username")?.Value
                       ?? principal?.FindFirst("username")?.Value
                       ?? principal?.Identity?.Name
                       ?? actorIdentity;

        var operationalEvent = new OperationalEvent
        {
            EventName = statusCode >= 500 ? "UnhandledExceptionOccurred" : "HandledApplicationException",
            Severity = statusCode >= 500 ? "Error" : "Warning",
            Category = statusCode >= 500 ? "UnhandledException" : "HandledAppException",
            Source = nameof(GlobalExceptionHandler),
            Message = exception.Message,
            IsSuccessful = false,
            FailureReason = exception.Message,
            ExceptionMessage = exception.Message,
            StackTrace = exception.ToString(),
            UserId = actorIdentity,
            Username = username,
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
            CorrelationId = correlationId,
            HttpMethod = httpContext.Request.Method,
            HttpPath = httpContext.Request.Path,
            HttpStatusCode = statusCode,
            OperationName = $"{httpContext.Request.Method} {httpContext.Request.Path}",
            Action = "Exception",
            Resource = httpContext.Request.Path,
            Properties = new Dictionary<string, object?>
            {
                ["errorCode"] = errorCode,
                ["instance"] = httpContext.Request.Path,
                ["requestId"] = httpContext.TraceIdentifier
            }
        };

        await operationalEventPublisher.PublishAsync(operationalEvent, cancellationToken);

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
        MapException(Exception exception, bool isDevelopment, IApiTextLocalizer localizer)
    {
        if (exception is AppException appException)
        {
            var title = appException.StatusCode switch
            {
                StatusCodes.Status400BadRequest => localizer.Get("validation_title"),
                StatusCodes.Status401Unauthorized => localizer.Get("auth_required_title"),
                StatusCodes.Status403Forbidden => localizer.Get("forbidden_title"),
                StatusCodes.Status404NotFound => localizer.Get("not_found_title"),
                StatusCodes.Status429TooManyRequests => localizer.Get("rate_limited_title"),
                _ => localizer.Get("request_failed_title")
            };

            var detail = appException.Detail
                         ?? (isDevelopment
                             ? appException.Message
                             : ResolveLocalizedDetail(appException.ErrorCode, localizer));

            return (appException.StatusCode, title, detail, appException.ErrorCode, appException.Errors);
        }

        var defaultDetail = isDevelopment
            ? exception.Message
            : localizer.Get("internal_error_detail");

        return (
            StatusCodes.Status500InternalServerError,
            localizer.Get("internal_error_title"),
            defaultDetail,
            "internal_error",
            null);
    }

    private static string ResolveLocalizedDetail(string errorCode, IApiTextLocalizer localizer)
    {
        var localizedByCode = localizer.Get(errorCode);
        return string.Equals(localizedByCode, errorCode, StringComparison.Ordinal)
            ? localizer.Get("app_error_detail")
            : localizedByCode;
    }
}
