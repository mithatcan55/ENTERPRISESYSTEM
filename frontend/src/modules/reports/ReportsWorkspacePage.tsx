import type { Template } from "@pdfme/common";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useBrandTheme } from "../../app/providers/BrandProvider";
import type { ApiError } from "../../core/api/httpClient";
import { StandardDataTable, type TableColumn } from "../../design-system/data-display/StandardDataTable";
import { PageHeader } from "../../design-system/patterns/PageHeader";
import { PanelCard } from "../../design-system/primitives/PanelCard";
import { createDefaultPdfmeTemplate } from "./pdfme/defaultTemplate";
import { PdfmeDesignerPanel } from "./pdfme/PdfmeDesignerPanel";
import { reportDesignerCapabilities, starterReportTemplates } from "./reporting.catalog";
import { clearStoredReportTemplate, readStoredReportTemplate, writeStoredReportTemplate } from "./reporting.storage";
import type { ReportDesignerCapability, ReportTemplateListItem } from "./reporting.types";
import {
  archiveReportTemplate,
  createReportTemplate,
  getReportTemplate,
  listReportTemplates,
  publishReportTemplate,
  updateReportTemplate,
  type ReportTemplateDetailApi,
  type ReportTemplateListItemApi
} from "./reports.api";

function tryParseTemplateJson(templateJson: string, fallback: Template) {
  try {
    return JSON.parse(templateJson) as Template;
  } catch {
    return fallback;
  }
}

function tryParseSampleInputJson(sampleInputJson: string, fallback: Record<string, string>) {
  try {
    return JSON.parse(sampleInputJson) as Record<string, string>;
  } catch {
    return fallback;
  }
}

