using Approvals.Application.Contracts;
using Approvals.Application.Queries;
using global::Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Approvals.Infrastructure.Delegations.Queries;

public sealed class ListDelegationAssignmentsQueryHandler(ApprovalsDbContext dbContext) : IListDelegationAssignmentsQueryHandler
{
    public async Task<PagedResult<DelegationAssignmentListItemDto>> HandleAsync(DelegationAssignmentQueryRequest request, CancellationToken cancellationToken)
    {
        var query = dbContext.DelegationAssignments
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.StartsAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => x.ToListItemDto())
            .ToListAsync(cancellationToken);

        return new PagedResult<DelegationAssignmentListItemDto>(items, request.Page, request.PageSize, totalCount);
    }
}
