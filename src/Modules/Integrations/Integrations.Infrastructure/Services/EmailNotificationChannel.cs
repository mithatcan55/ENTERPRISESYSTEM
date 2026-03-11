using Application.Observability;
using Identity.Application.Services;
using Infrastructure.Observability;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Integrations.Infrastructure.Services;

public sealed class EmailNotificationChannel(
    IIdentityNotificationService identityNotificationService,
    IOptions<EmailNotificationOptions> options,
    ILogger<EmailNotificationChannel> logger) : INotificationChannel
{
    public string ChannelName => "email";

    public async Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        var to = message.Metadata.TryGetValue("to", out var target)
            ? target
            : options.Value.To;

        if (string.IsNullOrWhiteSpace(to))
        {
            logger.LogDebug("Email notification skipped because recipient metadata is missing. EventName={EventName}", message.EventName);
            return;
        }

        await identityNotificationService.QueueAdminMailAsync(to, message.Subject, message.Body, cancellationToken);
    }
}
