using Microsoft.Extensions.DependencyInjection;

namespace Operations.Presentation.DependencyInjection;

public static class OperationsPresentationMvcBuilderExtensions
{
    public static IMvcBuilder AddOperationsPresentationModule(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddApplicationPart(typeof(OperationsPresentationMvcBuilderExtensions).Assembly);
        return mvcBuilder;
    }
}
