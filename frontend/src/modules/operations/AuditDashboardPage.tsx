import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { KpiCard } from "../../design-system/patterns/KpiCard";
import { PageHeader } from "../../design-system/patterns/PageHeader";
import { PanelCard } from "../../design-system/primitives/PanelCard";
import { getAuditDashboardSummary } from "./operations.api";

export function AuditDashboardPage() {
  const { t } = useTranslation(["operations"]);
  const summaryQuery = useQuery({
    queryKey: ["operations", "audit-summary"],
    queryFn: ({ signal }) => getAuditDashboardSummary(24, signal)
  });

  const summary = summaryQuery.data;

  return (
    <div className="page-grid">
      <PageHeader title={t("auditPageTitle")} description={t("auditPageDescription")} />

      <section className="kpi-grid">
        <KpiCard title={t("systemErrors")} value={String(summary?.systemErrorCount ?? 0)} tone="danger" delta={t("last24Hours")} />
        <KpiCard title={t("failedLogins")} value={String(summary?.failedLoginCount ?? 0)} tone="accent" delta={t("last24Hours")} />
        <KpiCard
          title={t("sessionRevokeRate")}
          value={`${summary?.sessionRevokeRatePercent ?? 0}%`}
          tone="neutral"
          delta={t("windowHours", { count: summary?.windowHours ?? 24 })}
        />
        <KpiCard title={t("criticalEvents")} value={String(summary?.topCriticalEvents.length ?? 0)} tone="primary" delta={t("activeSummary")} />
      </section>

      <section className="dashboard-main-grid">
        <PanelCard title={t("failedLoginTrend")} subtitle={t("hourlyView")}>
          <div className="event-list">
            {(summary?.failedLoginTrend ?? []).map((item) => (
              <div key={item.hour} className="event-list__item">
                <strong>{new Intl.DateTimeFormat("tr-TR", { hour: "2-digit", minute: "2-digit" }).format(new Date(item.hour))}</strong>
                <span>{item.count}</span>
              </div>
            ))}
            {!summary?.failedLoginTrend.length ? <div className="event-list__item">{t("noTrendData")}</div> : null}
          </div>
        </PanelCard>

        <PanelCard title={t("topCriticalEvents")} subtitle={t("topList")}>
          <div className="event-list">
            {(summary?.topCriticalEvents ?? []).map((item) => (
              <div key={item.eventType} className="event-list__item event-list__item--accent">
                <strong>{item.eventType}</strong>
                <span>{item.count}</span>
              </div>
            ))}
            {!summary?.topCriticalEvents.length ? <div className="event-list__item">{t("noCriticalEvents")}</div> : null}
          </div>
        </PanelCard>
      </section>
    </div>
  );
}
