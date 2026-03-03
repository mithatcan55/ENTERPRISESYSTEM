using Host.Api.Integrations.Services;
using Host.Api.Integrations.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/integrations/reference")]
[Authorize]
public sealed class IntegrationsController(IExternalDataGateway externalDataGateway) : ControllerBase
{
    /// <summary>
    /// Harici referans servisinden şirket bilgisini getirir.
    /// </summary>
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
