using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Observability;

public sealed class WebhookNotificationOptions
{
    public const string SectionName = "Observability:Notifications:Webhook";

    [Required]
    public string Url { get; set; } = string.Empty;
}