export function ReportsWorkspacePage() {
  const { t } = useTranslation("reports");
  const { theme } = useBrandTheme();
  const queryClient = useQueryClient();
  const [selectedTemplate, setSelectedTemplate] = useState<ReportTemplateListItem>(starterReportTemplates[0]);
  const [selectedRegistryTemplateId, setSelectedRegistryTemplateId] = useState<number | null>(null);
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
  const [designerSampleInput, setDesignerSampleInput] = useState<Record<string, string>>(starterReportTemplates[0].sampleInput);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);

  const registryQuery = useQuery({
    queryKey: ["report-templates"],
    queryFn: ({ signal }) => listReportTemplates(signal)
  });

  const detailQuery = useQuery({
    queryKey: ["report-template-detail", selectedRegistryTemplateId],
    queryFn: ({ signal }) => getReportTemplate(selectedRegistryTemplateId!, signal),
    enabled: selectedRegistryTemplateId !== null
  });

  useEffect(() => {
    if (selectedRegistryTemplateId !== null || detailQuery.data) {
      return;
    }

    // Starter blueprint secildiginde kaydedilmemis yerel calismayi geri yukluyoruz.
    // Boylece kullanici backend kaydi olusturmadan once taslakla oynayabilir.
    const storedTemplate = readStoredReportTemplate(selectedTemplate.id);
    setWorkingTemplate(storedTemplate ?? activeDefaultTemplate);
    setDesignerSampleInput(selectedTemplate.sampleInput);
    setPreviewUrl(null);
  }, [activeDefaultTemplate, detailQuery.data, selectedRegistryTemplateId, selectedTemplate]);

  useEffect(() => {
    if (!detailQuery.data) {
      return;
    }

    // Registry kaydi acildiginda designer artik local draft degil,
    // veritabanindaki aktif taslak/versiyon ustunden beslenir.
    setWorkingTemplate(tryParseTemplateJson(detailQuery.data.templateJson, activeDefaultTemplate));
    setDesignerSampleInput(tryParseSampleInputJson(detailQuery.data.sampleInputJson, selectedTemplate.sampleInput));
    setPreviewUrl(null);
  }, [activeDefaultTemplate, detailQuery.data, selectedTemplate.sampleInput]);

  useEffect(() => {
    if (selectedRegistryTemplateId !== null) {
      return;
    }

    writeStoredReportTemplate(selectedTemplate.id, workingTemplate);
  }, [selectedRegistryTemplateId, selectedTemplate.id, workingTemplate]);

  useEffect(() => {
    return () => {
      if (previewUrl) {
        URL.revokeObjectURL(previewUrl);
      }
    };
  }, [previewUrl]);

  const createMutation = useMutation<ReportTemplateDetailApi, ApiError, void>({
    mutationFn: () =>
      createReportTemplate({
        code: selectedTemplate.code,
        name: selectedTemplate.name,
        description: selectedTemplate.description,
        moduleKey: selectedTemplate.moduleKey,
        type: selectedTemplate.type,
        templateJson: JSON.stringify(workingTemplate),
        sampleInputJson: JSON.stringify(designerSampleInput),
        notes: "Frontend starter blueprint uzerinden olusturuldu."
      }),
    onSuccess: async (detail) => {
      setSelectedRegistryTemplateId(detail.id);
      await queryClient.invalidateQueries({ queryKey: ["report-templates"] });
      await queryClient.invalidateQueries({ queryKey: ["report-template-detail", detail.id] });
    }
  });

  const updateMutation = useMutation<ReportTemplateDetailApi, ApiError, void>({
    mutationFn: () =>
      updateReportTemplate(selectedRegistryTemplateId!, {
        name: detailQuery.data?.name ?? selectedTemplate.name,
        description: detailQuery.data?.description ?? selectedTemplate.description,
        moduleKey: detailQuery.data?.moduleKey ?? selectedTemplate.moduleKey,
        type: detailQuery.data?.type ?? selectedTemplate.type,
        templateJson: JSON.stringify(workingTemplate),
        sampleInputJson: JSON.stringify(designerSampleInput),
        notes: "Frontend designer uzerinden yeni versiyon kaydi."
      }),
    onSuccess: async (detail) => {
      await queryClient.invalidateQueries({ queryKey: ["report-templates"] });
      await queryClient.invalidateQueries({ queryKey: ["report-template-detail", detail.id] });
    }
  });

  const publishMutation = useMutation<ReportTemplateDetailApi, ApiError, void>({
    mutationFn: () => publishReportTemplate(selectedRegistryTemplateId!),
    onSuccess: async (detail) => {
      await queryClient.invalidateQueries({ queryKey: ["report-templates"] });
      await queryClient.invalidateQueries({ queryKey: ["report-template-detail", detail.id] });
    }
  });

  const archiveMutation = useMutation<ReportTemplateDetailApi, ApiError, void>({
    mutationFn: () => archiveReportTemplate(selectedRegistryTemplateId!),
    onSuccess: async (detail) => {
      await queryClient.invalidateQueries({ queryKey: ["report-templates"] });
      await queryClient.invalidateQueries({ queryKey: ["report-template-detail", detail.id] });
    }
  });

  const templateColumns: Array<TableColumn<ReportTemplateListItem>> = [
    { key: "code", header: "Kod", cell: (item) => item.code },
    { key: "name", header: "Ad", cell: (item) => item.name },
    { key: "type", header: "Tip", cell: (item) => item.type },
    { key: "version", header: "Versiyon", cell: (item) => item.version }
  ];

  const registryColumns: Array<TableColumn<ReportTemplateListItemApi>> = [
    { key: "code", header: "Kod", cell: (item) => item.code },
    { key: "name", header: "Ad", cell: (item) => item.name },
    { key: "status", header: "Durum", cell: (item) => item.status },
    { key: "currentVersionNumber", header: "Aktif Versiyon", cell: (item) => String(item.currentVersionNumber) }
  ];

  const capabilityColumns: Array<TableColumn<ReportDesignerCapability>> = [
    { key: "label", header: "Yetkinlik", cell: (item) => item.label },
    { key: "description", header: "Aciklama", cell: (item) => item.description }
  ];

  const actionError =
    createMutation.error ??
    updateMutation.error ??
    publishMutation.error ??
    archiveMutation.error ??
    null;

  const registryItems = registryQuery.data?.items ?? [];
  const versionItems = detailQuery.data?.versions ?? [];

  return (
    <div className="page-grid">
      <PageHeader title={t("workspaceTitle")} description={t("workspaceDescription")} />

      <PanelCard title={t("draftStorageTitle")} subtitle={t("draftStorageSubtitle")}>
        <div className="report-storage-notice">
          <strong>{t("draftStorageHeadline")}</strong>
          <span>{t("draftStorageBody")}</span>
          <span>{t("draftStorageBodySecondary")}</span>
          <div className="report-storage-notice__actions">
            <button
              type="button"
              className="secondary-button"
              onClick={() => {
                clearStoredReportTemplate(selectedTemplate.id);
                setSelectedRegistryTemplateId(null);
                setWorkingTemplate(activeDefaultTemplate);
                setDesignerSampleInput(selectedTemplate.sampleInput);
                setPreviewUrl(null);
              }}
            >
              {t("resetDraft")}
            </button>
          </div>
        </div>
      </PanelCard>

      <div className="workspace-grid workspace-grid--two-columns">
        <PanelCard title={t("starterTitle")} subtitle={t("starterSubtitle")}>
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
                onClick: (item) => {
                  setSelectedRegistryTemplateId(null);
                  setSelectedTemplate(item);
                }
              }
            ]}
          />
        </PanelCard>

        <PanelCard title={t("registryTitle")} subtitle={t("registrySubtitle")}>
          <StandardDataTable
            columns={registryColumns}
            items={registryItems}
            rowKey={(item) => item.id}
            emptyTitle={t("emptyBackendRegistryTitle")}
            emptyDescription={t("emptyBackendRegistryDescription")}
            actions={[
              {
                key: "select",
                label: t("openRegistryRecord"),
                onClick: (item) => setSelectedRegistryTemplateId(item.id)
              }
            ]}
          />
        </PanelCard>
      </div>

      <div className="workspace-grid workspace-grid--two-columns">
        <PanelCard title={t("capabilitiesTitle")} subtitle={t("capabilitiesSubtitle")}>
          <StandardDataTable
            columns={capabilityColumns}
            items={reportDesignerCapabilities}
            rowKey={(item) => item.key}
            emptyTitle={t("emptyCapabilitiesTitle")}
            emptyDescription={t("emptyCapabilitiesDescription")}
          />
        </PanelCard>

        <PanelCard title={t("registryActionsTitle")} subtitle={t("registryActionsSubtitle")}>
          <div className="report-storage-notice">
            <strong>{selectedRegistryTemplateId ? t("selectedRegistryRecord") : t("selectedStarterRecord")}</strong>
            <span>
              {selectedRegistryTemplateId && detailQuery.data
                ? `${detailQuery.data.code} / v${detailQuery.data.currentVersionNumber} / ${detailQuery.data.status}`
                : `${selectedTemplate.code} / ${selectedTemplate.name}`}
            </span>
            {selectedRegistryTemplateId && detailQuery.data ? (
              <span>
                {detailQuery.data.publishedVersionNumber
                  ? t("publishedVersionLabel", { version: detailQuery.data.publishedVersionNumber })
                  : t("notPublishedYet")}
              </span>
            ) : (
              <span>{t("starterDraftHint")}</span>
            )}
            <div className="pdfme-designer-panel__toolbar">
              <button
                type="button"
                className="secondary-button"
                onClick={() => void createMutation.mutateAsync()}
                disabled={createMutation.isPending}
              >
                {t("saveAsNewDraft")}
              </button>
              <button
                type="button"
                className="secondary-button"
                onClick={() => void updateMutation.mutateAsync()}
                disabled={selectedRegistryTemplateId === null || updateMutation.isPending}
              >
                {t("saveNewVersion")}
              </button>
              <button
                type="button"
                className="secondary-button"
                onClick={() => void publishMutation.mutateAsync()}
                disabled={selectedRegistryTemplateId === null || publishMutation.isPending}
              >
                {t("publishRecord")}
              </button>
              <button
                type="button"
                className="danger-button"
                onClick={() => void archiveMutation.mutateAsync()}
                disabled={selectedRegistryTemplateId === null || archiveMutation.isPending}
              >
                {t("archiveRecord")}
              </button>
            </div>
            {actionError ? (
              <div className="form-feedback form-feedback--error">{actionError.detail ?? actionError.title}</div>
            ) : null}
          </div>
        </PanelCard>
      </div>

      <div className="workspace-grid workspace-grid--two-columns">
        <PanelCard
          title={`${t("designerTitle")} / ${selectedRegistryTemplateId && detailQuery.data ? detailQuery.data.code : selectedTemplate.code}`}
          subtitle={
            selectedRegistryTemplateId && detailQuery.data
              ? `${detailQuery.data.name} - ${detailQuery.data.description}`
              : `${selectedTemplate.name} - ${selectedTemplate.description}`
          }
        >
          <PdfmeDesignerPanel
            template={workingTemplate}
            sampleInput={designerSampleInput}
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

      <PanelCard title={t("versionsTitle")} subtitle={t("versionsSubtitle")}>
        {selectedRegistryTemplateId && detailQuery.data ? (
          <div className="report-version-list">
            {versionItems.map((version) => (
              <div key={version.id} className="report-version-list__item">
                <div>
                  <strong>{t("versionLabel", { version: version.versionNumber })}</strong>
                  <span>{version.notes}</span>
                </div>
                <div className="report-version-list__meta">
                  <span>{version.isPublished ? t("versionPublished") : t("versionDraft")}</span>
                  <span>{new Date(version.createdAt).toLocaleString()}</span>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="standard-table__empty">
            <strong>{t("noVersionsTitle")}</strong>
            <span>{t("noVersionsDescription")}</span>
          </div>
        )}
      </PanelCard>
    </div>
  );
}
