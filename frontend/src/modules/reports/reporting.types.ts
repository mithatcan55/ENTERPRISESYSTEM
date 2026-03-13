// Reporting modulu sadece tek bir designer sayfasindan ibaret olmayacak.
// Registry, template metadata ve render niyeti once tip seviyesinde netlesirse
// sonraki adimlarda pdfme editorunu daha temiz baglariz.
export type ReportTemplateType = "document" | "label" | "table" | "summary";

export type ReportTemplateStatus = "draft" | "published" | "archived";

export type ReportTemplateListItem = {
  id: string;
  code: string;
  name: string;
  type: ReportTemplateType;
  status: ReportTemplateStatus;
  updatedAt: string;
  version: string;
};

export type ReportDesignerCapability = {
  key: string;
  label: string;
  description: string;
};
