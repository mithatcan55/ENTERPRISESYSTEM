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
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await authLifecycleService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await authLifecycleService.ChangePasswordAsync(request, cancellationToken);
        return NoContent();
    }
}
