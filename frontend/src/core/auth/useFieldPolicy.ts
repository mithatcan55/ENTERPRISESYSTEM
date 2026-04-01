import { useQuery } from "@tanstack/react-query";
import { evaluateFieldPolicies, type FieldPolicyDecision } from "../api/field-policies.api";

type Surface = "LIST" | "FORM" | "DETAIL" | "ANY";

type UseFieldPolicyResult = {
  decisions: FieldPolicyDecision[];
  isLoading: boolean;
  /** Returns true if fieldName is visible (default: true when no policies defined) */
  isVisible: (fieldName: string) => boolean;
  /** Returns true if fieldName is editable (default: true when no policies defined) */
  isEditable: (fieldName: string) => boolean;
  /** Returns true if fieldName is filterable (default: true when no policies defined) */
  isFilterable: (fieldName: string) => boolean;
  /** Returns true if fieldName is exportable (default: true when no policies defined) */
  isExportable: (fieldName: string) => boolean;
  /** Returns true if fieldName value should be masked */
  isMasked: (fieldName: string) => boolean;
  /** Returns masking mode string or null */
  getMaskingMode: (fieldName: string) => string | null;
};

export function useFieldPolicy(
  entityName: string,
  surface: Surface,
  fieldValues?: Record<string, string | null>
): UseFieldPolicyResult {
  const { data, isLoading } = useQuery({
    queryKey: ["field-policies", entityName, surface, fieldValues],
    queryFn: ({ signal }) =>
      evaluateFieldPolicies({ entityName, surface, fieldValues }, signal),
    // If no policies are defined for this entity, API returns [] — that's fine
    staleTime: 30_000,
  });

  const decisionMap = new Map<string, FieldPolicyDecision>(
    (data ?? []).map((d) => [d.fieldName.toUpperCase(), d])
  );

  function getDecision(fieldName: string) {
    return decisionMap.get(fieldName.toUpperCase());
  }

  return {
    decisions: data ?? [],
    isLoading,
    isVisible: (fieldName) => getDecision(fieldName)?.visible ?? true,
    isEditable: (fieldName) => getDecision(fieldName)?.editable ?? true,
    isFilterable: (fieldName) => getDecision(fieldName)?.filterable ?? true,
    isExportable: (fieldName) => getDecision(fieldName)?.exportable ?? true,
    isMasked: (fieldName) => getDecision(fieldName)?.masked ?? false,
    getMaskingMode: (fieldName) => getDecision(fieldName)?.maskingMode ?? null,
  };
}
