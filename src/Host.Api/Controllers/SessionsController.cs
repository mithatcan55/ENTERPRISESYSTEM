using Host.Api.Identity.Contracts;
using Host.Api.Identity.Services;
using Host.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/sessions")]
public sealed class SessionsController(
    IAuthLifecycleService authLifecycleService,
    ICurrentUserContext currentUserContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
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

    [HttpPost("{sessionId:int}/revoke")]
    public async Task<IActionResult> Revoke(
        int sessionId,
        [FromBody] RevokeSessionRequest? request,
        CancellationToken cancellationToken)
    {
        await authLifecycleService.RevokeSessionAsync(sessionId, request?.Reason, cancellationToken);
        return NoContent();
    }
}
