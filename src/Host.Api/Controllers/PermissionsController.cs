using Host.Api.Identity.Contracts;
using Host.Api.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/permissions/actions")]
[Authorize(Roles = "SYS_ADMIN")]
public sealed class PermissionsController(IUserPermissionService userPermissionService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] UserActionPermissionQueryRequest request, CancellationToken cancellationToken)
    {
        var permissions = await userPermissionService.ListActionPermissionsAsync(request, cancellationToken);
        return Ok(permissions);
    }

    [HttpPost]
    public async Task<IActionResult> Upsert([FromBody] UpsertUserActionPermissionRequest request, CancellationToken cancellationToken)
    {
        var permission = await userPermissionService.UpsertActionPermissionAsync(request, cancellationToken);
        return Ok(permission);
    }
}
