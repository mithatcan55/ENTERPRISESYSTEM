using Application.Pipeline;
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
    IRequestExecutionPipeline requestExecutionPipeline,
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
        var permissions = await requestExecutionPipeline.ExecuteQueryAsync(
            new ListUserActionPermissionsQuery(request),
            _ => listUserActionPermissionsQueryHandler.HandleAsync(request, cancellationToken),
            cancellationToken,
            "Permissions.ListActionPermissions");
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
        var permission = await requestExecutionPipeline.ExecuteCommandAsync(
            new UpsertUserActionPermissionCommand(request),
            _ => upsertUserActionPermissionCommandHandler.HandleAsync(request, cancellationToken),
            cancellationToken,
            "Permissions.UpsertActionPermission");
        return Ok(permission);
    }

    [HttpDelete("{permissionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int permissionId, CancellationToken cancellationToken)
    {
        await requestExecutionPipeline.ExecuteCommandAsync(
            new DeleteUserActionPermissionCommand(permissionId),
            _ => deleteUserActionPermissionCommandHandler.HandleAsync(permissionId, cancellationToken),
            cancellationToken,
            "Permissions.DeleteActionPermission");
        return NoContent();
    }
}
