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
        // Query parametresi verilmezse claim fallback devreye girer.
        // Bu sayede kullanici genelde kendi session'larini ekstra bilgi vermeden gorebilir.
        var resolvedUserId = userId;
        if (!resolvedUserId.HasValue && currentUserContext.TryGetUserId(out var claimUserId))
        {
            resolvedUserId = claimUserId;
        }

        if (!resolvedUserId.HasValue)
        {
            return BadRequest("userId query ile veya claim icinde saglanmalidir.");
        }

        var sessions = await authLifecycleService.ListSessionsAsync(resolvedUserId.Value, onlyActive, cancellationToken);
        return Ok(sessions);
    }

    [HttpPost("{sessionId:int}/revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke(
        int sessionId,
        [FromBody] RevokeSessionRequest? request,
        CancellationToken cancellationToken)
    {
        // Revoke gerekcesi opsiyoneldir ama denetim izi icin degerlidir.
        await authLifecycleService.RevokeSessionAsync(sessionId, request?.Reason, cancellationToken);
        return NoContent();
    }
}
