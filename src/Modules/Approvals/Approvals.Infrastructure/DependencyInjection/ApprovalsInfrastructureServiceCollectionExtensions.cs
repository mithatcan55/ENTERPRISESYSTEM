using Approvals.Application.Commands;
using Approvals.Application.Queries;
using Approvals.Infrastructure.Delegations.Commands;
using Approvals.Infrastructure.Delegations.Queries;
using Approvals.Infrastructure.Services;
using Approvals.Infrastructure.Workflows;
using Approvals.Infrastructure.Workflows.Commands;
using Approvals.Infrastructure.Workflows.Queries;
using Microsoft.Extensions.DependencyInjection;
using Approvals.Application.Services;

namespace Approvals.Infrastructure.DependencyInjection;

public static class ApprovalsInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddApprovalsInfrastructureModule(this IServiceCollection services)
    {
        services.AddScoped<ApprovalWorkflowResolver>();
        services.AddScoped<ApproverResolutionService>();
        services.AddScoped<ApprovalDeadlineProcessor>();
        services.AddScoped<IApprovalTriggerService, ApprovalTriggerService>();
        services.AddScoped<IListApprovalWorkflowsQueryHandler, ListApprovalWorkflowsQueryHandler>();
        services.AddScoped<IGetApprovalWorkflowDetailQueryHandler, GetApprovalWorkflowDetailQueryHandler>();
        services.AddScoped<IResolveApprovalWorkflowQueryHandler, ResolveApprovalWorkflowQueryHandler>();
        services.AddScoped<ICreateApprovalWorkflowCommandHandler, CreateApprovalWorkflowCommandHandler>();
        services.AddScoped<IUpdateApprovalWorkflowCommandHandler, UpdateApprovalWorkflowCommandHandler>();
        services.AddScoped<IGetApprovalInstanceDetailQueryHandler, GetApprovalInstanceDetailQueryHandler>();
        services.AddScoped<IListPendingApprovalsQueryHandler, ListPendingApprovalsQueryHandler>();
        services.AddScoped<IStartApprovalInstanceCommandHandler, StartApprovalInstanceCommandHandler>();
        services.AddScoped<IDecideApprovalStepCommandHandler, DecideApprovalStepCommandHandler>();
        services.AddScoped<IListDelegationAssignmentsQueryHandler, ListDelegationAssignmentsQueryHandler>();
        services.AddScoped<ICreateDelegationAssignmentCommandHandler, CreateDelegationAssignmentCommandHandler>();
        services.AddScoped<ISetDelegationAssignmentStatusCommandHandler, SetDelegationAssignmentStatusCommandHandler>();
        services.AddHostedService<ApprovalDeadlineProcessorService>();
        return services;
    }
}
