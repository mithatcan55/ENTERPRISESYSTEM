using Host.Api.Identity.Contracts;
using Host.Api.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth-strict")]
[Authorize]
public sealed class AuthController(IAuthLifecycleService authLifecycleService) : ControllerBase
{
    /// <summary>
    /// Kullanıcı kimlik doğrulaması yapar ve session + efektif yetki özetini döner.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await authLifecycleService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Kimliği doğrulanmış kullanıcının şifresini değiştirir.
    /// </summary>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await authLifecycleService.ChangePasswordAsync(request, cancellationToken);
        return NoContent();
    }
}
