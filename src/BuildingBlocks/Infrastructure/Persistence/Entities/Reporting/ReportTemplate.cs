using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Reporting;

/// <summary>
/// Raporun kimliğini, durumunu ve aktif versiyon bilgisini tutar.
/// Template'in govdesi ayrica version tablosunda saklanir.
/// </summary>
public sealed class ReportTemplate : AuditableIntEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ModuleKey { get; set; } = string.Empty;
    public string Type { get; set; } = "document";
    public string Status { get; set; } = "Draft";
    public int CurrentVersionNumber { get; set; } = 1;
    public int? PublishedVersionNumber { get; set; }
}
