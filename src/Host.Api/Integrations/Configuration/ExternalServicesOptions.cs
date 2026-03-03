namespace Host.Api.Integrations.Configuration;

public sealed class ExternalServicesOptions
{
    public ReferenceApiOptions ReferenceApi { get; set; } = new();
}

public sealed class ReferenceApiOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 5;
}
