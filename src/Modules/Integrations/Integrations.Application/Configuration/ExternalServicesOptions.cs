namespace Integrations.Application.Configuration;

public sealed class ExternalServicesOptions
{
    public ReferenceApiOptions ReferenceApi { get; set; } = new();
    public CaniasGatewayOptions CaniasGateway { get; set; } = new();
}

public sealed class ReferenceApiOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 5;
}

public sealed class CaniasGatewayOptions
{
    public string BaseUrl { get; set; } = "http://localhost:3002";
    public int TimeoutSeconds { get; set; } = 30;
    public string ApiKey { get; set; } = string.Empty;
}
