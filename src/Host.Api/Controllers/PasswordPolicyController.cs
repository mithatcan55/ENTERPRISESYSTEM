using Host.Api.Identity.Configuration;
using Host.Api.Identity.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/ops/security/password-policy")]
[Authorize(Roles = "SYS_ADMIN")]
public sealed class PasswordPolicyController(IOptions<PasswordPolicyOptions> passwordPolicyOptions) : ControllerBase
{
    /// <summary>
    /// Aktif password policy konfigürasyonunu getirir.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PasswordPolicySnapshotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public ActionResult<PasswordPolicySnapshotDto> Get()
    {
        var options = passwordPolicyOptions.Value;

        var snapshot = new PasswordPolicySnapshotDto(
            options.MinLength,
            options.RequireUppercase,
            options.RequireLowercase,
            options.RequireDigit,
            options.RequireSpecialCharacter,
            options.HistoryCount,
            options.MinimumPasswordAgeMinutes);

        return Ok(snapshot);
    }
}
