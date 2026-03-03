using Host.Api.Identity.Contracts;
using Host.Api.Identity.Services;
using Host.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/sessions")]
[Authorize]
public sealed class SessionsController(
    IAuthLifecycleService authLifecycleService,
    ICurrentUserContext currentUserContext) : ControllerBase
{
    /// <summary>
    /// Kullanıcının aktif veya tüm session kayıtlarını listeler.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SessionListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<SessionListItemDto>>> List(
        [FromQuery] int? userId,
        [FromQuery] bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var resolvedUserId = userId;
        if (!resolvedUserId.HasValue && currentUserContext.TryGetUserId(out var claimUserId))
        {
            resolvedUserId = claimUserId;
        }

        if (!resolvedUserId.HasValue)
        {
            return BadRequest("userId query ile veya claim içinde sağlanmalıdır.");
        }

        var sessions = await authLifecycleService.ListSessionsAsync(resolvedUserId.Value, onlyActive, cancellationToken);
        return Ok(sessions);
    }

    /// <summary>
    /// Belirli bir session kaydını revoke eder.
    /// </summary>
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
        await authLifecycleService.RevokeSessionAsync(sessionId, request?.Reason, cancellationToken);
        return NoContent();
    }
}
