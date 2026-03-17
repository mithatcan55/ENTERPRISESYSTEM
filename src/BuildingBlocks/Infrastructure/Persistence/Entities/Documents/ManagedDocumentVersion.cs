using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Documents;

/// <summary>
/// Ayni dokumanin fiziksel dosya versiyonlarini tutar.
/// Her dokuman icin tek bir current version aktif olur.
/// </summary>
public sealed class ManagedDocumentVersion : AuditableIntEntity
{
    public int ManagedDocumentId { get; set; }
    public int VersionNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Checksum { get; set; }
    public string? ChangeNote { get; set; }
    public bool IsCurrent { get; set; }
}
