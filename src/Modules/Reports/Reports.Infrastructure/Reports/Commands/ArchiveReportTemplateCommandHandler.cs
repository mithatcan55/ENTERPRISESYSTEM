using Application.Exceptions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Reports.Application.Commands;
using Reports.Application.Contracts;
using Reports.Infrastructure.Reports.Queries;

namespace Reports.Infrastructure.Reports.Commands;

public sealed class ArchiveReportTemplateCommandHandler(ReportsDbContext reportsDbContext) : IArchiveReportTemplateCommandHandler
{
    public async Task<ReportTemplateDetailDto> HandleAsync(int reportTemplateId, CancellationToken cancellationToken)
    {
        var template = await reportsDbContext.ReportTemplates
            .FirstOrDefaultAsync(x => x.Id == reportTemplateId && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundAppException($"Rapor sablonu bulunamadi. Id={reportTemplateId}");

        template.Status = "Archived";
        await reportsDbContext.SaveChangesAsync(cancellationToken);

        return await new GetReportTemplateDetailQueryHandler(reportsDbContext).HandleAsync(template.Id, cancellationToken);
    }
}
