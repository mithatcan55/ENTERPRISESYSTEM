using Reports.Application.Contracts;

namespace Reports.Application.Commands;

public interface IUpdateReportTemplateCommandHandler
{
    Task<ReportTemplateDetailDto> HandleAsync(int reportTemplateId, UpdateReportTemplateRequest request, CancellationToken cancellationToken);
}
