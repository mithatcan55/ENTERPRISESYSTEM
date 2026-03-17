using Application.Exceptions;
using Documents.Application.Commands;
using Documents.Application.Contracts;
using Documents.Application.Queries;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Documents;
using Microsoft.EntityFrameworkCore;

namespace Documents.Infrastructure.Documents.Commands;

public sealed class AddManagedDocumentVersionCommandHandler(
    DocumentsDbContext documentsDbContext,
    IGetManagedDocumentDetailQueryHandler getManagedDocumentDetailQueryHandler) : IAddManagedDocumentVersionCommandHandler
{
    public async Task<ManagedDocumentDetailDto> HandleAsync(int documentId, AddManagedDocumentVersionRequest request, CancellationToken cancellationToken)
    {
        Validate(request);

        var document = await documentsDbContext.ManagedDocuments
            .FirstOrDefaultAsync(x => x.Id == documentId && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundAppException($"Dokuman bulunamadi. id={documentId}");

        var currentVersions = await documentsDbContext.ManagedDocumentVersions
            .Where(x => !x.IsDeleted && x.ManagedDocumentId == documentId && x.IsCurrent)
            .ToListAsync(cancellationToken);

        foreach (var currentVersion in currentVersions)
        {
            currentVersion.IsCurrent = false;
        }

        var nextVersionNumber = await documentsDbContext.ManagedDocumentVersions
            .Where(x => !x.IsDeleted && x.ManagedDocumentId == documentId)
            .Select(x => x.VersionNumber)
            .DefaultIfEmpty(0)
            .MaxAsync(cancellationToken) + 1;

        documentsDbContext.ManagedDocumentVersions.Add(new ManagedDocumentVersion
        {
            ManagedDocumentId = document.Id,
            VersionNumber = nextVersionNumber,
            FileName = request.FileName.Trim(),
            ContentType = request.ContentType.Trim(),
            StoragePath = request.StoragePath.Trim(),
            FileSize = request.FileSize,
            Checksum = string.IsNullOrWhiteSpace(request.Checksum) ? null : request.Checksum.Trim(),
            ChangeNote = string.IsNullOrWhiteSpace(request.ChangeNote) ? null : request.ChangeNote.Trim(),
            IsCurrent = true
        });

        await documentsDbContext.SaveChangesAsync(cancellationToken);
        return await getManagedDocumentDetailQueryHandler.HandleAsync(document.Id, cancellationToken);
    }

    private static void Validate(AddManagedDocumentVersionRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            errors["fileName"] = ["FileName zorunludur."];
        }

        if (string.IsNullOrWhiteSpace(request.ContentType))
        {
            errors["contentType"] = ["ContentType zorunludur."];
        }

        if (string.IsNullOrWhiteSpace(request.StoragePath))
        {
            errors["storagePath"] = ["StoragePath zorunludur."];
        }

        if (request.FileSize <= 0)
        {
            errors["fileSize"] = ["FileSize sifirdan buyuk olmalidir."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationAppException("Dokuman versiyon dogrulamasi basarisiz.", errors);
        }
    }
}
