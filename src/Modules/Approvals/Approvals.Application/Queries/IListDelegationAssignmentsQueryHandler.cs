using Approvals.Application.Contracts;

namespace Approvals.Application.Queries;

public interface IListDelegationAssignmentsQueryHandler
{
    Task<PagedResult<DelegationAssignmentListItemDto>> HandleAsync(DelegationAssignmentQueryRequest request, CancellationToken cancellationToken);
}
