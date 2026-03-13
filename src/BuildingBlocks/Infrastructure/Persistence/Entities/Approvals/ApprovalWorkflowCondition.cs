using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Approvals;

/// <summary>
/// Tutar, sirket, departman veya belge tipi gibi alanlar icin
/// workflow seciminde kullanilan kosul kurallarini tutar.
/// </summary>
public sealed class ApprovalWorkflowCondition : AuditableIntEntity
{
    public int ApprovalWorkflowDefinitionId { get; set; }
    public string FieldKey { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
