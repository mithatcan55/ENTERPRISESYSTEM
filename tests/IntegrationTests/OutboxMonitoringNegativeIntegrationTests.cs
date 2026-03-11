using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Host.Api.Integrations.Contracts;
using Host.Api.Integrations.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IntegrationTests;

public sealed class OutboxMonitoringNegativeIntegrationTests(OutboxMonitoringTestWebApplicationFactory factory)
    : IClassFixture<OutboxMonitoringTestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ListMessages_Should_Return401_When_NotAuthenticated()
    {
        var response = await _client.GetAsync("/api/ops/outbox/messages");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ListMessages_Should_Return403_When_RoleIsNotAllowed()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/ops/outbox/messages");
        request.Headers.Add(TestAuthHandler.UserHeaderName, "operator.user");
        request.Headers.Add(TestAuthHandler.RoleHeaderName, "SYS_AUDITOR");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ListMessages_Should_Return400_When_QueryIsInvalid()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/ops/outbox/messages?page=abc&pageSize=10");
        request.Headers.Add(TestAuthHandler.UserHeaderName, "admin.user");
        request.Headers.Add(TestAuthHandler.RoleHeaderName, "SYS_ADMIN");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ListMessages_Should_Clamp_PagingBoundaries()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/ops/outbox/messages?page=-7&pageSize=9999");
        request.Headers.Add(TestAuthHandler.UserHeaderName, "admin.user");
        request.Headers.Add(TestAuthHandler.RoleHeaderName, "SYS_ADMIN");

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<OutboxPagedResultDto>();

        Assert.NotNull(payload);
        Assert.Equal(1, payload.Page);
        Assert.Equal(200, payload.PageSize);
    }

    public sealed record OutboxPagedResultDto(
        IReadOnlyList<object> Items,
        int Page,
        int PageSize,
        int TotalCount);
}

public sealed class OutboxMonitoringTestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            var hostedServices = services
                .Where(descriptor => descriptor.ServiceType == typeof(IHostedService)
                    && descriptor.ImplementationType == typeof(ExternalOutboxDispatcherService))
                .ToArray();

            foreach (var descriptor in hostedServices)
            {
                services.Remove(descriptor);
            }

            services.RemoveAll<IExternalOutboxService>();
            services.AddScoped<IExternalOutboxService, FakeExternalOutboxService>();

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
        });
    }
}

public sealed class FakeExternalOutboxService : IExternalOutboxService
{
    public Task<OutboxPagedResult<OutboxMessageListItemDto>> ListMessagesAsync(OutboxMessageQueryRequest request, CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 50 : Math.Min(request.PageSize, 200);

        return Task.FromResult(new OutboxPagedResult<OutboxMessageListItemDto>(
            Items: Array.Empty<OutboxMessageListItemDto>(),
            Page: page,
            PageSize: pageSize,
            TotalCount: 0));
    }

    public Task<OutboxMessageQueuedDto> QueueMailAsync(QueueMailRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OutboxMessageQueuedDto(1, OutboxEventTypes.MailNotification, "Pending", DateTime.UtcNow));
    }

    public Task<OutboxMessageQueuedDto> QueueExcelReportAsync(QueueExcelReportRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OutboxMessageQueuedDto(2, OutboxEventTypes.ExcelReport, "Pending", DateTime.UtcNow));
    }
}

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "TestAuth";
    public const string UserHeaderName = "X-Test-User";
    public const string RoleHeaderName = "X-Test-Role";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserHeaderName, out var usernameValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var username = usernameValues.ToString();
        if (string.IsNullOrWhiteSpace(username))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var role = Request.Headers.TryGetValue(RoleHeaderName, out var roleValues)
            ? roleValues.ToString()
            : "SYS_OPERATOR";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, username),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
