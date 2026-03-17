using Documents.Application.Contracts;

namespace Documents.Application.Commands;

public interface ILinkDocumentCommandHandler
{
    Task<DocumentAssociationDto> HandleAsync(int documentId, LinkDocumentRequest request, CancellationToken cancellationToken);
}
