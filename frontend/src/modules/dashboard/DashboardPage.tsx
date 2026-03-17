import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { KpiCard } from "../../design-system/patterns/KpiCard";
import { PageHeader } from "../../design-system/patterns/PageHeader";
import { QuickActionCard } from "../../design-system/patterns/QuickActionCard";
import { PanelCard } from "../../design-system/primitives/PanelCard";
import { listPendingApprovals } from "../approvals/approvals.api";
import { listOutboxMessages } from "../integrations/outbox/outbox.api";
import { getAuditDashboardSummary, getSecurityLogs } from "../operations/operations.api";

export function DashboardPage() {
  const { t } = useTranslation(["common", "operations", "integrations", "approvals"]);
  const summaryQuery = useQuery({
    queryKey: ["dashboard", "audit-summary"],
    queryFn: ({ signal }) => getAuditDashboardSummary(24, signal)
  });
  const securityQuery = useQuery({
    queryKey: ["dashboard", "security"],
    queryFn: ({ signal }) => getSecurityLogs({ page: 1, pageSize: 3 }, signal)
  });
  const outboxQuery = useQuery({
    queryKey: ["dashboard", "outbox"],
    queryFn: ({ signal }) => listOutboxMessages({ page: 1, pageSize: 3 }, signal)
  });
  const approvalQuery = useQuery({
    queryKey: ["dashboard", "approvals"],
    queryFn: ({ signal }) => listPendingApprovals(signal)
  });
  const summary = summaryQuery.data;
  const securityItems = securityQuery.data?.items ?? [];
  const outboxItems = outboxQuery.data?.items ?? [];
  const approvalItems = approvalQuery.data?.items ?? [];

  return (
    <div className="page-grid">
      <PageHeader
        title={t("dashboardTitle")}
        description="Platformun cekirdek yonetim alanlari ve operasyonel saglik gostergeleri."
      />

      <section className="kpi-grid">
        <KpiCard title={t("operations:systemErrors")} value={String(summary?.systemErrorCount ?? 0)} tone="danger" delta={t("operations:last24Hours")} />
        <KpiCard title={t("operations:failedLogins")} value={String(summary?.failedLoginCount ?? 0)} tone="accent" delta={t("operations:last24Hours")} />
        <KpiCard title={t("operations:sessionRevokeRate")} value={`${summary?.sessionRevokeRatePercent ?? 0}%`} tone="neutral" delta={t("operations:activeSummary")} />
        <KpiCard title={t("pendingOutbox")} value={String(outboxQuery.data?.totalCount ?? 0)} tone="primary" delta={t("integrations:queueStatus")} />
        <KpiCard title={t("pendingApprovals")} value={String(approvalQuery.data?.totalCount ?? 0)} tone="accent" delta={t("approvals:pendingListSubtitle")} />
      </section>

      <section className="dashboard-main-grid">
        <PanelCard title={t("securityEvents")} subtitle={t("viewAll")}>
          <div className="event-list">
            {securityItems.map((item) => (
              <div key={item.id} className={`event-list__item ${item.isSuccessful ? "" : "event-list__item--danger"}`}>
                {item.eventType ?? t("operations:unknownEvent")} / {item.username ?? "-"}
              </div>
            ))}
            {!securityItems.length ? <div className="event-list__item">{t("operations:noCriticalEvents")}</div> : null}
          </div>
        </PanelCard>

        <PanelCard title={t("alerts")} subtitle="Audit & operations">
          <div className="event-list">
            {outboxItems.map((item) => (
              <div key={item.id} className={`event-list__item ${item.lastError ? "event-list__item--accent" : ""}`}>
                {item.eventType} / {item.status}
              </div>
            ))}
            {!outboxItems.length ? <div className="event-list__item">{t("integrations:noOutboxDescription")}</div> : null}
          </div>
        </PanelCard>

        <PanelCard title={t("approvalInboxTitle")} subtitle={t("approvals:viewApprovalQueue")}>
          <div className="event-list">
            {approvalItems.map((item) => (
              <div key={`${item.approvalInstanceId}-${item.approvalInstanceStepId}`} className="event-list__item">
                <strong>{item.workflowCode}</strong>
                <span>{item.referenceType} / {item.referenceId}</span>
                <span>{t("approvals:stepLabel", { step: item.stepOrder })}</span>
              </div>
            ))}
            {!approvalItems.length ? <div className="event-list__item">{t("approvals:emptyPendingDescription")}</div> : null}
          </div>
        </PanelCard>
      </section>

      <section className="quick-action-grid">
        <QuickActionCard title="Yeni Kullanici" description="Identity modulune hizli gecis" to="/identity/users" />
        <QuickActionCard title="Rol ve Yetki" description="Rol, action ve T-Code yonetimi" to="/authorization/roles" />
        <QuickActionCard title="Audit Merkezi" description="Log ve denetim ekranlari" to="/operations/audit" />
        <QuickActionCard title="Outbox" description="Kuyruk ve entegrasyon akislarini izle" to="/integrations/outbox" />
        <QuickActionCard title={t("approvalInboxTitle")} description={t("approvals:pendingListSubtitle")} to="/approvals/inbox" />
      </section>
    </div>
  );
}
