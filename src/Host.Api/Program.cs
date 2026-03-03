using Infrastructure.Persistence;
using Infrastructure.Persistence.Auditing;
using Host.Api.Authorization.Services;
using Host.Api.Exceptions;
using Host.Api.Middleware;
using Host.Api.Services;
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
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditActorAccessor, HttpContextAuditActorAccessor>();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddScoped<ITCodeAuthorizationService, TCodeAuthorizationService>();

builder.Services.AddDbContext<LogDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("LogDb");
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", LogDbContext.LogsSchema);
    });
});

builder.Services.AddDbContext<BusinessDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("BusinessDb");
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", BusinessDbContext.AuthorizationSchema);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<RequestLifecycleLoggingMiddleware>();
app.MapGet("/health", () => Results.Ok(new { Status = "ok" }));
app.MapControllers();

app.Run();
