using Documents.Application.Contracts;

namespace Documents.Application.Queries;

public interface IListManagedDocumentsQueryHandler
{
    Task<IReadOnlyList<ManagedDocumentListItemDto>> HandleAsync(DocumentQueryRequest request, CancellationToken cancellationToken);
}
