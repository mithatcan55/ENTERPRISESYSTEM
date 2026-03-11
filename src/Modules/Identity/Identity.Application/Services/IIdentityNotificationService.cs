namespace Identity.Application.Services;

public interface IIdentityNotificationService
{
    Task QueueAdminMailAsync(string to, string subject, string body, CancellationToken cancellationToken);
}
