using Reports.Application.Contracts;

namespace Reports.Application.Queries;

public interface IGetReportTemplateDetailQueryHandler
{
    Task<ReportTemplateDetailDto> HandleAsync(int reportTemplateId, CancellationToken cancellationToken);
}
