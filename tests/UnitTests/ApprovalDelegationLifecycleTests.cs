using Approvals.Application.Contracts;
using Approvals.Infrastructure.Delegations.Commands;
using Approvals.Infrastructure.Services;
using Application.Observability;
using Application.Security;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Auditing;
using Infrastructure.Persistence.Entities.Approvals;
using Microsoft.EntityFrameworkCore;

namespace UnitTests;

public sealed class ApprovalDelegationLifecycleTests
{
    [Fact]
    public async Task SetDelegationStatus_Should_Revoke_And_Reactivate_Assignment()
    {
        await using var approvalsDbContext = CreateApprovalsDbContext(nameof(SetDelegationStatus_Should_Revoke_And_Reactivate_Assignment));
        var publisher = new RecordingOperationalEventPublisher();

        approvalsDbContext.DelegationAssignments.Add(new DelegationAssignment
        {
            Id = 10,
            DelegatorUserId = 42,
            DelegateUserId = 77,
            ScopeType = "workflow",
            IncludedScopesJson = """["OT_WORKFLOW"]""",
            ExcludedScopesJson = "[]",
            StartsAt = DateTime.UtcNow.AddHours(-1),
            EndsAt = DateTime.UtcNow.AddDays(5),
            IsActive = true,
            Notes = "Izin delegasyonu"
        });

        await approvalsDbContext.SaveChangesAsync();

        var handler = new SetDelegationAssignmentStatusCommandHandler(
            approvalsDbContext,
            new FakeCurrentUserContext(42),
            publisher);

        var revoked = await handler.HandleAsync(10, new SetDelegationAssignmentStatusRequest(false, "Iptal edildi"), CancellationToken.None);
        Assert.False(revoked.IsActive);
        Assert.Equal(42, revoked.RevokedByUserId);
        Assert.NotNull(revoked.RevokedAt);
        Assert.Equal("Iptal edildi", revoked.RevokedReason);

        var reactivated = await handler.HandleAsync(10, new SetDelegationAssignmentStatusRequest(true, string.Empty), CancellationToken.None);
        Assert.True(reactivated.IsActive);
        Assert.Null(reactivated.RevokedByUserId);
        Assert.Null(reactivated.RevokedAt);
        Assert.Equal(string.Empty, reactivated.RevokedReason);

        Assert.Contains(publisher.Events, x => x.EventName == "approval.delegation.revoked");
        Assert.Contains(publisher.Events, x => x.EventName == "approval.delegation.reactivated");
    }

    [Fact]
    public async Task DeadlineProcessor_Should_SystemReject_ExpiredPendingStep()
    {
        await using var approvalsDbContext = CreateApprovalsDbContext(nameof(DeadlineProcessor_Should_SystemReject_ExpiredPendingStep));
        var publisher = new RecordingOperationalEventPublisher();

        approvalsDbContext.ApprovalWorkflowDefinitions.Add(new ApprovalWorkflowDefinition
        {
            Id = 1,
            Code = "OT_WORKFLOW",
            Name = "Overtime",
            Description = "Approval",
            ModuleKey = "Overtime",
            DocumentType = "Request"
        });

        approvalsDbContext.ApprovalWorkflowSteps.Add(new ApprovalWorkflowStep
        {
            Id = 11,
            ApprovalWorkflowDefinitionId = 1,
            StepOrder = 1,
            Name = "Manager",
            ApproverType = "specific_user",
            ApproverValue = "20",
            DecisionDeadlineHours = 1,
            TimeoutDecision = "reject"
        });

        approvalsDbContext.ApprovalInstances.Add(new ApprovalInstance
        {
            Id = 100,
            ApprovalWorkflowDefinitionId = 1,
            ReferenceType = "OvertimeRequest",
            ReferenceId = "OT-100",
            RequesterUserId = 10,
            Status = "Pending",
            CurrentStepOrder = 1,
            PayloadJson = "{}"
        });

        approvalsDbContext.ApprovalInstanceSteps.Add(new ApprovalInstanceStep
        {
            Id = 1001,
            ApprovalInstanceId = 100,
            ApprovalWorkflowStepId = 11,
            StepOrder = 1,
            AssignedUserId = 20,
            Status = "Pending",
            DueAt = DateTime.UtcNow.AddMinutes(-5)
        });

        await approvalsDbContext.SaveChangesAsync();

        var processor = new ApprovalDeadlineProcessor(approvalsDbContext, publisher);

        var processedCount = await processor.ProcessBatchAsync(CancellationToken.None);

        Assert.Equal(1, processedCount);

        var instance = await approvalsDbContext.ApprovalInstances.SingleAsync(x => x.Id == 100);
        var step = await approvalsDbContext.ApprovalInstanceSteps.SingleAsync(x => x.Id == 1001);
        var decision = await approvalsDbContext.ApprovalDecisions.SingleAsync();

        Assert.Equal("Rejected", instance.Status);
        Assert.Equal("Rejected", step.Status);
        Assert.True(decision.IsSystemDecision);
        Assert.Equal(0, decision.ActorUserId);
        Assert.Equal("reject", decision.Decision);
        Assert.Contains(publisher.Events, x => x.EventName == "approval.step.deadline.processed");
    }

    private static ApprovalsDbContext CreateApprovalsDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApprovalsDbContext>()
            .UseInMemoryDatabase($"approvals-lifecycle-{databaseName}")
            .Options;

        return new ApprovalsDbContext(options, new FakeAuditActorAccessor());
    }

    private sealed class FakeAuditActorAccessor : IAuditActorAccessor
    {
        public string GetActorId() => "test.actor";
    }

    private sealed class FakeCurrentUserContext(int userId) : ICurrentUserContext
    {
        public bool TryGetUserId(out int resolvedUserId)
        {
            resolvedUserId = userId;
            return true;
        }

        public bool TryGetActorIdentity(out string actorIdentity)
        {
            actorIdentity = $"user:{userId}";
            return true;
        }

        public bool TryGetUsername(out string username)
        {
            username = $"user{userId}";
            return true;
        }

        public bool TryGetCompanyId(out int companyId)
        {
            companyId = 1;
            return true;
        }

        public bool TryGetUserCode(out string userCode)
        {
            userCode = $"USER_{userId}";
            return true;
        }

        public bool IsInRole(string roleCode) => string.Equals(roleCode, "SYS_ADMIN", StringComparison.OrdinalIgnoreCase) && userId == 1;
    }

    private sealed class RecordingOperationalEventPublisher : IOperationalEventPublisher
    {
        public List<OperationalEvent> Events { get; } = [];

        public Task PublishAsync(OperationalEvent operationalEvent, CancellationToken cancellationToken = default)
        {
            Events.Add(operationalEvent);
            return Task.CompletedTask;
        }
    }
}
