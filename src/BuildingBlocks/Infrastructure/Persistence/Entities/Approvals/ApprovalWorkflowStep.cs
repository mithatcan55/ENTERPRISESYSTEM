using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Approvals;

/// <summary>
/// Her step farkli approver tipi ile tanimlanabilir.
/// Böylece "sef", "mudur", "CFO" gibi seviyeler sabit kod degil veri olur.
/// </summary>
public sealed class ApprovalWorkflowStep : AuditableIntEntity
{
    public int ApprovalWorkflowDefinitionId { get; set; }
    public int StepOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApproverType { get; set; } = string.Empty;
    public string ApproverValue { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;
    public bool IsParallel { get; set; }
    public int MinimumApproverCount { get; set; } = 1;
}
