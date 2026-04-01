namespace Integrations.Application.Contracts;

// ── Request DTOs ────────────────────────────────────────────────────

/// <summary>
/// Frontend'den backend'e gelen istek — hangi endpoint'i hangi parametrelerle calistirmak istedigini belirtir.
/// </summary>
public sealed record CaniasGatewayRunRequest
{
    public string Endpoint { get; init; } = string.Empty;
    public Dictionary<string, string>? Params { get; init; }
    public string Format { get; init; } = "json";
}

// ── Response DTOs ───────────────────────────────────────────────────

/// <summary>
/// CaniasGateway'den donen standart JSON response.
/// </summary>
public sealed record CaniasGatewayResponse<T>
{
    public bool Success { get; init; }
    public CaniasGatewayData<T>? Data { get; init; }
    public string? Message { get; init; }
}

public sealed record CaniasGatewayData<T>
{
    public IReadOnlyList<T> Rows { get; init; } = [];
    public CaniasGatewayMeta? Meta { get; init; }
}

public sealed record CaniasGatewayMeta
{
    public string? Endpoint { get; init; }
    public int RowCount { get; init; }
    public string? Duration { get; init; }
    public string? Format { get; init; }
}

/// <summary>
/// CaniasGateway servis parametre bilgisi.
/// </summary>
public sealed record CaniasGatewayParamInfo
{
    public string? Endpoint { get; init; }
    public string? ServiceName { get; init; }
    public IReadOnlyList<CaniasGatewayParam> Parameters { get; init; } = [];
}

public sealed record CaniasGatewayParam
{
    public string Name { get; init; } = string.Empty;
    public string? Label { get; init; }
    public string Type { get; init; } = "string";
    public bool Required { get; init; }
    public string? Default { get; init; }
    public IReadOnlyList<CaniasGatewayParamOption>? Options { get; init; }
}

public sealed record CaniasGatewayParamOption
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
}

// ── Proxy Response (Backend → Frontend) ─────────────────────────────

/// <summary>
/// EnterpriseSystem backend'in frontend'e döndüğü normalize response.
/// </summary>
public sealed record ErpQueryResult
{
    public IReadOnlyList<Dictionary<string, object?>> Rows { get; init; } = [];
    public int RowCount { get; init; }
    public string? Duration { get; init; }
    public string Endpoint { get; init; } = string.Empty;
}

public sealed record ErpServiceInfo
{
    public string Endpoint { get; init; } = string.Empty;
    public string? ServiceName { get; init; }
    public IReadOnlyList<CaniasGatewayParam> Parameters { get; init; } = [];
}

/// <summary>
/// Servis listesi item'i — frontend rapor katalogu icin.
/// </summary>
public sealed record ErpServiceListItem
{
    public string Endpoint { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Category { get; init; } = "other";
    public bool IsActive { get; init; }
    public IReadOnlyList<CaniasGatewayParam> Parameters { get; init; } = [];
}

/// <summary>
/// CaniasGateway /api/services response item (raw).
/// </summary>
public sealed record CaniasGatewayServiceItem
{
    public string? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Method { get; init; }
    public bool IsActive { get; init; }
    public bool IsPublic { get; init; }
    public string? Parameters { get; init; }
}

/// <summary>
/// CaniasGateway /api/services paginated response wrapper.
/// </summary>
public sealed record CaniasGatewayServicesResponse
{
    public bool Success { get; init; }
    public CaniasGatewayServicesData? Data { get; init; }
}

public sealed record CaniasGatewayServicesData
{
    public IReadOnlyList<CaniasGatewayServiceItem> Services { get; init; } = [];
    public int Total { get; init; }
}

/// <summary>
/// Excel export request — frontend'den backend'e.
/// </summary>
public sealed record ErpExcelExportRequest
{
    public string Endpoint { get; init; } = string.Empty;
    public Dictionary<string, string>? Params { get; init; }
    public string? SheetName { get; init; }
}
