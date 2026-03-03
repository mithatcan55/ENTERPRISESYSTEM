using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.LogDb
{
    /// <inheritdoc />
    public partial class InitialLogSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "logs");

            migrationBuilder.CreateTable(
                name: "database_query_logs",
                schema: "logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Operation = table.Column<string>(type: "text", nullable: true),
                    TableName = table.Column<string>(type: "text", nullable: true),
                    CommandText = table.Column<string>(type: "text", nullable: true),
                    Parameters = table.Column<string>(type: "text", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    RowsAffected = table.Column<int>(type: "integer", nullable: false),
                    IsError = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_database_query_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "entity_change_logs",
                schema: "logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Username = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    EntityType = table.Column<string>(type: "text", nullable: true),
                    EntityId = table.Column<string>(type: "text", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: true),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    ChangedProperties = table.Column<string>(type: "text", nullable: true),
                    TableName = table.Column<string>(type: "text", nullable: true),
                    SchemaName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_change_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "http_request_logs",
                schema: "logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: true),
                    Method = table.Column<string>(type: "text", nullable: true),
                    Path = table.Column<string>(type: "text", nullable: true),
                    QueryString = table.Column<string>(type: "text", nullable: true),
                    RequestHeaders = table.Column<string>(type: "text", nullable: true),
                    RequestBody = table.Column<string>(type: "text", nullable: true),
                    RequestSize = table.Column<long>(type: "bigint", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: false),
                    ResponseHeaders = table.Column<string>(type: "text", nullable: true),
                    ResponseBody = table.Column<string>(type: "text", nullable: true),
                    ResponseSize = table.Column<long>(type: "bigint", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Username = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    IsError = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ErrorStackTrace = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_http_request_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "page_visit_logs",
                schema: "logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Username = table.Column<string>(type: "text", nullable: true),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    Department = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    PagePath = table.Column<string>(type: "text", nullable: true),
                    PageTitle = table.Column<string>(type: "text", nullable: true),
                    PageCategory = table.Column<string>(type: "text", nullable: true),
                    QueryString = table.Column<string>(type: "text", nullable: true),
                    Referrer = table.Column<string>(type: "text", nullable: true),
                    SessionId = table.Column<string>(type: "text", nullable: true),
                    CorrelationId = table.Column<string>(type: "text", nullable: true),
                    VisitDurationMs = table.Column<long>(type: "bigint", nullable: false),
                    Browser = table.Column<string>(type: "text", nullable: true),
                    BrowserVersion = table.Column<string>(type: "text", nullable: true),
                    OperatingSystem = table.Column<string>(type: "text", nullable: true),
                    DeviceType = table.Column<string>(type: "text", nullable: true),
                    IsMobile = table.Column<bool>(type: "boolean", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    AdditionalData = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_page_visit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "performance_logs",
                schema: "logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    OperationName = table.Column<string>(type: "text", nullable: true),
                    OperationType = table.Column<string>(type: "text", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    MemoryBefore = table.Column<long>(type: "bigint", nullable: false),
                    MemoryAfter = table.Column<long>(type: "bigint", nullable: false),
                    MemoryUsed = table.Column<long>(type: "bigint", nullable: false),
                    IsSlowOperation = table.Column<bool>(type: "boolean", nullable: false),
                    ThresholdMs = table.Column<long>(type: "bigint", nullable: false),
                    AdditionalData = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_performance_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "security_event_logs",
                schema: "logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: true),
                    Severity = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Username = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    Resource = table.Column<string>(type: "text", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: true),
                    IsSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    AdditionalData = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_security_event_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "system_logs",
                schema: "logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: true),
                    Level = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    MessageTemplate = table.Column<string>(type: "text", nullable: true),
                    Exception = table.Column<string>(type: "text", nullable: true),
                    StackTrace = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Username = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    CorrelationId = table.Column<string>(type: "text", nullable: true),
                    RequestId = table.Column<string>(type: "text", nullable: true),
                    SessionId = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    HttpMethod = table.Column<string>(type: "text", nullable: true),
                    HttpPath = table.Column<string>(type: "text", nullable: true),
                    QueryString = table.Column<string>(type: "text", nullable: true),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    HttpDurationMs = table.Column<long>(type: "bigint", nullable: true),
                    RequestBody = table.Column<string>(type: "text", nullable: true),
                    ResponseBody = table.Column<string>(type: "text", nullable: true),
                    RequestHeaders = table.Column<string>(type: "text", nullable: true),
                    ResponseHeaders = table.Column<string>(type: "text", nullable: true),
                    DbOperation = table.Column<string>(type: "text", nullable: true),
                    DbTable = table.Column<string>(type: "text", nullable: true),
                    DbCommand = table.Column<string>(type: "text", nullable: true),
                    DbParameters = table.Column<string>(type: "text", nullable: true),
                    DbDurationMs = table.Column<long>(type: "bigint", nullable: true),
                    DbRowsAffected = table.Column<int>(type: "integer", nullable: true),
                    EntityType = table.Column<string>(type: "text", nullable: true),
                    EntityId = table.Column<string>(type: "text", nullable: true),
                    EntityAction = table.Column<string>(type: "text", nullable: true),
                    OperationName = table.Column<string>(type: "text", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    MemoryUsedBytes = table.Column<long>(type: "bigint", nullable: true),
                    MachineName = table.Column<string>(type: "text", nullable: true),
                    Environment = table.Column<string>(type: "text", nullable: true),
                    ApplicationName = table.Column<string>(type: "text", nullable: true),
                    ApplicationVersion = table.Column<string>(type: "text", nullable: true),
                    ProcessId = table.Column<int>(type: "integer", nullable: true),
                    ThreadId = table.Column<int>(type: "integer", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_logs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "database_query_logs",
                schema: "logs");

            migrationBuilder.DropTable(
                name: "entity_change_logs",
                schema: "logs");

            migrationBuilder.DropTable(
                name: "http_request_logs",
                schema: "logs");

            migrationBuilder.DropTable(
                name: "page_visit_logs",
                schema: "logs");

            migrationBuilder.DropTable(
                name: "performance_logs",
                schema: "logs");

            migrationBuilder.DropTable(
                name: "security_event_logs",
                schema: "logs");

            migrationBuilder.DropTable(
                name: "system_logs",
                schema: "logs");
        }
    }
}
