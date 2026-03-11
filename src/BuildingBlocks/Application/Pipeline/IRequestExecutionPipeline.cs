namespace Application.Pipeline;

public interface IRequestExecutionPipeline
{
    Task<TResponse> ExecuteQueryAsync<TRequest, TResponse>(
        TRequest request,
        Func<CancellationToken, Task<TResponse>> execute,
        CancellationToken cancellationToken,
        string? operationName = null);

    Task<TResponse> ExecuteCommandAsync<TRequest, TResponse>(
        TRequest request,
        Func<CancellationToken, Task<TResponse>> execute,
        CancellationToken cancellationToken,
        string? operationName = null);

    Task ExecuteCommandAsync<TRequest>(
        TRequest request,
        Func<CancellationToken, Task> execute,
        CancellationToken cancellationToken,
        string? operationName = null);
}
