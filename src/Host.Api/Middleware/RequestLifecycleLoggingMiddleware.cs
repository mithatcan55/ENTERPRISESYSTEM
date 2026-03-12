using Application.Security;
using System.Diagnostics;
using System.Text;
using Host.Api.Observability;
using Host.Api.Services;
using Infrastructure.Observability;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Http.Features;

namespace Host.Api.Middleware;

public sealed class RequestLifecycleLoggingMiddleware(RequestDelegate next, ILogger<RequestLifecycleLoggingMiddleware> logger)
{
    public async Task Invoke(
        HttpContext context,
        ILogEventWriter logEventWriter,
        ICurrentUserContext currentUserContext,
        ISensitiveDataRedactor sensitiveDataRedactor)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        var memoryBefore = GC.GetTotalMemory(false);

        context.Request.EnableBuffering();
        var requestBodyRaw = await ReadBodyAsync(context.Request.Body);
        context.Request.Body.Position = 0;

        var originalResponseBody = context.Response.Body;
        await using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        Exception? unhandledException = null;

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            unhandledException = ex;
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            responseBuffer.Position = 0;
            var responseBodyRaw = await ReadBodyAsync(responseBuffer);
            responseBuffer.Position = 0;
            await responseBuffer.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;

            var correlationId = context.Items[CorrelationIdMiddleware.CorrelationItemKey]?.ToString() ?? context.TraceIdentifier;
            var userId = currentUserContext.TryGetActorIdentity(out var actorIdentity)
                ? actorIdentity
                : null;
            var username = currentUserContext.TryGetUsername(out var resolvedUsername)
                ? resolvedUsername
                : userId;
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var sessionId = context.Features.Get<ISessionFeature>()?.Session?.Id;
            var requestHeaders = sensitiveDataRedactor.RedactHeaders(context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()));
            var responseHeaders = sensitiveDataRedactor.RedactHeaders(context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()));
            var requestBody = sensitiveDataRedactor.RedactBody(requestBodyRaw, context.Request.ContentType, context.Request.Path);
            var responseBody = sensitiveDataRedactor.RedactBody(responseBodyRaw, context.Response.ContentType, context.Request.Path);
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var isError = unhandledException is not null || context.Response.StatusCode >= 500;

            var httpLog = new HttpRequestLog
            {
                Timestamp = startedAt,
                CorrelationId = correlationId,
                Method = context.Request.Method,
                Path = context.Request.Path,
                QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
                RequestHeaders = requestHeaders,
                RequestBody = requestBody,
                RequestSize = context.Request.ContentLength ?? requestBodyRaw.Length,
                StatusCode = context.Response.StatusCode,
                ResponseHeaders = responseHeaders,
                ResponseBody = responseBody,
                ResponseSize = context.Response.ContentLength ?? responseBodyRaw.Length,
                DurationMs = stopwatch.ElapsedMilliseconds,
                UserId = userId,
                Username = username,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsError = isError,
                ErrorMessage = unhandledException?.Message,
                ErrorStackTrace = unhandledException?.ToString()
            };

            var systemLog = new SystemLog
            {
                Timestamp = startedAt,
                TimeZone = TimeZoneInfo.Local.Id,
                Level = isError ? "Error" : "Information",
                Category = "HttpLifecycle",
                Source = "RequestLifecycleLoggingMiddleware",
                Message = $"{context.Request.Method} {context.Request.Path} completed with {context.Response.StatusCode} in {stopwatch.ElapsedMilliseconds}ms",
                MessageTemplate = "{Method} {Path} completed with {StatusCode} in {DurationMs}ms",
                Exception = unhandledException?.Message,
                StackTrace = unhandledException?.StackTrace,
                UserId = userId,
                Username = username,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CorrelationId = correlationId,
                RequestId = context.TraceIdentifier,
                SessionId = sessionId,
                HttpMethod = context.Request.Method,
                HttpPath = context.Request.Path,
                QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
                HttpStatusCode = context.Response.StatusCode,
                HttpDurationMs = stopwatch.ElapsedMilliseconds,
                RequestBody = requestBody,
                ResponseBody = responseBody,
                RequestHeaders = requestHeaders,
                ResponseHeaders = responseHeaders,
                DurationMs = stopwatch.ElapsedMilliseconds,
                MachineName = Environment.MachineName,
                Environment = context.RequestServices.GetService<IHostEnvironment>()?.EnvironmentName,
                ApplicationName = context.RequestServices.GetService<IHostEnvironment>()?.ApplicationName,
                ApplicationVersion = typeof(RequestLifecycleLoggingMiddleware).Assembly.GetName().Version?.ToString(),
                ProcessId = Environment.ProcessId,
                ThreadId = Environment.CurrentManagedThreadId
            };
            var performanceLog = new PerformanceLog
            {
                Timestamp = startedAt,
                CorrelationId = correlationId,
                UserId = userId,
                OperationName = $"{context.Request.Method} {context.Request.Path}",
                OperationType = "HTTP",
                DurationMs = stopwatch.ElapsedMilliseconds,
                MemoryBefore = memoryBefore,
                MemoryAfter = GC.GetTotalMemory(false),
                MemoryUsed = Math.Max(0, GC.GetTotalMemory(false) - memoryBefore),
                IsSlowOperation = stopwatch.ElapsedMilliseconds >= 1000,
                ThresholdMs = 1000,
                AdditionalData = System.Text.Json.JsonSerializer.Serialize(new
                {
                    context.Response.StatusCode,
                    context.TraceIdentifier,
                    IsError = isError
                })
            };

            await logEventWriter.WriteHttpAsync(httpLog, context.RequestAborted);
            await logEventWriter.WriteSystemAsync(systemLog, context.RequestAborted);
            await logEventWriter.WritePerformanceAsync(performanceLog, context.RequestAborted);

            if (HttpMethods.IsGet(context.Request.Method) && context.Response.StatusCode < 400)
            {
                var pageVisitLog = new PageVisitLog
                {
                    Timestamp = startedAt,
                    UserId = userId,
                    Username = username,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    PagePath = context.Request.Path,
                    QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
                    Referrer = context.Request.Headers.Referer.ToString(),
                    SessionId = sessionId,
                    CorrelationId = correlationId,
                    VisitDurationMs = stopwatch.ElapsedMilliseconds,
                    IsMobile = userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase),
                    AdditionalData = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        context.Response.StatusCode,
                        RouteValues = context.Request.RouteValues.ToDictionary(x => x.Key, x => x.Value?.ToString())
                    })
                };

                await logEventWriter.WritePageVisitAsync(pageVisitLog, context.RequestAborted);
            }

            logger.LogInformation("HTTP lifecycle logged. CorrelationId: {CorrelationId}", correlationId);
        }
    }

    private static async Task<string> ReadBodyAsync(Stream stream)
    {
        if (!stream.CanRead)
        {
            return string.Empty;
        }

        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }
}
