using Host.Api.Operations.Contracts;
using Host.Api.Operations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/ops/audit/dashboard")]
[Authorize(Roles = "SYS_ADMIN,SYS_OPERATOR")]
public sealed class AuditDashboardController(IAuditDashboardService auditDashboardService) : ControllerBase
{
    /// <summary>
    /// Operasyonel denetim dashboard'u için özet KPI verilerini döner.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(AuditDashboardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuditDashboardSummaryDto>> Summary([FromQuery] int windowHours = 24, CancellationToken cancellationToken = default)
    {
        var result = await auditDashboardService.GetSummaryAsync(windowHours, cancellationToken);
        return Ok(result);
    }
}
