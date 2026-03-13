using Approvals.Application.Contracts;

namespace Approvals.Application.Queries;

public interface IListApprovalWorkflowsQueryHandler
{
    Task<PagedResult<ApprovalWorkflowListItemDto>> HandleAsync(ApprovalWorkflowQueryRequest request, CancellationToken cancellationToken);
}
