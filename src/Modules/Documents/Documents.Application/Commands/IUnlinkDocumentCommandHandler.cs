namespace Documents.Application.Commands;

public interface IUnlinkDocumentCommandHandler
{
    Task HandleAsync(int associationId, CancellationToken cancellationToken);
}
