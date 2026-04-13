using Application.Exceptions;
using Application.Observability;
using Application.Pipeline;
using Application.Security;
using Infrastructure.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests;

public sealed class RequestExecutionPipelineTests
{
    [Fact]
    public async Task ExecuteCommand_Should_Run_Validators_PreChecks_Handler_In_Order_And_Publish_Success_Event()
    {
        var executionLog = new List<string>();
        var publisher = new CapturingOperationalEventPublisher();

        var services = new ServiceCollection();
        services.AddSingleton<IOperationalEventPublisher>(publisher);
        services.AddScoped<IRequestValidator<TestRequest>>(_ => new TrackingValidator(executionLog));
        services.AddScoped<IRequestPreCheck<TestRequest>>(_ => new TrackingPreCheck(executionLog));

        await using var serviceProvider = services.BuildServiceProvider();
        var pipeline = new RequestExecutionPipeline(serviceProvider, publisher, new FakeCurrentUserContext());

        var result = await pipeline.ExecuteCommandAsync<TestRequest, string>(
            new TestRequest(),
            _ =>
            {
                executionLog.Add("handler");
                return Task.FromResult("ok");
            },
            CancellationToken.None,
            operationName: "CreateUser");

        Assert.Equal("ok", result);
        Assert.Equal(["validator", "precheck", "handler"], executionLog);

        var publishedEvent = Assert.Single(publisher.Events);
        Assert.Equal("CommandExecuted", publishedEvent.EventName);
        Assert.Equal("CreateUser", publishedEvent.OperationName);
        Assert.True(publishedEvent.IsSuccessful);
    }

    [Fact]
    public async Task ExecuteCommand_Should_Block_AdminOnly_Request_When_User_Is_Not_Admin()
    {
        var publisher = new CapturingOperationalEventPublisher();

        await using var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var pipeline = new RequestExecutionPipeline(serviceProvider, publisher, new FakeCurrentUserContext(isAdmin: false));

        await Assert.ThrowsAsync<ForbiddenAppException>(() =>
            pipeline.ExecuteCommandAsync(
                new AdminOnlyTestRequest(),
                _ => Task.CompletedTask,
                CancellationToken.None,
                operationName: "DeleteRole"));

        Assert.Empty(publisher.Events);
    }

    private sealed class TestRequest;

    private sealed class AdminOnlyTestRequest : IAdminOnlyRequest;

    private sealed class TrackingValidator(List<string> executionLog) : IRequestValidator<TestRequest>
    {
        public Task ValidateAsync(TestRequest request, CancellationToken cancellationToken)
        {
            executionLog.Add("validator");
            return Task.CompletedTask;
        }
    }

    private sealed class TrackingPreCheck(List<string> executionLog) : IRequestPreCheck<TestRequest>
    {
        public Task CheckAsync(TestRequest request, CancellationToken cancellationToken)
        {
            executionLog.Add("precheck");
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCurrentUserContext(bool isAdmin = true) : ICurrentUserContext
    {
        public bool TryGetUserId(out int userId)
        {
            userId = 1;
            return true;
        }

        public bool TryGetSessionId(out int sessionId)
        {
            sessionId = 1;
            return true;
        }

        public bool TryGetActorIdentity(out string actorIdentity)
        {
            actorIdentity = "core.admin";
            return true;
        }

        public bool TryGetUsername(out string username)
        {
            username = "core.admin";
            return true;
        }

        public bool TryGetCompanyId(out int companyId)
        {
            companyId = 1;
            return true;
        }

        public bool TryGetUserCode(out string userCode)
        {
            userCode = "CORE_ADMIN";
            return true;
        }

        public bool IsInRole(string roleCode) => isAdmin && string.Equals(roleCode, "SYS_ADMIN", StringComparison.OrdinalIgnoreCase);
    }

    private sealed class CapturingOperationalEventPublisher : IOperationalEventPublisher
    {
        public List<OperationalEvent> Events { get; } = [];

        public Task PublishAsync(OperationalEvent operationalEvent, CancellationToken cancellationToken = default)
        {
            Events.Add(operationalEvent);
            return Task.CompletedTask;
        }
    }
}
