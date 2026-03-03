using Host.Api.Identity.Contracts;
using Host.Api.Identity.Services;
using Host.Api.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(IUserManagementService userManagementService) : ControllerBase
{
    [HttpGet]
    [TCodeAuthorize("SYS03")]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var users = await userManagementService.ListAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPost]
    [TCodeAuthorize("SYS01")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var created = await userManagementService.CreateAsync(request, cancellationToken);
        return Created($"/api/users/{created.Id}", created);
    }
}
