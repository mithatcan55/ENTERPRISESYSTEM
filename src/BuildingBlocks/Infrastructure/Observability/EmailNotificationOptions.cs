using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Observability;

public sealed class EmailNotificationOptions
{
    public const string SectionName = "Observability:Notifications:Email";

    [EmailAddress]
    public string? To { get; set; }
}
