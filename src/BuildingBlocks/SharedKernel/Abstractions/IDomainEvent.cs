namespace SharedKernel.Abstractions;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
