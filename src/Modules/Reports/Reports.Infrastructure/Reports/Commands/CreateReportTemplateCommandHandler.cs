using Application.Exceptions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Reporting;
using Microsoft.EntityFrameworkCore;
using Reports.Application.Commands;
using Reports.Application.Contracts;
using Reports.Infrastructure.Reports.Queries;
using System.Text.Json;

namespace Reports.Infrastructure.Reports.Commands;

public sealed class CreateReportTemplateCommandHandler(ReportsDbContext reportsDbContext) : ICreateReportTemplateCommandHandler
{
    public async Task<ReportTemplateDetailDto> HandleAsync(CreateReportTemplateRequest request, CancellationToken cancellationToken)
    {
        ValidateRequest(request.Code, request.Name, request.ModuleKey, request.TemplateJson, request.SampleInputJson);

        var duplicateExists = await reportsDbContext.ReportTemplates
            .AnyAsync(x => !x.IsDeleted && x.Code == request.Code, cancellationToken);

        if (duplicateExists)
        {
            throw new ValidationAppException(
                "Rapor sablonu dogrulanamadi.",
                new Dictionary<string, string[]>
                {
                    ["code"] = ["Ayni kod ile tanimli bir rapor sablonu zaten var."]
                });
        }

        var template = new ReportTemplate
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            ModuleKey = request.ModuleKey.Trim(),
            Type = request.Type.Trim(),
            Status = "Draft",
            CurrentVersionNumber = 1
        };

        reportsDbContext.ReportTemplates.Add(template);
        await reportsDbContext.SaveChangesAsync(cancellationToken);

        reportsDbContext.ReportTemplateVersions.Add(new ReportTemplateVersion
        {
            ReportTemplateId = template.Id,
            VersionNumber = 1,
            TemplateJson = request.TemplateJson,
            SampleInputJson = request.SampleInputJson,
            Notes = request.Notes.Trim(),
            IsPublished = false
        });

        await reportsDbContext.SaveChangesAsync(cancellationToken);

        return await new GetReportTemplateDetailQueryHandler(reportsDbContext).HandleAsync(template.Id, cancellationToken);
    }

    internal static void ValidateRequest(string code, string name, string moduleKey, string templateJson, string sampleInputJson)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(code))
        {
            errors["code"] = ["Rapor kodu zorunludur."];
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            errors["name"] = ["Rapor adi zorunludur."];
        }

        if (string.IsNullOrWhiteSpace(moduleKey))
        {
            errors["moduleKey"] = ["Module anahtari zorunludur."];
        }

        if (string.IsNullOrWhiteSpace(templateJson) || !IsValidJson(templateJson))
        {
            errors["templateJson"] = ["TemplateJson gecerli bir JSON olmali."];
        }

        if (string.IsNullOrWhiteSpace(sampleInputJson) || !IsValidJson(sampleInputJson))
        {
            errors["sampleInputJson"] = ["SampleInputJson gecerli bir JSON olmali."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationAppException("Rapor sablonu dogrulanamadi.", errors);
        }
    }

    private static bool IsValidJson(string json)
    {
        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
