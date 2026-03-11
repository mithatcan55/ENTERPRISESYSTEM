using Microsoft.Extensions.DependencyInjection;

namespace Identity.Presentation.DependencyInjection;

public static class IdentityPresentationMvcBuilderExtensions
{
    public static IMvcBuilder AddIdentityPresentationModule(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddApplicationPart(typeof(IdentityPresentationMvcBuilderExtensions).Assembly);
        return mvcBuilder;
    }
}
