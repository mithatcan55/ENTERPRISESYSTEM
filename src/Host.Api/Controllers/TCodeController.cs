using Host.Api.Authorization.Services;
using Microsoft.AspNetCore.Mvc;

namespace Host.Api.Controllers;

/// <summary>
/// T-Code bazlı ekran erişimi için yetki doğrulama endpoint'i.
/// </summary>
[ApiController]
[Route("api/tcode")]
public sealed class TCodeController(ITCodeAuthorizationService authorizationService) : ControllerBase
{
    [HttpGet("{transactionCode}")]
    public async Task<IActionResult> Resolve(
        string transactionCode,
        [FromQuery] int userId,
        [FromQuery] int companyId,
        [FromQuery] decimal? amount,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(transactionCode))
        {
            return BadRequest("Transaction code boş olamaz.");
        }

        var result = await authorizationService.AuthorizeAsync(transactionCode, userId, companyId, amount, cancellationToken);
        if (!result.IsAllowed)
        {
            return StatusCode(StatusCodes.Status403Forbidden, result);
        }

        return Ok(result);
    }
}
