using Application.Exceptions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Reports.Application.Contracts;
using Reports.Application.Queries;

namespace Reports.Infrastructure.Reports.Queries;

public sealed class GetReportTemplateDetailQueryHandler(ReportsDbContext reportsDbContext) : IGetReportTemplateDetailQueryHandler
{
    public async Task<ReportTemplateDetailDto> HandleAsync(int reportTemplateId, CancellationToken cancellationToken)
    {
        var template = await reportsDbContext.ReportTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == reportTemplateId && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundAppException($"Rapor sablonu bulunamadi. Id={reportTemplateId}");

        var versions = await reportsDbContext.ReportTemplateVersions
            .AsNoTracking()
            .Where(x => x.ReportTemplateId == reportTemplateId && !x.IsDeleted)
            .OrderByDescending(x => x.VersionNumber)
            .Select(x => new ReportTemplateVersionDto(
                x.Id,
                x.VersionNumber,
                x.IsPublished,
                x.PublishedAt,
                x.CreatedAt,
                x.Notes))
            .ToListAsync(cancellationToken);

        var currentVersion = await reportsDbContext.ReportTemplateVersions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ReportTemplateId == reportTemplateId && !x.IsDeleted && x.VersionNumber == template.CurrentVersionNumber,
                cancellationToken)
            ?? throw new NotFoundAppException($"Rapor sablonunun aktif versiyonu bulunamadi. TemplateId={reportTemplateId}");

        return new ReportTemplateDetailDto(
            template.Id,
            template.Code,
            template.Name,
            template.Description,
            template.ModuleKey,
            template.Type,
            template.Status,
            template.CurrentVersionNumber,
            template.PublishedVersionNumber,
            currentVersion.TemplateJson,
            currentVersion.SampleInputJson,
            versions);
    }
}
