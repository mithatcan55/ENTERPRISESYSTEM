using Application.Pipeline;
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
    IRequestExecutionPipeline requestExecutionPipeline,
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
        // Roles akisi T-Code yerine dogrudan role korumasi ile aciliyor.
        // Bunun nedeni bu endpoint'lerin sistem yonetimi niteliginde olmasi.
        var roles = await requestExecutionPipeline.ExecuteQueryAsync(
            new ListRolesQuery(),
            _ => listRolesQueryHandler.HandleAsync(cancellationToken),
            cancellationToken,
            "Roles.List");
        return Ok(roles);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RoleListItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RoleListItemDto>> Create([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await requestExecutionPipeline.ExecuteCommandAsync(
            new CreateRoleCommand(request),
            _ => createRoleCommandHandler.HandleAsync(request, cancellationToken),
            cancellationToken,
            "Roles.Create");
        return Created($"/api/roles/{role.Id}", role);
    }

    [HttpPost("{roleId:int}/assign/{userId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(int roleId, int userId, CancellationToken cancellationToken)
    {
        // Route parametreleri command modeline acikca tasiniyor.
        // Boylece command kendi niyetini controller imzasindan bagimsiz sekilde tasir.
        await requestExecutionPipeline.ExecuteCommandAsync(
            new AssignRoleCommand(userId, roleId),
            _ => assignRoleCommandHandler.HandleAsync(userId, roleId, cancellationToken),
            cancellationToken,
            "Roles.Assign");
        return NoContent();
    }

    [HttpDelete("{roleId:int}/assign/{userId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unassign(int roleId, int userId, CancellationToken cancellationToken)
    {
        await requestExecutionPipeline.ExecuteCommandAsync(
            new UnassignRoleCommand(userId, roleId),
            _ => unassignRoleCommandHandler.HandleAsync(userId, roleId, cancellationToken),
            cancellationToken,
            "Roles.Unassign");
        return NoContent();
    }

    [HttpGet("users/{userId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<UserRoleItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<UserRoleItemDto>>> ListUserRoles(int userId, CancellationToken cancellationToken)
    {
        var roles = await requestExecutionPipeline.ExecuteQueryAsync(
            new ListUserRolesQuery(userId),
            _ => listUserRolesQueryHandler.HandleAsync(userId, cancellationToken),
            cancellationToken,
            "Roles.ListUserRoles");
        return Ok(roles);
    }

    [HttpDelete("{roleId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(int roleId, CancellationToken cancellationToken)
    {
        await requestExecutionPipeline.ExecuteCommandAsync(
            new DeleteRoleCommand(roleId),
            _ => deleteRoleCommandHandler.HandleAsync(roleId, cancellationToken),
            cancellationToken,
            "Roles.Delete");
        return NoContent();
    }
}
