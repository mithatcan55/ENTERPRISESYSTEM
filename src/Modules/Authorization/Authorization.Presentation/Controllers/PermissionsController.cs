using Identity.Application.Contracts;
using Identity.Application.Permissions.Commands;
using Identity.Application.Permissions.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.Presentation.Controllers;

[ApiController]
[Route("api/permissions/actions")]
[Authorize(Roles = "SYS_ADMIN")]
public sealed class PermissionsController(
    IListUserActionPermissionsQueryHandler listUserActionPermissionsQueryHandler,
    IUpsertUserActionPermissionCommandHandler upsertUserActionPermissionCommandHandler,
    IDeleteUserActionPermissionCommandHandler deleteUserActionPermissionCommandHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserActionPermissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<UserActionPermissionDto>>> List([FromQuery] UserActionPermissionQueryRequest request, CancellationToken cancellationToken)
    {
        var permissions = await listUserActionPermissionsQueryHandler.HandleAsync(request, cancellationToken);
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
        var permission = await upsertUserActionPermissionCommandHandler.HandleAsync(request, cancellationToken);
        return Ok(permission);
    }

    [HttpDelete("{permissionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int permissionId, CancellationToken cancellationToken)
    {
        await deleteUserActionPermissionCommandHandler.HandleAsync(permissionId, cancellationToken);
        return NoContent();
    }
}
