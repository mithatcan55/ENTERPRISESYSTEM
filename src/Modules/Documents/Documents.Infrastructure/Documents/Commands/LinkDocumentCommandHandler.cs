using Application.Exceptions;
using Documents.Application.Commands;
using Documents.Application.Contracts;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Documents;
using Microsoft.EntityFrameworkCore;

namespace Documents.Infrastructure.Documents.Commands;

public sealed class LinkDocumentCommandHandler(
    DocumentsDbContext documentsDbContext) : ILinkDocumentCommandHandler
{
    public async Task<DocumentAssociationDto> HandleAsync(int documentId, LinkDocumentRequest request, CancellationToken cancellationToken)
    {
        Validate(request);

        var documentExists = await documentsDbContext.ManagedDocuments
            .AsNoTracking()
            .AnyAsync(x => x.Id == documentId && !x.IsDeleted, cancellationToken);

        if (!documentExists)
        {
            throw new NotFoundAppException($"Dokuman bulunamadi. id={documentId}");
        }

        var ownerEntityName = request.OwnerEntityName.Trim().ToUpperInvariant();
        var linkType = request.LinkType.Trim().ToUpperInvariant();
        var ownerEntityId = request.OwnerEntityId.Trim();

        var duplicateExists = await documentsDbContext.DocumentAssociations
            .AsNoTracking()
            .AnyAsync(
                x => !x.IsDeleted
                     && x.ManagedDocumentId == documentId
                     && x.OwnerEntityName == ownerEntityName
                     && x.OwnerEntityId == ownerEntityId
                     && x.LinkType == linkType,
                cancellationToken);

        if (duplicateExists)
        {
            throw new ValidationAppException(
                "Dokuman iliskilendirme dogrulamasi basarisiz.",
                new Dictionary<string, string[]>
                {
                    ["documentId"] = ["Ayni iliski zaten mevcut."]
                });
        }

        if (request.IsPrimary)
        {
            var primaryRecords = await documentsDbContext.DocumentAssociations
                .Where(x => !x.IsDeleted
                            && x.OwnerEntityName == ownerEntityName
                            && x.OwnerEntityId == ownerEntityId
                            && x.LinkType == linkType
                            && x.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var primaryRecord in primaryRecords)
            {
                primaryRecord.IsPrimary = false;
            }
        }

        var association = new DocumentAssociation
        {
            ManagedDocumentId = documentId,
            OwnerEntityName = ownerEntityName,
            OwnerEntityId = ownerEntityId,
            LinkType = linkType,
            IsPrimary = request.IsPrimary,
            SortOrder = request.SortOrder
        };

        documentsDbContext.DocumentAssociations.Add(association);
        await documentsDbContext.SaveChangesAsync(cancellationToken);
        return association.ToDto();
    }

    private static void Validate(LinkDocumentRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.OwnerEntityName))
        {
            errors["ownerEntityName"] = ["OwnerEntityName zorunludur."];
        }

        if (string.IsNullOrWhiteSpace(request.OwnerEntityId))
        {
            errors["ownerEntityId"] = ["OwnerEntityId zorunludur."];
        }

        if (string.IsNullOrWhiteSpace(request.LinkType))
        {
            errors["linkType"] = ["LinkType zorunludur."];
        }

        if (request.SortOrder < 0)
        {
            errors["sortOrder"] = ["SortOrder sifirdan kucuk olamaz."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationAppException("Dokuman iliskilendirme dogrulamasi basarisiz.", errors);
        }
    }
}
