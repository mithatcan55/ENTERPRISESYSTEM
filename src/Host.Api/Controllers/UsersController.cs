using Host.Api.Identity.Contracts;
using Host.Api.Identity.Services;
using Host.Api.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(IUserManagementService userManagementService) : ControllerBase
{
    /// <summary>
    /// Kullanıcı listesini getirir.
    /// </summary>
    [HttpGet]
    [TCodeAuthorize("SYS03")]
    [ProducesResponseType(typeof(IReadOnlyList<UserListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<UserListItemDto>>> List(CancellationToken cancellationToken)
    {
        var users = await userManagementService.ListAsync(cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// Yeni kullanıcı oluşturur.
    /// </summary>
    [HttpPost]
    [TCodeAuthorize("SYS01")]
    [ProducesResponseType(typeof(CreatedUserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreatedUserDto>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var created = await userManagementService.CreateAsync(request, cancellationToken);
        return Created($"/api/users/{created.Id}", created);
    }
}
