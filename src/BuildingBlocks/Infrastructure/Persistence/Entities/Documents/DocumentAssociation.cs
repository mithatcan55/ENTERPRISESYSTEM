using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Documents;

/// <summary>
/// Dokumanlari farkli modullere generic olarak baglar.
/// Boylece ayni PDF hem dokuman yonetiminde hem material gibi modullerde kullanilabilir.
/// </summary>
public sealed class DocumentAssociation : AuditableIntEntity
{
    public int ManagedDocumentId { get; set; }
    public string OwnerEntityName { get; set; } = string.Empty;
    public string OwnerEntityId { get; set; } = string.Empty;
    public string LinkType { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}
