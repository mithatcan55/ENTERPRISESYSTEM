using Application.Exceptions;
using Application.Observability;
using Application.Pipeline;
using Application.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Pipeline;

/// <summary>
/// Ortak command/query pipeline.
/// Amaç:
/// 1. Validation davranışını tek yerde çalıştırmak
/// 2. Pre-check davranışını tek yerde çalıştırmak
/// 3. Süre, başarı/başarısızlık ve event üretimini standart hale getirmek
/// </summary>
public sealed class RequestExecutionPipeline(
    IServiceProvider serviceProvider,
    IOperationalEventPublisher operationalEventPublisher,
    ICurrentUserContext currentUserContext) : IRequestExecutionPipeline
{
    public Task<TResponse> ExecuteQueryAsync<TRequest, TResponse>(
        TRequest request,
        Func<CancellationToken, Task<TResponse>> execute,
        CancellationToken cancellationToken,
        string? operationName = null)
        => ExecuteAsync(request, execute, cancellationToken, operationName, "Query");

    public Task<TResponse> ExecuteCommandAsync<TRequest, TResponse>(
        TRequest request,
        Func<CancellationToken, Task<TResponse>> execute,
        CancellationToken cancellationToken,
        string? operationName = null)
        => ExecuteAsync(request, execute, cancellationToken, operationName, "Command");

    public async Task ExecuteCommandAsync<TRequest>(
        TRequest request,
        Func<CancellationToken, Task> execute,
        CancellationToken cancellationToken,
        string? operationName = null)
    {
        await ExecuteAsync(
            request,
            async token =>
            {
                await execute(token);
                return true;
            },
            cancellationToken,
            operationName,
            "Command");
    }

    private async Task<TResponse> ExecuteAsync<TRequest, TResponse>(
        TRequest request,
        Func<CancellationToken, Task<TResponse>> execute,
        CancellationToken cancellationToken,
        string? operationName,
        string pipelineType)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var startedTicks = Environment.TickCount64;
        var resolvedOperationName = operationName ?? typeof(TRequest).Name;

        await RunValidationAsync(request, cancellationToken);
        await RunPreChecksAsync(request, cancellationToken);

        try
        {
            var response = await execute(cancellationToken);

            await PublishAsync(
                eventName: $"{pipelineType}Executed",
                severity: "Information",
                category: $"{pipelineType}Pipeline",
                message: $"{resolvedOperationName} basariyla tamamlandi.",
                operationName: resolvedOperationName,
                durationMs: Environment.TickCount64 - startedTicks,
                isSuccessful: true,
                failureReason: null,
                cancellationToken: cancellationToken,
                properties: new Dictionary<string, object?>
                {
                    ["requestType"] = typeof(TRequest).FullName,
                    ["startedAt"] = startedAt
                });

            return response;
        }
        catch (Exception exception)
        {
            await PublishAsync(
                eventName: $"{pipelineType}Failed",
                severity: exception is AppException ? "Warning" : "Error",
                category: $"{pipelineType}Pipeline",
                message: $"{resolvedOperationName} basarisiz oldu.",
                operationName: resolvedOperationName,
                durationMs: Environment.TickCount64 - startedTicks,
                isSuccessful: false,
                failureReason: exception.Message,
                cancellationToken: cancellationToken,
                properties: new Dictionary<string, object?>
                {
                    ["requestType"] = typeof(TRequest).FullName,
                    ["startedAt"] = startedAt,
                    ["exceptionType"] = exception.GetType().FullName
                },
                exception: exception);

            throw;
        }
    }

    private async Task RunValidationAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
    {
        var validators = serviceProvider.GetServices<IRequestValidator<TRequest>>().ToList();
        foreach (var validator in validators)
        {
            await validator.ValidateAsync(request, cancellationToken);
        }
    }

    private async Task RunPreChecksAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
    {
        // AdminOnly marker varsa burada ortak bir ikinci savunma hattı çalıştırılır.
        if (request is IAdminOnlyRequest && !currentUserContext.IsInRole("SYS_ADMIN"))
        {
            throw new ForbiddenAppException("Bu islem yalnizca SYS_ADMIN rolune sahip kullanicilar tarafindan calistirilabilir.");
        }

        var preChecks = serviceProvider.GetServices<IRequestPreCheck<TRequest>>().ToList();
        foreach (var preCheck in preChecks)
        {
            await preCheck.CheckAsync(request, cancellationToken);
        }
    }

    private async Task PublishAsync(
        string eventName,
        string severity,
        string category,
        string message,
        string operationName,
        long durationMs,
        bool isSuccessful,
        string? failureReason,
        CancellationToken cancellationToken,
        Dictionary<string, object?> properties,
        Exception? exception = null)
    {
        var userId = currentUserContext.TryGetActorIdentity(out var actorIdentity) ? actorIdentity : null;
        var username = currentUserContext.TryGetUsername(out var resolvedUsername) ? resolvedUsername : userId;

        await operationalEventPublisher.PublishAsync(new OperationalEvent
        {
            EventName = eventName,
            Severity = severity,
            Category = category,
            Message = message,
            Source = nameof(RequestExecutionPipeline),
            OperationName = operationName,
            DurationMs = durationMs,
            IsSuccessful = isSuccessful,
            FailureReason = failureReason,
            UserId = userId,
            Username = username,
            Properties = properties,
            ExceptionMessage = exception?.Message,
            StackTrace = exception?.ToString()
        }, cancellationToken);
    }
}
