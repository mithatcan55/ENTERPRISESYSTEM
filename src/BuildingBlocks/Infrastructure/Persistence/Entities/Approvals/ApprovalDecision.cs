using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Approvals;

/// <summary>
/// Approve / reject / return gibi kararlari audit zinciri bozulmadan saklar.
/// Sonradan WhatsApp veya mail onayi gelse bile ayni tabloya dusmesi hedeflenir.
/// </summary>
public sealed class ApprovalDecision : AuditableIntEntity
{
    public int ApprovalInstanceStepId { get; set; }
    public int ActorUserId { get; set; }
    public bool IsSystemDecision { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
}
