using System;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.BusinessDb
{
    [DbContext(typeof(BusinessDbContext))]
    [Migration("20260316170000_AddAuthorizationFieldPolicies")]
    /// <inheritdoc />
    public partial class AddAuthorizationFieldPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthorizationFieldDefinitions",
                schema: "authorizeSchema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FieldName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DataType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AllowedSurfaces = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DefaultVisible = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultEditable = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultFilterable = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultExportable = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_AuthorizationFieldDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthorizationFieldPolicies",
                schema: "authorizeSchema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FieldName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Surface = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Effect = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConditionFieldName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ConditionOperator = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CompareValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MaskingMode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_AuthorizationFieldPolicies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationFieldDefinitions_EntityName_FieldName",
                schema: "authorizeSchema",
                table: "AuthorizationFieldDefinitions",
                columns: new[] { "EntityName", "FieldName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationFieldPolicies_EntityName_FieldName_Surface_Priori~",
                schema: "authorizeSchema",
                table: "AuthorizationFieldPolicies",
                columns: new[] { "EntityName", "FieldName", "Surface", "Priority" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorizationFieldPolicies",
                schema: "authorizeSchema");

            migrationBuilder.DropTable(
                name: "AuthorizationFieldDefinitions",
                schema: "authorizeSchema");
        }
    }
}
