using Approvals.Application.Contracts;
using Approvals.Infrastructure.Services;
using Approvals.Infrastructure.Workflows;
using Approvals.Infrastructure.Workflows.Commands;
using Approvals.Infrastructure.Workflows.Queries;
using Application.Security;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Auditing;
using Infrastructure.Persistence.Entities.Approvals;
using Infrastructure.Persistence.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace UnitTests;

public sealed class ApprovalTriggerServiceTests
{
    [Fact]
    public async Task Trigger_Should_Return_NotRequired_When_NoWorkflowMatches()
    {
        await using var approvalsDbContext = CreateApprovalsDbContext(nameof(Trigger_Should_Return_NotRequired_When_NoWorkflowMatches));
        await using var identityDbContext = CreateIdentityDbContext(nameof(Trigger_Should_Return_NotRequired_When_NoWorkflowMatches));

        var triggerService = CreateTriggerService(approvalsDbContext, identityDbContext, 10);

        var result = await triggerService.TriggerAsync(
            new ApprovalTriggerRequest(
                ModuleKey: "Leave",
                DocumentType: "Request",
                ReferenceType: "LeaveRequest",
                ReferenceId: "LV-001",
                RequesterUserId: 10,
                PayloadJson: """{"days":3}"""),
            CancellationToken.None);

        Assert.False(result.RequiresApproval);
        Assert.False(result.Started);
        Assert.Equal("not_required", result.Outcome);
        Assert.Null(result.ApprovalInstanceId);
    }

    [Fact]
    public async Task Trigger_Should_Return_AlreadyPending_When_Open_Instance_Exists()
    {
        await using var approvalsDbContext = CreateApprovalsDbContext(nameof(Trigger_Should_Return_AlreadyPending_When_Open_Instance_Exists));
        await using var identityDbContext = CreateIdentityDbContext(nameof(Trigger_Should_Return_AlreadyPending_When_Open_Instance_Exists));

        SeedIdentity(identityDbContext);
        SeedWorkflow(approvalsDbContext);

        approvalsDbContext.ApprovalInstances.Add(new ApprovalInstance
        {
            Id = 700,
            ApprovalWorkflowDefinitionId = 1,
            ReferenceType = "OvertimeRequest",
            ReferenceId = "OT-700",
            RequesterUserId = 10,
            Status = "Pending",
            CurrentStepOrder = 1,
            PayloadJson = "{}"
        });

        approvalsDbContext.ApprovalInstanceSteps.Add(new ApprovalInstanceStep
        {
            Id = 701,
            ApprovalInstanceId = 700,
            ApprovalWorkflowStepId = 11,
            StepOrder = 1,
            AssignedUserId = 20,
            Status = "Pending",
            DueAt = DateTime.UtcNow.AddHours(24)
        });

        await approvalsDbContext.SaveChangesAsync();
        await identityDbContext.SaveChangesAsync();

        var triggerService = CreateTriggerService(approvalsDbContext, identityDbContext, 10);

        var result = await triggerService.TriggerAsync(
            new ApprovalTriggerRequest(
                ModuleKey: "Overtime",
                DocumentType: "Request",
                ReferenceType: "OvertimeRequest",
                ReferenceId: "OT-700",
                RequesterUserId: 10,
                PayloadJson: """{"amount":2500}"""),
            CancellationToken.None);

        Assert.True(result.RequiresApproval);
        Assert.False(result.Started);
        Assert.Equal("already_pending", result.Outcome);
        Assert.Equal(700, result.ApprovalInstanceId);
        Assert.NotNull(result.ApprovalInstance);
    }

    private static ApprovalTriggerService CreateTriggerService(ApprovalsDbContext approvalsDbContext, IdentityDbContext identityDbContext, int userId)
    {
        var detailQueryHandler = new GetApprovalInstanceDetailQueryHandler(approvalsDbContext);

        return new ApprovalTriggerService(
            approvalsDbContext,
            new ApprovalWorkflowResolver(approvalsDbContext),
            new StartApprovalInstanceCommandHandler(
                approvalsDbContext,
                new FakeCurrentUserContext(userId),
                new ApprovalWorkflowResolver(approvalsDbContext),
                new ApproverResolutionService(identityDbContext, approvalsDbContext),
                detailQueryHandler),
            detailQueryHandler);
    }

    private static void SeedWorkflow(ApprovalsDbContext approvalsDbContext)
    {
        approvalsDbContext.ApprovalWorkflowDefinitions.Add(new ApprovalWorkflowDefinition
        {
            Id = 1,
            Code = "OT_WORKFLOW",
            Name = "Overtime Approval",
            Description = "Iki adimli onay",
            ModuleKey = "Overtime",
            DocumentType = "Request"
        });

        approvalsDbContext.ApprovalWorkflowSteps.Add(new ApprovalWorkflowStep
        {
            Id = 11,
            ApprovalWorkflowDefinitionId = 1,
            StepOrder = 1,
            Name = "Direktor",
            ApproverType = "specific_user",
            ApproverValue = "20",
            DecisionDeadlineHours = 24,
            TimeoutDecision = "reject"
        });
    }

    private static void SeedIdentity(IdentityDbContext identityDbContext)
    {
        identityDbContext.Users.AddRange(
            new User { Id = 10, UserCode = "REQUESTER", Email = "requester@test.local", PasswordHash = "x" },
            new User { Id = 20, UserCode = "DIRECTOR", Email = "director@test.local", PasswordHash = "x" });
    }

    private static ApprovalsDbContext CreateApprovalsDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApprovalsDbContext>()
            .UseInMemoryDatabase($"approvals-trigger-{databaseName}")
            .Options;

        return new ApprovalsDbContext(options, new FakeAuditActorAccessor());
    }

    private static IdentityDbContext CreateIdentityDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase($"identity-trigger-{databaseName}")
            .Options;

        return new IdentityDbContext(options, new FakeAuditActorAccessor());
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

        public bool TryGetSessionId(out int sessionId)
        {
            sessionId = 1;
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

        public bool IsInRole(string roleCode) => false;
    }
}
