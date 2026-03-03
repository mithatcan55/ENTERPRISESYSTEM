using Host.Api.DependencyInjection;
using Identity.Presentation.DependencyInjection;
using Infrastructure.DependencyInjection;
using Host.Api.Middleware;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHostCoreServices(builder.Configuration);
builder.Services.AddInfrastructurePersistence(builder.Configuration);
builder.Services.AddIdentityPresentationModule();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseRateLimiter();
app.UseSerilogRequestLogging();
app.UseMiddleware<RequestLifecycleLoggingMiddleware>();
app.MapGet("/health", () => Results.Ok(new { Status = "ok" }));
app.MapControllers();

app.Run();
