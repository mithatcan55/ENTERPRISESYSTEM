import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { ApiError } from "../../../core/api/httpClient";
import { StandardDataTable, type TableColumn } from "../../../design-system/data-display/StandardDataTable";
import { StandardForm, type FormField } from "../../../design-system/forms/StandardForm";
import { PageHeader } from "../../../design-system/patterns/PageHeader";
import { PanelCard } from "../../../design-system/primitives/PanelCard";
import {
  deleteActionPermission,
  listActionPermissions,
  upsertActionPermission,
  type UpsertPermissionPayload,
  type UserActionPermissionItem
} from "./permissions.api";

const initialQuery = {
  userId: 1,
  subModulePageId: 1,
  transactionCode: "SYS01"
};

const initialUpsertForm: UpsertPermissionPayload = {
  userId: 1,
  subModulePageId: 1,
  transactionCode: "SYS01",
  actionCode: "READ",
  isAllowed: true
};

export function PermissionsPage() {
  const { t } = useTranslation(["authorization"]);
  const queryClient = useQueryClient();
  const [query, setQuery] = useState(initialQuery);
  const [upsertForm, setUpsertForm] = useState<UpsertPermissionPayload>(initialUpsertForm);

  const permissionsQuery = useQuery({
    queryKey: ["authorization", "permissions", query],
    queryFn: ({ signal }) => listActionPermissions(query, signal)
  });

  const upsertMutation = useMutation<UserActionPermissionItem, ApiError, UpsertPermissionPayload>({
    mutationFn: (payload) => upsertActionPermission(payload),
    onSuccess: async () => {
      // Upsert ekrani ayni karttan allow/deny uretebildigi icin listeyi her seferinde tazeliyoruz.
      await queryClient.invalidateQueries({ queryKey: ["authorization", "permissions"] });
    }
  });

  const deleteMutation = useMutation<void, ApiError, { permissionId: number }>({
    mutationFn: ({ permissionId }) => deleteActionPermission(permissionId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["authorization", "permissions"] });
    }
  });

  const columns: Array<TableColumn<UserActionPermissionItem>> = [
    { key: "transactionCode", header: t("transactionCode"), cell: (item) => item.transactionCode },
    { key: "actionCode", header: t("actionCode"), cell: (item) => item.actionCode },
    {
      key: "isAllowed",
      header: t("decision"),
      cell: (item) => (
        <span className={`status-badge ${item.isAllowed ? "status-badge--success" : "status-badge--warning"}`}>
          {item.isAllowed ? t("allowed") : t("denied")}
        </span>
      )
    },
    {
      key: "modifiedAt",
      header: t("updatedAt"),
      cell: (item) =>
        new Intl.DateTimeFormat("tr-TR").format(new Date(item.modifiedAt ?? item.createdAt))
    }
  ];

  const filterFields: FormField[] = [
    { key: "userId", label: t("userId"), type: "number", value: query.userId },
    { key: "subModulePageId", label: t("subModulePageId"), type: "number", value: query.subModulePageId ?? 1 },
    { key: "transactionCode", label: t("transactionCode"), type: "text", value: query.transactionCode ?? "" }
  ];

  const upsertFields: FormField[] = [
    { key: "userId", label: t("userId"), type: "number", value: upsertForm.userId },
    { key: "subModulePageId", label: t("subModulePageId"), type: "number", value: upsertForm.subModulePageId ?? 1 },
    { key: "transactionCode", label: t("transactionCode"), type: "text", value: upsertForm.transactionCode ?? "" },
    { key: "actionCode", label: t("actionCode"), type: "text", value: upsertForm.actionCode },
    {
      key: "isAllowed",
      label: t("decision"),
      type: "switch",
      value: upsertForm.isAllowed,
      helpText: t("decisionHelp")
    }
  ];

  return (
    <div className="page-grid">
      <PageHeader title={t("permissionsPageTitle")} description={t("permissionsPageDescription")} />
      <div className="workspace-grid workspace-grid--two-columns">
        <PanelCard title={t("permissionsListTitle")} subtitle={t("permissionsListSubtitle")}>
          <StandardForm
            fields={filterFields}
            onChange={(key, value) =>
              setQuery((current) => ({
                ...current,
                [key]: key === "userId" || key === "subModulePageId" ? Number(value) : value
              }))
            }
          />

          <div className="spacer-block" />

          <StandardDataTable
            columns={columns}
            items={permissionsQuery.data ?? []}
            rowKey={(item) => item.id}
            loading={permissionsQuery.isLoading}
            emptyTitle={t("noPermissionsTitle")}
            emptyDescription={t("noPermissionsDescription")}
            actions={[
              {
                key: "delete",
                label: t("deletePermission"),
                tone: "danger",
                onClick: (item) => deleteMutation.mutate({ permissionId: item.id })
              }
            ]}
          />
        </PanelCard>

        <PanelCard title={t("upsertPermissionTitle")} subtitle={t("upsertPermissionSubtitle")}>
          <StandardForm
            fields={upsertFields}
            onChange={(key, value) =>
              setUpsertForm((current) => ({
                ...current,
                [key]:
                  key === "userId" || key === "subModulePageId"
                    ? Number(value)
                    : key === "isAllowed"
                      ? Boolean(value)
                      : value
              }))
            }
            onSubmit={() => upsertMutation.mutate(upsertForm)}
            submitLabel={upsertMutation.isPending ? t("saving") : t("savePermission")}
          />

          {upsertMutation.isError ? (
            <p className="form-feedback form-feedback--error">
              {upsertMutation.error.detail ?? upsertMutation.error.title}
            </p>
          ) : null}

          {upsertMutation.isSuccess ? (
            <p className="form-feedback form-feedback--success">{t("savePermissionSuccess")}</p>
          ) : null}
        </PanelCard>
      </div>
    </div>
  );
}
