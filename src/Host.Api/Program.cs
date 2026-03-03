using Infrastructure.Persistence;
using Host.Api.Middleware;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddDbContext<LogDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("LogDb");
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", LogDbContext.LogsSchema);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<RequestLifecycleLoggingMiddleware>();
app.MapGet("/health", () => Results.Ok(new { Status = "ok" }));
app.MapControllers();

app.Run();
