using Reports.Application.Contracts;

namespace Reports.Application.Queries;

public interface IListReportTemplatesQueryHandler
{
    Task<PagedResult<ReportTemplateListItemDto>> HandleAsync(ReportTemplateQueryRequest request, CancellationToken cancellationToken);
}
