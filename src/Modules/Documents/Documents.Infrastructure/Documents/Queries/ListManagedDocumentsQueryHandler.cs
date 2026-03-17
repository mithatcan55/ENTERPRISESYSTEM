using Documents.Application.Contracts;
using Documents.Application.Queries;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Documents.Infrastructure.Documents.Queries;

public sealed class ListManagedDocumentsQueryHandler(
    DocumentsDbContext documentsDbContext) : IListManagedDocumentsQueryHandler
{
    public async Task<IReadOnlyList<ManagedDocumentListItemDto>> HandleAsync(DocumentQueryRequest request, CancellationToken cancellationToken)
    {
        var query = documentsDbContext.ManagedDocuments
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.Code.Contains(search) || x.Title.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.DocumentType))
        {
            var documentType = request.DocumentType.Trim().ToUpperInvariant();
            query = query.Where(x => x.DocumentType == documentType);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim().ToUpperInvariant();
            query = query.Where(x => x.Status == status);
        }

        return await query
            .OrderBy(x => x.Code)
            .Select(document => new ManagedDocumentListItemDto(
                document.Id,
                document.Code,
                document.Title,
                document.DocumentType,
                document.Status,
                documentsDbContext.ManagedDocumentVersions
                    .Where(version => !version.IsDeleted && version.ManagedDocumentId == document.Id && version.IsCurrent)
                    .Select(version => version.VersionNumber)
                    .FirstOrDefault(),
                documentsDbContext.ManagedDocumentVersions
                    .Where(version => !version.IsDeleted && version.ManagedDocumentId == document.Id && version.IsCurrent)
                    .Select(version => version.FileName)
                    .FirstOrDefault(),
                documentsDbContext.ManagedDocumentVersions
                    .Where(version => !version.IsDeleted && version.ManagedDocumentId == document.Id && version.IsCurrent)
                    .Select(version => version.ContentType)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);
    }
}
