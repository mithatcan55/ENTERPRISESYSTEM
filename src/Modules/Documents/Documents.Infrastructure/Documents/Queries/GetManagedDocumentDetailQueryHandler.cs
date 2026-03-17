using Application.Exceptions;
using Documents.Application.Contracts;
using Documents.Application.Queries;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Documents.Infrastructure.Documents.Queries;

public sealed class GetManagedDocumentDetailQueryHandler(
    DocumentsDbContext documentsDbContext) : IGetManagedDocumentDetailQueryHandler
{
    public async Task<ManagedDocumentDetailDto> HandleAsync(int documentId, CancellationToken cancellationToken)
    {
        var document = await documentsDbContext.ManagedDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == documentId && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundAppException($"Dokuman bulunamadi. id={documentId}");

        var versions = await documentsDbContext.ManagedDocumentVersions
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.ManagedDocumentId == documentId)
            .OrderByDescending(x => x.VersionNumber)
            .ToListAsync(cancellationToken);

        var associations = await documentsDbContext.DocumentAssociations
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.ManagedDocumentId == documentId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return new ManagedDocumentDetailDto(
            document.Id,
            document.Code,
            document.Title,
            document.Description,
            document.DocumentType,
            document.Status,
            versions.Select(x => x.ToDto()).ToList(),
            associations.Select(x => x.ToDto()).ToList());
    }
}
