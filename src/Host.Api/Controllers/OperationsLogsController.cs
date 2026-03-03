using Host.Api.Operations.Contracts;
using Host.Api.Operations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/ops/logs")]
[Authorize(Roles = "SYS_ADMIN,SYS_OPERATOR")]
public sealed class OperationsLogsController(IOperationsLogQueryService operationsLogQueryService) : ControllerBase
{
    /// <summary>
    /// Sistem loglarını filtreli ve sayfalı olarak listeler.
    /// </summary>
    [HttpGet("system")]
    [ProducesResponseType(typeof(PagedResult<SystemLogListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<SystemLogListItemDto>>> SystemLogs([FromQuery] LogQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await operationsLogQueryService.QuerySystemLogsAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Güvenlik olay loglarını filtreli ve sayfalı olarak listeler.
    /// </summary>
    [HttpGet("security")]
    [ProducesResponseType(typeof(PagedResult<SecurityEventListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<SecurityEventListItemDto>>> SecurityLogs([FromQuery] LogQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await operationsLogQueryService.QuerySecurityEventsAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// HTTP istek loglarını filtreli ve sayfalı olarak listeler.
    /// </summary>
    [HttpGet("http")]
    [ProducesResponseType(typeof(PagedResult<HttpRequestLogListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<HttpRequestLogListItemDto>>> HttpLogs([FromQuery] LogQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await operationsLogQueryService.QueryHttpRequestLogsAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Entity değişim loglarını filtreli ve sayfalı olarak listeler.
    /// </summary>
    [HttpGet("entity-changes")]
    [ProducesResponseType(typeof(PagedResult<EntityChangeLogListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<EntityChangeLogListItemDto>>> EntityChanges([FromQuery] LogQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await operationsLogQueryService.QueryEntityChangeLogsAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Entity değişim loglarını CSV formatında dışa aktarır.
    /// </summary>
    [HttpGet("entity-changes/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportEntityChanges([FromQuery] LogQueryRequest request, CancellationToken cancellationToken)
    {
        var csv = await operationsLogQueryService.ExportEntityChangeLogsCsvAsync(request, cancellationToken);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "entity-change-logs.csv");
    }

    /// <summary>
    /// Session kayıtlarını admin perspektifinde filtreli ve sayfalı listeler.
    /// </summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(PagedResult<SessionAdminListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<SessionAdminListItemDto>>> Sessions([FromQuery] SessionAdminQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await operationsLogQueryService.QuerySessionsAdminAsync(request, cancellationToken);
        return Ok(result);
    }
}
