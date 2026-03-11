namespace Infrastructure.Observability;

public sealed class ObservabilityRoutingOptions
{
    public const string SectionName = "Observability:Routing";
    public List<OperationalEventRouteOptions> Routes { get; set; } = [];
}

public sealed class OperationalEventRouteOptions
{
    // EventName '*' ise fallback route gibi davranir.
    public string EventName { get; set; } = "*";
    public bool WriteSystemLog { get; set; } = true;
    public bool WriteSecurityLog { get; set; }
    public bool WritePerformanceLog { get; set; }
    public bool SendNotification { get; set; }
    // NotificationChannels sadece SendNotification=true ise kullanilir.
    public List<string> NotificationChannels { get; set; } = [];
    // MinimumSeverities route'un hangi siddetten itibaren aktif olacagini belirler.
    public List<string> MinimumSeverities { get; set; } = [];
}
