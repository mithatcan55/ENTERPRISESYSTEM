// Reporting modulu sadece tek bir designer sayfasindan ibaret olmayacak.
// Registry, template metadata ve render niyeti once tip seviyesinde netlesirse
// sonraki adimlarda pdfme editorunu daha temiz baglariz.
export type ReportTemplateType = "document" | "label" | "table" | "summary";

export type ReportTemplateStatus = "draft" | "published" | "archived";

export type ReportTemplateStorageMode = "local-draft" | "backend-registry";

export type ReportTemplateListItem = {
  id: string;
  code: string;
  name: string;
  description: string;
  moduleKey: string;
  type: ReportTemplateType;
  status: ReportTemplateStatus;
  storageMode: ReportTemplateStorageMode;
  updatedAt: string;
  version: string;
  sampleInput: Record<string, string>;
};

export type ReportDesignerCapability = {
  key: string;
  label: string;
  description: string;
};
