using Application.Exceptions;
using Documents.Application.Commands;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Documents.Infrastructure.Documents.Commands;

public sealed class UnlinkDocumentCommandHandler(
    DocumentsDbContext documentsDbContext) : IUnlinkDocumentCommandHandler
{
    public async Task HandleAsync(int associationId, CancellationToken cancellationToken)
    {
        var association = await documentsDbContext.DocumentAssociations
            .FirstOrDefaultAsync(x => x.Id == associationId && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundAppException($"Dokuman iliskisi bulunamadi. id={associationId}");

        documentsDbContext.DocumentAssociations.Remove(association);
        await documentsDbContext.SaveChangesAsync(cancellationToken);
    }
}
