namespace Application.Observability;

public interface INotificationChannel
{
    string ChannelName { get; }
    Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken = default);
}
