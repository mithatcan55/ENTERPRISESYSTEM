using Approvals.Application.Commands;
using Approvals.Application.Queries;
using Approvals.Infrastructure.Delegations.Commands;
using Approvals.Infrastructure.Delegations.Queries;
using Approvals.Infrastructure.Workflows.Commands;
using Approvals.Infrastructure.Workflows.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Approvals.Infrastructure.DependencyInjection;

public static class ApprovalsInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddApprovalsInfrastructureModule(this IServiceCollection services)
    {
        services.AddScoped<IListApprovalWorkflowsQueryHandler, ListApprovalWorkflowsQueryHandler>();
        services.AddScoped<IGetApprovalWorkflowDetailQueryHandler, GetApprovalWorkflowDetailQueryHandler>();
        services.AddScoped<ICreateApprovalWorkflowCommandHandler, CreateApprovalWorkflowCommandHandler>();
        services.AddScoped<IUpdateApprovalWorkflowCommandHandler, UpdateApprovalWorkflowCommandHandler>();
        services.AddScoped<IListDelegationAssignmentsQueryHandler, ListDelegationAssignmentsQueryHandler>();
        services.AddScoped<ICreateDelegationAssignmentCommandHandler, CreateDelegationAssignmentCommandHandler>();
        return services;
    }
}
