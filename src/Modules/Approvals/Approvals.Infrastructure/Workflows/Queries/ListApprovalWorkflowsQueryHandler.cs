using Approvals.Application.Contracts;
using Approvals.Application.Queries;
using global::Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Approvals.Infrastructure.Workflows.Queries;

public sealed class ListApprovalWorkflowsQueryHandler(ApprovalsDbContext dbContext) : IListApprovalWorkflowsQueryHandler
{
    public async Task<PagedResult<ApprovalWorkflowListItemDto>> HandleAsync(ApprovalWorkflowQueryRequest request, CancellationToken cancellationToken)
    {
        var query = dbContext.ApprovalWorkflowDefinitions
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(x => x.Code.Contains(request.Search) || x.Name.Contains(request.Search) || x.DocumentType.Contains(request.Search));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.ModuleKey)
            .ThenBy(x => x.DocumentType)
            .ThenBy(x => x.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => x.ToListItemDto())
            .ToListAsync(cancellationToken);

        return new PagedResult<ApprovalWorkflowListItemDto>(items, request.Page, request.PageSize, totalCount);
    }
}
