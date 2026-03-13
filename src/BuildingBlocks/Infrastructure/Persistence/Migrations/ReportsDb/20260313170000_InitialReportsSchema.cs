using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.ReportsDb
{
    [DbContext(typeof(ReportsDbContext))]
    [Migration("20260313170000_InitialReportsSchema")]
    public partial class InitialReportsSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "authorizeSchema");

            migrationBuilder.CreateTable(
                name: "ReportTemplates",
                schema: "authorizeSchema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ModuleKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CurrentVersionNumber = table.Column<int>(type: "integer", nullable: false),
                    PublishedVersionNumber = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportTemplateVersions",
                schema: "authorizeSchema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReportTemplateId = table.Column<int>(type: "integer", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    TemplateJson = table.Column<string>(type: "text", nullable: false),
                    SampleInputJson = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTemplateVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportTemplateVersions_ReportTemplates_ReportTemplateId",
                        column: x => x.ReportTemplateId,
                        principalSchema: "authorizeSchema",
                        principalTable: "ReportTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplates_Code",
                schema: "authorizeSchema",
                table: "ReportTemplates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplates_ModuleKey_Status",
                schema: "authorizeSchema",
                table: "ReportTemplates",
                columns: new[] { "ModuleKey", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplateVersions_ReportTemplateId_IsPublished",
                schema: "authorizeSchema",
                table: "ReportTemplateVersions",
                columns: new[] { "ReportTemplateId", "IsPublished" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplateVersions_ReportTemplateId_VersionNumber",
                schema: "authorizeSchema",
                table: "ReportTemplateVersions",
                columns: new[] { "ReportTemplateId", "VersionNumber" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportTemplateVersions",
                schema: "authorizeSchema");

            migrationBuilder.DropTable(
                name: "ReportTemplates",
                schema: "authorizeSchema");
        }
    }
}
