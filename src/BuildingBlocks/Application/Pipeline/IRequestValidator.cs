namespace Application.Pipeline;

public interface IRequestValidator<in TRequest>
{
    Task ValidateAsync(TRequest request, CancellationToken cancellationToken);
}
