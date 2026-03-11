using Application.Observability;
using Integrations.Application.Contracts;
using Integrations.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Integrations.Presentation.Controllers;

[ApiController]
[Route("api/ops/outbox")]
[Authorize(Roles = "SYS_ADMIN,SYS_OPERATOR")]
public sealed class OutboxController(IExternalOutboxService externalOutboxService) : ControllerBase
{
    [HttpGet("messages")]
    [OperationLog("Outbox.ListMessages", "Outbox")]
    [ProducesResponseType(typeof(OutboxPagedResult<OutboxMessageListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OutboxPagedResult<OutboxMessageListItemDto>>> ListMessages([FromQuery] OutboxMessageQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await externalOutboxService.ListMessagesAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("mail")]
    [OperationLog("Outbox.QueueMail", "Outbox")]
    [ProducesResponseType(typeof(OutboxMessageQueuedDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OutboxMessageQueuedDto>> QueueMail([FromBody] QueueMailRequest request, CancellationToken cancellationToken)
    {
        var queued = await externalOutboxService.QueueMailAsync(request, cancellationToken);
        return Accepted(queued);
    }

    [HttpPost("excel")]
    [OperationLog("Outbox.QueueExcel", "Outbox")]
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
