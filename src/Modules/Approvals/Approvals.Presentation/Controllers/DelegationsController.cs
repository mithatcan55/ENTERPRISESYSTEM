using Approvals.Application.Commands;
using Approvals.Application.Contracts;
using Approvals.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Approvals.Presentation.Controllers;

[ApiController]
[Route("api/approvals/delegations")]
[Authorize(Roles = "SYS_ADMIN,SYS_OPERATOR")]
public sealed class DelegationsController(
    IListDelegationAssignmentsQueryHandler listDelegationAssignmentsQueryHandler,
    ICreateDelegationAssignmentCommandHandler createDelegationAssignmentCommandHandler,
    ISetDelegationAssignmentStatusCommandHandler setDelegationAssignmentStatusCommandHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<DelegationAssignmentListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<DelegationAssignmentListItemDto>>> List([FromQuery] DelegationAssignmentQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await listDelegationAssignmentsQueryHandler.HandleAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DelegationAssignmentDetailDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<DelegationAssignmentDetailDto>> Create([FromBody] CreateDelegationAssignmentRequest request, CancellationToken cancellationToken)
    {
        var result = await createDelegationAssignmentCommandHandler.HandleAsync(request, cancellationToken);
        return Created($"/api/approvals/delegations/{result.Id}", result);
    }

    [HttpPut("{delegationAssignmentId:int}/status")]
    [ProducesResponseType(typeof(DelegationAssignmentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DelegationAssignmentDetailDto>> SetStatus(
        int delegationAssignmentId,
        [FromBody] SetDelegationAssignmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await setDelegationAssignmentStatusCommandHandler.HandleAsync(delegationAssignmentId, request, cancellationToken);
        return Ok(result);
    }
}
