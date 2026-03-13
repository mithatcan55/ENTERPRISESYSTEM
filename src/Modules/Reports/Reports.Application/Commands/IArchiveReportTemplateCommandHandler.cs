using Reports.Application.Contracts;

namespace Reports.Application.Commands;

public interface IArchiveReportTemplateCommandHandler
{
    Task<ReportTemplateDetailDto> HandleAsync(int reportTemplateId, CancellationToken cancellationToken);
}
