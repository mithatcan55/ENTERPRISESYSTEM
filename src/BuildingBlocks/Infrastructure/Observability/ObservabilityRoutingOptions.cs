namespace Infrastructure.Observability;

public sealed class ObservabilityRoutingOptions
{
    public const string SectionName = "Observability:Routing";
    public List<OperationalEventRouteOptions> Routes { get; set; } = [];
}

public sealed class OperationalEventRouteOptions
{
    public string EventName { get; set; } = "*";
    public bool WriteSystemLog { get; set; } = true;
    public bool WriteSecurityLog { get; set; }
    public bool WritePerformanceLog { get; set; }
    public bool SendNotification { get; set; }
    public List<string> NotificationChannels { get; set; } = [];
    public List<string> MinimumSeverities { get; set; } = [];
}
