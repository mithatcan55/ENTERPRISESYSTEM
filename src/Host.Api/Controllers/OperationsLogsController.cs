using Host.Api.Operations.Contracts;
using Host.Api.Operations.Services;
using Microsoft.AspNetCore.Mvc;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/ops/logs")]
public sealed class OperationsLogsController(IOperationsLogQueryService operationsLogQueryService) : ControllerBase
{
    [HttpGet("system")]
    public async Task<IActionResult> SystemLogs([FromQuery] LogQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await operationsLogQueryService.QuerySystemLogsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("security")]
    public async Task<IActionResult> SecurityLogs([FromQuery] LogQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await operationsLogQueryService.QuerySecurityEventsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("http")]
    public async Task<IActionResult> HttpLogs([FromQuery] LogQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await operationsLogQueryService.QueryHttpRequestLogsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("entity-changes")]
    public async Task<IActionResult> EntityChanges([FromQuery] LogQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await operationsLogQueryService.QueryEntityChangeLogsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("entity-changes/export")]
    public async Task<IActionResult> ExportEntityChanges([FromQuery] LogQueryRequest request, CancellationToken cancellationToken)
    {
        var csv = await operationsLogQueryService.ExportEntityChangeLogsCsvAsync(request, cancellationToken);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "entity-change-logs.csv");
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> Sessions([FromQuery] SessionAdminQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await operationsLogQueryService.QuerySessionsAdminAsync(request, cancellationToken);
        return Ok(result);
    }
}
