using Approvals.Application.Contracts;

namespace Approvals.Application.Services;

public interface IApprovalTriggerService
{
    Task<ApprovalTriggerResult> TriggerAsync(ApprovalTriggerRequest request, CancellationToken cancellationToken);
}
