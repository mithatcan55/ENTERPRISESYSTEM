using Documents.Application.Contracts;

namespace Documents.Application.Commands;

public interface IAddManagedDocumentVersionCommandHandler
{
    Task<ManagedDocumentDetailDto> HandleAsync(int documentId, AddManagedDocumentVersionRequest request, CancellationToken cancellationToken);
}
