using Authorization.Application.Contracts;
using Authorization.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.Presentation.Controllers;

[ApiController]
[Route("api/authorization/field-policies")]
public sealed class FieldPoliciesController(
    IAuthorizationFieldPolicyAdminService adminService,
    IAuthorizationFieldPolicyEvaluator evaluator) : ControllerBase
{
    [HttpGet("definitions")]
    [Authorize(Roles = "SYS_ADMIN")]
    [ProducesResponseType(typeof(IReadOnlyList<AuthorizationFieldDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuthorizationFieldDefinitionDto>>> ListDefinitions(
        [FromQuery] string? entityName,
        CancellationToken cancellationToken)
    {
        var items = await adminService.ListDefinitionsAsync(entityName, cancellationToken);
        return Ok(items);
    }

    [HttpPost("definitions")]
    [Authorize(Roles = "SYS_ADMIN")]
    [ProducesResponseType(typeof(AuthorizationFieldDefinitionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthorizationFieldDefinitionDto>> UpsertDefinition(
        [FromBody] UpsertAuthorizationFieldDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var item = await adminService.UpsertDefinitionAsync(request, cancellationToken);
        return Ok(item);
    }

    [HttpDelete("definitions/{definitionId:int}")]
    [Authorize(Roles = "SYS_ADMIN")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteDefinition(int definitionId, CancellationToken cancellationToken)
    {
        await adminService.DeleteDefinitionAsync(definitionId, cancellationToken);
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "SYS_ADMIN")]
    [ProducesResponseType(typeof(IReadOnlyList<AuthorizationFieldPolicyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuthorizationFieldPolicyDto>>> ListPolicies(
        [FromQuery] string? entityName,
        [FromQuery] string? fieldName,
        CancellationToken cancellationToken)
    {
        var items = await adminService.ListPoliciesAsync(entityName, fieldName, cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "SYS_ADMIN")]
    [ProducesResponseType(typeof(AuthorizationFieldPolicyDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthorizationFieldPolicyDto>> UpsertPolicy(
        [FromBody] UpsertAuthorizationFieldPolicyRequest request,
        CancellationToken cancellationToken)
    {
        var item = await adminService.UpsertPolicyAsync(request, cancellationToken);
        return Ok(item);
    }

    [HttpDelete("{policyId:int}")]
    [Authorize(Roles = "SYS_ADMIN")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeletePolicy(int policyId, CancellationToken cancellationToken)
    {
        await adminService.DeletePolicyAsync(policyId, cancellationToken);
        return NoContent();
    }

    [HttpPost("evaluate")]
    [Authorize(Roles = "SYS_ADMIN")]
    [ProducesResponseType(typeof(IReadOnlyList<AuthorizationFieldPolicyDecisionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuthorizationFieldPolicyDecisionDto>>> Evaluate(
        [FromBody] EvaluateAuthorizationFieldPolicyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await evaluator.EvaluateAsync(request, cancellationToken);
        return Ok(result);
    }
}
