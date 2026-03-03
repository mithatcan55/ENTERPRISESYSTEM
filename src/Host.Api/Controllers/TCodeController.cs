using Host.Api.Authorization.Services;
using Host.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Host.Api.Controllers;

/// <summary>
/// T-Code bazlı ekran erişimi için yetki doğrulama endpoint'i.
/// </summary>
[ApiController]
[Route("api/tcode")]
public sealed class TCodeController(
    ITCodeAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext) : ControllerBase
{
    [HttpGet("{transactionCode}")]
    public async Task<IActionResult> Resolve(
        string transactionCode,
        [FromQuery] int? userId,
        [FromQuery] int? companyId,
        [FromQuery] decimal? amount,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(transactionCode))
        {
            return BadRequest("Transaction code boş olamaz.");
        }

        var resolvedUserId = userId;
        if (!resolvedUserId.HasValue && currentUserContext.TryGetUserId(out var claimUserId))
        {
            resolvedUserId = claimUserId;
        }

        var resolvedCompanyId = companyId;
        if (!resolvedCompanyId.HasValue && currentUserContext.TryGetCompanyId(out var claimCompanyId))
        {
            resolvedCompanyId = claimCompanyId;
        }

        if (!resolvedUserId.HasValue || !resolvedCompanyId.HasValue)
        {
            return BadRequest("userId ve companyId query ile veya claim içinde sağlanmalıdır.");
        }

        var result = await authorizationService.AuthorizeAsync(transactionCode, resolvedUserId.Value, resolvedCompanyId.Value, amount, cancellationToken);
        if (!result.IsAllowed)
        {
            return StatusCode(StatusCodes.Status403Forbidden, result);
        }

        return Ok(result);
    }
}
