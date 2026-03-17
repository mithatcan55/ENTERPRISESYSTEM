using Approvals.Application.Contracts;
using Approvals.Application.Queries;
using Application.Exceptions;
using Application.Security;
using global::Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Approvals.Infrastructure.Workflows.Queries;

public sealed class ListPendingApprovalsQueryHandler(
    ApprovalsDbContext approvalsDbContext,
    ICurrentUserContext currentUserContext) : IListPendingApprovalsQueryHandler
{
    public async Task<PagedResult<PendingApprovalListItemDto>> HandleAsync(PendingApprovalQueryRequest request, CancellationToken cancellationToken)
    {
        var assignedUserId = request.AssignedUserId;

        if (!assignedUserId.HasValue && currentUserContext.TryGetUserId(out var currentUserId))
        {
            assignedUserId = currentUserId;
        }

        if (!assignedUserId.HasValue)
        {
            throw new ForbiddenAppException("Pending approval listesi icin kullanici kimligi cozulmeli.");
        }

        var query =
            from step in approvalsDbContext.ApprovalInstanceSteps.AsNoTracking()
            join instance in approvalsDbContext.ApprovalInstances.AsNoTracking() on step.ApprovalInstanceId equals instance.Id
            join workflow in approvalsDbContext.ApprovalWorkflowDefinitions.AsNoTracking() on instance.ApprovalWorkflowDefinitionId equals workflow.Id
            where !step.IsDeleted && !instance.IsDeleted && !workflow.IsDeleted
            select new { step, instance, workflow };

        if (assignedUserId.HasValue)
        {
            query = query.Where(x => x.step.AssignedUserId == assignedUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(x => x.step.Status == request.Status);
        }
        else
        {
            query = query.Where(x => x.step.Status == "Pending");
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.step.DueAt ?? DateTime.MaxValue)
            .ThenByDescending(x => x.step.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new PendingApprovalListItemDto(
                x.instance.Id,
                x.step.Id,
                x.workflow.Code,
                x.instance.ReferenceType,
                x.instance.ReferenceId,
                x.step.StepOrder,
                x.step.AssignedUserId,
                x.step.Status,
                x.step.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<PendingApprovalListItemDto>(items, request.Page, request.PageSize, totalCount);
    }
}
