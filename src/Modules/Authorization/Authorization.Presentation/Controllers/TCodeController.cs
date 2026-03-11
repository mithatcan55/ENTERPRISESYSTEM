using Application.Security;
using Authorization.Application.Contracts;
using Authorization.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.Presentation.Controllers;

[ApiController]
[Route("api/tcode")]
[Authorize]
public sealed class TCodeController(
    ITCodeAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext) : ControllerBase
{
    [HttpGet("{transactionCode}")]
    [ProducesResponseType(typeof(TCodeAccessResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TCodeAccessResult), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TCodeAccessResult>> Resolve(
        string transactionCode,
        [FromQuery] int? userId,
        [FromQuery] int? companyId,
        [FromQuery] string? actionCode,
        [FromQuery] decimal? amount,
        CancellationToken cancellationToken,
        [FromQuery] bool denyOnUnsatisfiedConditions = true)
    {
        if (string.IsNullOrWhiteSpace(transactionCode))
        {
            return BadRequest("Transaction code bos olamaz.");
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
            return BadRequest("userId ve companyId query ile veya claim icinde saglanmalidir.");
        }

        var reservedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "userId", "companyId", "transactionCode", "actionCode", "denyOnUnsatisfiedConditions"
        };

        var contextValues = Request.Query
            .Where(x => !reservedKeys.Contains(x.Key))
            .ToDictionary(x => x.Key, x => (string?)x.Value.ToString(), StringComparer.OrdinalIgnoreCase);

        if (amount.HasValue)
        {
            contextValues["amount"] = amount.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        var result = await authorizationService.AuthorizeAsync(
            transactionCode,
            resolvedUserId.Value,
            resolvedCompanyId.Value,
            contextValues,
            actionCode,
            denyOnUnsatisfiedConditions,
            cancellationToken);

        if (!result.IsAllowed)
        {
            return StatusCode(StatusCodes.Status403Forbidden, result);
        }

        return Ok(result);
    }
}
