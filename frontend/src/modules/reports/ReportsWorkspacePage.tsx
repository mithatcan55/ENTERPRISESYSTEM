import type { Template } from "@pdfme/common";
import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useBrandTheme } from "../../app/providers/BrandProvider";
import { StandardDataTable, type TableColumn } from "../../design-system/data-display/StandardDataTable";
import { PageHeader } from "../../design-system/patterns/PageHeader";
import { PanelCard } from "../../design-system/primitives/PanelCard";
import { createDefaultPdfmeTemplate } from "./pdfme/defaultTemplate";
import { PdfmeDesignerPanel } from "./pdfme/PdfmeDesignerPanel";
import { reportDesignerCapabilities, starterReportTemplates } from "./reporting.catalog";
import { clearStoredReportTemplate, readStoredReportTemplate, writeStoredReportTemplate } from "./reporting.storage";
import type { ReportDesignerCapability, ReportTemplateListItem } from "./reporting.types";

export function ReportsWorkspacePage() {
  const { t } = useTranslation("reports");
  const { theme } = useBrandTheme();
  const [selectedTemplate, setSelectedTemplate] = useState<ReportTemplateListItem>(starterReportTemplates[0]);
  const activeDefaultTemplate = useMemo(
    () =>
      createDefaultPdfmeTemplate({
        primary: theme.colors.primary,
        neutralBrand: theme.colors.neutralBrand,
        accent: theme.colors.accent
      }),
    [theme.colors.accent, theme.colors.neutralBrand, theme.colors.primary]
  );
  const [workingTemplate, setWorkingTemplate] = useState<Template>(activeDefaultTemplate);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);

  useEffect(() => {
    const storedTemplate = readStoredReportTemplate(selectedTemplate.id);
    setWorkingTemplate(storedTemplate ?? activeDefaultTemplate);
    setPreviewUrl(null);
  }, [activeDefaultTemplate, selectedTemplate.id]);

  useEffect(() => {
    writeStoredReportTemplate(selectedTemplate.id, workingTemplate);
  }, [selectedTemplate.id, workingTemplate]);

  useEffect(() => {
    return () => {
      if (previewUrl) {
        URL.revokeObjectURL(previewUrl);
      }
    };
  }, [previewUrl]);

  const templateColumns: Array<TableColumn<ReportTemplateListItem>> = [
    { key: "code", header: "Kod", cell: (item) => item.code },
    { key: "name", header: "Ad", cell: (item) => item.name },
    { key: "type", header: "Tip", cell: (item) => item.type },
    { key: "version", header: "Versiyon", cell: (item) => item.version },
    { key: "storageMode", header: "Saklama", cell: (item) => item.storageMode }
  ];

  const capabilityColumns: Array<TableColumn<ReportDesignerCapability>> = [
    { key: "label", header: "Yetkinlik", cell: (item) => item.label },
    { key: "description", header: "Aciklama", cell: (item) => item.description }
  ];

  return (
    <div className="page-grid">
      <PageHeader title={t("workspaceTitle")} description={t("workspaceDescription")} />

      <PanelCard title={t("draftStorageTitle")} subtitle={t("draftStorageSubtitle")}>
        <div className="report-storage-notice">
          <strong>{t("draftStorageHeadline")}</strong>
          <span>{t("draftStorageBody")}</span>
          <div className="report-storage-notice__actions">
            <button
              type="button"
              className="secondary-button"
              onClick={() => {
                clearStoredReportTemplate(selectedTemplate.id);
                setWorkingTemplate(activeDefaultTemplate);
                setPreviewUrl(null);
              }}
            >
              {t("resetDraft")}
            </button>
          </div>
        </div>
      </PanelCard>

      <div className="workspace-grid workspace-grid--two-columns">
        <PanelCard title={t("registryTitle")} subtitle={t("registrySubtitle")}>
          <StandardDataTable
            columns={templateColumns}
            items={starterReportTemplates}
            rowKey={(item) => item.id}
            emptyTitle={t("emptyRegistryTitle")}
            emptyDescription={t("emptyRegistryDescription")}
            actions={[
              {
                key: "select",
                label: t("openDraft"),
                onClick: (item) => setSelectedTemplate(item)
              }
            ]}
          />
        </PanelCard>

        <PanelCard title={t("capabilitiesTitle")} subtitle={t("capabilitiesSubtitle")}>
          <StandardDataTable
            columns={capabilityColumns}
            items={reportDesignerCapabilities}
            rowKey={(item) => item.key}
            emptyTitle={t("emptyCapabilitiesTitle")}
            emptyDescription={t("emptyCapabilitiesDescription")}
          />
        </PanelCard>
      </div>

      <div className="workspace-grid workspace-grid--two-columns">
        <PanelCard
          title={`${t("designerTitle")} / ${selectedTemplate.code}`}
          subtitle={`${selectedTemplate.name} - ${selectedTemplate.description}`}
        >
          <PdfmeDesignerPanel
            template={workingTemplate}
            sampleInput={selectedTemplate.sampleInput}
            primaryColor={theme.colors.primary}
            onTemplateChange={setWorkingTemplate}
            onPreviewReady={(nextUrl) => {
              setPreviewUrl((currentUrl) => {
                if (currentUrl) {
                  URL.revokeObjectURL(currentUrl);
                }

                return nextUrl;
              });
            }}
          />
        </PanelCard>

        <PanelCard title={t("previewTitle")} subtitle={t("previewSubtitle")}>
          {previewUrl ? (
            <iframe title="pdf-preview" src={previewUrl} className="pdf-preview-frame" />
          ) : (
            <div className="standard-table__empty">
              <strong>{t("noPreviewTitle")}</strong>
              <span>{t("noPreviewDescription")}</span>
            </div>
          )}
        </PanelCard>
      </div>
    </div>
  );
}
