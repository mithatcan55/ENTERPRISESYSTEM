using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Approvals;

/// <summary>
/// Yetki devri kaydi tum role/permission alanlarini tek tek kolona bozmadan
/// scope JSON alanlari ile esnek tutar.
/// </summary>
public sealed class DelegationAssignment : AuditableIntEntity
{
    public int DelegatorUserId { get; set; }
    public int DelegateUserId { get; set; }
    public string ScopeType { get; set; } = string.Empty;
    public string IncludedScopesJson { get; set; } = "[]";
    public string ExcludedScopesJson { get; set; } = "[]";
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string Notes { get; set; } = string.Empty;
}
