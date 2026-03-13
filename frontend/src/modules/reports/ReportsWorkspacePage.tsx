import { StandardDataTable, type TableColumn } from "../../design-system/data-display/StandardDataTable";
import { PageHeader } from "../../design-system/patterns/PageHeader";
import { PanelCard } from "../../design-system/primitives/PanelCard";
import { reportDesignerCapabilities, starterReportTemplates } from "./reporting.catalog";
import type { ReportDesignerCapability, ReportTemplateListItem } from "./reporting.types";

export function ReportsWorkspacePage() {
  const templateColumns: Array<TableColumn<ReportTemplateListItem>> = [
    { key: "code", header: "Kod", cell: (item) => item.code },
    { key: "name", header: "Ad", cell: (item) => item.name },
    { key: "type", header: "Tip", cell: (item) => item.type },
    { key: "version", header: "Versiyon", cell: (item) => item.version }
  ];

  const capabilityColumns: Array<TableColumn<ReportDesignerCapability>> = [
    { key: "label", header: "Yetkinlik", cell: (item) => item.label },
    { key: "description", header: "Aciklama", cell: (item) => item.description }
  ];

  return (
    <div className="page-grid">
      <PageHeader
        title="Reports Workspace"
        description="pdfme tabanli rapor tasarimcisi icin altyapi hazirlik ekrani."
      />

      <div className="workspace-grid workspace-grid--two-columns">
        <PanelCard title="Template Registry" subtitle="Ilk rapor katalogu">
          <StandardDataTable
            columns={templateColumns}
            items={starterReportTemplates}
            rowKey={(item) => item.id}
            emptyTitle="Kayitli rapor yok"
            emptyDescription="Ilk raporlar taslak olarak bu listeye dusecek."
          />
        </PanelCard>

        <PanelCard title="Designer Capabilities" subtitle="pdfme odakli hedefler">
          <StandardDataTable
            columns={capabilityColumns}
            items={reportDesignerCapabilities}
            rowKey={(item) => item.key}
            emptyTitle="Yetkinlik tanimi yok"
            emptyDescription="Designer kapsam maddeleri burada listelenir."
          />
        </PanelCard>
      </div>
    </div>
  );
}
