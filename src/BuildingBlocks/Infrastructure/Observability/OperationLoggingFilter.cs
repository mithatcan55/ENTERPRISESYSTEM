using System.Diagnostics;
using System.Text.Json;
using Application.Observability;
using Application.Security;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Infrastructure.Observability;

public sealed class OperationLoggingFilter(
    IOperationalEventPublisher operationalEventPublisher,
    ICurrentUserContext currentUserContext) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
        var skipLogging = descriptor?.MethodInfo.GetCustomAttributes(typeof(SkipOperationLogAttribute), inherit: true).Any() == true
            || descriptor?.ControllerTypeInfo.GetCustomAttributes(typeof(SkipOperationLogAttribute), inherit: true).Any() == true;

        if (skipLogging)
        {
            await next();
            return;
        }

        var startedAt = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        ActionExecutedContext? executedContext = null;

        try
        {
            executedContext = await next();
        }
        finally
        {
            stopwatch.Stop();

            var metadata = descriptor?.MethodInfo
                               .GetCustomAttributes(typeof(OperationLogAttribute), inherit: true)
                               .OfType<OperationLogAttribute>()
                               .LastOrDefault()
                           ?? descriptor?.ControllerTypeInfo
                               .GetCustomAttributes(typeof(OperationLogAttribute), inherit: true)
                               .OfType<OperationLogAttribute>()
                               .LastOrDefault();
            var operationName = metadata?.OperationName
                ?? $"{descriptor?.ControllerName ?? "Unknown"}.{descriptor?.ActionName ?? "Unknown"}";
            var category = metadata?.Category ?? "BusinessOperation";
            var isSuccess = executedContext?.Exception is null && (executedContext?.HttpContext.Response.StatusCode ?? 500) < 500;
            var userId = currentUserContext.TryGetActorIdentity(out var actorIdentity) ? actorIdentity : null;
            var username = currentUserContext.TryGetUsername(out var resolvedUsername) ? resolvedUsername : userId;
            var httpContext = context.HttpContext;
            var argumentSummary = SummarizeArguments(context.ActionArguments);

            var operationalEvent = new OperationalEvent
            {
                EventName = isSuccess ? "BusinessOperationCompleted" : "BusinessOperationFailed",
                Severity = isSuccess ? "Information" : "Warning",
                Category = category,
                Source = nameof(OperationLoggingFilter),
                Message = $"{operationName} {(isSuccess ? "completed" : "failed")} in {stopwatch.ElapsedMilliseconds}ms",
                IsSuccessful = isSuccess,
                FailureReason = executedContext?.Exception?.Message,
                ExceptionMessage = executedContext?.Exception?.Message,
                StackTrace = executedContext?.Exception?.StackTrace,
                UserId = userId,
                Username = username,
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = httpContext.Request.Headers["User-Agent"].ToString(),
                CorrelationId = httpContext.Items["CorrelationId"]?.ToString() ?? httpContext.TraceIdentifier,
                HttpMethod = httpContext.Request.Method,
                HttpPath = httpContext.Request.Path,
                HttpStatusCode = httpContext.Response.StatusCode,
                OperationName = operationName,
                DurationMs = stopwatch.ElapsedMilliseconds,
                Action = descriptor?.ActionName,
                Resource = httpContext.Request.Path,
                Properties = new Dictionary<string, object?>
                {
                    ["controller"] = descriptor?.ControllerName,
                    ["action"] = descriptor?.ActionName,
                    ["arguments"] = argumentSummary,
                    ["requestId"] = httpContext.TraceIdentifier
                }
            };

            await operationalEventPublisher.PublishAsync(operationalEvent, httpContext.RequestAborted);
        }
    }

    private static string? SummarizeArguments(IDictionary<string, object?> actionArguments)
    {
        if (actionArguments.Count == 0)
        {
            return null;
        }

        var summary = actionArguments.ToDictionary(
            x => x.Key,
            x => SummarizeValue(x.Value));

        return JsonSerializer.Serialize(summary);
    }

    private static object? SummarizeValue(object? value)
    {
        if (value is null)
        {
            return null;
        }

        var type = value.GetType();
        if (type.IsPrimitive || value is string or decimal or Guid or DateTime or DateTimeOffset)
        {
            return value;
        }

        if (value is CancellationToken)
        {
            return "CancellationToken";
        }

        return new
        {
            Type = type.Name
        };
    }
}
