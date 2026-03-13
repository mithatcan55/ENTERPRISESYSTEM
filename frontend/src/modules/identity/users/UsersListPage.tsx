import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { ApiError } from "../../../core/api/httpClient";
import { PageHeader } from "../../../design-system/patterns/PageHeader";
import { PanelCard } from "../../../design-system/primitives/PanelCard";
import { StandardDataTable, type TableColumn } from "../../../design-system/data-display/StandardDataTable";
import { StandardForm, type FormField } from "../../../design-system/forms/StandardForm";
import { createUser, listUsers, type CreateUserPayload, type UserListItem } from "./users.api";

const initialCreateUserForm: CreateUserPayload = {
  userCode: "",
  username: "",
  email: "",
  password: "",
  companyId: 1,
  notifyAdminByMail: false,
  adminEmail: ""
};

export function UsersListPage() {
  const { t } = useTranslation(["common", "identity"]);
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [createUserForm, setCreateUserForm] = useState<CreateUserPayload>(initialCreateUserForm);

  const usersQuery = useQuery({
    queryKey: ["identity", "users"],
    queryFn: ({ signal }) => listUsers(signal)
  });

  const createUserMutation = useMutation<UserListItem, ApiError, CreateUserPayload>({
    mutationFn: (payload: CreateUserPayload) => createUser(payload),
    onSuccess: async () => {
      // Formu sifirlayip listeyi invalidate etmek, CRUD ekranlarinda tekrar kullanilacak
      // standart optimistic olmayan temel akistir.
      setCreateUserForm(initialCreateUserForm);
      await queryClient.invalidateQueries({ queryKey: ["identity", "users"] });
    }
  });

  const normalizedSearch = search.trim().toLowerCase();
  const filteredUsers = !normalizedSearch
    ? usersQuery.data ?? []
    : (usersQuery.data ?? []).filter(
      (item) =>
        item.userCode.toLowerCase().includes(normalizedSearch) ||
        item.username.toLowerCase().includes(normalizedSearch) ||
        item.email.toLowerCase().includes(normalizedSearch)
    );

  const pageSize = 8;
  const pagedUsers = filteredUsers.slice((page - 1) * pageSize, page * pageSize);

  const columns: Array<TableColumn<UserListItem>> = [
    {
      key: "userCode",
      header: t("identity:userCode"),
      sortable: true,
      cell: (item) => item.userCode
    },
    {
      key: "username",
      header: t("identity:username"),
      sortable: true,
      cell: (item) => item.username
    },
    {
      key: "email",
      header: t("identity:email"),
      cell: (item) => item.email
    },
    {
      key: "status",
      header: t("identity:status"),
      mobileLabel: t("identity:status"),
      cell: (item) => (
        <span className={`status-badge ${item.isActive ? "status-badge--success" : "status-badge--muted"}`}>
          {item.isActive ? t("identity:active") : t("identity:inactive")}
        </span>
      )
    },
    {
      key: "password",
      header: t("identity:passwordState"),
      mobileLabel: t("identity:passwordState"),
      cell: (item) => (
        <span
          className={`status-badge ${item.mustChangePassword ? "status-badge--warning" : "status-badge--default"}`}
        >
          {item.mustChangePassword ? t("identity:mustChangePassword") : t("identity:passwordHealthy")}
        </span>
      )
    },
    {
      key: "createdAt",
      header: t("identity:createdAt"),
      align: "right",
      cell: (item) => new Intl.DateTimeFormat("tr-TR").format(new Date(item.createdAt))
    }
  ];

  const createUserFields: FormField[] = [
    {
      key: "userCode",
      label: t("identity:userCode"),
      type: "text",
      value: createUserForm.userCode,
      placeholder: t("identity:userCodePlaceholder")
    },
    {
      key: "username",
      label: t("identity:username"),
      type: "text",
      value: createUserForm.username,
      placeholder: t("identity:usernamePlaceholder")
    },
    {
      key: "email",
      label: t("identity:email"),
      type: "email",
      value: createUserForm.email,
      placeholder: t("identity:emailPlaceholder")
    },
    {
      key: "password",
      label: t("identity:temporaryPassword"),
      type: "password",
      value: createUserForm.password,
      placeholder: t("identity:temporaryPasswordPlaceholder")
    },
    {
      key: "companyId",
      label: t("identity:companyId"),
      type: "number",
      value: createUserForm.companyId
    },
    {
      key: "notifyAdminByMail",
      label: t("identity:notifyAdminByMail"),
      type: "switch",
      value: createUserForm.notifyAdminByMail,
      helpText: t("identity:notifyAdminHelp")
    },
    {
      key: "adminEmail",
      label: t("identity:adminEmail"),
      type: "email",
      value: createUserForm.adminEmail ?? "",
      placeholder: t("identity:adminEmailPlaceholder")
    }
  ];

  return (
    <div className="page-grid">
      <PageHeader title={t("identity:usersPageTitle")} description={t("identity:usersPageDescription")} />

      <div className="workspace-grid workspace-grid--two-columns">
        <PanelCard title={t("identity:usersListTitle")} subtitle={t("identity:usersListSubtitle")}>
          <StandardDataTable
            columns={columns}
            items={pagedUsers}
            rowKey={(item) => item.id}
            loading={usersQuery.isLoading}
            emptyTitle={t("identity:noUsersTitle")}
            emptyDescription={t("identity:noUsersDescription")}
            searchValue={search}
            onSearchChange={(value) => {
              setSearch(value);
              setPage(1);
            }}
            searchPlaceholder={t("identity:userSearchPlaceholder")}
            totalCount={filteredUsers.length}
            page={page}
            pageSize={pageSize}
            onPageChange={setPage}
            actions={[
              {
                key: "detail",
                label: t("identity:viewDetail"),
                onClick: (item) => window.alert(`${item.username} detay ekrani sonraki adimda baglanacak.`)
              },
              {
                key: "deactivate",
                label: t("identity:deactivate"),
                tone: "danger",
                hidden: (item) => !item.isActive,
                onClick: (item) => window.alert(`${item.username} icin deaktif akisi sonraki adimda baglanacak.`)
              }
            ]}
          />
        </PanelCard>

        <PanelCard title={t("identity:createUserTitle")} subtitle={t("identity:createUserSubtitle")}>
          <StandardForm
            fields={createUserFields}
            onChange={(key, value) =>
              setCreateUserForm((current) => ({
                ...current,
                [key]: value
              }))
            }
            onSubmit={() => createUserMutation.mutate(createUserForm)}
            submitLabel={createUserMutation.isPending ? t("identity:saving") : t("identity:createUserAction")}
          />

          {createUserMutation.isError ? (
            <p className="form-feedback form-feedback--error">
              {createUserMutation.error.detail ?? createUserMutation.error.title}
            </p>
          ) : null}

          {createUserMutation.isSuccess ? (
            <p className="form-feedback form-feedback--success">{t("identity:createUserSuccess")}</p>
          ) : null}
        </PanelCard>
      </div>
    </div>
  );
}
