using System;
using global::Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.ApprovalsDb;

[DbContext(typeof(ApprovalsDbContext))]
[Migration("20260316113000_AddDelegationLifecycleAndApprovalDeadlines")]
public partial class AddDelegationLifecycleAndApprovalDeadlines : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "DecisionDeadlineHours",
            schema: "authorizeSchema",
            table: "ApprovalWorkflowSteps",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TimeoutDecision",
            schema: "authorizeSchema",
            table: "ApprovalWorkflowSteps",
            type: "character varying(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "reject");

        migrationBuilder.AddColumn<bool>(
            name: "IsSystemDecision",
            schema: "authorizeSchema",
            table: "ApprovalDecisions",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<int>(
            name: "RevokedByUserId",
            schema: "authorizeSchema",
            table: "DelegationAssignments",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "RevokedAt",
            schema: "authorizeSchema",
            table: "DelegationAssignments",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "RevokedReason",
            schema: "authorizeSchema",
            table: "DelegationAssignments",
            type: "character varying(2000)",
            maxLength: 2000,
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.CreateIndex(
            name: "IX_ApprovalDecisions_IsSystemDecision_Decision_CreatedAt",
            schema: "authorizeSchema",
            table: "ApprovalDecisions",
            columns: new[] { "IsSystemDecision", "Decision", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_DelegationAssignments_DelegatorUserId_IsActive_StartsAt_EndsAt",
            schema: "authorizeSchema",
            table: "DelegationAssignments",
            columns: new[] { "DelegatorUserId", "IsActive", "StartsAt", "EndsAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_ApprovalDecisions_IsSystemDecision_Decision_CreatedAt",
            schema: "authorizeSchema",
            table: "ApprovalDecisions");

        migrationBuilder.DropIndex(
            name: "IX_DelegationAssignments_DelegatorUserId_IsActive_StartsAt_EndsAt",
            schema: "authorizeSchema",
            table: "DelegationAssignments");

        migrationBuilder.DropColumn(
            name: "DecisionDeadlineHours",
            schema: "authorizeSchema",
            table: "ApprovalWorkflowSteps");

        migrationBuilder.DropColumn(
            name: "TimeoutDecision",
            schema: "authorizeSchema",
            table: "ApprovalWorkflowSteps");

        migrationBuilder.DropColumn(
            name: "IsSystemDecision",
            schema: "authorizeSchema",
            table: "ApprovalDecisions");

        migrationBuilder.DropColumn(
            name: "RevokedByUserId",
            schema: "authorizeSchema",
            table: "DelegationAssignments");

        migrationBuilder.DropColumn(
            name: "RevokedAt",
            schema: "authorizeSchema",
            table: "DelegationAssignments");

        migrationBuilder.DropColumn(
            name: "RevokedReason",
            schema: "authorizeSchema",
            table: "DelegationAssignments");
    }
}
