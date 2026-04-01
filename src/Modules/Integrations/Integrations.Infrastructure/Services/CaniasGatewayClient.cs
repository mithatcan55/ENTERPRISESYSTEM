using System.Net.Http.Json;
using System.Text.Json;
using Integrations.Application.Configuration;
using Integrations.Application.Contracts;
using Integrations.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Integrations.Infrastructure.Services;

public sealed class CaniasGatewayClient : ICaniasGatewayClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CaniasGatewayOptions _options;
    private readonly ILogger<CaniasGatewayClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public CaniasGatewayClient(
        IHttpClientFactory httpClientFactory,
        IOptions<ExternalServicesOptions> options,
        ILogger<CaniasGatewayClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value.CaniasGateway;
        _logger = logger;
    }

    // ── Public API ──────────────────────────────────────────────────

    public async Task<ErpQueryResult> RunAsync(string endpoint, Dictionary<string, string>? parameters, CancellationToken ct = default)
    {
        var client = CreateClient();
        var url = BuildGatewayUrl(endpoint, parameters);

        _logger.LogInformation("CaniasGateway request: GET {Url}", url);

        var response = await client.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<CaniasGatewayResponse<Dictionary<string, object?>>>(JsonOptions, ct);

        if (body is null || !body.Success)
        {
            _logger.LogWarning("CaniasGateway returned unsuccessful response for {Endpoint}: {Message}", endpoint, body?.Message);
            return new ErpQueryResult { Endpoint = endpoint };
        }

        return new ErpQueryResult
        {
            Rows = body.Data?.Rows ?? [],
            RowCount = body.Data?.Meta?.RowCount ?? 0,
            Duration = body.Data?.Meta?.Duration,
            Endpoint = endpoint,
        };
    }

    public async Task<ErpServiceInfo> GetParamsAsync(string endpoint, CancellationToken ct = default)
    {
        var client = CreateClient();
        var response = await client.GetAsync($"/api/gw/{endpoint}/params", ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<CaniasGatewayParamInfo>(JsonOptions, ct);

        return new ErpServiceInfo
        {
            Endpoint = endpoint,
            ServiceName = body?.ServiceName,
            Parameters = body?.Parameters ?? [],
        };
    }

    public async Task<IReadOnlyList<ErpServiceListItem>> ListServicesAsync(CancellationToken ct = default)
    {
        var client = CreateClient();

        _logger.LogInformation("CaniasGateway listing services");

        var response = await client.GetAsync("/api/services?limit=200", ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<CaniasGatewayServicesResponse>(JsonOptions, ct);

        if (body?.Data?.Services is null)
            return [];

        var result = new List<ErpServiceListItem>();
        foreach (var svc in body.Data.Services.Where(s => s.IsActive))
        {
            var parameters = ParseParameters(svc.Parameters);
            result.Add(new ErpServiceListItem
            {
                Endpoint = svc.Endpoint,
                Name = svc.Name,
                Description = svc.Description,
                Category = InferCategory(svc),
                IsActive = svc.IsActive,
                Parameters = parameters,
            });
        }

        return result;
    }

    // ── Client Factory ──────────────────────────────────────────────

    /// <summary>
    /// API Key header ile HttpClient olusturur. JWT login gereksiz.
    /// </summary>
    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("canias-gateway");
        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            client.DefaultRequestHeaders.Remove("x-api-key");
            client.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        }
        return client;
    }

    // ── Helpers ─────────────────────────────────────────────────────

    private static string BuildGatewayUrl(string endpoint, Dictionary<string, string>? parameters)
    {
        var url = $"/api/gw/{endpoint}";
        if (parameters is null || parameters.Count == 0)
            return url;

        var queryString = string.Join("&", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
        return $"{url}?{queryString}";
    }

    private static IReadOnlyList<CaniasGatewayParam> ParseParameters(string? parametersJson)
    {
        if (string.IsNullOrWhiteSpace(parametersJson))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<CaniasGatewayParam>>(parametersJson, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static string InferCategory(CaniasGatewayServiceItem svc)
    {
        var text = $"{svc.Endpoint} {svc.Name} {svc.Description}".ToLowerInvariant();

        if (text.Contains("stok") || text.Contains("stock") || text.Contains("envanter") || text.Contains("inventory"))
            return "stock";
        if (text.Contains("siparis") || text.Contains("order") || text.Contains("satis") || text.Contains("sales"))
            return "sales";
        if (text.Contains("uretim") || text.Contains("production") || text.Contains("motor") || text.Contains("planlama"))
            return "production";
        if (text.Contains("finans") || text.Contains("finance") || text.Contains("muhasebe") || text.Contains("accounting"))
            return "finance";
        if (text.Contains("log") || text.Contains("audit") || text.Contains("denetim") || text.Contains("kritik"))
            return "system";

        return "other";
    }
}
