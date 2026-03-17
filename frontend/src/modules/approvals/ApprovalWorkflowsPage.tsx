import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import type { ApiError } from "../../core/api/httpClient";
import { StandardDataTable, type TableColumn } from "../../design-system/data-display/StandardDataTable";
import { StandardForm, type FormField } from "../../design-system/forms/StandardForm";
import { PageHeader } from "../../design-system/patterns/PageHeader";
import { PanelCard } from "../../design-system/primitives/PanelCard";
import {
  type ApprovalWorkflowConditionRequest,
  type ApprovalWorkflowDetail,
  type ApprovalWorkflowListItem,
  type ApprovalWorkflowStepRequest,
  type CreateApprovalWorkflowPayload,
  type ResolvedApprovalWorkflow,
  type UpdateApprovalWorkflowPayload,
  createApprovalWorkflow,
  getApprovalWorkflow,
  listApprovalWorkflows,
  resolveApprovalWorkflow,
  updateApprovalWorkflow
} from "./approvals.api";

const starterSteps: Array<ApprovalWorkflowStepRequest> = [
  { stepOrder: 1, name: "Direktor", approverType: "role", approverValue: "DIRECTOR_ROLE", isRequired: true, isParallel: false, minimumApproverCount: 1 },
  { stepOrder: 2, name: "CFO", approverType: "specific_user", approverValue: "40", isRequired: true, isParallel: false, minimumApproverCount: 1 }
];

const starterConditions: Array<ApprovalWorkflowConditionRequest> = [{ fieldKey: "amount", operator: "gte", value: "1000" }];

const initialCreateForm: CreateApprovalWorkflowPayload = {
  code: "OT_WORKFLOW",
  name: "Fazla Mesai Onayi",
  description: "Calisan > Direktor > CFO",
  moduleKey: "Overtime",
  documentType: "Request",
  isActive: true,
  steps: starterSteps,
  conditions: starterConditions
};

const initialResolveForm = {
  moduleKey: "Overtime",
  documentType: "Request",
  payloadJson: JSON.stringify({ amount: 2500 }, null, 2)
};

type WorkflowTarget = "create" | "edit";

const approverTypeOptions = [
  { value: "role", labelKey: "approverTypeRole" },
  { value: "specific_user", labelKey: "approverTypeSpecificUser" }
] as const;

const conditionOperatorOptions = [
  { value: "eq", labelKey: "operatorEq" },
  { value: "neq", labelKey: "operatorNeq" },
  { value: "gt", labelKey: "operatorGt" },
  { value: "gte", labelKey: "operatorGte" },
  { value: "lt", labelKey: "operatorLt" },
  { value: "lte", labelKey: "operatorLte" },
  { value: "contains", labelKey: "operatorContains" }
] as const;

function mapDetailToUpdatePayload(detail: ApprovalWorkflowDetail): UpdateApprovalWorkflowPayload {
  return {
    name: detail.name,
    description: detail.description,
    moduleKey: detail.moduleKey,
    documentType: detail.documentType,
    isActive: detail.isActive,
    steps: detail.steps.map((step) => ({
      stepOrder: step.stepOrder,
      name: step.name,
      approverType: step.approverType,
      approverValue: step.approverValue,
      isRequired: step.isRequired,
      isParallel: step.isParallel,
      minimumApproverCount: step.minimumApproverCount
    })),
    conditions: detail.conditions.map((condition) => ({
      fieldKey: condition.fieldKey,
      operator: condition.operator,
      value: condition.value
    }))
  };
}

