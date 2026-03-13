using Reports.Application.Contracts;

namespace Reports.Infrastructure.Reports;

internal static class ReportTemplateMappingExtensions
{
    public static ReportTemplateListItemDto ToListItemDto(
        this global::Infrastructure.Persistence.Entities.Reporting.ReportTemplate template) =>
        new(
            template.Id,
            template.Code,
            template.Name,
            template.Description,
            template.ModuleKey,
            template.Type,
            template.Status,
            template.CurrentVersionNumber,
            template.PublishedVersionNumber,
            template.ModifiedAt ?? template.CreatedAt);
}
