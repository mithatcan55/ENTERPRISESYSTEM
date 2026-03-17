using System.Globalization;
using System.Text.Json;
using Approvals.Application.Contracts;
using Application.Exceptions;
using global::Infrastructure.Persistence;
using global::Infrastructure.Persistence.Entities.Approvals;
using Microsoft.EntityFrameworkCore;

namespace Approvals.Infrastructure.Workflows;

/// <summary>
/// Approval workflow secimi kod icindeki sabit if/else'lere degil,
/// veritabanindaki tanim + kosul kayitlarina dayanir.
/// </summary>
public sealed class ApprovalWorkflowResolver(ApprovalsDbContext approvalsDbContext)
{
    public async Task<ResolvedWorkflowSelection> ResolveAsync(string moduleKey, string documentType, string payloadJson, CancellationToken cancellationToken)
    {
        var payload = ParsePayload(payloadJson);

        var workflows = await approvalsDbContext.ApprovalWorkflowDefinitions
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive && x.ModuleKey == moduleKey && x.DocumentType == documentType)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        if (workflows.Count == 0)
        {
            throw new NotFoundAppException($"Uygun approval workflow bulunamadi. Module={moduleKey}, DocumentType={documentType}");
        }

        var workflowIds = workflows.Select(x => x.Id).ToList();

        var conditions = await approvalsDbContext.ApprovalWorkflowConditions
            .AsNoTracking()
            .Where(x => !x.IsDeleted && workflowIds.Contains(x.ApprovalWorkflowDefinitionId))
            .ToListAsync(cancellationToken);

        var steps = await approvalsDbContext.ApprovalWorkflowSteps
            .AsNoTracking()
            .Where(x => !x.IsDeleted && workflowIds.Contains(x.ApprovalWorkflowDefinitionId))
            .OrderBy(x => x.StepOrder)
            .ToListAsync(cancellationToken);

        ApprovalWorkflowDefinition? selectedWorkflow = null;
        List<ApprovalWorkflowCondition> selectedConditions = [];
        var selectedMatchedConditionCount = -1;

        foreach (var workflow in workflows)
        {
            var workflowConditions = conditions.Where(x => x.ApprovalWorkflowDefinitionId == workflow.Id).ToList();

            if (!AllConditionsMatch(workflowConditions, payload))
            {
                continue;
            }

            if (workflowConditions.Count > selectedMatchedConditionCount)
            {
                selectedWorkflow = workflow;
                selectedConditions = workflowConditions;
                selectedMatchedConditionCount = workflowConditions.Count;
            }
        }

        if (selectedWorkflow is null)
        {
            throw new NotFoundAppException($"Payload ile eslesen approval workflow bulunamadi. Module={moduleKey}, DocumentType={documentType}");
        }

        var selectedSteps = steps
            .Where(x => x.ApprovalWorkflowDefinitionId == selectedWorkflow.Id)
            .OrderBy(x => x.StepOrder)
            .ToList();

        if (selectedSteps.Count == 0)
        {
            throw new ValidationAppException("Secilen workflow icin onay adimi bulunamadi.", new Dictionary<string, string[]>
            {
                ["steps"] = ["Workflow en az bir approval step icermelidir."]
            });
        }

        return new ResolvedWorkflowSelection(selectedWorkflow, selectedSteps, selectedConditions, selectedMatchedConditionCount);
    }

    private static Dictionary<string, string> ParsePayload(string payloadJson)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return result;
        }

        using var document = JsonDocument.Parse(payloadJson);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                continue;
            }

            result[property.Name] = property.Value.ToString();
        }

        return result;
    }

    private static bool AllConditionsMatch(IReadOnlyCollection<ApprovalWorkflowCondition> conditions, IReadOnlyDictionary<string, string> payload)
    {
        foreach (var condition in conditions)
        {
            if (!payload.TryGetValue(condition.FieldKey, out var actualValue))
            {
                return false;
            }

            if (!Matches(condition.Operator, actualValue, condition.Value))
            {
                return false;
            }
        }

        return true;
    }

    private static bool Matches(string @operator, string actualValue, string expectedValue)
    {
        return @operator.Trim().ToLowerInvariant() switch
        {
            "eq" or "=" => string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase),
            "neq" or "!=" => !string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase),
            "contains" => actualValue.Contains(expectedValue, StringComparison.OrdinalIgnoreCase),
            "gt" => CompareNumeric(actualValue, expectedValue) > 0,
            "gte" => CompareNumeric(actualValue, expectedValue) >= 0,
            "lt" => CompareNumeric(actualValue, expectedValue) < 0,
            "lte" => CompareNumeric(actualValue, expectedValue) <= 0,
            _ => false
        };
    }

    private static int CompareNumeric(string actualValue, string expectedValue)
    {
        if (!decimal.TryParse(actualValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var actual))
        {
            decimal.TryParse(actualValue, NumberStyles.Any, CultureInfo.GetCultureInfo("tr-TR"), out actual);
        }

        if (!decimal.TryParse(expectedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var expected))
        {
            decimal.TryParse(expectedValue, NumberStyles.Any, CultureInfo.GetCultureInfo("tr-TR"), out expected);
        }

        return actual.CompareTo(expected);
    }
}

public sealed record ResolvedWorkflowSelection(
    ApprovalWorkflowDefinition Workflow,
    IReadOnlyList<ApprovalWorkflowStep> Steps,
    IReadOnlyList<ApprovalWorkflowCondition> Conditions,
    int MatchedConditionCount);
