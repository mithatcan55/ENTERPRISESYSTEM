import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { ApiError } from "../../core/api/httpClient";
import { StandardDataTable, type TableColumn } from "../../design-system/data-display/StandardDataTable";
import { StandardForm, type FormField } from "../../design-system/forms/StandardForm";
import { PageHeader } from "../../design-system/patterns/PageHeader";
import { PanelCard } from "../../design-system/primitives/PanelCard";
import {
  createDelegation,
  listDelegations,
  type CreateDelegationAssignmentPayload,
  type DelegationAssignmentDetail,
  type DelegationAssignmentListItem
} from "./approvals.api";

const initialDelegationForm: CreateDelegationAssignmentPayload = {
  delegatorUserId: 20,
  delegateUserId: 30,
  scopeType: "workflow",
  includedScopesJson: JSON.stringify(["OT_WORKFLOW"], null, 2),
  excludedScopesJson: "[]",
  startsAt: new Date().toISOString().slice(0, 16),
  endsAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().slice(0, 16),
  notes: "Yillik izin vekaleti"
};

export function DelegationsPage() {
  const { t } = useTranslation("approvals");
  const queryClient = useQueryClient();
  const [delegationForm, setDelegationForm] = useState(initialDelegationForm);

  const delegationsQuery = useQuery({
    queryKey: ["approvals", "delegations"],
    queryFn: ({ signal }) => listDelegations(signal)
  });

  const createMutation = useMutation<DelegationAssignmentDetail, ApiError, void>({
    mutationFn: () => createDelegation(delegationForm),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["approvals", "delegations"] });
    }
  });

  const columns: Array<TableColumn<DelegationAssignmentListItem>> = [
    { key: "delegatorUserId", header: t("delegatorUserId"), cell: (item) => item.delegatorUserId },
    { key: "delegateUserId", header: t("delegateUserId"), cell: (item) => item.delegateUserId },
    { key: "scopeType", header: t("scopeType"), cell: (item) => item.scopeType },
    { key: "startsAt", header: t("startsAt"), cell: (item) => new Date(item.startsAt).toLocaleString() },
    { key: "endsAt", header: t("endsAt"), cell: (item) => new Date(item.endsAt).toLocaleString() },
    { key: "isActive", header: t("status"), cell: (item) => <span className={`status-badge ${item.isActive ? "status-badge--success" : "status-badge--muted"}`}>{item.isActive ? t("active") : t("inactive")}</span> }
  ];

  const fields: Array<FormField> = [
    { key: "delegatorUserId", label: t("delegatorUserId"), type: "number", value: delegationForm.delegatorUserId },
    { key: "delegateUserId", label: t("delegateUserId"), type: "number", value: delegationForm.delegateUserId },
    { key: "scopeType", label: t("scopeType"), type: "select", value: delegationForm.scopeType, options: [{ value: "workflow", label: t("scopeWorkflow") }, { value: "all", label: t("scopeAll") }] },
    { key: "includedScopesJson", label: t("includedScopesJson"), type: "textarea", value: delegationForm.includedScopesJson },
    { key: "excludedScopesJson", label: t("excludedScopesJson"), type: "textarea", value: delegationForm.excludedScopesJson },
    { key: "startsAt", label: t("startsAt"), type: "text", value: delegationForm.startsAt },
    { key: "endsAt", label: t("endsAt"), type: "text", value: delegationForm.endsAt },
    { key: "notes", label: t("notes"), type: "textarea", value: delegationForm.notes }
  ];

  return (
    <div className="page-grid">
      <PageHeader title={t("delegationsPageTitle")} description={t("delegationsPageDescription")} />

      <div className="workspace-grid workspace-grid--two-columns">
        <PanelCard title={t("delegationListTitle")} subtitle={t("delegationListSubtitle")}>
          <StandardDataTable
            columns={columns}
            items={delegationsQuery.data?.items ?? []}
            rowKey={(item) => item.id}
            loading={delegationsQuery.isLoading}
            emptyTitle={t("emptyDelegationTitle")}
            emptyDescription={t("emptyDelegationDescription")}
          />
        </PanelCard>

        <PanelCard title={t("createDelegationTitle")} subtitle={t("createDelegationSubtitle")}>
          <StandardForm
            fields={fields}
            onChange={(key, value) => setDelegationForm((current) => ({ ...current, [key]: key === "delegatorUserId" || key === "delegateUserId" ? Number(value) : value }))}
            onSubmit={() => void createMutation.mutateAsync()}
            submitLabel={createMutation.isPending ? t("saving") : t("createDelegationAction")}
          />
          {createMutation.isError ? <p className="form-feedback form-feedback--error">{createMutation.error.detail ?? createMutation.error.title}</p> : null}
          {createMutation.isSuccess ? <p className="form-feedback form-feedback--success">{t("createDelegationSuccess")}</p> : null}
        </PanelCard>
      </div>
    </div>
  );
}
