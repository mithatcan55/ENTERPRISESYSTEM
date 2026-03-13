import type { ReportDesignerCapability, ReportTemplateListItem } from "./reporting.types";

export const starterReportTemplates: ReportTemplateListItem[] = [
  {
    id: "report-user-access-summary",
    code: "RPT-ACCESS-001",
    name: "Kullanici Yetki Ozeti",
    type: "summary",
    status: "draft",
    updatedAt: new Date().toISOString(),
    version: "0.1.0"
  },
  {
    id: "report-outbox-operation-log",
    code: "RPT-OPS-002",
    name: "Outbox Operasyon Ozeti",
    type: "table",
    status: "draft",
    updatedAt: new Date().toISOString(),
    version: "0.1.0"
  }
];

export const reportDesignerCapabilities: ReportDesignerCapability[] = [
  {
    key: "labels",
    label: "Label / text",
    description: "Serbest metin, baslik, aciklama ve sabit etiket alanlari."
  },
  {
    key: "images",
    label: "Image / logo",
    description: "Sirket logosu, imza ve rapor icindeki statik veya dinamik gorseller."
  },
  {
    key: "dynamicFields",
    label: "Dynamic fields",
    description: "Payload icinden gelen alanlari belirli koordinatlara veya bloklara baglama."
  },
  {
    key: "tables",
    label: "Table / loop",
    description: "Tekrarlayan satirlar, liste bloklari ve sayfa tasmasinda otomatik devam."
  },
  {
    key: "pageMeta",
    label: "Page meta",
    description: "Sayfa numarasi, toplam sayfa, ust bilgi ve alt bilgi alanlari."
  }
];
