using System.Net;
using System.Net.Http.Json;
using Documents.Application.Commands;
using Documents.Application.Contracts;
using Documents.Application.Queries;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace IntegrationTests;

public sealed class DocumentsApiIntegrationTests(DocumentsApiTestWebApplicationFactory factory)
    : IClassFixture<DocumentsApiTestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Documents_Should_Return401_When_NotAuthenticated()
    {
        var response = await _client.GetAsync("/api/documents");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Documents_Should_Return403_When_RoleIsNotAllowed()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/documents");
        request.Headers.Add(TestAuthHandler.UserHeaderName, "auditor.user");
        request.Headers.Add(TestAuthHandler.RoleHeaderName, "SYS_AUDITOR");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateDocument_Should_Return201_For_SysAdmin()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/documents");
        request.Headers.Add(TestAuthHandler.UserHeaderName, "admin.user");
        request.Headers.Add(TestAuthHandler.RoleHeaderName, "SYS_ADMIN");
        request.Content = JsonContent.Create(new CreateManagedDocumentRequest(
            Code: "DOC-001",
            Title: "Motor Teknik Resim",
            Description: "Ana teknik dokuman",
            DocumentType: "technical_pdf",
            FileName: "motor.pdf",
            ContentType: "application/pdf",
            StoragePath: "/files/motor.pdf",
            FileSize: 2048,
            Checksum: "abc123",
            ChangeNote: "ilk surum"));

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ManagedDocumentDetailDto>();
        Assert.NotNull(payload);
        Assert.Equal("DOC-001", payload.Code);
        Assert.Single(payload.Versions);
    }
}

public sealed class DocumentsApiTestWebApplicationFactory : WebApplicationFactory<Program>
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

            services.RemoveAll<IListManagedDocumentsQueryHandler>();
            services.RemoveAll<IGetManagedDocumentDetailQueryHandler>();
            services.RemoveAll<ICreateManagedDocumentCommandHandler>();
            services.RemoveAll<IAddManagedDocumentVersionCommandHandler>();
            services.RemoveAll<ILinkDocumentCommandHandler>();
            services.RemoveAll<IUnlinkDocumentCommandHandler>();

            services.AddScoped<IListManagedDocumentsQueryHandler, FakeListManagedDocumentsQueryHandler>();
            services.AddScoped<IGetManagedDocumentDetailQueryHandler, FakeGetManagedDocumentDetailQueryHandler>();
            services.AddScoped<ICreateManagedDocumentCommandHandler, FakeCreateManagedDocumentCommandHandler>();
            services.AddScoped<IAddManagedDocumentVersionCommandHandler, FakeAddManagedDocumentVersionCommandHandler>();
            services.AddScoped<ILinkDocumentCommandHandler, FakeLinkDocumentCommandHandler>();
            services.AddScoped<IUnlinkDocumentCommandHandler, FakeUnlinkDocumentCommandHandler>();
        });
    }
}

public sealed class FakeListManagedDocumentsQueryHandler : IListManagedDocumentsQueryHandler
{
    public Task<IReadOnlyList<ManagedDocumentListItemDto>> HandleAsync(DocumentQueryRequest request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ManagedDocumentListItemDto> result =
        [
            new ManagedDocumentListItemDto(1, "DOC-001", "Motor Teknik Resim", "technical_pdf", "active", 1, "motor.pdf", "application/pdf")
        ];

        return Task.FromResult(result);
    }
}

public sealed class FakeGetManagedDocumentDetailQueryHandler : IGetManagedDocumentDetailQueryHandler
{
    public Task<ManagedDocumentDetailDto> HandleAsync(int documentId, CancellationToken cancellationToken)
        => Task.FromResult(FakeDocument(documentId));

    internal static ManagedDocumentDetailDto FakeDocument(int documentId)
        => new(
            documentId,
            "DOC-001",
            "Motor Teknik Resim",
            "Ana teknik dokuman",
            "technical_pdf",
            "active",
            [
                new ManagedDocumentVersionDto(11, 1, "motor.pdf", "application/pdf", "/files/motor.pdf", 2048, "abc123", "ilk surum", true, DateTime.UtcNow, "admin.user")
            ],
            []);
}

public sealed class FakeCreateManagedDocumentCommandHandler : ICreateManagedDocumentCommandHandler
{
    public Task<ManagedDocumentDetailDto> HandleAsync(CreateManagedDocumentRequest request, CancellationToken cancellationToken)
        => Task.FromResult(new ManagedDocumentDetailDto(
            1,
            request.Code,
            request.Title,
            request.Description,
            request.DocumentType,
            "active",
            [
                new ManagedDocumentVersionDto(11, 1, request.FileName, request.ContentType, request.StoragePath, request.FileSize, request.Checksum, request.ChangeNote, true, DateTime.UtcNow, "admin.user")
            ],
            []));
}

public sealed class FakeAddManagedDocumentVersionCommandHandler : IAddManagedDocumentVersionCommandHandler
{
    public Task<ManagedDocumentDetailDto> HandleAsync(int documentId, AddManagedDocumentVersionRequest request, CancellationToken cancellationToken)
        => Task.FromResult(new ManagedDocumentDetailDto(
            documentId,
            "DOC-001",
            "Motor Teknik Resim",
            "Ana teknik dokuman",
            "technical_pdf",
            "active",
            [
                new ManagedDocumentVersionDto(12, 2, request.FileName, request.ContentType, request.StoragePath, request.FileSize, request.Checksum, request.ChangeNote, true, DateTime.UtcNow, "admin.user")
            ],
            []));
}

public sealed class FakeLinkDocumentCommandHandler : ILinkDocumentCommandHandler
{
    public Task<DocumentAssociationDto> HandleAsync(int documentId, LinkDocumentRequest request, CancellationToken cancellationToken)
        => Task.FromResult(new DocumentAssociationDto(21, documentId, request.OwnerEntityName, request.OwnerEntityId, request.LinkType, request.IsPrimary, request.SortOrder, DateTime.UtcNow, "admin.user"));
}

public sealed class FakeUnlinkDocumentCommandHandler : IUnlinkDocumentCommandHandler
{
    public Task HandleAsync(int associationId, CancellationToken cancellationToken) => Task.CompletedTask;
}
