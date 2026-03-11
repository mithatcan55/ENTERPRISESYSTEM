namespace Host.Api.Integrations.Services;

public interface IEmailDeliveryService
{
    Task SendAsync(string to, string subject, string body, string? attachmentPath, CancellationToken cancellationToken);
}
