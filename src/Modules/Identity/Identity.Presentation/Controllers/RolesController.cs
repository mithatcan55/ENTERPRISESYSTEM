using Identity.Application.Contracts;
using Identity.Application.Roles.Commands;
using Identity.Application.Roles.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Presentation.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = "SYS_ADMIN")]
public sealed class RolesController(
    IListRolesQueryHandler listRolesQueryHandler,
    ICreateRoleCommandHandler createRoleCommandHandler,
    IAssignRoleCommandHandler assignRoleCommandHandler,
    IUnassignRoleCommandHandler unassignRoleCommandHandler,
    IListUserRolesQueryHandler listUserRolesQueryHandler,
    IDeleteRoleCommandHandler deleteRoleCommandHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RoleListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<RoleListItemDto>>> List(CancellationToken cancellationToken)
    {
        var roles = await listRolesQueryHandler.HandleAsync(cancellationToken);
        return Ok(roles);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RoleListItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RoleListItemDto>> Create([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await createRoleCommandHandler.HandleAsync(request, cancellationToken);
        return Created($"/api/roles/{role.Id}", role);
    }

    [HttpPost("{roleId:int}/assign/{userId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(int roleId, int userId, CancellationToken cancellationToken)
    {
        await assignRoleCommandHandler.HandleAsync(userId, roleId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{roleId:int}/assign/{userId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unassign(int roleId, int userId, CancellationToken cancellationToken)
    {
        await unassignRoleCommandHandler.HandleAsync(userId, roleId, cancellationToken);
        return NoContent();
    }

    [HttpGet("users/{userId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<UserRoleItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<UserRoleItemDto>>> ListUserRoles(int userId, CancellationToken cancellationToken)
    {
        var roles = await listUserRolesQueryHandler.HandleAsync(userId, cancellationToken);
        return Ok(roles);
    }

    [HttpDelete("{roleId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(int roleId, CancellationToken cancellationToken)
    {
        await deleteRoleCommandHandler.HandleAsync(roleId, cancellationToken);
        return NoContent();
    }
}
