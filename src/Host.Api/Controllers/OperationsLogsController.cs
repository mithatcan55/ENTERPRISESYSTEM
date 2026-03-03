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
}
