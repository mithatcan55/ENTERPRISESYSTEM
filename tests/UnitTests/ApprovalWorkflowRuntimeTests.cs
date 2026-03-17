using Approvals.Application.Contracts;
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

public sealed class ApprovalWorkflowRuntimeTests
{
    [Fact]
    public async Task Resolver_Should_Pick_Most_Specific_Matching_Workflow()
    {
        await using var approvalsDbContext = CreateApprovalsDbContext(nameof(Resolver_Should_Pick_Most_Specific_Matching_Workflow));

        approvalsDbContext.ApprovalWorkflowDefinitions.AddRange(
            new ApprovalWorkflowDefinition
            {
                Id = 1,
                Code = "OT_STANDARD",
                Name = "Standard Fazla Mesai",
                Description = "Temel akıs",
                ModuleKey = "Overtime",
                DocumentType = "Request"
            },
            new ApprovalWorkflowDefinition
            {
                Id = 2,
                Code = "OT_HIGH_AMOUNT",
                Name = "Yuksek Tutar Fazla Mesai",
                Description = "Daha spesifik akıs",
                ModuleKey = "Overtime",
                DocumentType = "Request"
            });

        approvalsDbContext.ApprovalWorkflowSteps.AddRange(
            new ApprovalWorkflowStep
            {
                Id = 11,
                ApprovalWorkflowDefinitionId = 1,
                StepOrder = 1,
                Name = "Manager",
                ApproverType = "specific_user",
                ApproverValue = "100"
            },
            new ApprovalWorkflowStep
            {
                Id = 21,
                ApprovalWorkflowDefinitionId = 2,
                StepOrder = 1,
                Name = "Director",
                ApproverType = "specific_user",
                ApproverValue = "200"
            });

        approvalsDbContext.ApprovalWorkflowConditions.AddRange(
            new ApprovalWorkflowCondition
            {
                Id = 201,
                ApprovalWorkflowDefinitionId = 2,
                FieldKey = "amount",
                Operator = "gte",
                Value = "1000"
            },
            new ApprovalWorkflowCondition
            {
                Id = 202,
                ApprovalWorkflowDefinitionId = 2,
                FieldKey = "companyId",
                Operator = "eq",
                Value = "1"
            });

        await approvalsDbContext.SaveChangesAsync();

        var resolver = new ApprovalWorkflowResolver(approvalsDbContext);

        var result = await resolver.ResolveAsync(
            "Overtime",
            "Request",
            """{"amount":1500,"companyId":1}""",
            CancellationToken.None);

        Assert.Equal(2, result.Workflow.Id);
        Assert.Equal("OT_HIGH_AMOUNT", result.Workflow.Code);
        Assert.Equal(2, result.MatchedConditionCount);
    }

    [Fact]
    public async Task StartAndApprove_Should_Apply_Delegation_And_Advance_To_Next_Step()
    {
        await using var approvalsDbContext = CreateApprovalsDbContext(nameof(StartAndApprove_Should_Apply_Delegation_And_Advance_To_Next_Step));
        await using var identityDbContext = CreateIdentityDbContext(nameof(StartAndApprove_Should_Apply_Delegation_And_Advance_To_Next_Step));

        SeedWorkflow(approvalsDbContext);
        SeedIdentity(identityDbContext);

        approvalsDbContext.DelegationAssignments.Add(new DelegationAssignment
        {
            Id = 900,
            DelegatorUserId = 20,
            DelegateUserId = 30,
            ScopeType = "workflow",
            IncludedScopesJson = """["OT_WORKFLOW"]""",
            ExcludedScopesJson = "[]",
            StartsAt = DateTime.UtcNow.AddDays(-1),
            EndsAt = DateTime.UtcNow.AddDays(1),
            Notes = "Izinli oldugu icin vekalet"
        });

        await approvalsDbContext.SaveChangesAsync();
        await identityDbContext.SaveChangesAsync();

        var detailQueryHandler = new GetApprovalInstanceDetailQueryHandler(approvalsDbContext);
        var startHandler = new StartApprovalInstanceCommandHandler(
            approvalsDbContext,
            new FakeCurrentUserContext(userId: 10),
            new ApprovalWorkflowResolver(approvalsDbContext),
            new ApproverResolutionService(identityDbContext, approvalsDbContext),
            detailQueryHandler);

        var started = await startHandler.HandleAsync(
            new StartApprovalInstanceRequest(
                "Overtime",
                "Request",
                "OvertimeRequest",
                "OT-0001",
                RequesterUserId: null,
                PayloadJson: """{"amount":2500}"""),
            CancellationToken.None);

        var firstStep = Assert.Single(started.Steps, x => x.StepOrder == 1);
        Assert.Equal(30, firstStep.AssignedUserId);
        Assert.Equal("Pending", firstStep.Status);
        Assert.NotNull(firstStep.DueAt);

        var decideHandler = new DecideApprovalStepCommandHandler(
            approvalsDbContext,
            new FakeCurrentUserContext(userId: 30),
            detailQueryHandler);

        var approved = await decideHandler.HandleAsync(
            firstStep.Id,
            new DecideApprovalStepRequest("approve", "Onaylandi"),
            CancellationToken.None);

        Assert.Equal("Pending", approved.Status);

        var nextStep = Assert.Single(approved.Steps, x => x.StepOrder == 2);
        Assert.Equal(40, nextStep.AssignedUserId);
        Assert.Equal("Pending", nextStep.Status);
        Assert.NotNull(nextStep.DueAt);
    }

