using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Operations.Application.Contracts;
using Operations.Application.Services;

namespace IntegrationTests;

public sealed class AuditDashboardApiIntegrationTests(AuditDashboardApiTestWebApplicationFactory factory)
    : IClassFixture<AuditDashboardApiTestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Summary_Should_Return401_When_NotAuthenticated()
    {
        var response = await _client.GetAsync("/api/ops/audit/dashboard/summary");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Summary_Should_Return403_When_RoleIsNotAllowed()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/ops/audit/dashboard/summary");
        request.Headers.Add(TestAuthHandler.UserHeaderName, "auditor.user");
        request.Headers.Add(TestAuthHandler.RoleHeaderName, "SYS_AUDITOR");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Summary_Should_Return200_With_DashboardPayload()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/ops/audit/dashboard/summary?windowHours=24");
        request.Headers.Add(TestAuthHandler.UserHeaderName, "admin.user");
        request.Headers.Add(TestAuthHandler.RoleHeaderName, "SYS_ADMIN");

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AuditDashboardSummaryDto>();

        Assert.NotNull(payload);
        Assert.Equal(24, payload.WindowHours);
        Assert.Equal(5, payload.FailedLoginCount);
        Assert.Equal(12, payload.SystemErrorCount);
    }
}

public sealed class AuditDashboardApiTestWebApplicationFactory : WebApplicationFactory<Program>
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

            services.RemoveAll<IAuditDashboardService>();
            services.AddScoped<IAuditDashboardService, FakeAuditDashboardService>();
        });
    }
}

public sealed class FakeAuditDashboardService : IAuditDashboardService
{
    public Task<AuditDashboardSummaryDto> GetSummaryAsync(int windowHours, CancellationToken cancellationToken)
    {
        return Task.FromResult(new AuditDashboardSummaryDto(
            GeneratedAt: DateTimeOffset.UtcNow,
            WindowHours: windowHours,
            SystemErrorCount: 12,
            FailedLoginCount: 5,
            SessionRevokeRatePercent: 3.5m,
            FailedLoginTrend:
            [
                new HourlyMetricDto(DateTimeOffset.UtcNow.AddHours(-1), 2),
                new HourlyMetricDto(DateTimeOffset.UtcNow, 3)
            ],
            TopCriticalEvents:
            [
                new TopEventDto("AuthLifecycle", 4),
                new TopEventDto("ApprovalDeadline", 2)
            ]));
    }
}
