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
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var roles = await roleManagementService.ListRolesAsync(cancellationToken);
        return Ok(roles);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await roleManagementService.CreateRoleAsync(request, cancellationToken);
        return Created($"/api/roles/{role.Id}", role);
    }

    [HttpPost("{roleId:int}/assign/{userId:int}")]
    public async Task<IActionResult> Assign(int roleId, int userId, CancellationToken cancellationToken)
    {
        await roleManagementService.AssignRoleAsync(userId, roleId, cancellationToken);
        return NoContent();
    }

    [HttpGet("users/{userId:int}")]
    public async Task<IActionResult> ListUserRoles(int userId, CancellationToken cancellationToken)
    {
        var roles = await roleManagementService.ListUserRolesAsync(userId, cancellationToken);
        return Ok(roles);
    }
}
