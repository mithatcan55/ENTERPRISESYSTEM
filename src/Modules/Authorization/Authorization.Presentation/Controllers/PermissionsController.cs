using Identity.Application.Contracts;
using Identity.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.Presentation.Controllers;

[ApiController]
[Route("api/permissions/actions")]
[Authorize(Roles = "SYS_ADMIN")]
public sealed class PermissionsController(IUserPermissionService userPermissionService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserActionPermissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<UserActionPermissionDto>>> List([FromQuery] UserActionPermissionQueryRequest request, CancellationToken cancellationToken)
    {
        var permissions = await userPermissionService.ListActionPermissionsAsync(request, cancellationToken);
        return Ok(permissions);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserActionPermissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserActionPermissionDto>> Upsert([FromBody] UpsertUserActionPermissionRequest request, CancellationToken cancellationToken)
    {
        var permission = await userPermissionService.UpsertActionPermissionAsync(request, cancellationToken);
        return Ok(permission);
    }

    [HttpDelete("{permissionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int permissionId, CancellationToken cancellationToken)
    {
        await userPermissionService.DeleteActionPermissionAsync(permissionId, cancellationToken);
        return NoContent();
    }
}
