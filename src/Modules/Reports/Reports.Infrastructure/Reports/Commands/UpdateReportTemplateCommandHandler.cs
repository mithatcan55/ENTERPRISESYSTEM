using Application.Exceptions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Reporting;
using Microsoft.EntityFrameworkCore;
using Reports.Application.Commands;
using Reports.Application.Contracts;
using Reports.Infrastructure.Reports.Queries;

namespace Reports.Infrastructure.Reports.Commands;

public sealed class UpdateReportTemplateCommandHandler(ReportsDbContext reportsDbContext) : IUpdateReportTemplateCommandHandler
{
    public async Task<ReportTemplateDetailDto> HandleAsync(int reportTemplateId, UpdateReportTemplateRequest request, CancellationToken cancellationToken)
    {
        CreateReportTemplateCommandHandler.ValidateRequest("TMP", request.Name, request.ModuleKey, request.TemplateJson, request.SampleInputJson);

        var template = await reportsDbContext.ReportTemplates
            .FirstOrDefaultAsync(x => x.Id == reportTemplateId && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundAppException($"Rapor sablonu bulunamadi. Id={reportTemplateId}");

        var nextVersionNumber = template.CurrentVersionNumber + 1;

        template.Name = request.Name.Trim();
        template.Description = request.Description.Trim();
        template.ModuleKey = request.ModuleKey.Trim();
        template.Type = request.Type.Trim();
        template.Status = "Draft";
        template.CurrentVersionNumber = nextVersionNumber;

        reportsDbContext.ReportTemplateVersions.Add(new ReportTemplateVersion
        {
            ReportTemplateId = template.Id,
            VersionNumber = nextVersionNumber,
            TemplateJson = request.TemplateJson,
            SampleInputJson = request.SampleInputJson,
            Notes = request.Notes.Trim(),
            IsPublished = false
        });

        await reportsDbContext.SaveChangesAsync(cancellationToken);

        return await new GetReportTemplateDetailQueryHandler(reportsDbContext).HandleAsync(template.Id, cancellationToken);
    }
}
