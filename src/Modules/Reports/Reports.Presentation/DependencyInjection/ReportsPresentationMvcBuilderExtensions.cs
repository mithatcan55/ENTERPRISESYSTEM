using Microsoft.Extensions.DependencyInjection;

namespace Reports.Presentation.DependencyInjection;

public static class ReportsPresentationMvcBuilderExtensions
{
    public static IMvcBuilder AddReportsPresentationModule(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddApplicationPart(typeof(ReportsPresentationMvcBuilderExtensions).Assembly);
        return mvcBuilder;
    }
}
