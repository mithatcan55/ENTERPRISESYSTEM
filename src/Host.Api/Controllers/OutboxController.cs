using Host.Api.Integrations.Contracts;
using Host.Api.Integrations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/ops/outbox")]
[Authorize(Roles = "SYS_ADMIN,SYS_OPERATOR")]
public sealed class OutboxController(IExternalOutboxService externalOutboxService) : ControllerBase
{
    /// <summary>
    /// Mail gönderim işini outbox kuyruğuna alır.
    /// </summary>
    [HttpPost("mail")]
    [ProducesResponseType(typeof(OutboxMessageQueuedDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OutboxMessageQueuedDto>> QueueMail([FromBody] QueueMailRequest request, CancellationToken cancellationToken)
    {
        var queued = await externalOutboxService.QueueMailAsync(request, cancellationToken);
        return Accepted(queued);
    }

    /// <summary>
    /// Excel/CSV rapor üretim işini outbox kuyruğuna alır.
    /// </summary>
    [HttpPost("excel")]
    [ProducesResponseType(typeof(OutboxMessageQueuedDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OutboxMessageQueuedDto>> QueueExcel([FromBody] QueueExcelReportRequest request, CancellationToken cancellationToken)
    {
        var queued = await externalOutboxService.QueueExcelReportAsync(request, cancellationToken);
        return Accepted(queued);
    }
}
