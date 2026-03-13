using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Reports.Application.Contracts;
using Reports.Application.Queries;

namespace Reports.Infrastructure.Reports.Queries;

public sealed class ListReportTemplatesQueryHandler(ReportsDbContext reportsDbContext) : IListReportTemplatesQueryHandler
{
    public async Task<PagedResult<ReportTemplateListItemDto>> HandleAsync(ReportTemplateQueryRequest request, CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = reportsDbContext.ReportTemplates
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.Code, $"%{search}%") ||
                EF.Functions.ILike(x.Name, $"%{search}%") ||
                EF.Functions.ILike(x.Description, $"%{search}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.ModuleKey))
        {
            query = query.Where(x => x.ModuleKey == request.ModuleKey);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(x => x.Status == request.Status);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.ToListItemDto())
            .ToListAsync(cancellationToken);

        return new PagedResult<ReportTemplateListItemDto>(items, page, pageSize, totalCount);
    }
}
