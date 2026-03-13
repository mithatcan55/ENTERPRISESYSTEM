using Reports.Application.Contracts;

namespace Reports.Application.Commands;

public interface IPublishReportTemplateCommandHandler
{
    Task<ReportTemplateDetailDto> HandleAsync(int reportTemplateId, CancellationToken cancellationToken);
}
