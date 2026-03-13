using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Approvals;

/// <summary>
/// Gercek bir belge veya is kaydi icin baslayan onay surecinin runtime kaydidir.
/// PayloadJson esnek alanlari tasir; core alanlar ise kolon olarak saklanir.
/// </summary>
public sealed class ApprovalInstance : AuditableIntEntity
{
    public int ApprovalWorkflowDefinitionId { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public string ReferenceId { get; set; } = string.Empty;
    public int RequesterUserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CurrentStepOrder { get; set; }
    public string PayloadJson { get; set; } = "{}";
}
