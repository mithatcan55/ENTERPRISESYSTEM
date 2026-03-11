namespace Application.Pipeline;

public interface IRequestPreCheck<in TRequest>
{
    Task CheckAsync(TRequest request, CancellationToken cancellationToken);
}
