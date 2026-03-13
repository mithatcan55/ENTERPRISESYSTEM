using System;
using global::Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.ApprovalsDb;

[DbContext(typeof(ApprovalsDbContext))]
[Migration("20260313183000_InitialApprovalsSchema")]
public partial class InitialApprovalsSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "authorizeSchema");

        migrationBuilder.CreateTable(
            name: "ApprovalWorkflowDefinitions",
            schema: "authorizeSchema",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Code = table.Column<string>(maxLength: 100, nullable: false),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Description = table.Column<string>(maxLength: 2000, nullable: false),
                ModuleKey = table.Column<string>(maxLength: 100, nullable: false),
                DocumentType = table.Column<string>(maxLength: 100, nullable: false),
                IsActive = table.Column<bool>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedBy = table.Column<string>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                ModifiedBy = table.Column<string>(nullable: true),
                ModifiedAt = table.Column<DateTime>(nullable: true),
                DeletedBy = table.Column<string>(nullable: true),
                DeletedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApprovalWorkflowDefinitions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ApprovalInstances",
            schema: "authorizeSchema",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ApprovalWorkflowDefinitionId = table.Column<int>(nullable: false),
                ReferenceType = table.Column<string>(maxLength: 100, nullable: false),
                ReferenceId = table.Column<string>(maxLength: 200, nullable: false),
                RequesterUserId = table.Column<int>(nullable: false),
                Status = table.Column<string>(maxLength: 50, nullable: false),
                CurrentStepOrder = table.Column<int>(nullable: false),
                PayloadJson = table.Column<string>(type: "text", nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedBy = table.Column<string>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                ModifiedBy = table.Column<string>(nullable: true),
                ModifiedAt = table.Column<DateTime>(nullable: true),
                DeletedBy = table.Column<string>(nullable: true),
                DeletedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApprovalInstances", x => x.Id);
                table.ForeignKey(
                    name: "FK_ApprovalInstances_ApprovalWorkflowDefinitions_ApprovalWorkflowDefinitionId",
                    column: x => x.ApprovalWorkflowDefinitionId,
                    principalSchema: "authorizeSchema",
                    principalTable: "ApprovalWorkflowDefinitions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ApprovalWorkflowConditions",
            schema: "authorizeSchema",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ApprovalWorkflowDefinitionId = table.Column<int>(nullable: false),
                FieldKey = table.Column<string>(maxLength: 100, nullable: false),
                Operator = table.Column<string>(maxLength: 50, nullable: false),
                Value = table.Column<string>(maxLength: 1000, nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedBy = table.Column<string>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                ModifiedBy = table.Column<string>(nullable: true),
                ModifiedAt = table.Column<DateTime>(nullable: true),
                DeletedBy = table.Column<string>(nullable: true),
                DeletedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApprovalWorkflowConditions", x => x.Id);
                table.ForeignKey(
                    name: "FK_ApprovalWorkflowConditions_ApprovalWorkflowDefinitions_ApprovalWorkflowDefinitionId",
                    column: x => x.ApprovalWorkflowDefinitionId,
                    principalSchema: "authorizeSchema",
                    principalTable: "ApprovalWorkflowDefinitions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ApprovalWorkflowSteps",
            schema: "authorizeSchema",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ApprovalWorkflowDefinitionId = table.Column<int>(nullable: false),
                StepOrder = table.Column<int>(nullable: false),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                ApproverType = table.Column<string>(maxLength: 100, nullable: false),
                ApproverValue = table.Column<string>(maxLength: 300, nullable: false),
                IsRequired = table.Column<bool>(nullable: false),
                IsParallel = table.Column<bool>(nullable: false),
                MinimumApproverCount = table.Column<int>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedBy = table.Column<string>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                ModifiedBy = table.Column<string>(nullable: true),
                ModifiedAt = table.Column<DateTime>(nullable: true),
                DeletedBy = table.Column<string>(nullable: true),
                DeletedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApprovalWorkflowSteps", x => x.Id);
                table.ForeignKey(
                    name: "FK_ApprovalWorkflowSteps_ApprovalWorkflowDefinitions_ApprovalWorkflowDefinitionId",
                    column: x => x.ApprovalWorkflowDefinitionId,
                    principalSchema: "authorizeSchema",
                    principalTable: "ApprovalWorkflowDefinitions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "DelegationAssignments",
            schema: "authorizeSchema",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                DelegatorUserId = table.Column<int>(nullable: false),
                DelegateUserId = table.Column<int>(nullable: false),
                ScopeType = table.Column<string>(maxLength: 100, nullable: false),
                IncludedScopesJson = table.Column<string>(type: "text", nullable: false),
                ExcludedScopesJson = table.Column<string>(type: "text", nullable: false),
                StartsAt = table.Column<DateTime>(nullable: false),
                EndsAt = table.Column<DateTime>(nullable: false),
                IsActive = table.Column<bool>(nullable: false),
                Notes = table.Column<string>(maxLength: 2000, nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedBy = table.Column<string>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                ModifiedBy = table.Column<string>(nullable: true),
                ModifiedAt = table.Column<DateTime>(nullable: true),
                DeletedBy = table.Column<string>(nullable: true),
                DeletedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DelegationAssignments", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ApprovalInstanceSteps",
            schema: "authorizeSchema",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ApprovalInstanceId = table.Column<int>(nullable: false),
                ApprovalWorkflowStepId = table.Column<int>(nullable: false),
                StepOrder = table.Column<int>(nullable: false),
                AssignedUserId = table.Column<int>(nullable: true),
                Status = table.Column<string>(maxLength: 50, nullable: false),
                DueAt = table.Column<DateTime>(nullable: true),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedBy = table.Column<string>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                ModifiedBy = table.Column<string>(nullable: true),
                ModifiedAt = table.Column<DateTime>(nullable: true),
                DeletedBy = table.Column<string>(nullable: true),
                DeletedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApprovalInstanceSteps", x => x.Id);
                table.ForeignKey(
                    name: "FK_ApprovalInstanceSteps_ApprovalInstances_ApprovalInstanceId",
                    column: x => x.ApprovalInstanceId,
                    principalSchema: "authorizeSchema",
                    principalTable: "ApprovalInstances",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ApprovalInstanceSteps_ApprovalWorkflowSteps_ApprovalWorkflowStepId",
                    column: x => x.ApprovalWorkflowStepId,
                    principalSchema: "authorizeSchema",
                    principalTable: "ApprovalWorkflowSteps",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ApprovalDecisions",
            schema: "authorizeSchema",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ApprovalInstanceStepId = table.Column<int>(nullable: false),
                ActorUserId = table.Column<int>(nullable: false),
                Decision = table.Column<string>(maxLength: 50, nullable: false),
                Comment = table.Column<string>(maxLength: 2000, nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedBy = table.Column<string>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                ModifiedBy = table.Column<string>(nullable: true),
                ModifiedAt = table.Column<DateTime>(nullable: true),
                DeletedBy = table.Column<string>(nullable: true),
                DeletedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApprovalDecisions", x => x.Id);
                table.ForeignKey(
                    name: "FK_ApprovalDecisions_ApprovalInstanceSteps_ApprovalInstanceStepId",
                    column: x => x.ApprovalInstanceStepId,
                    principalSchema: "authorizeSchema",
                    principalTable: "ApprovalInstanceSteps",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_ApprovalWorkflowDefinitions_Code", schema: "authorizeSchema", table: "ApprovalWorkflowDefinitions", column: "Code", unique: true);
        migrationBuilder.CreateIndex(name: "IX_ApprovalWorkflowDefinitions_ModuleKey_DocumentType_IsActive", schema: "authorizeSchema", table: "ApprovalWorkflowDefinitions", columns: new[] { "ModuleKey", "DocumentType", "IsActive" });
        migrationBuilder.CreateIndex(name: "IX_ApprovalInstances_ApprovalWorkflowDefinitionId", schema: "authorizeSchema", table: "ApprovalInstances", column: "ApprovalWorkflowDefinitionId");
        migrationBuilder.CreateIndex(name: "IX_ApprovalInstances_ReferenceType_ReferenceId_Status", schema: "authorizeSchema", table: "ApprovalInstances", columns: new[] { "ReferenceType", "ReferenceId", "Status" });
        migrationBuilder.CreateIndex(name: "IX_ApprovalWorkflowConditions_ApprovalWorkflowDefinitionId_FieldKey_Operator", schema: "authorizeSchema", table: "ApprovalWorkflowConditions", columns: new[] { "ApprovalWorkflowDefinitionId", "FieldKey", "Operator" });
        migrationBuilder.CreateIndex(name: "IX_ApprovalWorkflowSteps_ApprovalWorkflowDefinitionId_StepOrder", schema: "authorizeSchema", table: "ApprovalWorkflowSteps", columns: new[] { "ApprovalWorkflowDefinitionId", "StepOrder" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_DelegationAssignments_DelegatorUserId_DelegateUserId_IsActive_EndsAt", schema: "authorizeSchema", table: "DelegationAssignments", columns: new[] { "DelegatorUserId", "DelegateUserId", "IsActive", "EndsAt" });
        migrationBuilder.CreateIndex(name: "IX_ApprovalInstanceSteps_ApprovalInstanceId_StepOrder", schema: "authorizeSchema", table: "ApprovalInstanceSteps", columns: new[] { "ApprovalInstanceId", "StepOrder" });
        migrationBuilder.CreateIndex(name: "IX_ApprovalInstanceSteps_ApprovalWorkflowStepId", schema: "authorizeSchema", table: "ApprovalInstanceSteps", column: "ApprovalWorkflowStepId");
        migrationBuilder.CreateIndex(name: "IX_ApprovalInstanceSteps_AssignedUserId_Status", schema: "authorizeSchema", table: "ApprovalInstanceSteps", columns: new[] { "AssignedUserId", "Status" });
        migrationBuilder.CreateIndex(name: "IX_ApprovalDecisions_ApprovalInstanceStepId_ActorUserId_CreatedAt", schema: "authorizeSchema", table: "ApprovalDecisions", columns: new[] { "ApprovalInstanceStepId", "ActorUserId", "CreatedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ApprovalDecisions", schema: "authorizeSchema");
        migrationBuilder.DropTable(name: "DelegationAssignments", schema: "authorizeSchema");
        migrationBuilder.DropTable(name: "ApprovalInstanceSteps", schema: "authorizeSchema");
        migrationBuilder.DropTable(name: "ApprovalWorkflowConditions", schema: "authorizeSchema");
        migrationBuilder.DropTable(name: "ApprovalInstances", schema: "authorizeSchema");
        migrationBuilder.DropTable(name: "ApprovalWorkflowSteps", schema: "authorizeSchema");
        migrationBuilder.DropTable(name: "ApprovalWorkflowDefinitions", schema: "authorizeSchema");
    }
}
