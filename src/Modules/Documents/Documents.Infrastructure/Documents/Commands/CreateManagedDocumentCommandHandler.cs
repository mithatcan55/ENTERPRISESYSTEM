using Application.Exceptions;
using Documents.Application.Commands;
using Documents.Application.Contracts;
using Documents.Application.Queries;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Documents;
using Microsoft.EntityFrameworkCore;

namespace Documents.Infrastructure.Documents.Commands;

public sealed class CreateManagedDocumentCommandHandler(
    DocumentsDbContext documentsDbContext,
    IGetManagedDocumentDetailQueryHandler getManagedDocumentDetailQueryHandler) : ICreateManagedDocumentCommandHandler
{
    public async Task<ManagedDocumentDetailDto> HandleAsync(CreateManagedDocumentRequest request, CancellationToken cancellationToken)
    {
        Validate(request);

        var code = request.Code.Trim().ToUpperInvariant();
        var exists = await documentsDbContext.ManagedDocuments
            .AsNoTracking()
            .AnyAsync(x => !x.IsDeleted && x.Code == code, cancellationToken);

        if (exists)
        {
            throw new ValidationAppException(
                "Dokuman olusturma dogrulamasi basarisiz.",
                new Dictionary<string, string[]>
                {
                    ["code"] = [$"{code} kodu zaten kullaniliyor."]
                });
        }

        var document = new ManagedDocument
        {
            Code = code,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DocumentType = request.DocumentType.Trim().ToUpperInvariant(),
            Status = "ACTIVE"
        };

        documentsDbContext.ManagedDocuments.Add(document);
        await documentsDbContext.SaveChangesAsync(cancellationToken);

        documentsDbContext.ManagedDocumentVersions.Add(new ManagedDocumentVersion
        {
            ManagedDocumentId = document.Id,
            VersionNumber = 1,
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

    private static void Validate(CreateManagedDocumentRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            errors["code"] = ["Code zorunludur."];
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors["title"] = ["Title zorunludur."];
        }

        if (string.IsNullOrWhiteSpace(request.DocumentType))
        {
            errors["documentType"] = ["DocumentType zorunludur."];
        }

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
            throw new ValidationAppException("Dokuman olusturma dogrulamasi basarisiz.", errors);
        }
    }
}
