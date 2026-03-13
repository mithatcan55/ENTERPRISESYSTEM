using Microsoft.Extensions.DependencyInjection;
using Reports.Application.Commands;
using Reports.Application.Queries;
using Reports.Infrastructure.Reports.Commands;
using Reports.Infrastructure.Reports.Queries;

namespace Reports.Infrastructure.DependencyInjection;

public static class ReportsInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddReportsInfrastructureModule(this IServiceCollection services)
    {
        services.AddScoped<IListReportTemplatesQueryHandler, ListReportTemplatesQueryHandler>();
        services.AddScoped<IGetReportTemplateDetailQueryHandler, GetReportTemplateDetailQueryHandler>();
        services.AddScoped<ICreateReportTemplateCommandHandler, CreateReportTemplateCommandHandler>();
        services.AddScoped<IUpdateReportTemplateCommandHandler, UpdateReportTemplateCommandHandler>();
        services.AddScoped<IPublishReportTemplateCommandHandler, PublishReportTemplateCommandHandler>();
        services.AddScoped<IArchiveReportTemplateCommandHandler, ArchiveReportTemplateCommandHandler>();
        return services;
    }
}
