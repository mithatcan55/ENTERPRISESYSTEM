using Approvals.Infrastructure.DependencyInjection;
using Approvals.Presentation.DependencyInjection;
using Authorization.Infrastructure.DependencyInjection;
using Authorization.Presentation.DependencyInjection;
using Host.Api.DependencyInjection;
using Identity.Presentation.DependencyInjection;
using Identity.Infrastructure.DependencyInjection;
using Infrastructure.DependencyInjection;
using Integrations.Infrastructure.DependencyInjection;
using Integrations.Presentation.DependencyInjection;
using Host.Api.Middleware;
using Operations.Infrastructure.DependencyInjection;
using Operations.Presentation.DependencyInjection;
using Reports.Infrastructure.DependencyInjection;
using Reports.Presentation.DependencyInjection;
using Scalar.AspNetCore;
using Serilog;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

var mvcBuilder = builder.Services.AddControllers();
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("tr-TR"),
        new CultureInfo("de-DE"),
        new CultureInfo("en-US")
    };

    options.DefaultRequestCulture = new RequestCulture("tr-TR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
builder.Services.AddOpenApi();
builder.Services.AddHostCoreServices(builder.Configuration, builder.Environment);
builder.Services.AddInfrastructurePersistence(builder.Configuration);
mvcBuilder.AddIdentityPresentationModule();
mvcBuilder.AddAuthorizationPresentationModule();
mvcBuilder.AddOperationsPresentationModule();
mvcBuilder.AddIntegrationsPresentationModule();
mvcBuilder.AddReportsPresentationModule();
mvcBuilder.AddApprovalsPresentationModule();
builder.Services.AddIdentityInfrastructureModule();
builder.Services.AddAuthorizationInfrastructureModule();
builder.Services.AddOperationsInfrastructureModule();
builder.Services.AddIntegrationsInfrastructureModule(builder.Configuration);
builder.Services.AddReportsInfrastructureModule();
builder.Services.AddApprovalsInfrastructureModule();

var app = builder.Build();

var localizationOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseRequestLocalization(localizationOptions.Value);
app.UseMiddleware<CorrelationIdMiddleware>();
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseSerilogRequestLogging();
    app.UseMiddleware<RequestLifecycleLoggingMiddleware>();
}
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { Status = "ok" }));
app.MapControllers();

app.Run();

public partial class Program;
