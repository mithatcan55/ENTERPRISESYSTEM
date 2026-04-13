using Application.Observability;
using Identity.Application.Contracts;
using Identity.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Identity.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth-strict")]
[Authorize]
public sealed class AuthController(IAuthLifecycleService authLifecycleService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [OperationLog("Auth.Login", "Authentication")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        // Login anonim aciktir ama yine de operation log zincirine dahildir.
        // Bu sayede basarili ve basarisiz giris denemeleri izlenebilir olur.
        var response = await authLifecycleService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [OperationLog("Auth.RefreshToken", "Authentication")]
    [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RefreshTokenResponseDto>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        // Refresh akisinda da yeni token verilse bile session ve kullanici durumu tekrar dogrulanir.
        var response = await authLifecycleService.RefreshAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("logout")]
    [OperationLog("Auth.Logout", "Authentication")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] RevokeSessionRequest request, CancellationToken cancellationToken)
    {
        await authLifecycleService.LogoutAsync(request.Reason, cancellationToken);
        return NoContent();
    }

    [HttpPost("change-password")]
    [OperationLog("Auth.ChangePassword", "Authentication")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        // Change-password endpoint'i kimligi dogrulanmis kullanici bekler.
        // Gercek "kendi hesabini degistirme" kurali servis katmaninda enforce edilir.
        await authLifecycleService.ChangePasswordAsync(request, cancellationToken);
        return NoContent();
    }
}
