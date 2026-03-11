namespace Application.Observability;

public interface IOperationalEventPublisher
{
    Task PublishAsync(OperationalEvent operationalEvent, CancellationToken cancellationToken = default);
}
