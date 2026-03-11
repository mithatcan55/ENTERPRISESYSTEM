using Authorization.Application.Security;
using Identity.Application.Contracts;
using Identity.Application.Users.Commands;
using Identity.Application.Users.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Presentation.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(
    IListUsersQueryHandler listUsersQueryHandler,
    ICreateUserCommandHandler createUserCommandHandler,
    IUpdateUserCommandHandler updateUserCommandHandler,
    IDeactivateUserCommandHandler deactivateUserCommandHandler,
    IReactivateUserCommandHandler reactivateUserCommandHandler,
    IDeleteUserCommandHandler deleteUserCommandHandler) : ControllerBase
{
    [HttpGet]
    [TCodeAuthorize("SYS03", "READ")]
    [ProducesResponseType(typeof(IReadOnlyList<UserListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<UserListItemDto>>> List(CancellationToken cancellationToken)
    {
        var users = await listUsersQueryHandler.HandleAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPost]
    [TCodeAuthorize("SYS01", "CREATE")]
    [ProducesResponseType(typeof(CreatedUserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreatedUserDto>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var created = await createUserCommandHandler.HandleAsync(request, cancellationToken);
        return Created($"/api/users/{created.Id}", created);
    }

    [HttpPut("{userId:int}")]
    [TCodeAuthorize("SYS01", "UPDATE")]
    [ProducesResponseType(typeof(UserListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserListItemDto>> Update(int userId, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var updated = await updateUserCommandHandler.HandleAsync(userId, request, cancellationToken);
        return Ok(updated);
    }

    [HttpPost("{userId:int}/deactivate")]
    [TCodeAuthorize("SYS01", "DEACTIVATE")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int userId, CancellationToken cancellationToken)
    {
        await deactivateUserCommandHandler.HandleAsync(userId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{userId:int}/reactivate")]
    [TCodeAuthorize("SYS01", "REACTIVATE")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivate(int userId, CancellationToken cancellationToken)
    {
        await reactivateUserCommandHandler.HandleAsync(userId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{userId:int}")]
    [TCodeAuthorize("SYS01", "DELETE")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int userId, CancellationToken cancellationToken)
    {
        await deleteUserCommandHandler.HandleAsync(userId, cancellationToken);
        return NoContent();
    }
}
