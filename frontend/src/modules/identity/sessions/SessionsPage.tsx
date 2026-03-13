import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { ApiError } from "../../../core/api/httpClient";
import { StandardDataTable, type TableColumn } from "../../../design-system/data-display/StandardDataTable";
import { StandardForm, type FormField } from "../../../design-system/forms/StandardForm";
import { PageHeader } from "../../../design-system/patterns/PageHeader";
import { PanelCard } from "../../../design-system/primitives/PanelCard";
import { listSessions, revokeSession, type RevokeSessionPayload, type SessionListItem } from "./sessions.api";

const initialRevokeForm: RevokeSessionPayload = {
  reason: ""
};

export function SessionsPage() {
  const { t } = useTranslation(["identity"]);
  const queryClient = useQueryClient();
  const [sessionQueryUserId, setSessionQueryUserId] = useState<number>(1);
  const [onlyActive, setOnlyActive] = useState(true);
  const [selectedSession, setSelectedSession] = useState<SessionListItem | null>(null);
  const [revokeForm, setRevokeForm] = useState<RevokeSessionPayload>(initialRevokeForm);

  const sessionsQuery = useQuery({
    queryKey: ["identity", "sessions", sessionQueryUserId, onlyActive],
    queryFn: ({ signal }) => listSessions(sessionQueryUserId, onlyActive, signal)
  });

  const revokeSessionMutation = useMutation<void, ApiError, { sessionId: number; payload: RevokeSessionPayload }>({
    mutationFn: ({ sessionId, payload }) => revokeSession(sessionId, payload),
    onSuccess: async () => {
      // Revoke sonrasi listeyi tekrar sorguluyoruz; session ekraninda gercek durum DB tarafindan gelsin istiyoruz.
      setRevokeForm(initialRevokeForm);
      await queryClient.invalidateQueries({ queryKey: ["identity", "sessions"] });
      setSelectedSession((current) => (current ? { ...current, isRevoked: true } : current));
    }
  });

  const columns: Array<TableColumn<SessionListItem>> = [
    {
      key: "sessionKey",
      header: t("sessionKey"),
      cell: (item) => item.sessionKey.slice(0, 12)
    },
    {
      key: "clientIpAddress",
      header: t("clientIp"),
      cell: (item) => item.clientIpAddress ?? "-"
    },
    {
      key: "lastSeenAt",
      header: t("lastSeen"),
      cell: (item) =>
        item.lastSeenAt ? new Intl.DateTimeFormat("tr-TR").format(new Date(item.lastSeenAt)) : "-"
    },
    {
      key: "expiresAt",
      header: t("expiresAt"),
      cell: (item) => new Intl.DateTimeFormat("tr-TR").format(new Date(item.expiresAt))
    },
    {
      key: "status",
      header: t("status"),
      cell: (item) => (
        <span className={`status-badge ${item.isRevoked ? "status-badge--muted" : "status-badge--success"}`}>
          {item.isRevoked ? t("revoked") : t("active")}
        </span>
      )
    }
  ];

  const filterFields: FormField[] = [
    {
      key: "sessionQueryUserId",
      label: t("userId"),
      type: "number",
      value: sessionQueryUserId
    },
    {
      key: "onlyActive",
      label: t("onlyActiveSessions"),
      type: "switch",
      value: onlyActive,
      helpText: t("onlyActiveSessionsHelp")
    }
  ];

  const revokeFields: FormField[] = [
    {
      key: "reason",
      label: t("revokeReason"),
      type: "textarea",
      value: revokeForm.reason ?? "",
      placeholder: t("revokeReasonPlaceholder")
    }
  ];

  return (
    <div className="page-grid">
      <PageHeader title={t("sessionsPageTitle")} description={t("sessionsPageDescription")} />

      <div className="workspace-grid workspace-grid--two-columns">
        <PanelCard title={t("sessionsListTitle")} subtitle={t("sessionsListSubtitle")}>
          <StandardForm
            fields={filterFields}
            onChange={(key, value) => {
              if (key === "sessionQueryUserId") {
                setSessionQueryUserId(Number(value));
              }

              if (key === "onlyActive") {
                setOnlyActive(Boolean(value));
              }
            }}
          />

          <div className="spacer-block" />

          <StandardDataTable
            columns={columns}
            items={sessionsQuery.data ?? []}
            rowKey={(item) => item.id}
            loading={sessionsQuery.isLoading}
            emptyTitle={t("noSessionsTitle")}
            emptyDescription={t("noSessionsDescription")}
            actions={[
              {
                key: "detail",
                label: t("viewDetail"),
                onClick: (item) => setSelectedSession(item)
              },
              {
                key: "revoke",
                label: t("revokeSession"),
                tone: "danger",
                hidden: (item) => item.isRevoked,
                onClick: (item) => setSelectedSession(item)
              }
            ]}
          />
        </PanelCard>

        <PanelCard title={t("sessionDetailTitle")} subtitle={t("sessionDetailSubtitle")}>
          {selectedSession ? (
            <>
              <div className="detail-summary">
                <div>
                  <span>{t("sessionKey")}</span>
                  <strong>{selectedSession.sessionKey}</strong>
                </div>
                <div>
                  <span>{t("clientIp")}</span>
                  <strong>{selectedSession.clientIpAddress ?? "-"}</strong>
                </div>
                <div>
                  <span>{t("userAgent")}</span>
                  <strong>{selectedSession.userAgent ?? "-"}</strong>
                </div>
              </div>

              <StandardForm
                fields={revokeFields}
                onChange={(key, value) =>
                  setRevokeForm((current) => ({
                    ...current,
                    [key]: value
                  }))
                }
                onSubmit={() => revokeSessionMutation.mutate({ sessionId: selectedSession.id, payload: revokeForm })}
                submitLabel={revokeSessionMutation.isPending ? t("saving") : t("revokeSession")}
              />

              <div className="detail-actions">
                <button className="secondary-button" type="button" onClick={() => setSelectedSession(null)}>
                  {t("closeDetail")}
                </button>
              </div>

              {revokeSessionMutation.isError ? (
                <p className="form-feedback form-feedback--error">
                  {revokeSessionMutation.error.detail ?? revokeSessionMutation.error.title}
                </p>
              ) : null}

              {revokeSessionMutation.isSuccess ? (
                <p className="form-feedback form-feedback--success">{t("revokeSessionSuccess")}</p>
              ) : null}
            </>
          ) : (
            <div className="standard-table__empty">
              <strong>{t("sessionDetailEmptyTitle")}</strong>
              <span>{t("sessionDetailEmptyDescription")}</span>
            </div>
          )}
        </PanelCard>
      </div>
    </div>
  );
}
