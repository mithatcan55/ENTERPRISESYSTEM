using Reports.Application.Contracts;

namespace Reports.Application.Commands;

public interface ICreateReportTemplateCommandHandler
{
    Task<ReportTemplateDetailDto> HandleAsync(CreateReportTemplateRequest request, CancellationToken cancellationToken);
}
