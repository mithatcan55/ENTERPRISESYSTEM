using System;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.DocumentsDb;

[DbContext(typeof(DocumentsDbContext))]
[Migration("20260317093000_InitialDocumentsSchema")]
public partial class InitialDocumentsSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ManagedDocuments",
            schema: "authorizeSchema",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                table.PrimaryKey("PK_ManagedDocuments", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "DocumentAssociations",
            schema: "authorizeSchema",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ManagedDocumentId = table.Column<int>(type: "integer", nullable: false),
                OwnerEntityName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                OwnerEntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                LinkType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                table.PrimaryKey("PK_DocumentAssociations", x => x.Id);
                table.ForeignKey(
                    name: "FK_DocumentAssociations_ManagedDocuments_ManagedDocumentId",
                    column: x => x.ManagedDocumentId,
                    principalSchema: "authorizeSchema",
                    principalTable: "ManagedDocuments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ManagedDocumentVersions",
            schema: "authorizeSchema",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ManagedDocumentId = table.Column<int>(type: "integer", nullable: false),
                VersionNumber = table.Column<int>(type: "integer", nullable: false),
                FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                ContentType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                StoragePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                FileSize = table.Column<long>(type: "bigint", nullable: false),
                Checksum = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                ChangeNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
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
                table.PrimaryKey("PK_ManagedDocumentVersions", x => x.Id);
                table.ForeignKey(
                    name: "FK_ManagedDocumentVersions_ManagedDocuments_ManagedDocumentId",
                    column: x => x.ManagedDocumentId,
                    principalSchema: "authorizeSchema",
                    principalTable: "ManagedDocuments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DocumentAssociations_ManagedDocumentId_OwnerEntityName_OwnerEn~",
            schema: "authorizeSchema",
            table: "DocumentAssociations",
            columns: new[] { "ManagedDocumentId", "OwnerEntityName", "OwnerEntityId", "LinkType" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_DocumentAssociations_OwnerEntityName_OwnerEntityId_LinkType",
            schema: "authorizeSchema",
            table: "DocumentAssociations",
            columns: new[] { "OwnerEntityName", "OwnerEntityId", "LinkType" });

        migrationBuilder.CreateIndex(
            name: "IX_ManagedDocuments_Code",
            schema: "authorizeSchema",
            table: "ManagedDocuments",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ManagedDocuments_DocumentType_Status",
            schema: "authorizeSchema",
            table: "ManagedDocuments",
            columns: new[] { "DocumentType", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_ManagedDocumentVersions_ManagedDocumentId_IsCurrent",
            schema: "authorizeSchema",
            table: "ManagedDocumentVersions",
            columns: new[] { "ManagedDocumentId", "IsCurrent" });

        migrationBuilder.CreateIndex(
            name: "IX_ManagedDocumentVersions_ManagedDocumentId_VersionNumber",
            schema: "authorizeSchema",
            table: "ManagedDocumentVersions",
            columns: new[] { "ManagedDocumentId", "VersionNumber" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DocumentAssociations",
            schema: "authorizeSchema");

        migrationBuilder.DropTable(
            name: "ManagedDocumentVersions",
            schema: "authorizeSchema");

        migrationBuilder.DropTable(
            name: "ManagedDocuments",
            schema: "authorizeSchema");
    }
}
