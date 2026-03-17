using Approvals.Application.Commands;
using Approvals.Application.Contracts;
using Approvals.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Approvals.Presentation.Controllers;

[ApiController]
[Route("api/approvals/instances")]
[Authorize]
public sealed class ApprovalInstancesController(
    IStartApprovalInstanceCommandHandler startApprovalInstanceCommandHandler,
    IGetApprovalInstanceDetailQueryHandler getApprovalInstanceDetailQueryHandler,
    IListPendingApprovalsQueryHandler listPendingApprovalsQueryHandler,
    IDecideApprovalStepCommandHandler decideApprovalStepCommandHandler) : ControllerBase
{
    [HttpGet("pending")]
    [ProducesResponseType(typeof(PagedResult<PendingApprovalListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PendingApprovalListItemDto>>> Pending([FromQuery] PendingApprovalQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await listPendingApprovalsQueryHandler.HandleAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{approvalInstanceId:int}")]
    [ProducesResponseType(typeof(ApprovalInstanceDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApprovalInstanceDetailDto>> Get(int approvalInstanceId, CancellationToken cancellationToken)
    {
        var result = await getApprovalInstanceDetailQueryHandler.HandleAsync(approvalInstanceId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("start")]
    [ProducesResponseType(typeof(ApprovalInstanceDetailDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApprovalInstanceDetailDto>> Start([FromBody] StartApprovalInstanceRequest request, CancellationToken cancellationToken)
    {
        var result = await startApprovalInstanceCommandHandler.HandleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { approvalInstanceId = result.Id }, result);
    }

    [HttpPost("steps/{approvalInstanceStepId:int}/decide")]
    [ProducesResponseType(typeof(ApprovalInstanceDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApprovalInstanceDetailDto>> Decide(int approvalInstanceStepId, [FromBody] DecideApprovalStepRequest request, CancellationToken cancellationToken)
    {
        var result = await decideApprovalStepCommandHandler.HandleAsync(approvalInstanceStepId, request, cancellationToken);
        return Ok(result);
    }
}
