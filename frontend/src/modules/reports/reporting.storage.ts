import type { Template } from "@pdfme/common";

const storagePrefix = "enterprise-system-report-template";

function getStorageKey(templateId: string) {
  return `${storagePrefix}:${templateId}`;
}

// Report designer bu asamada veritabanina yazmadigi icin taslaklari tarayici
// localStorage alaninda sakliyoruz. Bu davranis dokumanda da acikca belirtilir.
export function readStoredReportTemplate(templateId: string): Template | null {
  const rawValue = window.localStorage.getItem(getStorageKey(templateId));

  if (!rawValue) {
    return null;
  }

  try {
    return JSON.parse(rawValue) as Template;
  } catch {
    return null;
  }
}

export function writeStoredReportTemplate(templateId: string, template: Template) {
  window.localStorage.setItem(getStorageKey(templateId), JSON.stringify(template));
}

export function clearStoredReportTemplate(templateId: string) {
  window.localStorage.removeItem(getStorageKey(templateId));
}
