using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Persistence.Migrations.BusinessDb
{
    /// <inheritdoc />
    public partial class InitialAuthorizationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "authorizeSchema");

            migrationBuilder.CreateTable(
                name: "Modules",
                schema: "authorizeSchema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    RouteLink = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Modules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserCompanyPermissions",
                schema: "authorizeSchema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    AuthorizationLevel = table.Column<short>(type: "smallint", nullable: false),
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
                    table.PrimaryKey("PK_UserCompanyPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubModules",
                schema: "authorizeSchema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ModuleId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RouteLink = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_SubModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubModules_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "authorizeSchema",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserModulePermissions",
                schema: "authorizeSchema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ModuleId = table.Column<int>(type: "integer", nullable: false),
                    AuthorizationLevel = table.Column<short>(type: "smallint", nullable: false),
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
                    table.PrimaryKey("PK_UserModulePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserModulePermissions_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "authorizeSchema",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubModulePages",
                schema: "authorizeSchema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubModuleId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TransactionCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RouteLink = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_SubModulePages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubModulePages_SubModules_SubModuleId",
                        column: x => x.SubModuleId,
                        principalSchema: "authorizeSchema",
                        principalTable: "SubModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSubModulePermissions",
                schema: "authorizeSchema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SubModuleId = table.Column<int>(type: "integer", nullable: false),
                    AuthorizationLevel = table.Column<short>(type: "smallint", nullable: false),
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
                    table.PrimaryKey("PK_UserSubModulePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSubModulePermissions_SubModules_SubModuleId",
                        column: x => x.SubModuleId,
                        principalSchema: "authorizeSchema",
                        principalTable: "SubModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPageActionPermissions",
                schema: "authorizeSchema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SubModulePageId = table.Column<int>(type: "integer", nullable: false),
                    ActionCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    AuthorizationLevel = table.Column<short>(type: "smallint", nullable: false),
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
                    table.PrimaryKey("PK_UserPageActionPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPageActionPermissions_SubModulePages_SubModulePageId",
                        column: x => x.SubModulePageId,
                        principalSchema: "authorizeSchema",
                        principalTable: "SubModulePages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPageConditionPermissions",
                schema: "authorizeSchema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SubModulePageId = table.Column<int>(type: "integer", nullable: false),
                    FieldName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Operator = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AuthorizationLevel = table.Column<short>(type: "smallint", nullable: false),
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
                    table.PrimaryKey("PK_UserPageConditionPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPageConditionPermissions_SubModulePages_SubModulePageId",
                        column: x => x.SubModulePageId,
                        principalSchema: "authorizeSchema",
                        principalTable: "SubModulePages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPagePermissions",
                schema: "authorizeSchema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SubModulePageId = table.Column<int>(type: "integer", nullable: false),
                    AuthorizationLevel = table.Column<short>(type: "smallint", nullable: false),
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
                    table.PrimaryKey("PK_UserPagePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPagePermissions_SubModulePages_SubModulePageId",
                        column: x => x.SubModulePageId,
                        principalSchema: "authorizeSchema",
                        principalTable: "SubModulePages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "authorizeSchema",
                table: "Modules",
                columns: new[] { "Id", "Code", "CompanyId", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "ModifiedAt", "ModifiedBy", "Name", "RouteLink" },
                values: new object[] { 1, "SYS", 1, new DateTime(2026, 3, 3, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, null, "Sistem yönetimi ana modülü", false, null, null, "System", "/system" });

            migrationBuilder.InsertData(
                schema: "authorizeSchema",
                table: "SubModules",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "ModifiedAt", "ModifiedBy", "ModuleId", "Name", "RouteLink" },
                values: new object[] { 1, "SYS_USER", new DateTime(2026, 3, 3, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, null, "Kullanıcı işlemleri", false, null, null, 1, "UserManagement", "/system/users" });

            migrationBuilder.InsertData(
                schema: "authorizeSchema",
                table: "SubModulePages",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "IsDeleted", "ModifiedAt", "ModifiedBy", "Name", "RouteLink", "SubModuleId", "TransactionCode" },
                values: new object[,]
                {
                    { 1, "USER_CREATE", new DateTime(2026, 3, 3, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, null, false, null, null, "Create User", "/system/users/create", 1, "SYS01" },
                    { 2, "USER_UPDATE", new DateTime(2026, 3, 3, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, null, false, null, null, "Update User", "/system/users/update", 1, "SYS02" },
                    { 3, "USER_VIEW", new DateTime(2026, 3, 3, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, null, false, null, null, "View User", "/system/users/view", 1, "SYS03" },
                    { 4, "USER_REPORT", new DateTime(2026, 3, 3, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, null, false, null, null, "User Report", "/system/users/report", 1, "SYS04" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Code",
                schema: "authorizeSchema",
                table: "Modules",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubModulePages_Code",
                schema: "authorizeSchema",
                table: "SubModulePages",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubModulePages_SubModuleId",
                schema: "authorizeSchema",
                table: "SubModulePages",
                column: "SubModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_SubModulePages_TransactionCode",
                schema: "authorizeSchema",
                table: "SubModulePages",
                column: "TransactionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubModules_Code",
                schema: "authorizeSchema",
                table: "SubModules",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubModules_ModuleId",
                schema: "authorizeSchema",
                table: "SubModules",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCompanyPermissions_UserId_CompanyId",
                schema: "authorizeSchema",
                table: "UserCompanyPermissions",
                columns: new[] { "UserId", "CompanyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserModulePermissions_ModuleId",
                schema: "authorizeSchema",
                table: "UserModulePermissions",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserModulePermissions_UserId_ModuleId",
                schema: "authorizeSchema",
                table: "UserModulePermissions",
                columns: new[] { "UserId", "ModuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPageActionPermissions_SubModulePageId",
                schema: "authorizeSchema",
                table: "UserPageActionPermissions",
                column: "SubModulePageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPageActionPermissions_UserId_SubModulePageId_ActionCode",
                schema: "authorizeSchema",
                table: "UserPageActionPermissions",
                columns: new[] { "UserId", "SubModulePageId", "ActionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPageConditionPermissions_SubModulePageId",
                schema: "authorizeSchema",
                table: "UserPageConditionPermissions",
                column: "SubModulePageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPageConditionPermissions_UserId_SubModulePageId_FieldNa~",
                schema: "authorizeSchema",
                table: "UserPageConditionPermissions",
                columns: new[] { "UserId", "SubModulePageId", "FieldName", "Operator", "Value" });

            migrationBuilder.CreateIndex(
                name: "IX_UserPagePermissions_SubModulePageId",
                schema: "authorizeSchema",
                table: "UserPagePermissions",
                column: "SubModulePageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPagePermissions_UserId_SubModulePageId",
                schema: "authorizeSchema",
                table: "UserPagePermissions",
                columns: new[] { "UserId", "SubModulePageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSubModulePermissions_SubModuleId",
                schema: "authorizeSchema",
                table: "UserSubModulePermissions",
                column: "SubModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubModulePermissions_UserId_SubModuleId",
                schema: "authorizeSchema",
                table: "UserSubModulePermissions",
                columns: new[] { "UserId", "SubModuleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCompanyPermissions",
                schema: "authorizeSchema");

            migrationBuilder.DropTable(
                name: "UserModulePermissions",
                schema: "authorizeSchema");

            migrationBuilder.DropTable(
                name: "UserPageActionPermissions",
                schema: "authorizeSchema");

            migrationBuilder.DropTable(
                name: "UserPageConditionPermissions",
                schema: "authorizeSchema");

            migrationBuilder.DropTable(
                name: "UserPagePermissions",
                schema: "authorizeSchema");

            migrationBuilder.DropTable(
                name: "UserSubModulePermissions",
                schema: "authorizeSchema");

            migrationBuilder.DropTable(
                name: "SubModulePages",
                schema: "authorizeSchema");

            migrationBuilder.DropTable(
                name: "SubModules",
                schema: "authorizeSchema");

            migrationBuilder.DropTable(
                name: "Modules",
                schema: "authorizeSchema");
        }
    }
}
