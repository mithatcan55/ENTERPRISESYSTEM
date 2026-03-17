namespace Documents.Application.Contracts;

public sealed record DocumentQueryRequest(
    string? Search,
    string? DocumentType,
    string? Status);

public sealed record ManagedDocumentListItemDto(
    int Id,
    string Code,
    string Title,
    string DocumentType,
    string Status,
    int CurrentVersionNumber,
    string? CurrentFileName,
    string? CurrentContentType);

public sealed record ManagedDocumentVersionDto(
    int Id,
    int VersionNumber,
    string FileName,
    string ContentType,
    string StoragePath,
    long FileSize,
    string? Checksum,
    string? ChangeNote,
    bool IsCurrent,
    DateTime CreatedAt,
    string? CreatedBy);

public sealed record DocumentAssociationDto(
    int Id,
    int ManagedDocumentId,
    string OwnerEntityName,
    string OwnerEntityId,
    string LinkType,
    bool IsPrimary,
    int SortOrder,
    DateTime CreatedAt,
    string? CreatedBy);

public sealed record ManagedDocumentDetailDto(
    int Id,
    string Code,
    string Title,
    string? Description,
    string DocumentType,
    string Status,
    IReadOnlyList<ManagedDocumentVersionDto> Versions,
    IReadOnlyList<DocumentAssociationDto> Associations);

public sealed record CreateManagedDocumentRequest(
    string Code,
    string Title,
    string? Description,
    string DocumentType,
    string FileName,
    string ContentType,
    string StoragePath,
    long FileSize,
    string? Checksum,
    string? ChangeNote);

public sealed record AddManagedDocumentVersionRequest(
    string FileName,
    string ContentType,
    string StoragePath,
    long FileSize,
    string? Checksum,
    string? ChangeNote);

public sealed record LinkDocumentRequest(
    string OwnerEntityName,
    string OwnerEntityId,
    string LinkType,
    bool IsPrimary,
    int SortOrder);
