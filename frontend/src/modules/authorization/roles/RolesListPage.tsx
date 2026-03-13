import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { ApiError } from "../../../core/api/httpClient";
import { StandardDataTable, type TableColumn } from "../../../design-system/data-display/StandardDataTable";
import { StandardForm, type FormField } from "../../../design-system/forms/StandardForm";
import { PageHeader } from "../../../design-system/patterns/PageHeader";
import { PanelCard } from "../../../design-system/primitives/PanelCard";
import { createRole, listRoles, type CreateRolePayload, type RoleListItem } from "./roles.api";

const initialCreateRoleForm: CreateRolePayload = {
  code: "",
  name: "",
  description: ""
};

export function RolesListPage() {
  const { t } = useTranslation(["authorization"]);
  const queryClient = useQueryClient();
  const [createRoleForm, setCreateRoleForm] = useState<CreateRolePayload>(initialCreateRoleForm);

  const rolesQuery = useQuery({
    queryKey: ["authorization", "roles"],
    queryFn: ({ signal }) => listRoles(signal)
  });

  const createRoleMutation = useMutation<RoleListItem, ApiError, CreateRolePayload>({
    mutationFn: (payload) => createRole(payload),
    onSuccess: async () => {
      setCreateRoleForm(initialCreateRoleForm);
      await queryClient.invalidateQueries({ queryKey: ["authorization", "roles"] });
    }
  });

  const columns: Array<TableColumn<RoleListItem>> = [
    { key: "code", header: t("code"), cell: (item) => item.code },
    { key: "name", header: t("name"), cell: (item) => item.name },
    { key: "description", header: t("description"), cell: (item) => item.description ?? "-" },
    {
      key: "type",
      header: t("type"),
      cell: (item) => (
        <span className={`status-badge ${item.isSystemRole ? "status-badge--default" : "status-badge--success"}`}>
          {item.isSystemRole ? t("systemRole") : t("customRole")}
        </span>
      )
    }
  ];

  const createRoleFields: FormField[] = [
    { key: "code", label: t("code"), type: "text", value: createRoleForm.code, placeholder: "SYS_ADMIN" },
    { key: "name", label: t("name"), type: "text", value: createRoleForm.name, placeholder: t("namePlaceholder") },
    {
      key: "description",
      label: t("description"),
      type: "textarea",
      value: createRoleForm.description ?? "",
      placeholder: t("descriptionPlaceholder")
    }
  ];

  return (
    <div className="page-grid">
      <PageHeader title={t("rolesPageTitle")} description={t("rolesPageDescription")} />
      <div className="workspace-grid workspace-grid--two-columns">
        <PanelCard title={t("rolesListTitle")} subtitle={t("rolesListSubtitle")}>
          <StandardDataTable
            columns={columns}
            items={rolesQuery.data ?? []}
            rowKey={(item) => item.id}
            loading={rolesQuery.isLoading}
            emptyTitle={t("noRolesTitle")}
            emptyDescription={t("noRolesDescription")}
          />
        </PanelCard>

        <PanelCard title={t("createRoleTitle")} subtitle={t("createRoleSubtitle")}>
          <StandardForm
            fields={createRoleFields}
            onChange={(key, value) =>
              setCreateRoleForm((current) => ({
                ...current,
                [key]: value
              }))
            }
            onSubmit={() => createRoleMutation.mutate(createRoleForm)}
            submitLabel={createRoleMutation.isPending ? t("saving") : t("createRoleAction")}
          />

          {createRoleMutation.isError ? (
            <p className="form-feedback form-feedback--error">
              {createRoleMutation.error.detail ?? createRoleMutation.error.title}
            </p>
          ) : null}

          {createRoleMutation.isSuccess ? (
            <p className="form-feedback form-feedback--success">{t("createRoleSuccess")}</p>
          ) : null}
        </PanelCard>
      </div>
    </div>
  );
}
