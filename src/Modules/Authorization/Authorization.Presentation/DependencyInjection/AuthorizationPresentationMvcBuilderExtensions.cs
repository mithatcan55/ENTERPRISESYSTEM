using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Presentation.DependencyInjection;

public static class AuthorizationPresentationMvcBuilderExtensions
{
    public static IMvcBuilder AddAuthorizationPresentationModule(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddApplicationPart(typeof(AuthorizationPresentationMvcBuilderExtensions).Assembly);
        return mvcBuilder;
    }
}
