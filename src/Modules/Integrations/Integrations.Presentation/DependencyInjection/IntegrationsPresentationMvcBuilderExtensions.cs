using Microsoft.Extensions.DependencyInjection;

namespace Integrations.Presentation.DependencyInjection;

public static class IntegrationsPresentationMvcBuilderExtensions
{
    public static IMvcBuilder AddIntegrationsPresentationModule(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddApplicationPart(typeof(IntegrationsPresentationMvcBuilderExtensions).Assembly);
        return mvcBuilder;
    }
}
