using Approvals.Application.Contracts;

namespace Approvals.Application.Queries;

public interface IListPendingApprovalsQueryHandler
{
    Task<PagedResult<PendingApprovalListItemDto>> HandleAsync(PendingApprovalQueryRequest request, CancellationToken cancellationToken);
}
