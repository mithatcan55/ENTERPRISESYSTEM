using System;
using global::Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.ApprovalsDb;

[DbContext(typeof(ApprovalsDbContext))]
partial class ApprovalsDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("authorizeSchema")
            .HasAnnotation("ProductVersion", "9.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("Infrastructure.Persistence.Entities.Approvals.ApprovalWorkflowDefinition", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
            b.Property<string>("Code").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            b.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("CreatedBy").HasColumnType("text");
            b.Property<string>("DeletedBy").HasColumnType("text");
            b.Property<DateTime?>("DeletedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("Description").IsRequired().HasMaxLength(2000).HasColumnType("character varying(2000)");
            b.Property<string>("DocumentType").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            b.Property<bool>("IsActive").HasColumnType("boolean");
            b.Property<bool>("IsDeleted").HasColumnType("boolean");
            b.Property<string>("ModifiedBy").HasColumnType("text");
            b.Property<DateTime?>("ModifiedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("ModuleKey").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            b.Property<string>("Name").IsRequired().HasMaxLength(200).HasColumnType("character varying(200)");
            b.HasKey("Id");
            b.HasIndex("Code").IsUnique();
            b.HasIndex("ModuleKey", "DocumentType", "IsActive");
            b.ToTable("ApprovalWorkflowDefinitions", "authorizeSchema");
        });

        modelBuilder.Entity("Infrastructure.Persistence.Entities.Approvals.ApprovalWorkflowStep", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
            b.Property<int>("ApprovalWorkflowDefinitionId").HasColumnType("integer");
            b.Property<string>("ApproverType").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            b.Property<string>("ApproverValue").IsRequired().HasMaxLength(300).HasColumnType("character varying(300)");
            b.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("CreatedBy").HasColumnType("text");
            b.Property<string>("DeletedBy").HasColumnType("text");
            b.Property<DateTime?>("DeletedAt").HasColumnType("timestamp with time zone");
            b.Property<bool>("IsDeleted").HasColumnType("boolean");
            b.Property<bool>("IsParallel").HasColumnType("boolean");
            b.Property<bool>("IsRequired").HasColumnType("boolean");
            b.Property<int>("MinimumApproverCount").HasColumnType("integer");
            b.Property<string>("ModifiedBy").HasColumnType("text");
            b.Property<DateTime?>("ModifiedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("Name").IsRequired().HasMaxLength(200).HasColumnType("character varying(200)");
            b.Property<int>("StepOrder").HasColumnType("integer");
            b.HasKey("Id");
            b.HasIndex("ApprovalWorkflowDefinitionId", "StepOrder").IsUnique();
            b.ToTable("ApprovalWorkflowSteps", "authorizeSchema");
        });

        modelBuilder.Entity("Infrastructure.Persistence.Entities.Approvals.ApprovalWorkflowCondition", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
            b.Property<int>("ApprovalWorkflowDefinitionId").HasColumnType("integer");
            b.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("CreatedBy").HasColumnType("text");
            b.Property<string>("DeletedBy").HasColumnType("text");
            b.Property<DateTime?>("DeletedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("FieldKey").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            b.Property<bool>("IsDeleted").HasColumnType("boolean");
            b.Property<string>("ModifiedBy").HasColumnType("text");
            b.Property<DateTime?>("ModifiedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("Operator").IsRequired().HasMaxLength(50).HasColumnType("character varying(50)");
            b.Property<string>("Value").IsRequired().HasMaxLength(1000).HasColumnType("character varying(1000)");
            b.HasKey("Id");
            b.HasIndex("ApprovalWorkflowDefinitionId", "FieldKey", "Operator");
            b.ToTable("ApprovalWorkflowConditions", "authorizeSchema");
        });

        modelBuilder.Entity("Infrastructure.Persistence.Entities.Approvals.ApprovalInstance", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
            b.Property<int>("ApprovalWorkflowDefinitionId").HasColumnType("integer");
            b.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("CreatedBy").HasColumnType("text");
            b.Property<int>("CurrentStepOrder").HasColumnType("integer");
            b.Property<string>("DeletedBy").HasColumnType("text");
            b.Property<DateTime?>("DeletedAt").HasColumnType("timestamp with time zone");
            b.Property<bool>("IsDeleted").HasColumnType("boolean");
            b.Property<string>("ModifiedBy").HasColumnType("text");
            b.Property<DateTime?>("ModifiedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("PayloadJson").IsRequired().HasColumnType("text");
            b.Property<string>("ReferenceId").IsRequired().HasMaxLength(200).HasColumnType("character varying(200)");
            b.Property<string>("ReferenceType").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            b.Property<int>("RequesterUserId").HasColumnType("integer");
            b.Property<string>("Status").IsRequired().HasMaxLength(50).HasColumnType("character varying(50)");
            b.HasKey("Id");
            b.HasIndex("ApprovalWorkflowDefinitionId");
            b.HasIndex("ReferenceType", "ReferenceId", "Status");
            b.ToTable("ApprovalInstances", "authorizeSchema");
        });

        modelBuilder.Entity("Infrastructure.Persistence.Entities.Approvals.ApprovalInstanceStep", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
            b.Property<int?>("AssignedUserId").HasColumnType("integer");
            b.Property<int>("ApprovalInstanceId").HasColumnType("integer");
            b.Property<int>("ApprovalWorkflowStepId").HasColumnType("integer");
            b.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("CreatedBy").HasColumnType("text");
            b.Property<string>("DeletedBy").HasColumnType("text");
            b.Property<DateTime?>("DeletedAt").HasColumnType("timestamp with time zone");
            b.Property<DateTime?>("DueAt").HasColumnType("timestamp with time zone");
            b.Property<bool>("IsDeleted").HasColumnType("boolean");
            b.Property<string>("ModifiedBy").HasColumnType("text");
            b.Property<DateTime?>("ModifiedAt").HasColumnType("timestamp with time zone");
            b.Property<int>("StepOrder").HasColumnType("integer");
            b.Property<string>("Status").IsRequired().HasMaxLength(50).HasColumnType("character varying(50)");
            b.HasKey("Id");
            b.HasIndex("ApprovalInstanceId", "StepOrder");
            b.HasIndex("ApprovalWorkflowStepId");
            b.HasIndex("AssignedUserId", "Status");
            b.ToTable("ApprovalInstanceSteps", "authorizeSchema");
        });

        modelBuilder.Entity("Infrastructure.Persistence.Entities.Approvals.ApprovalDecision", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
            b.Property<int>("ActorUserId").HasColumnType("integer");
            b.Property<int>("ApprovalInstanceStepId").HasColumnType("integer");
            b.Property<string>("Comment").IsRequired().HasMaxLength(2000).HasColumnType("character varying(2000)");
            b.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("CreatedBy").HasColumnType("text");
            b.Property<string>("Decision").IsRequired().HasMaxLength(50).HasColumnType("character varying(50)");
            b.Property<string>("DeletedBy").HasColumnType("text");
            b.Property<DateTime?>("DeletedAt").HasColumnType("timestamp with time zone");
            b.Property<bool>("IsDeleted").HasColumnType("boolean");
            b.Property<string>("ModifiedBy").HasColumnType("text");
            b.Property<DateTime?>("ModifiedAt").HasColumnType("timestamp with time zone");
            b.HasKey("Id");
            b.HasIndex("ApprovalInstanceStepId", "ActorUserId", "CreatedAt");
            b.ToTable("ApprovalDecisions", "authorizeSchema");
        });

        modelBuilder.Entity("Infrastructure.Persistence.Entities.Approvals.DelegationAssignment", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
            b.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("CreatedBy").HasColumnType("text");
            b.Property<int>("DelegateUserId").HasColumnType("integer");
            b.Property<int>("DelegatorUserId").HasColumnType("integer");
            b.Property<string>("DeletedBy").HasColumnType("text");
            b.Property<DateTime?>("DeletedAt").HasColumnType("timestamp with time zone");
            b.Property<DateTime>("EndsAt").HasColumnType("timestamp with time zone");
            b.Property<string>("ExcludedScopesJson").IsRequired().HasColumnType("text");
            b.Property<string>("IncludedScopesJson").IsRequired().HasColumnType("text");
            b.Property<bool>("IsActive").HasColumnType("boolean");
            b.Property<bool>("IsDeleted").HasColumnType("boolean");
            b.Property<string>("ModifiedBy").HasColumnType("text");
            b.Property<DateTime?>("ModifiedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("Notes").IsRequired().HasMaxLength(2000).HasColumnType("character varying(2000)");
            b.Property<string>("ScopeType").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            b.Property<DateTime>("StartsAt").HasColumnType("timestamp with time zone");
            b.HasKey("Id");
            b.HasIndex("DelegatorUserId", "DelegateUserId", "IsActive", "EndsAt");
            b.ToTable("DelegationAssignments", "authorizeSchema");
        });

        modelBuilder.Entity("Infrastructure.Persistence.Entities.Approvals.ApprovalInstance", b =>
        {
            b.HasOne("Infrastructure.Persistence.Entities.Approvals.ApprovalWorkflowDefinition", null)
                .WithMany()
                .HasForeignKey("ApprovalWorkflowDefinitionId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });

        modelBuilder.Entity("Infrastructure.Persistence.Entities.Approvals.ApprovalWorkflowCondition", b =>
        {
            b.HasOne("Infrastructure.Persistence.Entities.Approvals.ApprovalWorkflowDefinition", null)
                .WithMany()
                .HasForeignKey("ApprovalWorkflowDefinitionId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });

        modelBuilder.Entity("Infrastructure.Persistence.Entities.Approvals.ApprovalWorkflowStep", b =>
        {
            b.HasOne("Infrastructure.Persistence.Entities.Approvals.ApprovalWorkflowDefinition", null)
                .WithMany()
                .HasForeignKey("ApprovalWorkflowDefinitionId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });

        modelBuilder.Entity("Infrastructure.Persistence.Entities.Approvals.ApprovalInstanceStep", b =>
        {
            b.HasOne("Infrastructure.Persistence.Entities.Approvals.ApprovalInstance", null)
                .WithMany()
                .HasForeignKey("ApprovalInstanceId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.HasOne("Infrastructure.Persistence.Entities.Approvals.ApprovalWorkflowStep", null)
                .WithMany()
                .HasForeignKey("ApprovalWorkflowStepId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });

        modelBuilder.Entity("Infrastructure.Persistence.Entities.Approvals.ApprovalDecision", b =>
        {
            b.HasOne("Infrastructure.Persistence.Entities.Approvals.ApprovalInstanceStep", null)
                .WithMany()
                .HasForeignKey("ApprovalInstanceStepId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });
    }
}
