import { httpClient } from "../../core/api/httpClient";

export type ApprovalsPagedResult<T> = {
  items: Array<T>;
  page: number;
  pageSize: number;
  totalCount: number;
};

export type ApprovalWorkflowListItem = {
  id: number;
  code: string;
  name: string;
  description: string;
  moduleKey: string;
  documentType: string;
  isActive: boolean;
  createdAt: string;
  modifiedAt: string | null;
};

export type ApprovalWorkflowStep = {
  id: number;
  stepOrder: number;
  name: string;
  approverType: string;
  approverValue: string;
  isRequired: boolean;
  isParallel: boolean;
  minimumApproverCount: number;
};

export type ApprovalWorkflowCondition = {
  id: number;
  fieldKey: string;
  operator: string;
  value: string;
};

export type ApprovalWorkflowDetail = {
  id: number;
  code: string;
  name: string;
  description: string;
  moduleKey: string;
  documentType: string;
  isActive: boolean;
  steps: Array<ApprovalWorkflowStep>;
  conditions: Array<ApprovalWorkflowCondition>;
};

export type ApprovalWorkflowStepRequest = {
  stepOrder: number;
  name: string;
  approverType: string;
  approverValue: string;
  isRequired: boolean;
  isParallel: boolean;
  minimumApproverCount: number;
};

export type ApprovalWorkflowConditionRequest = {
  fieldKey: string;
  operator: string;
  value: string;
};

export type CreateApprovalWorkflowPayload = {
  code: string;
  name: string;
  description: string;
  moduleKey: string;
  documentType: string;
  isActive: boolean;
  steps: Array<ApprovalWorkflowStepRequest>;
  conditions: Array<ApprovalWorkflowConditionRequest>;
};

export type UpdateApprovalWorkflowPayload = Omit<CreateApprovalWorkflowPayload, "code">;

export type ResolveApprovalWorkflowPayload = {
  moduleKey: string;
  documentType: string;
  payloadJson: string;
};

export type ResolvedApprovalWorkflow = {
  workflowId: number;
  code: string;
  name: string;
  moduleKey: string;
  documentType: string;
  matchedConditionCount: number;
  steps: Array<{
    stepOrder: number;
    name: string;
    approverType: string;
    approverValue: string;
    isRequired: boolean;
    isParallel: boolean;
    minimumApproverCount: number;
  }>;
};

export type ApprovalInstanceStep = {
  id: number;
  stepOrder: number;
  assignedUserId: number | null;
  status: string;
  dueAt: string | null;
};

export type ApprovalDecision = {
  id: number;
  actorUserId: number;
  decision: string;
  comment: string;
  createdAt: string;
};

export type ApprovalInstanceDetail = {
  id: number;
  approvalWorkflowDefinitionId: number;
  workflowCode: string;
  referenceType: string;
  referenceId: string;
  requesterUserId: number;
  status: string;
  currentStepOrder: number;
  payloadJson: string;
  steps: Array<ApprovalInstanceStep>;
  decisions: Array<ApprovalDecision>;
};

export type PendingApprovalListItem = {
  approvalInstanceId: number;
  approvalInstanceStepId: number;
  workflowCode: string;
  referenceType: string;
  referenceId: string;
  stepOrder: number;
  assignedUserId: number | null;
  status: string;
  createdAt: string;
};

export type DecideApprovalStepPayload = {
  decision: string;
  comment: string;
};

export type DelegationAssignmentListItem = {
  id: number;
  delegatorUserId: number;
  delegateUserId: number;
  scopeType: string;
  startsAt: string;
  endsAt: string;
  isActive: boolean;
  notes: string;
};

export type DelegationAssignmentDetail = {
  id: number;
  delegatorUserId: number;
  delegateUserId: number;
  scopeType: string;
  includedScopesJson: string;
  excludedScopesJson: string;
  startsAt: string;
  endsAt: string;
  isActive: boolean;
  notes: string;
};

export type CreateDelegationAssignmentPayload = {
  delegatorUserId: number;
  delegateUserId: number;
  scopeType: string;
  includedScopesJson: string;
  excludedScopesJson: string;
  startsAt: string;
  endsAt: string;
  notes: string;
};

export async function listApprovalWorkflows(signal?: AbortSignal) {
  return httpClient.get<ApprovalsPagedResult<ApprovalWorkflowListItem>>("/api/approvals/workflows", signal);
}

export async function getApprovalWorkflow(approvalWorkflowDefinitionId: number, signal?: AbortSignal) {
  return httpClient.get<ApprovalWorkflowDetail>(`/api/approvals/workflows/${approvalWorkflowDefinitionId}`, signal);
}

export async function createApprovalWorkflow(payload: CreateApprovalWorkflowPayload, signal?: AbortSignal) {
  return httpClient.post<ApprovalWorkflowDetail>("/api/approvals/workflows", payload, signal);
}

export async function updateApprovalWorkflow(
  approvalWorkflowDefinitionId: number,
  payload: UpdateApprovalWorkflowPayload,
  signal?: AbortSignal
) {
  return httpClient.put<ApprovalWorkflowDetail>(`/api/approvals/workflows/${approvalWorkflowDefinitionId}`, payload, signal);
}

export async function resolveApprovalWorkflow(payload: ResolveApprovalWorkflowPayload, signal?: AbortSignal) {
  return httpClient.post<ResolvedApprovalWorkflow>("/api/approvals/workflows/resolve", payload, signal);
}

export async function listPendingApprovals(signal?: AbortSignal) {
  return httpClient.get<ApprovalsPagedResult<PendingApprovalListItem>>("/api/approvals/instances/pending", signal);
}

export async function getApprovalInstance(approvalInstanceId: number, signal?: AbortSignal) {
  return httpClient.get<ApprovalInstanceDetail>(`/api/approvals/instances/${approvalInstanceId}`, signal);
}

export async function decideApprovalStep(
  approvalInstanceStepId: number,
  payload: DecideApprovalStepPayload,
  signal?: AbortSignal
) {
  return httpClient.post<ApprovalInstanceDetail>(`/api/approvals/instances/steps/${approvalInstanceStepId}/decide`, payload, signal);
}

export async function listDelegations(signal?: AbortSignal) {
  return httpClient.get<ApprovalsPagedResult<DelegationAssignmentListItem>>("/api/approvals/delegations", signal);
}

export async function createDelegation(payload: CreateDelegationAssignmentPayload, signal?: AbortSignal) {
  return httpClient.post<DelegationAssignmentDetail>("/api/approvals/delegations", payload, signal);
}
