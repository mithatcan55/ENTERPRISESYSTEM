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
    /// <summary>
    /// Kullanıcının action permission kayıtlarını listeler.
    /// </summary>
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

    /// <summary>
    /// Kullanıcı action permission kaydını ekler veya günceller.
    /// </summary>
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
}
