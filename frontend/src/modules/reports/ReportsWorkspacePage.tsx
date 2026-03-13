import { PageHeader } from "../../design-system/patterns/PageHeader";
import { PanelCard } from "../../design-system/primitives/PanelCard";

export function ReportsWorkspacePage() {
  return (
    <div className="page-grid">
      <PageHeader
        title="Reports Workspace"
        description="pdfme tabanli rapor tasarimcisi icin altyapi hazirlik ekrani."
      />

      <div className="workspace-grid workspace-grid--two-columns">
        <PanelCard title="Designer Roadmap" subtitle="pdfme odakli ilk faz">
          <div className="event-list">
            <div className="event-list__item">Template registry ve report tanim kayitlari eklenecek.</div>
            <div className="event-list__item">Designer editor bu workspace icine gomulecek.</div>
            <div className="event-list__item">JSON template + backend payload binding standardi kurulacak.</div>
          </div>
        </PanelCard>

        <PanelCard title="Report Capabilities" subtitle="Hedef fonksiyonlar">
          <div className="event-list">
            <div className="event-list__item">Label, image, dynamic field ve table/loop alanlari</div>
            <div className="event-list__item">Page number, multi-page ve branded header/footer</div>
            <div className="event-list__item">Kaydet, onizle, PDF export ve sonraki fazda scheduler</div>
          </div>
        </PanelCard>
      </div>
    </div>
  );
}
