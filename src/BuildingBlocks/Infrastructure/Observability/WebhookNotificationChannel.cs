using System.Net.Http.Json;
using Application.Observability;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Observability;

public sealed class WebhookNotificationChannel(
    IHttpClientFactory httpClientFactory,
    IOptions<WebhookNotificationOptions> options,
    ILogger<WebhookNotificationChannel> logger) : INotificationChannel
{
    public string ChannelName => "webhook";

    public async Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.Value.Url))
        {
            logger.LogDebug("Webhook notification skipped because URL is empty.");
            return;
        }

        var client = httpClientFactory.CreateClient("observability-webhook");
        var response = await client.PostAsJsonAsync(string.Empty, message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Webhook notification failed. StatusCode={StatusCode}, EventName={EventName}", (int)response.StatusCode, message.EventName);
        }
    }
}
