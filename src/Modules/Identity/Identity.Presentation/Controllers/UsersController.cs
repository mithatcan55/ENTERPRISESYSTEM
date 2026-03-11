using Authorization.Application.Security;
using Application.Pipeline;
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
    IRequestExecutionPipeline requestExecutionPipeline,
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
        // Controller burada dogrudan query handler cagirmaz; once pipeline'a girer.
        // Boylece validation, pre-check ve event uretimi ortak standarda baglanmis olur.
        var users = await requestExecutionPipeline.ExecuteQueryAsync(
            new ListUsersQuery(),
            _ => listUsersQueryHandler.HandleAsync(cancellationToken),
            cancellationToken,
            "Users.List");
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
        // HTTP body'den gelen request ile pipeline request modeli ayni sey degil.
        // Presentation katmani API contract'ini alir, sonra bunu command modeline sarar.
        var created = await requestExecutionPipeline.ExecuteCommandAsync(
            new CreateUserCommand(request),
            _ => createUserCommandHandler.HandleAsync(request, cancellationToken),
            cancellationToken,
            "Users.Create");
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
        var updated = await requestExecutionPipeline.ExecuteCommandAsync(
            new UpdateUserCommand(userId, request),
            _ => updateUserCommandHandler.HandleAsync(userId, request, cancellationToken),
            cancellationToken,
            "Users.Update");
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
        // Response body'si olmayan mutating endpoint'lerde 204 secimi operasyonun tamamlandigini ama yeni temsil donmedigini anlatir.
        await requestExecutionPipeline.ExecuteCommandAsync(
            new DeactivateUserCommand(userId),
            _ => deactivateUserCommandHandler.HandleAsync(userId, cancellationToken),
            cancellationToken,
            "Users.Deactivate");
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
        await requestExecutionPipeline.ExecuteCommandAsync(
            new ReactivateUserCommand(userId),
            _ => reactivateUserCommandHandler.HandleAsync(userId, cancellationToken),
            cancellationToken,
            "Users.Reactivate");
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
        await requestExecutionPipeline.ExecuteCommandAsync(
            new DeleteUserCommand(userId),
            _ => deleteUserCommandHandler.HandleAsync(userId, cancellationToken),
            cancellationToken,
            "Users.Delete");
        return NoContent();
    }
}
