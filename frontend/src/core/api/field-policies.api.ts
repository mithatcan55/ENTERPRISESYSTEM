import { httpClient } from "./httpClient";

export type FieldPolicyDecision = {
  entityName: string;
  fieldName: string;
  displayName: string;
  visible: boolean;
  editable: boolean;
  filterable: boolean;
  exportable: boolean;
  masked: boolean;
  maskingMode: string | null;
};

export type EvaluateFieldPoliciesRequest = {
  entityName: string;
  surface: string;
  fieldValues?: Record<string, string | null>;
};

export function evaluateFieldPolicies(
  request: EvaluateFieldPoliciesRequest,
  signal?: AbortSignal
): Promise<FieldPolicyDecision[]> {
  return httpClient.post<FieldPolicyDecision[]>(
    "/api/authorization/field-policies/evaluate",
    { ...request, fieldValues: request.fieldValues ?? {} },
    signal
  );
}
