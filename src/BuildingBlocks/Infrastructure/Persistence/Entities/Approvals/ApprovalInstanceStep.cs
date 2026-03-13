using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Approvals;

/// <summary>
/// Runtime instance icindeki her step'in kime atandigini ve
/// hangi durumda oldugunu ayri satirda takip eder.
/// </summary>
public sealed class ApprovalInstanceStep : AuditableIntEntity
{
    public int ApprovalInstanceId { get; set; }
    public int ApprovalWorkflowStepId { get; set; }
    public int StepOrder { get; set; }
    public int? AssignedUserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? DueAt { get; set; }
}
