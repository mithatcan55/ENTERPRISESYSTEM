using Approvals.Application.Commands;
using Approvals.Application.Contracts;
using Approvals.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Approvals.Presentation.Controllers;

[ApiController]
[Route("api/approvals/workflows")]
[Authorize(Roles = "SYS_ADMIN,SYS_OPERATOR")]
public sealed class ApprovalWorkflowsController(
    IListApprovalWorkflowsQueryHandler listApprovalWorkflowsQueryHandler,
    IGetApprovalWorkflowDetailQueryHandler getApprovalWorkflowDetailQueryHandler,
    IResolveApprovalWorkflowQueryHandler resolveApprovalWorkflowQueryHandler,
    ICreateApprovalWorkflowCommandHandler createApprovalWorkflowCommandHandler,
    IUpdateApprovalWorkflowCommandHandler updateApprovalWorkflowCommandHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ApprovalWorkflowListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ApprovalWorkflowListItemDto>>> List([FromQuery] ApprovalWorkflowQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await listApprovalWorkflowsQueryHandler.HandleAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{approvalWorkflowDefinitionId:int}")]
    [ProducesResponseType(typeof(ApprovalWorkflowDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApprovalWorkflowDetailDto>> Get(int approvalWorkflowDefinitionId, CancellationToken cancellationToken)
    {
        var result = await getApprovalWorkflowDetailQueryHandler.HandleAsync(approvalWorkflowDefinitionId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("resolve")]
    [ProducesResponseType(typeof(ResolvedApprovalWorkflowDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResolvedApprovalWorkflowDto>> Resolve([FromBody] ResolveApprovalWorkflowRequest request, CancellationToken cancellationToken)
    {
        var result = await resolveApprovalWorkflowQueryHandler.HandleAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApprovalWorkflowDetailDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApprovalWorkflowDetailDto>> Create([FromBody] CreateApprovalWorkflowRequest request, CancellationToken cancellationToken)
    {
        var result = await createApprovalWorkflowCommandHandler.HandleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { approvalWorkflowDefinitionId = result.Id }, result);
    }

    [HttpPut("{approvalWorkflowDefinitionId:int}")]
    [ProducesResponseType(typeof(ApprovalWorkflowDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApprovalWorkflowDetailDto>> Update(int approvalWorkflowDefinitionId, [FromBody] UpdateApprovalWorkflowRequest request, CancellationToken cancellationToken)
    {
        var result = await updateApprovalWorkflowCommandHandler.HandleAsync(approvalWorkflowDefinitionId, request, cancellationToken);
        return Ok(result);
    }
}
