using Integrations.Application.Contracts;
using Integrations.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Integrations.Presentation.Controllers;

[ApiController]
[Route("api/erp")]
[Authorize]
public sealed class ErpController(
    ICaniasGatewayClient caniasGateway,
    IExcelExporter excelExporter,
    ILogger<ErpController> logger) : ControllerBase
{
    [HttpGet("services")]
    [ProducesResponseType(typeof(IReadOnlyList<ErpServiceListItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ErpServiceListItem>>> ListServices(CancellationToken ct)
    {
        try
        {
            var services = await caniasGateway.ListServicesAsync(ct);
            return Ok(services);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "CaniasGateway connection failed while listing services");
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "ERP gateway is not available." });
        }
    }

    [HttpPost("run")]
    [ProducesResponseType(typeof(ErpQueryResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<ErpQueryResult>> Run(
        [FromBody] CaniasGatewayRunRequest request, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Endpoint))
                return BadRequest(new { message = "Endpoint is required." });

            var result = await caniasGateway.RunAsync(request.Endpoint, request.Params, ct);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "CaniasGateway connection failed for endpoint {Endpoint}", request.Endpoint);
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "ERP gateway is not available.", endpoint = request.Endpoint });
        }
    }

    [HttpPost("export-excel")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportExcel(
        [FromBody] ErpExcelExportRequest request, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Endpoint))
                return BadRequest(new { message = "Endpoint is required." });

            var result = await caniasGateway.RunAsync(request.Endpoint, request.Params, ct);

            if (result.Rows.Count == 0)
                return BadRequest(new { message = "No data to export." });

            var excelBytes = excelExporter.Export(result.Rows, request.SheetName ?? request.Endpoint);
            var fileName = $"{request.Endpoint}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "CaniasGateway connection failed for Excel export of {Endpoint}", request.Endpoint);
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "ERP gateway is not available.", endpoint = request.Endpoint });
        }
    }

    [HttpGet("params/{endpoint}")]
    [ProducesResponseType(typeof(ErpServiceInfo), StatusCodes.Status200OK)]
    public async Task<ActionResult<ErpServiceInfo>> GetParams(string endpoint, CancellationToken ct)
    {
        try
        {
            var result = await caniasGateway.GetParamsAsync(endpoint, ct);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "CaniasGateway connection failed for params of {Endpoint}", endpoint);
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "ERP gateway is not available.", endpoint });
        }
    }
}
