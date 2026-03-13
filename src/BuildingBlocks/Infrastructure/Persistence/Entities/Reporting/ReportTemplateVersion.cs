using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Reporting;

/// <summary>
/// Her kaydetme isleminde yeni versiyon olusturularak rapor tasariminin
/// gecmisinin korunmasi hedeflenir.
/// </summary>
public sealed class ReportTemplateVersion : AuditableIntEntity
{
    public int ReportTemplateId { get; set; }
    public int VersionNumber { get; set; }
    public string TemplateJson { get; set; } = string.Empty;
    public string SampleInputJson { get; set; } = "{}";
    public string Notes { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
}
