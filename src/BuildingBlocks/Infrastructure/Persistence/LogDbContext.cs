using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class LogDbContext(DbContextOptions<LogDbContext> options) : DbContext(options)
{
    public const string LogsSchema = "logs";

    public DbSet<DatabaseQueryLog> DatabaseQueryLogs => Set<DatabaseQueryLog>();
    public DbSet<EntityChangeLog> EntityChangeLogs => Set<EntityChangeLog>();
    public DbSet<HttpRequestLog> HttpRequestLogs => Set<HttpRequestLog>();
    public DbSet<PageVisitLog> PageVisitLogs => Set<PageVisitLog>();
    public DbSet<PerformanceLog> PerformanceLogs => Set<PerformanceLog>();
    public DbSet<SecurityEventLog> SecurityEventLogs => Set<SecurityEventLog>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(LogsSchema);

        modelBuilder.Entity<DatabaseQueryLog>(entity =>
        {
            entity.ToTable("database_query_logs");
            entity.HasKey(x => x.Id);
        });

        modelBuilder.Entity<EntityChangeLog>(entity =>
        {
            entity.ToTable("entity_change_logs");
            entity.HasKey(x => x.Id);
        });

        modelBuilder.Entity<HttpRequestLog>(entity =>
        {
            entity.ToTable("http_request_logs");
            entity.HasKey(x => x.Id);
        });

        modelBuilder.Entity<PageVisitLog>(entity =>
        {
            entity.ToTable("page_visit_logs");
            entity.HasKey(x => x.Id);
        });

        modelBuilder.Entity<PerformanceLog>(entity =>
        {
            entity.ToTable("performance_logs");
            entity.HasKey(x => x.Id);
        });

        modelBuilder.Entity<SecurityEventLog>(entity =>
        {
            entity.ToTable("security_event_logs");
            entity.HasKey(x => x.Id);
        });

        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.ToTable("system_logs");
            entity.HasKey(x => x.Id);
        });

        base.OnModelCreating(modelBuilder);
    }
}
