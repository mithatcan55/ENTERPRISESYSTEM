namespace Infrastructure.Observability;

public sealed class ObservabilityRoutingOptions
{
    public const string SectionName = "Observability:Routing";
    public int DefaultPerformanceThresholdMs { get; set; } = 500;
    public bool DisableAllNotifications { get; set; }
    public List<OperationalEventRoutePresetOptions> Presets { get; set; } = [];
    public List<OperationalEventRouteOptions> Routes { get; set; } = [];
    public List<OperationalEventEscalationRuleOptions> EscalationRules { get; set; } = [];
}

public abstract class OperationalEventRouteCriteria
{
    public string EventName { get; set; } = "*";
    public string? Category { get; set; }
    public string? Source { get; set; }
    public string? OperationName { get; set; }
    public bool OnlyFailures { get; set; }
    public bool OnlySuccess { get; set; }
    public int? MinimumHttpStatusCode { get; set; }
    public int? MaximumHttpStatusCode { get; set; }
}

public sealed class OperationalEventRouteOptions : OperationalEventRouteCriteria
{
    // Preset sayesinde tekrar eden log/notification ayarlari tek isimle alinabilir.
    public string? Preset { get; set; }
    public bool WriteSystemLog { get; set; } = true;
    public bool WriteSecurityLog { get; set; }
    public bool WritePerformanceLog { get; set; }
    public int? PerformanceThresholdMs { get; set; }
    public bool SendNotification { get; set; }
    // NotificationChannels sadece SendNotification=true ise kullanilir.
    public List<string> NotificationChannels { get; set; } = [];
    // MinimumSeverities route'un hangi siddetten itibaren aktif olacagini belirler.
    public List<string> MinimumSeverities { get; set; } = [];
}

public sealed class OperationalEventRoutePresetOptions
{
    public string Name { get; set; } = string.Empty;
    public bool WriteSystemLog { get; set; } = true;
    public bool WriteSecurityLog { get; set; }
    public bool WritePerformanceLog { get; set; }
    public int? PerformanceThresholdMs { get; set; }
    public bool SendNotification { get; set; }
    public List<string> NotificationChannels { get; set; } = [];
    public List<string> MinimumSeverities { get; set; } = [];
}

public sealed class OperationalEventEscalationRuleOptions : OperationalEventRouteCriteria
{
    public int? MinimumDurationMs { get; set; }
    public List<string> MinimumSeverities { get; set; } = [];
    public bool ForceNotification { get; set; } = true;
    public List<string> NotificationChannels { get; set; } = [];
    public string? OverrideSeverity { get; set; }
}
