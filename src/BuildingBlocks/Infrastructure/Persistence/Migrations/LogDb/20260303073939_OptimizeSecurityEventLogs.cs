using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.LogDb
{
    /// <inheritdoc />
    public partial class OptimizeSecurityEventLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_security_event_logs_EventType_Timestamp",
                schema: "logs",
                table: "security_event_logs",
                columns: new[] { "EventType", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_security_event_logs_Resource_IsSuccessful_Timestamp",
                schema: "logs",
                table: "security_event_logs",
                columns: new[] { "Resource", "IsSuccessful", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_security_event_logs_Timestamp",
                schema: "logs",
                table: "security_event_logs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_security_event_logs_UserId_Timestamp",
                schema: "logs",
                table: "security_event_logs",
                columns: new[] { "UserId", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_security_event_logs_EventType_Timestamp",
                schema: "logs",
                table: "security_event_logs");

            migrationBuilder.DropIndex(
                name: "IX_security_event_logs_Resource_IsSuccessful_Timestamp",
                schema: "logs",
                table: "security_event_logs");

            migrationBuilder.DropIndex(
                name: "IX_security_event_logs_Timestamp",
                schema: "logs",
                table: "security_event_logs");

            migrationBuilder.DropIndex(
                name: "IX_security_event_logs_UserId_Timestamp",
                schema: "logs",
                table: "security_event_logs");
        }
    }
}
