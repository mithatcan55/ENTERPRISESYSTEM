using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reports.Application.Commands;
using Reports.Application.Contracts;
using Reports.Application.Queries;

namespace Reports.Presentation.Controllers;

[ApiController]
[Route("api/reports/templates")]
[Authorize(Roles = "SYS_ADMIN,SYS_OPERATOR")]
public sealed class ReportsController(
    IListReportTemplatesQueryHandler listReportTemplatesQueryHandler,
    IGetReportTemplateDetailQueryHandler getReportTemplateDetailQueryHandler,
    ICreateReportTemplateCommandHandler createReportTemplateCommandHandler,
    IUpdateReportTemplateCommandHandler updateReportTemplateCommandHandler,
    IPublishReportTemplateCommandHandler publishReportTemplateCommandHandler,
    IArchiveReportTemplateCommandHandler archiveReportTemplateCommandHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReportTemplateListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ReportTemplateListItemDto>>> List([FromQuery] ReportTemplateQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await listReportTemplatesQueryHandler.HandleAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{reportTemplateId:int}")]
    [ProducesResponseType(typeof(ReportTemplateDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReportTemplateDetailDto>> Get(int reportTemplateId, CancellationToken cancellationToken)
    {
        var result = await getReportTemplateDetailQueryHandler.HandleAsync(reportTemplateId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReportTemplateDetailDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ReportTemplateDetailDto>> Create([FromBody] CreateReportTemplateRequest request, CancellationToken cancellationToken)
    {
        var result = await createReportTemplateCommandHandler.HandleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { reportTemplateId = result.Id }, result);
    }

    [HttpPut("{reportTemplateId:int}")]
    [ProducesResponseType(typeof(ReportTemplateDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReportTemplateDetailDto>> Update(int reportTemplateId, [FromBody] UpdateReportTemplateRequest request, CancellationToken cancellationToken)
    {
        var result = await updateReportTemplateCommandHandler.HandleAsync(reportTemplateId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{reportTemplateId:int}/publish")]
    [ProducesResponseType(typeof(ReportTemplateDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReportTemplateDetailDto>> Publish(int reportTemplateId, CancellationToken cancellationToken)
    {
        var result = await publishReportTemplateCommandHandler.HandleAsync(reportTemplateId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{reportTemplateId:int}/archive")]
    [ProducesResponseType(typeof(ReportTemplateDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReportTemplateDetailDto>> Archive(int reportTemplateId, CancellationToken cancellationToken)
    {
        var result = await archiveReportTemplateCommandHandler.HandleAsync(reportTemplateId, cancellationToken);
        return Ok(result);
    }
}
