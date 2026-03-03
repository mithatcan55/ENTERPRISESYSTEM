using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Host.Api.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Http.Features;

namespace Host.Api.Middleware;

public sealed class RequestLifecycleLoggingMiddleware(RequestDelegate next, ILogger<RequestLifecycleLoggingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task Invoke(HttpContext context, LogDbContext logDbContext, ICurrentUserContext currentUserContext)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        context.Request.EnableBuffering();
        var requestBody = await ReadBodyAsync(context.Request.Body);
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
            var responseBody = await ReadBodyAsync(responseBuffer);
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
            var requestHeaders = JsonSerializer.Serialize(context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), JsonOptions);
            var responseHeaders = JsonSerializer.Serialize(context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), JsonOptions);
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
                RequestSize = context.Request.ContentLength ?? requestBody.Length,
                StatusCode = context.Response.StatusCode,
                ResponseHeaders = responseHeaders,
                ResponseBody = responseBody,
                ResponseSize = context.Response.ContentLength ?? responseBody.Length,
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

            logDbContext.HttpRequestLogs.Add(httpLog);
            logDbContext.SystemLogs.Add(systemLog);
            await logDbContext.SaveChangesAsync(context.RequestAborted);

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
