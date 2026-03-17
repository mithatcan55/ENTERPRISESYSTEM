using Approvals.Application.Contracts;

namespace Approvals.Application.Queries;

public interface IGetApprovalInstanceDetailQueryHandler
{
    Task<ApprovalInstanceDetailDto> HandleAsync(int approvalInstanceId, CancellationToken cancellationToken);
}
