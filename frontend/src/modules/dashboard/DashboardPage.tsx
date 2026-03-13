import { useTranslation } from "react-i18next";
import { KpiCard } from "../../design-system/patterns/KpiCard";
import { PageHeader } from "../../design-system/patterns/PageHeader";
import { QuickActionCard } from "../../design-system/patterns/QuickActionCard";
import { PanelCard } from "../../design-system/primitives/PanelCard";

export function DashboardPage() {
  const { t } = useTranslation("common");

  return (
    <div className="page-grid">
      <PageHeader
        title={t("dashboardTitle")}
        description="Platformun cekirdek yonetim alanlari ve operasyonel saglik gostergeleri."
      />

      <section className="kpi-grid">
        <KpiCard title={t("activeUsers")} value="128" tone="primary" delta="+12%" />
        <KpiCard title={t("openSessions")} value="342" tone="neutral" delta="+5%" />
        <KpiCard title={t("systemErrors")} value="4" tone="danger" delta="-33%" />
        <KpiCard title={t("pendingOutbox")} value="17" tone="accent" delta="+2" />
      </section>

      <section className="dashboard-main-grid">
        <PanelCard title={t("securityEvents")} subtitle={t("viewAll")}>
          <div className="event-list">
            <div className="event-list__item event-list__item--danger">3 failed login attempts detected</div>
            <div className="event-list__item">T-Code deny events remained within threshold</div>
            <div className="event-list__item">Session revoke operations completed successfully</div>
          </div>
        </PanelCard>

        <PanelCard title={t("alerts")} subtitle="Audit & operations">
          <div className="event-list">
            <div className="event-list__item event-list__item--accent">Outbox queue approaching retry threshold</div>
            <div className="event-list__item">One critical exception notification escalated by email</div>
            <div className="event-list__item">No rate-limit saturation detected in the last hour</div>
          </div>
        </PanelCard>
      </section>

      <section className="quick-action-grid">
        <QuickActionCard title="Yeni Kullanici" description="Identity modulune hizli gecis" to="/identity/users" />
        <QuickActionCard title="Rol ve Yetki" description="Rol, action ve T-Code yonetimi" to="/authorization/roles" />
        <QuickActionCard title="Audit Merkezi" description="Log ve denetim ekranlari" to="/operations/audit" />
        <QuickActionCard title="Outbox" description="Kuyruk ve entegrasyon akislarini izle" to="/integrations/outbox" />
      </section>
    </div>
  );
}
