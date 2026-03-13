namespace Reports.Application.Contracts;

public sealed record ReportTemplateListItemDto(
    int Id,
    string Code,
    string Name,
    string Description,
    string ModuleKey,
    string Type,
    string Status,
    int CurrentVersionNumber,
    int? PublishedVersionNumber,
    DateTime UpdatedAt);

public sealed record ReportTemplateVersionDto(
    int Id,
    int VersionNumber,
    bool IsPublished,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    string Notes);

public sealed record ReportTemplateDetailDto(
    int Id,
    string Code,
    string Name,
    string Description,
    string ModuleKey,
    string Type,
    string Status,
    int CurrentVersionNumber,
    int? PublishedVersionNumber,
    string TemplateJson,
    string SampleInputJson,
    IReadOnlyList<ReportTemplateVersionDto> Versions);

public sealed class ReportTemplateQueryRequest
{
    public string? Search { get; set; }
    public string? ModuleKey { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class CreateReportTemplateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ModuleKey { get; set; } = string.Empty;
    public string Type { get; set; } = "document";
    public string TemplateJson { get; set; } = string.Empty;
    public string SampleInputJson { get; set; } = "{}";
    public string Notes { get; set; } = string.Empty;
}

public sealed class UpdateReportTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ModuleKey { get; set; } = string.Empty;
    public string Type { get; set; } = "document";
    public string TemplateJson { get; set; } = string.Empty;
    public string SampleInputJson { get; set; } = "{}";
    public string Notes { get; set; } = string.Empty;
}
