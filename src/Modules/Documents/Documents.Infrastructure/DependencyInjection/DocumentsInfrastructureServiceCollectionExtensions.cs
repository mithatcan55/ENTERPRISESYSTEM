using Documents.Application.Commands;
using Documents.Application.Queries;
using Documents.Infrastructure.Documents.Commands;
using Documents.Infrastructure.Documents.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Documents.Infrastructure.DependencyInjection;

public static class DocumentsInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddDocumentsInfrastructureModule(this IServiceCollection services)
    {
        services.AddScoped<IListManagedDocumentsQueryHandler, ListManagedDocumentsQueryHandler>();
        services.AddScoped<IGetManagedDocumentDetailQueryHandler, GetManagedDocumentDetailQueryHandler>();
        services.AddScoped<ICreateManagedDocumentCommandHandler, CreateManagedDocumentCommandHandler>();
        services.AddScoped<IAddManagedDocumentVersionCommandHandler, AddManagedDocumentVersionCommandHandler>();
        services.AddScoped<ILinkDocumentCommandHandler, LinkDocumentCommandHandler>();
        services.AddScoped<IUnlinkDocumentCommandHandler, UnlinkDocumentCommandHandler>();
        return services;
    }
}
