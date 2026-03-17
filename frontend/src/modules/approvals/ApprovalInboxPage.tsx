import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { ApiError } from "../../core/api/httpClient";
import { StandardDataTable, type TableColumn } from "../../design-system/data-display/StandardDataTable";
import { StandardForm, type FormField } from "../../design-system/forms/StandardForm";
import { PageHeader } from "../../design-system/patterns/PageHeader";
import { PanelCard } from "../../design-system/primitives/PanelCard";
import { decideApprovalStep, getApprovalInstance, listPendingApprovals, type ApprovalInstanceDetail, type PendingApprovalListItem } from "./approvals.api";

const initialDecisionForm = {
  decision: "approve",
  comment: ""
};

function getApprovalStatusBadgeClass(status: string) {
  switch (status.toLowerCase()) {
    case "approved":
    case "completed":
      return "status-badge status-badge--success";
    case "rejected":
      return "status-badge status-badge--danger";
    case "returned":
      return "status-badge status-badge--warning";
    case "pending":
      return "status-badge status-badge--warning";
    default:
      return "status-badge status-badge--default";
  }
}

export function ApprovalInboxPage() {
  const { t } = useTranslation("approvals");
  const queryClient = useQueryClient();
  const [selectedApprovalInstanceId, setSelectedApprovalInstanceId] = useState<number | null>(null);
  const [selectedApprovalInstanceStepId, setSelectedApprovalInstanceStepId] = useState<number | null>(null);
  const [decisionForm, setDecisionForm] = useState(initialDecisionForm);

  const pendingQuery = useQuery({
    queryKey: ["approvals", "pending"],
    queryFn: ({ signal }) => listPendingApprovals(signal)
  });

  const detailQuery = useQuery({
    queryKey: ["approvals", "instance-detail", selectedApprovalInstanceId],
    queryFn: ({ signal }) => getApprovalInstance(selectedApprovalInstanceId!, signal),
    enabled: selectedApprovalInstanceId !== null
  });

  const decideMutation = useMutation<ApprovalInstanceDetail, ApiError, void>({
    mutationFn: () => decideApprovalStep(selectedApprovalInstanceStepId!, decisionForm),
    onSuccess: async (detail) => {
      setSelectedApprovalInstanceId(detail.id);
      await queryClient.invalidateQueries({ queryKey: ["approvals", "pending"] });
      await queryClient.invalidateQueries({ queryKey: ["approvals", "instance-detail", detail.id] });
      setDecisionForm(initialDecisionForm);
    }
  });

  const columns: Array<TableColumn<PendingApprovalListItem>> = [
    { key: "workflowCode", header: t("workflowCode"), cell: (item) => item.workflowCode },
    { key: "referenceType", header: t("referenceType"), cell: (item) => item.referenceType },
    { key: "referenceId", header: t("referenceId"), cell: (item) => item.referenceId },
    { key: "stepOrder", header: t("stepOrder"), cell: (item) => item.stepOrder },
    { key: "status", header: t("status"), cell: (item) => <span className={getApprovalStatusBadgeClass(item.status)}>{item.status}</span> }
  ];

  const decisionFields: Array<FormField> = [
    {
      key: "decision",
      label: t("decision"),
      type: "select",
      value: decisionForm.decision,
      options: [
        { value: "approve", label: t("approve") },
        { value: "reject", label: t("reject") },
        { value: "return", label: t("returnDecision") }
      ]
    },
    { key: "comment", label: t("comment"), type: "textarea", value: decisionForm.comment, placeholder: t("commentPlaceholder") }
  ];

  return (
    <div className="page-grid">
      <PageHeader title={t("inboxPageTitle")} description={t("inboxPageDescription")} />

      <div className="workspace-grid workspace-grid--two-columns">
        <PanelCard title={t("pendingListTitle")} subtitle={t("pendingListSubtitle")}>
          <StandardDataTable
            columns={columns}
            items={pendingQuery.data?.items ?? []}
            rowKey={(item) => `${item.approvalInstanceId}-${item.approvalInstanceStepId}`}
            loading={pendingQuery.isLoading}
            emptyTitle={t("emptyPendingTitle")}
            emptyDescription={t("emptyPendingDescription")}
            actions={[
              {
                key: "open",
                label: t("openApproval"),
                onClick: (item) => {
                  setSelectedApprovalInstanceId(item.approvalInstanceId);
                  setSelectedApprovalInstanceStepId(item.approvalInstanceStepId);
                }
              }
            ]}
          />
        </PanelCard>

        <PanelCard title={t("decisionPanelTitle")} subtitle={t("decisionPanelSubtitle")}>
          {detailQuery.data && selectedApprovalInstanceStepId ? (
            <>
              <div className="detail-summary">
                <div><span>{t("workflowCode")}</span><strong>{detailQuery.data.workflowCode}</strong></div>
                <div><span>{t("referenceId")}</span><strong>{detailQuery.data.referenceId}</strong></div>
                <div><span>{t("currentStep")}</span><strong>{detailQuery.data.currentStepOrder}</strong></div>
              </div>

              <div className="approval-overview-card">
                <div>
                  <span>{t("referenceType")}</span>
                  <strong>{detailQuery.data.referenceType}</strong>
                </div>
                <div>
                  <span>{t("requesterUserId")}</span>
                  <strong>{detailQuery.data.requesterUserId}</strong>
                </div>
                <div>
                  <span>{t("status")}</span>
                  <strong><span className={getApprovalStatusBadgeClass(detailQuery.data.status)}>{detailQuery.data.status}</span></strong>
                </div>
              </div>

              <StandardForm
                fields={decisionFields}
                onChange={(key, value) => setDecisionForm((current) => ({ ...current, [key]: value }))}
                onSubmit={() => void decideMutation.mutateAsync()}
                submitLabel={decideMutation.isPending ? t("saving") : t("submitDecision")}
              />

              <div className="approval-timeline">
                <strong className="approval-section-title">{t("stepTimelineTitle")}</strong>
                {detailQuery.data.steps.map((step) => (
                  <div key={step.id} className="approval-timeline__item">
                    <div>
                      <strong>{t("stepLabel", { step: step.stepOrder })}</strong>
                      <span>{t("assignedUserLabel", { userId: step.assignedUserId ?? "-" })}</span>
                      {step.dueAt ? <span>{t("dueAtLabel", { value: new Date(step.dueAt).toLocaleString() })}</span> : null}
                    </div>
                    <div className="approval-timeline__meta">
                      <span className={getApprovalStatusBadgeClass(step.status)}>{step.status}</span>
                    </div>
                  </div>
                ))}
              </div>

              {detailQuery.data.decisions.length ? (
                <div className="approval-timeline">
                  <strong className="approval-section-title">{t("decisionHistoryTitle")}</strong>
                  {detailQuery.data.decisions.map((decision) => (
                    <div key={decision.id} className="approval-timeline__item">
                      <div>
                        <strong>{decision.decision}</strong>
                        <span>{decision.comment || t("noComment")}</span>
                      </div>
                      <div className="approval-timeline__meta">
                        <span>{t("actorUserLabel", { userId: decision.actorUserId })}</span>
                        <span>{new Date(decision.createdAt).toLocaleString()}</span>
                      </div>
                    </div>
                  ))}
                </div>
              ) : null}

              <div className="approval-payload">
                <strong className="approval-section-title">{t("payloadPreviewTitle")}</strong>
                <pre>{detailQuery.data.payloadJson}</pre>
              </div>

              {decideMutation.isError ? <p className="form-feedback form-feedback--error">{decideMutation.error.detail ?? decideMutation.error.title}</p> : null}
              {decideMutation.isSuccess ? <p className="form-feedback form-feedback--success">{t("decisionSavedSuccess")}</p> : null}
            </>
          ) : (
            <div className="standard-table__empty">
              <strong>{t("noApprovalSelectedTitle")}</strong>
              <span>{t("noApprovalSelectedDescription")}</span>
            </div>
          )}
        </PanelCard>
      </div>
    </div>
  );
}
