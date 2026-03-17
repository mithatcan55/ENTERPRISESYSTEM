using Microsoft.Extensions.DependencyInjection;

namespace Documents.Presentation.DependencyInjection;

public static class DocumentsPresentationMvcBuilderExtensions
{
    public static IMvcBuilder AddDocumentsPresentationModule(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddApplicationPart(typeof(DocumentsPresentationMvcBuilderExtensions).Assembly);
        return mvcBuilder;
    }
}
