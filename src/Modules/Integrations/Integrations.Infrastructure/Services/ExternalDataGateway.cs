using System.Diagnostics;
using System.Net.Http.Json;
using Application.Exceptions;
using Application.Security;
using Infrastructure.Observability;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Integrations.Application.Contracts;
using Integrations.Application.Services;

namespace Integrations.Infrastructure.Services;

public sealed class ExternalDataGateway(
    IHttpClientFactory httpClientFactory,
    ILogEventWriter logEventWriter,
    IHttpContextAccessor httpContextAccessor,
    ICurrentUserContext currentUserContext) : IExternalDataGateway
{
    public async Task<ReferenceCompanyDto> GetReferenceCompanyAsync(int externalId, CancellationToken cancellationToken)
    {
        if (externalId <= 0)
        {
            throw new ValidationAppException(
                "Dis servis sorgu dogrulamasi basarisiz.",
                new Dictionary<string, string[]>
                {
                    ["externalId"] = ["externalId pozitif olmalidir."]
                });
        }

        var client = httpClientFactory.CreateClient("reference-api");
        var path = $"users/{externalId}";
        var startedAt = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await client.GetAsync(path, cancellationToken);
            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                await LogOutboundAsync(startedAt, stopwatch.ElapsedMilliseconds, path, (int)response.StatusCode, false, $"HTTP {(int)response.StatusCode}", cancellationToken);
                throw new AppExceptionAdapter(StatusCodes.Status502BadGateway, "external_service_error", "Dis servis cagrisi basarisiz oldu.");
            }

            var payload = await response.Content.ReadFromJsonAsync<JsonPlaceholderUser>(cancellationToken: cancellationToken);
            if (payload is null)
            {
                await LogOutboundAsync(startedAt, stopwatch.ElapsedMilliseconds, path, (int)response.StatusCode, false, "Bos payload", cancellationToken);
                throw new AppExceptionAdapter(StatusCodes.Status502BadGateway, "external_service_empty_payload", "Dis servis gecersiz yanit dondurdu.");
            }

            await LogOutboundAsync(startedAt, stopwatch.ElapsedMilliseconds, path, (int)response.StatusCode, true, null, cancellationToken);

            return new ReferenceCompanyDto(
                payload.Id,
                payload.Name ?? "N/A",
                payload.Email ?? "N/A",
                "ReferenceApi");
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            await LogOutboundAsync(startedAt, stopwatch.ElapsedMilliseconds, path, StatusCodes.Status504GatewayTimeout, false, "Timeout", cancellationToken);
            throw new AppExceptionAdapter(StatusCodes.Status504GatewayTimeout, "external_service_timeout", "Dis servis zaman asimina ugradi.", ex);
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            await LogOutboundAsync(startedAt, stopwatch.ElapsedMilliseconds, path, StatusCodes.Status502BadGateway, false, ex.Message, cancellationToken);
            throw new AppExceptionAdapter(StatusCodes.Status502BadGateway, "external_service_unreachable", "Dis servise ulasilamadi.", ex);
        }
    }

    private async Task LogOutboundAsync(
        DateTimeOffset startedAt,
        long durationMs,
        string path,
        int httpStatusCode,
        bool isSuccess,
        string? failureReason,
        CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var actor = currentUserContext.TryGetActorIdentity(out var actorIdentity) ? actorIdentity : "system";

        var log = new SystemLog
        {
            Timestamp = startedAt,
            TimeZone = TimeZoneInfo.Local.Id,
            Level = isSuccess ? "Information" : "Warning",
            Category = "ExternalIntegration",
            Source = nameof(ExternalDataGateway),
            Message = $"Reference API call completed with status {httpStatusCode} in {durationMs}ms",
            MessageTemplate = "Reference API call completed with status {StatusCode} in {DurationMs}ms",
            UserId = actor,
            Username = currentUserContext.TryGetUsername(out var username) ? username : actor,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
            CorrelationId = httpContext?.Items["CorrelationId"]?.ToString() ?? httpContext?.TraceIdentifier,
            HttpMethod = "GET",
            HttpPath = path,
            HttpStatusCode = httpStatusCode,
            HttpDurationMs = durationMs,
            Exception = failureReason,
            MachineName = Environment.MachineName,
            Environment = httpContext?.RequestServices.GetService<IHostEnvironment>()?.EnvironmentName,
            ApplicationName = httpContext?.RequestServices.GetService<IHostEnvironment>()?.ApplicationName,
            ApplicationVersion = typeof(ExternalDataGateway).Assembly.GetName().Version?.ToString(),
            ProcessId = Environment.ProcessId,
            ThreadId = Environment.CurrentManagedThreadId
        };

        await logEventWriter.WriteSystemAsync(log, cancellationToken);
    }

    private sealed record JsonPlaceholderUser(int Id, string? Name, string? Email);

    private sealed class AppExceptionAdapter(
        int statusCode,
        string errorCode,
        string message,
        Exception? innerException = null)
        : AppException(statusCode, errorCode, message, innerException: innerException);
}
