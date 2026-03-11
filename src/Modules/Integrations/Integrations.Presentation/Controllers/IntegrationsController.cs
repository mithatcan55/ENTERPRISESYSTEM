using Integrations.Application.Contracts;
using Integrations.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Integrations.Presentation.Controllers;

[ApiController]
[Route("api/integrations/reference")]
[Authorize]
public sealed class IntegrationsController(IExternalDataGateway externalDataGateway) : ControllerBase
{
    [HttpGet("company/{externalId:int}")]
    [ProducesResponseType(typeof(ReferenceCompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<ReferenceCompanyDto>> GetCompany(int externalId, CancellationToken cancellationToken)
    {
        var company = await externalDataGateway.GetReferenceCompanyAsync(externalId, cancellationToken);
        return Ok(company);
    }
}
