using Microsoft.Extensions.DependencyInjection;

namespace Approvals.Presentation.DependencyInjection;

public static class ApprovalsPresentationMvcBuilderExtensions
{
    public static IMvcBuilder AddApprovalsPresentationModule(this IMvcBuilder builder)
    {
        builder.AddApplicationPart(typeof(ApprovalsPresentationMvcBuilderExtensions).Assembly);
        return builder;
    }
}
