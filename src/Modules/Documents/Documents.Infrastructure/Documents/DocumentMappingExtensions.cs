using Documents.Application.Contracts;
using Infrastructure.Persistence.Entities.Documents;

namespace Documents.Infrastructure.Documents;

internal static class DocumentMappingExtensions
{
    public static ManagedDocumentVersionDto ToDto(this ManagedDocumentVersion version)
        => new(
            version.Id,
            version.VersionNumber,
            version.FileName,
            version.ContentType,
            version.StoragePath,
            version.FileSize,
            version.Checksum,
            version.ChangeNote,
            version.IsCurrent,
            version.CreatedAt,
            version.CreatedBy);

    public static DocumentAssociationDto ToDto(this DocumentAssociation association)
        => new(
            association.Id,
            association.ManagedDocumentId,
            association.OwnerEntityName,
            association.OwnerEntityId,
            association.LinkType,
            association.IsPrimary,
            association.SortOrder,
            association.CreatedAt,
            association.CreatedBy);
}
