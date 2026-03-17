using Documents.Application.Contracts;

namespace Documents.Application.Queries;

public interface IGetManagedDocumentDetailQueryHandler
{
    Task<ManagedDocumentDetailDto> HandleAsync(int documentId, CancellationToken cancellationToken);
}
