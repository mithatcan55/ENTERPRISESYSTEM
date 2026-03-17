using System.Net;
using System.Net.Http.Json;
using Approvals.Application.Commands;
using Approvals.Application.Contracts;
using Approvals.Application.Queries;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace IntegrationTests;

public sealed class ApprovalDelegationsApiIntegrationTests(ApprovalDelegationsApiTestWebApplicationFactory factory)
    : IClassFixture<ApprovalDelegationsApiTestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Delegations_Should_Return401_When_NotAuthenticated()
    {
        var response = await _client.GetAsync("/api/approvals/delegations");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delegations_Should_Return403_When_RoleIsNotAllowed()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/approvals/delegations");
        request.Headers.Add(TestAuthHandler.UserHeaderName, "auditor.user");
        request.Headers.Add(TestAuthHandler.RoleHeaderName, "SYS_AUDITOR");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateDelegation_Should_Return201_For_SysAdmin()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/approvals/delegations");
        request.Headers.Add(TestAuthHandler.UserHeaderName, "admin.user");
        request.Headers.Add(TestAuthHandler.UserIdHeaderName, "42");
        request.Headers.Add(TestAuthHandler.RoleHeaderName, "SYS_ADMIN");
        request.Content = JsonContent.Create(new CreateDelegationAssignmentRequest(
            DelegatorUserId: 42,
            DelegateUserId: 77,
            ScopeType: "workflow",
            IncludedScopesJson: """["OT_WORKFLOW"]""",
            ExcludedScopesJson: "[]",
            StartsAt: DateTime.UtcNow,
            EndsAt: DateTime.UtcNow.AddDays(7),
            Notes: "Izin"));

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<DelegationAssignmentDetailDto>();
        Assert.NotNull(payload);
        Assert.True(payload.IsActive);
        Assert.Equal(42, payload.DelegatorUserId);
        Assert.Equal(77, payload.DelegateUserId);
    }

    [Fact]
    public async Task SetDelegationStatus_Should_Return200_For_SysAdmin()
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/approvals/delegations/9/status");
        request.Headers.Add(TestAuthHandler.UserHeaderName, "admin.user");
        request.Headers.Add(TestAuthHandler.UserIdHeaderName, "1");
        request.Headers.Add(TestAuthHandler.RoleHeaderName, "SYS_ADMIN");
        request.Content = JsonContent.Create(new SetDelegationAssignmentStatusRequest(false, "Geri alindi"));

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<DelegationAssignmentDetailDto>();

        Assert.NotNull(payload);
        Assert.False(payload.IsActive);
        Assert.Equal("Geri alindi", payload.RevokedReason);
    }
}

public sealed class ApprovalDelegationsApiTestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(TestAuthHandler.SchemeName)
                    .RequireAuthenticatedUser()
                    .Build();
            });

            var hostedServices = services
                .Where(descriptor => descriptor.ServiceType == typeof(IHostedService))
                .ToArray();

            foreach (var descriptor in hostedServices)
            {
                services.Remove(descriptor);
            }

            services.RemoveAll<IListDelegationAssignmentsQueryHandler>();
            services.RemoveAll<ICreateDelegationAssignmentCommandHandler>();
            services.RemoveAll<ISetDelegationAssignmentStatusCommandHandler>();

            services.AddScoped<IListDelegationAssignmentsQueryHandler, FakeDelegationListQueryHandler>();
            services.AddScoped<ICreateDelegationAssignmentCommandHandler, FakeCreateDelegationCommandHandler>();
            services.AddScoped<ISetDelegationAssignmentStatusCommandHandler, FakeSetDelegationStatusCommandHandler>();
        });
    }
}

public sealed class FakeDelegationListQueryHandler : IListDelegationAssignmentsQueryHandler
{
    public Task<PagedResult<DelegationAssignmentListItemDto>> HandleAsync(DelegationAssignmentQueryRequest request, CancellationToken cancellationToken)
    {
        var item = new DelegationAssignmentListItemDto(
            Id: 9,
            DelegatorUserId: 42,
            DelegateUserId: 77,
            ScopeType: "workflow",
            StartsAt: DateTime.UtcNow.AddDays(-1),
            EndsAt: DateTime.UtcNow.AddDays(7),
            IsActive: true,
            RevokedByUserId: null,
            RevokedAt: null,
            RevokedReason: string.Empty,
            Notes: "Izin");

        return Task.FromResult(new PagedResult<DelegationAssignmentListItemDto>([item], 1, 20, 1));
    }
}

public sealed class FakeCreateDelegationCommandHandler : ICreateDelegationAssignmentCommandHandler
{
    public Task<DelegationAssignmentDetailDto> HandleAsync(CreateDelegationAssignmentRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new DelegationAssignmentDetailDto(
            Id: 9,
            DelegatorUserId: request.DelegatorUserId,
            DelegateUserId: request.DelegateUserId,
            ScopeType: request.ScopeType,
            IncludedScopesJson: request.IncludedScopesJson,
            ExcludedScopesJson: request.ExcludedScopesJson,
            StartsAt: request.StartsAt,
            EndsAt: request.EndsAt,
            IsActive: true,
            RevokedByUserId: null,
            RevokedAt: null,
            RevokedReason: string.Empty,
            Notes: request.Notes));
    }
}

public sealed class FakeSetDelegationStatusCommandHandler : ISetDelegationAssignmentStatusCommandHandler
{
    public Task<DelegationAssignmentDetailDto> HandleAsync(int delegationAssignmentId, SetDelegationAssignmentStatusRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new DelegationAssignmentDetailDto(
            Id: delegationAssignmentId,
            DelegatorUserId: 42,
            DelegateUserId: 77,
            ScopeType: "workflow",
            IncludedScopesJson: """["OT_WORKFLOW"]""",
            ExcludedScopesJson: "[]",
            StartsAt: DateTime.UtcNow.AddDays(-1),
            EndsAt: DateTime.UtcNow.AddDays(7),
            IsActive: request.IsActive,
            RevokedByUserId: request.IsActive ? null : 1,
            RevokedAt: request.IsActive ? null : DateTime.UtcNow,
            RevokedReason: request.IsActive ? string.Empty : request.Reason,
            Notes: "Izin"));
    }
}