function WorkflowStepsEditor(props: {
  title: string;
  description: string;
  steps: Array<ApprovalWorkflowStepRequest>;
  t: (key: string, options?: Record<string, unknown>) => string;
  onChange: (steps: Array<ApprovalWorkflowStepRequest>) => void;
}) {
  const { title, description, steps, t, onChange } = props;

  function updateStep(index: number, patch: Partial<ApprovalWorkflowStepRequest>) {
    onChange(
      steps.map((step, currentIndex) => {
        if (currentIndex !== index) {
          return step;
        }

        const next = { ...step, ...patch };
        if ("minimumApproverCount" in patch) {
          next.minimumApproverCount = Math.max(1, Number(patch.minimumApproverCount ?? 1));
        }

        return next;
      })
    );
  }

  function removeStep(index: number) {
    onChange(
      steps
        .filter((_, currentIndex) => currentIndex !== index)
        .map((step, currentIndex) => ({ ...step, stepOrder: currentIndex + 1 }))
    );
  }

  function addStep() {
    onChange([
      ...steps,
      {
        stepOrder: steps.length + 1,
        name: "",
        approverType: "role",
        approverValue: "",
        isRequired: true,
        isParallel: false,
        minimumApproverCount: 1
      }
    ]);
  }

  return (
    <div className="workflow-editor">
      <div className="workflow-editor__header">
        <div>
          <strong>{title}</strong>
          <span>{description}</span>
        </div>
        <button className="secondary-button" type="button" onClick={addStep}>
          {t("addStep")}
        </button>
      </div>
      <div className="workflow-editor__list">
        {steps.map((step, index) => (
          <div className="workflow-editor__item" key={`${step.stepOrder}-${index}`}>
            <div className="workflow-editor__item-header">
              <strong>{t("stepLabel", { step: index + 1 })}</strong>
              <button className="danger-button" type="button" onClick={() => removeStep(index)}>
                {t("removeStep")}
              </button>
            </div>
            <div className="workflow-editor__grid">
              <label className="workflow-editor__field">
                <span>{t("stepName")}</span>
                <input type="text" value={step.name} onChange={(event) => updateStep(index, { name: event.target.value })} />
              </label>
              <label className="workflow-editor__field">
                <span>{t("approverType")}</span>
                <select value={step.approverType} onChange={(event) => updateStep(index, { approverType: event.target.value })}>
                  {approverTypeOptions.map((option) => (
                    <option key={option.value} value={option.value}>
                      {t(option.labelKey)}
                    </option>
                  ))}
                </select>
              </label>
              <label className="workflow-editor__field">
                <span>{t("approverValue")}</span>
                <input
                  type="text"
                  value={step.approverValue}
                  onChange={(event) => updateStep(index, { approverValue: event.target.value })}
                />
              </label>
              <label className="workflow-editor__field">
                <span>{t("minimumApproverCount")}</span>
                <input
                  type="number"
                  min={1}
                  value={step.minimumApproverCount}
                  onChange={(event) => updateStep(index, { minimumApproverCount: Number(event.target.value) || 1 })}
                />
              </label>
              <label className="workflow-editor__toggle">
                <input type="checkbox" checked={step.isRequired} onChange={(event) => updateStep(index, { isRequired: event.target.checked })} />
                <span>{t("isRequired")}</span>
              </label>
              <label className="workflow-editor__toggle">
                <input type="checkbox" checked={step.isParallel} onChange={(event) => updateStep(index, { isParallel: event.target.checked })} />
                <span>{t("isParallel")}</span>
              </label>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function WorkflowConditionsEditor(props: {
  title: string;
  description: string;
  conditions: Array<ApprovalWorkflowConditionRequest>;
  t: (key: string, options?: Record<string, unknown>) => string;
  onChange: (conditions: Array<ApprovalWorkflowConditionRequest>) => void;
}) {
  const { title, description, conditions, t, onChange } = props;

  function updateCondition(index: number, patch: Partial<ApprovalWorkflowConditionRequest>) {
    onChange(conditions.map((condition, currentIndex) => (currentIndex === index ? { ...condition, ...patch } : condition)));
  }

  function removeCondition(index: number) {
    onChange(conditions.filter((_, currentIndex) => currentIndex !== index));
  }

  function addCondition() {
    onChange([...conditions, { fieldKey: "", operator: "eq", value: "" }]);
  }

  return (
    <div className="workflow-editor">
      <div className="workflow-editor__header">
        <div>
          <strong>{title}</strong>
          <span>{description}</span>
        </div>
        <button className="secondary-button" type="button" onClick={addCondition}>
          {t("addCondition")}
        </button>
      </div>
      <div className="workflow-editor__list">
        {conditions.length === 0 ? <p className="workflow-editor__empty">{t("noConditionsConfigured")}</p> : null}
        {conditions.map((condition, index) => (
          <div className="workflow-editor__item" key={`${condition.fieldKey}-${index}`}>
            <div className="workflow-editor__item-header">
              <strong>{t("conditionLabel", { condition: index + 1 })}</strong>
              <button className="danger-button" type="button" onClick={() => removeCondition(index)}>
                {t("removeCondition")}
              </button>
            </div>
            <div className="workflow-editor__grid workflow-editor__grid--conditions">
              <label className="workflow-editor__field">
                <span>{t("fieldKey")}</span>
                <input type="text" value={condition.fieldKey} onChange={(event) => updateCondition(index, { fieldKey: event.target.value })} />
              </label>
              <label className="workflow-editor__field">
                <span>{t("operator")}</span>
                <select value={condition.operator} onChange={(event) => updateCondition(index, { operator: event.target.value })}>
                  {conditionOperatorOptions.map((option) => (
                    <option key={option.value} value={option.value}>
                      {t(option.labelKey)}
                    </option>
                  ))}
                </select>
              </label>
              <label className="workflow-editor__field">
                <span>{t("conditionValue")}</span>
                <input type="text" value={condition.value} onChange={(event) => updateCondition(index, { value: event.target.value })} />
              </label>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

export function ApprovalWorkflowsPage() {
  const { t } = useTranslation("approvals");
  const queryClient = useQueryClient();
  const [selectedWorkflowId, setSelectedWorkflowId] = useState<number | null>(null);
  const [createForm, setCreateForm] = useState<CreateApprovalWorkflowPayload>(initialCreateForm);
  const [resolveForm, setResolveForm] = useState(initialResolveForm);
  const [editForm, setEditForm] = useState<UpdateApprovalWorkflowPayload | null>(null);
  const [editCode, setEditCode] = useState("");
  const [resolvedWorkflow, setResolvedWorkflow] = useState<ResolvedApprovalWorkflow | null>(null);

  const workflowsQuery = useQuery({
    queryKey: ["approvals", "workflows"],
    queryFn: ({ signal }) => listApprovalWorkflows(signal)
  });

  const detailQuery = useQuery({
    queryKey: ["approvals", "workflow-detail", selectedWorkflowId],
    queryFn: ({ signal }) => getApprovalWorkflow(selectedWorkflowId!, signal),
    enabled: selectedWorkflowId !== null
  });

  const createMutation = useMutation<ApprovalWorkflowDetail, ApiError, void>({
    mutationFn: async () => createApprovalWorkflow(createForm),
    onSuccess: async (detail) => {
      setSelectedWorkflowId(detail.id);
      setEditCode(detail.code);
      setEditForm(mapDetailToUpdatePayload(detail));
      await queryClient.invalidateQueries({ queryKey: ["approvals", "workflows"] });
    }
  });

  const updateMutation = useMutation<ApprovalWorkflowDetail, ApiError, void>({
    mutationFn: async () => {
      if (!selectedWorkflowId || !editForm) {
        throw new Error("No workflow selected.");
      }

      return updateApprovalWorkflow(selectedWorkflowId, editForm);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["approvals", "workflows"] });
      await queryClient.invalidateQueries({ queryKey: ["approvals", "workflow-detail", selectedWorkflowId] });
    }
  });

  const resolveMutation = useMutation<ResolvedApprovalWorkflow, ApiError, void>({
    mutationFn: () => resolveApprovalWorkflow(resolveForm),
    onSuccess: (result) => setResolvedWorkflow(result)
  });

  const columns: Array<TableColumn<ApprovalWorkflowListItem>> = [
    { key: "code", header: t("workflowCode"), cell: (item) => item.code },
    { key: "name", header: t("workflowName"), cell: (item) => item.name },
    { key: "moduleKey", header: t("moduleKey"), cell: (item) => item.moduleKey },
    { key: "documentType", header: t("documentType"), cell: (item) => item.documentType },
    { key: "status", header: t("status"), cell: (item) => <span className={`status-badge ${item.isActive ? "status-badge--success" : "status-badge--muted"}`}>{item.isActive ? t("active") : t("inactive")}</span> }
  ];

  const createFields: Array<FormField> = [
    { key: "code", label: t("workflowCode"), type: "text", value: createForm.code },
    { key: "name", label: t("workflowName"), type: "text", value: createForm.name },
    { key: "description", label: t("description"), type: "textarea", value: createForm.description },
    { key: "moduleKey", label: t("moduleKey"), type: "text", value: createForm.moduleKey },
    { key: "documentType", label: t("documentType"), type: "text", value: createForm.documentType },
    { key: "isActive", label: t("status"), type: "switch", value: createForm.isActive, helpText: t("workflowStatusHelp") }
  ];

  const editFields: Array<FormField> = editForm
    ? [
        { key: "name", label: t("workflowName"), type: "text", value: editForm.name },
        { key: "description", label: t("description"), type: "textarea", value: editForm.description },
        { key: "moduleKey", label: t("moduleKey"), type: "text", value: editForm.moduleKey },
        { key: "documentType", label: t("documentType"), type: "text", value: editForm.documentType },
        { key: "isActive", label: t("status"), type: "switch", value: editForm.isActive, helpText: t("workflowStatusHelp") }
      ]
    : [];

  const resolveFields: Array<FormField> = [
    { key: "moduleKey", label: t("moduleKey"), type: "text", value: resolveForm.moduleKey },
    { key: "documentType", label: t("documentType"), type: "text", value: resolveForm.documentType },
    { key: "payloadJson", label: t("payloadJson"), type: "textarea", value: resolveForm.payloadJson, helpText: t("payloadJsonHelp") }
  ];

  const selectedWorkflowSummary = useMemo(() => {
    if (!detailQuery.data) {
      return null;
    }

    return { steps: detailQuery.data.steps.length, conditions: detailQuery.data.conditions.length };
  }, [detailQuery.data]);

  function updateWorkflowFormValue(target: WorkflowTarget, key: string, value: string | number | boolean) {
    if (target === "create") {
      setCreateForm((current) => ({ ...current, [key]: value }));
      return;
    }

    setEditForm((current) => (current ? { ...current, [key]: value } : current));
  }

  return (
    <div className="page-grid">
      <PageHeader title={t("workflowsPageTitle")} description={t("workflowsPageDescription")} />

      <div className="workspace-grid workspace-grid--two-columns">
        <PanelCard title={t("workflowListTitle")} subtitle={t("workflowListSubtitle")}>
          <StandardDataTable
            columns={columns}
            items={workflowsQuery.data?.items ?? []}
            rowKey={(item) => item.id}
            loading={workflowsQuery.isLoading}
            emptyTitle={t("emptyWorkflowTitle")}
            emptyDescription={t("emptyWorkflowDescription")}
            actions={[
              {
                key: "select",
                label: t("openWorkflow"),
                onClick: async (item) => {
                  setSelectedWorkflowId(item.id);
                  setEditCode(item.code);
                  const detail = await queryClient.fetchQuery({
                    queryKey: ["approvals", "workflow-detail", item.id],
                    queryFn: ({ signal }) => getApprovalWorkflow(item.id, signal)
                  });
                  setEditForm(mapDetailToUpdatePayload(detail));
                }
              }
            ]}
          />
        </PanelCard>

        <PanelCard title={t("createWorkflowTitle")} subtitle={t("createWorkflowSubtitle")}>
          <StandardForm
            fields={createFields}
            onChange={(key, value) => updateWorkflowFormValue("create", key, value)}
            onSubmit={() => void createMutation.mutateAsync()}
            submitLabel={createMutation.isPending ? t("saving") : t("createWorkflowAction")}
          />
          <div className="spacer-block" />
          <WorkflowStepsEditor
            title={t("stepsEditorTitle")}
            description={t("stepsEditorDescription")}
            steps={createForm.steps}
            t={t}
            onChange={(steps) => setCreateForm((current) => ({ ...current, steps }))}
          />
          <div className="spacer-block" />
          <WorkflowConditionsEditor
            title={t("conditionsEditorTitle")}
            description={t("conditionsEditorDescription")}
            conditions={createForm.conditions}
            t={t}
            onChange={(conditions) => setCreateForm((current) => ({ ...current, conditions }))}
          />
          {createMutation.isError ? <p className="form-feedback form-feedback--error">{createMutation.error.detail ?? createMutation.error.title}</p> : null}
          {createMutation.isSuccess ? <p className="form-feedback form-feedback--success">{t("createWorkflowSuccess")}</p> : null}
        </PanelCard>
      </div>

      <div className="workspace-grid workspace-grid--two-columns">
        <PanelCard title={t("resolverTitle")} subtitle={t("resolverSubtitle")}>
          <StandardForm
            fields={resolveFields}
            onChange={(key, value) => setResolveForm((current) => ({ ...current, [key]: value }))}
            onSubmit={() => void resolveMutation.mutateAsync()}
            submitLabel={resolveMutation.isPending ? t("resolving") : t("resolveWorkflowAction")}
          />
          {resolveMutation.isError ? <p className="form-feedback form-feedback--error">{resolveMutation.error.detail ?? resolveMutation.error.title}</p> : null}
          {resolvedWorkflow ? (
            <div className="detail-summary">
              <div><span>{t("selectedWorkflow")}</span><strong>{resolvedWorkflow.code}</strong></div>
              <div><span>{t("matchedConditions")}</span><strong>{resolvedWorkflow.matchedConditionCount}</strong></div>
              <div><span>{t("stepCount")}</span><strong>{resolvedWorkflow.steps.length}</strong></div>
            </div>
          ) : null}
        </PanelCard>

        <PanelCard title={t("workflowDetailTitle")} subtitle={t("workflowDetailSubtitle")}>
          {detailQuery.data && editForm ? (
            <>
              <div className="detail-summary">
                <div><span>{t("workflowCode")}</span><strong>{editCode}</strong></div>
                <div><span>{t("stepCount")}</span><strong>{selectedWorkflowSummary?.steps ?? 0}</strong></div>
                <div><span>{t("conditionCount")}</span><strong>{selectedWorkflowSummary?.conditions ?? 0}</strong></div>
              </div>
              <StandardForm
                fields={editFields}
                onChange={(key, value) => updateWorkflowFormValue("edit", key, value)}
                onSubmit={() => void updateMutation.mutateAsync()}
                submitLabel={updateMutation.isPending ? t("saving") : t("updateWorkflowAction")}
              />
              <div className="spacer-block" />
              <WorkflowStepsEditor
                title={t("stepsEditorTitle")}
                description={t("stepsEditorDescription")}
                steps={editForm.steps}
                t={t}
                onChange={(steps) => setEditForm((current) => (current ? { ...current, steps } : current))}
              />
              <div className="spacer-block" />
              <WorkflowConditionsEditor
                title={t("conditionsEditorTitle")}
                description={t("conditionsEditorDescription")}
                conditions={editForm.conditions}
                t={t}
                onChange={(conditions) => setEditForm((current) => (current ? { ...current, conditions } : current))}
              />
              {updateMutation.isError ? <p className="form-feedback form-feedback--error">{updateMutation.error.detail ?? updateMutation.error.title}</p> : null}
              {updateMutation.isSuccess ? <p className="form-feedback form-feedback--success">{t("updateWorkflowSuccess")}</p> : null}
            </>
          ) : (
            <div className="standard-table__empty">
              <strong>{t("noWorkflowSelectedTitle")}</strong>
              <span>{t("noWorkflowSelectedDescription")}</span>
            </div>
          )}
        </PanelCard>
      </div>
    </div>
  );
}
