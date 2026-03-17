using Documents.Application.Commands;
using Documents.Application.Contracts;
using Documents.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Documents.Presentation.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize(Roles = "SYS_ADMIN,SYS_OPERATOR")]
public sealed class DocumentsController(
    IListManagedDocumentsQueryHandler listManagedDocumentsQueryHandler,
    IGetManagedDocumentDetailQueryHandler getManagedDocumentDetailQueryHandler,
    ICreateManagedDocumentCommandHandler createManagedDocumentCommandHandler,
    IAddManagedDocumentVersionCommandHandler addManagedDocumentVersionCommandHandler,
    ILinkDocumentCommandHandler linkDocumentCommandHandler,
    IUnlinkDocumentCommandHandler unlinkDocumentCommandHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ManagedDocumentListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ManagedDocumentListItemDto>>> List([FromQuery] DocumentQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await listManagedDocumentsQueryHandler.HandleAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{documentId:int}")]
    [ProducesResponseType(typeof(ManagedDocumentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ManagedDocumentDetailDto>> Get(int documentId, CancellationToken cancellationToken)
    {
        var result = await getManagedDocumentDetailQueryHandler.HandleAsync(documentId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ManagedDocumentDetailDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ManagedDocumentDetailDto>> Create([FromBody] CreateManagedDocumentRequest request, CancellationToken cancellationToken)
    {
        var result = await createManagedDocumentCommandHandler.HandleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { documentId = result.Id }, result);
    }

    [HttpPost("{documentId:int}/versions")]
    [ProducesResponseType(typeof(ManagedDocumentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ManagedDocumentDetailDto>> AddVersion(
        int documentId,
        [FromBody] AddManagedDocumentVersionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await addManagedDocumentVersionCommandHandler.HandleAsync(documentId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{documentId:int}/links")]
    [ProducesResponseType(typeof(DocumentAssociationDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DocumentAssociationDto>> Link(
        int documentId,
        [FromBody] LinkDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await linkDocumentCommandHandler.HandleAsync(documentId, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("links/{associationId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Unlink(int associationId, CancellationToken cancellationToken)
    {
        await unlinkDocumentCommandHandler.HandleAsync(associationId, cancellationToken);
        return NoContent();
    }
}
