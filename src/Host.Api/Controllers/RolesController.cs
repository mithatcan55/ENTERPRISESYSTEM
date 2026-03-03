using Host.Api.Identity.Contracts;
using Host.Api.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = "SYS_ADMIN")]
public sealed class RolesController(IRoleManagementService roleManagementService) : ControllerBase
{
    /// <summary>
    /// Tüm role kayıtlarını listeler.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RoleListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<RoleListItemDto>>> List(CancellationToken cancellationToken)
    {
        var roles = await roleManagementService.ListRolesAsync(cancellationToken);
        return Ok(roles);
    }

    /// <summary>
    /// Yeni bir role kaydı oluşturur.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RoleListItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RoleListItemDto>> Create([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await roleManagementService.CreateRoleAsync(request, cancellationToken);
        return Created($"/api/roles/{role.Id}", role);
    }

    /// <summary>
    /// Kullanıcıya role ataması yapar.
    /// </summary>
    [HttpPost("{roleId:int}/assign/{userId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(int roleId, int userId, CancellationToken cancellationToken)
    {
        await roleManagementService.AssignRoleAsync(userId, roleId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Belirli kullanıcıya atanmış role kayıtlarını listeler.
    /// </summary>
    [HttpGet("users/{userId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<UserRoleItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<UserRoleItemDto>>> ListUserRoles(int userId, CancellationToken cancellationToken)
    {
        var roles = await roleManagementService.ListUserRolesAsync(userId, cancellationToken);
        return Ok(roles);
    }
}
