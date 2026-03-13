import type { ReportDesignerCapability, ReportTemplateListItem } from "./reporting.types";

export const starterReportTemplates: ReportTemplateListItem[] = [
  {
    id: "report-user-access-summary",
    code: "RPT-ACCESS-001",
    name: "Kullanici Yetki Ozeti",
    description: "Kullanici, rol ve action permission ozetini marka basligi ile cikarir.",
    moduleKey: "identity",
    type: "summary",
    status: "draft",
    storageMode: "local-draft",
    updatedAt: new Date().toISOString(),
    version: "0.1.0",
    sampleInput: {
      reportTitle: "Kullanici Yetki Ozeti",
      reportSubtitle: "Identity modulu icin ornek rapor",
      reportOwner: "CORE_ADMIN",
      reportDate: "2026-03-13",
      reportFooter: "Sayfa 1 / 1"
    }
  },
  {
    id: "report-outbox-operation-log",
    code: "RPT-OPS-002",
    name: "Outbox Operasyon Ozeti",
    description: "Outbox kuyrugu, son dispatch denemeleri ve hata sayilarini ozetler.",
    moduleKey: "integrations",
    type: "table",
    status: "draft",
    storageMode: "local-draft",
    updatedAt: new Date().toISOString(),
    version: "0.1.0",
    sampleInput: {
      reportTitle: "Outbox Operasyon Ozeti",
      reportSubtitle: "Integrations modulu icin ornek rapor",
      reportOwner: "system.dispatcher",
      reportDate: "2026-03-13",
      reportFooter: "Sayfa 1 / 1"
    }
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
