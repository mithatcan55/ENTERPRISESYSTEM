using Application.Exceptions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Reports.Application.Commands;
using Reports.Application.Contracts;
using Reports.Infrastructure.Reports.Queries;

namespace Reports.Infrastructure.Reports.Commands;

public sealed class PublishReportTemplateCommandHandler(ReportsDbContext reportsDbContext) : IPublishReportTemplateCommandHandler
{
    public async Task<ReportTemplateDetailDto> HandleAsync(int reportTemplateId, CancellationToken cancellationToken)
    {
        var template = await reportsDbContext.ReportTemplates
            .FirstOrDefaultAsync(x => x.Id == reportTemplateId && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundAppException($"Rapor sablonu bulunamadi. Id={reportTemplateId}");

        var version = await reportsDbContext.ReportTemplateVersions
            .FirstOrDefaultAsync(
                x => x.ReportTemplateId == reportTemplateId && !x.IsDeleted && x.VersionNumber == template.CurrentVersionNumber,
                cancellationToken)
            ?? throw new NotFoundAppException($"Rapor sablonunun yayina alinacak aktif versiyonu bulunamadi. Id={reportTemplateId}");

        var versions = await reportsDbContext.ReportTemplateVersions
            .Where(x => x.ReportTemplateId == reportTemplateId && !x.IsDeleted && x.IsPublished)
            .ToListAsync(cancellationToken);

        foreach (var previousVersion in versions)
        {
            previousVersion.IsPublished = false;
            previousVersion.PublishedAt = null;
        }

        version.IsPublished = true;
        version.PublishedAt = DateTime.UtcNow;
        template.Status = "Published";
        template.PublishedVersionNumber = version.VersionNumber;

        await reportsDbContext.SaveChangesAsync(cancellationToken);

        return await new GetReportTemplateDetailQueryHandler(reportsDbContext).HandleAsync(template.Id, cancellationToken);
    }
}
