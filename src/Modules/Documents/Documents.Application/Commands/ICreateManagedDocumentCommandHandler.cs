using Documents.Application.Contracts;

namespace Documents.Application.Commands;

public interface ICreateManagedDocumentCommandHandler
{
    Task<ManagedDocumentDetailDto> HandleAsync(CreateManagedDocumentRequest request, CancellationToken cancellationToken);
}
