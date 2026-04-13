using Application.Security;
using Identity.Application.Contracts;
using Identity.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Presentation.Controllers;

[ApiController]
[Route("api/sessions")]
[Authorize]
public sealed class SessionsController(
    IAuthLifecycleService authLifecycleService,
    ICurrentUserContext currentUserContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SessionListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<SessionListItemDto>>> List(
        [FromQuery] int? userId,
        [FromQuery] bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var isPrivilegedActor = currentUserContext.IsInRole("SYS_ADMIN")
                                || currentUserContext.IsInRole("SYS_OPERATOR");

        var resolvedUserId = userId;

        if (!currentUserContext.TryGetUserId(out var claimUserId))
        {
            return BadRequest("Authenticated user claim icinde userId bulunamadi.");
        }

        if (!isPrivilegedActor)
        {
            if (resolvedUserId.HasValue && resolvedUserId.Value != claimUserId)
            {
                return Forbid();
            }

            resolvedUserId = claimUserId;
        }

        var sessions = await authLifecycleService.ListSessionsAsync(resolvedUserId, onlyActive, cancellationToken);
        return Ok(sessions);
    }

    [HttpPost("{sessionId:int}/revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke(
        int sessionId,
        [FromBody] RevokeSessionRequest request,
        CancellationToken cancellationToken)
    {
        await authLifecycleService.RevokeSessionAsync(sessionId, request.Reason, cancellationToken);
        return NoContent();
    }

    [HttpPost("revoke-bulk")]
    [ProducesResponseType(typeof(RevokeBulkSessionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RevokeBulkSessionsResponse>> RevokeBulk(
        [FromBody] RevokeBulkSessionsRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authLifecycleService.RevokeSessionsBulkAsync(request, cancellationToken);
        return Ok(response);
    }
}
