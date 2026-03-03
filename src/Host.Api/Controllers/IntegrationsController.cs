using Host.Api.Integrations.Services;
using Microsoft.AspNetCore.Mvc;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/integrations/reference")]
public sealed class IntegrationsController(IExternalDataGateway externalDataGateway) : ControllerBase
{
    [HttpGet("company/{externalId:int}")]
    public async Task<IActionResult> GetCompany(int externalId, CancellationToken cancellationToken)
    {
        var company = await externalDataGateway.GetReferenceCompanyAsync(externalId, cancellationToken);
        return Ok(company);
    }
}