    [Fact]
    public async Task PendingInbox_Should_Default_To_Current_User_Assigned_Steps()
    {
        await using var approvalsDbContext = CreateApprovalsDbContext(nameof(PendingInbox_Should_Default_To_Current_User_Assigned_Steps));

        approvalsDbContext.ApprovalWorkflowDefinitions.Add(new ApprovalWorkflowDefinition
        {
            Id = 1,
            Code = "OT_WORKFLOW",
            Name = "Overtime",
            Description = "Approval",
            ModuleKey = "Overtime",
            DocumentType = "Request"
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

        approvalsDbContext.ApprovalInstanceSteps.AddRange(
            new ApprovalInstanceStep
            {
                Id = 1001,
                ApprovalInstanceId = 100,
                ApprovalWorkflowStepId = 11,
                StepOrder = 1,
                AssignedUserId = 77,
                Status = "Pending"
            },
            new ApprovalInstanceStep
            {
                Id = 1002,
                ApprovalInstanceId = 100,
                ApprovalWorkflowStepId = 12,
                StepOrder = 2,
                AssignedUserId = 88,
                Status = "Waiting"
            });

        await approvalsDbContext.SaveChangesAsync();

        var handler = new ListPendingApprovalsQueryHandler(approvalsDbContext, new FakeCurrentUserContext(userId: 77));

        var result = await handler.HandleAsync(new PendingApprovalQueryRequest(), CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal(100, item.ApprovalInstanceId);
        Assert.Equal(1001, item.ApprovalInstanceStepId);
        Assert.Equal(77, item.AssignedUserId);
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

        approvalsDbContext.ApprovalWorkflowSteps.AddRange(
            new ApprovalWorkflowStep
            {
                Id = 11,
                ApprovalWorkflowDefinitionId = 1,
                StepOrder = 1,
                Name = "Direktor",
                ApproverType = "role",
                ApproverValue = "DIRECTOR_ROLE",
                DecisionDeadlineHours = 24,
                TimeoutDecision = "reject"
            },
            new ApprovalWorkflowStep
            {
                Id = 12,
                ApprovalWorkflowDefinitionId = 1,
                StepOrder = 2,
                Name = "CFO",
                ApproverType = "specific_user",
                ApproverValue = "40",
                DecisionDeadlineHours = 48,
                TimeoutDecision = "reject"
            });
    }

    private static void SeedIdentity(IdentityDbContext identityDbContext)
    {
        identityDbContext.Users.AddRange(
            new User { Id = 10, UserCode = "REQUESTER", Username = "requester", Email = "requester@test.local", PasswordHash = "x" },
            new User { Id = 20, UserCode = "DIRECTOR", Username = "director", Email = "director@test.local", PasswordHash = "x" },
            new User { Id = 30, UserCode = "DELEGATE", Username = "delegate", Email = "delegate@test.local", PasswordHash = "x" },
            new User { Id = 40, UserCode = "CFO", Username = "cfo", Email = "cfo@test.local", PasswordHash = "x" });

        identityDbContext.Roles.Add(new Role
        {
            Id = 200,
            Code = "DIRECTOR_ROLE",
            Name = "Director"
        });

        identityDbContext.UserRoles.Add(new UserRole
        {
            Id = 300,
            UserId = 20,
            RoleId = 200
        });
    }

    private static ApprovalsDbContext CreateApprovalsDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApprovalsDbContext>()
            .UseInMemoryDatabase($"approvals-{databaseName}")
            .Options;

        return new ApprovalsDbContext(options, new FakeAuditActorAccessor());
    }

    private static IdentityDbContext CreateIdentityDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase($"identity-{databaseName}")
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
}
