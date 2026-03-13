using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Approvals;

/// <summary>
/// Onay akisinin ana tanimini tutar.
/// Seviye sayisi, adimlar ve kosullar kod yerine veri ile buraya baglanir.
/// </summary>
public sealed class ApprovalWorkflowDefinition : AuditableIntEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ModuleKey { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
