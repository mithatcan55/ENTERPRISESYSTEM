import { httpClient } from "../../core/api/httpClient";

export type ReportTemplateListItemApi = {
  id: number;
  code: string;
  name: string;
  description: string;
  moduleKey: string;
  type: string;
  status: string;
  currentVersionNumber: number;
  publishedVersionNumber: number | null;
  updatedAt: string;
};

export type ReportTemplateVersionApi = {
  id: number;
  versionNumber: number;
  isPublished: boolean;
  publishedAt: string | null;
  createdAt: string;
  notes: string;
};

export type ReportTemplateDetailApi = {
  id: number;
  code: string;
  name: string;
  description: string;
  moduleKey: string;
  type: string;
  status: string;
  currentVersionNumber: number;
  publishedVersionNumber: number | null;
  templateJson: string;
  sampleInputJson: string;
  versions: Array<ReportTemplateVersionApi>;
};

export type ReportsPagedResult<T> = {
  items: Array<T>;
  page: number;
  pageSize: number;
  totalCount: number;
};

export type CreateReportTemplatePayload = {
  code: string;
  name: string;
  description: string;
  moduleKey: string;
  type: string;
  templateJson: string;
  sampleInputJson: string;
  notes: string;
};

export type UpdateReportTemplatePayload = {
  name: string;
  description: string;
  moduleKey: string;
  type: string;
  templateJson: string;
  sampleInputJson: string;
  notes: string;
};

export async function listReportTemplates(signal?: AbortSignal) {
  return httpClient.get<ReportsPagedResult<ReportTemplateListItemApi>>("/api/reports/templates", signal);
}

export async function getReportTemplate(reportTemplateId: number, signal?: AbortSignal) {
  return httpClient.get<ReportTemplateDetailApi>(`/api/reports/templates/${reportTemplateId}`, signal);
}

export async function createReportTemplate(payload: CreateReportTemplatePayload, signal?: AbortSignal) {
  return httpClient.post<ReportTemplateDetailApi>("/api/reports/templates", payload, signal);
}

export async function updateReportTemplate(
  reportTemplateId: number,
  payload: UpdateReportTemplatePayload,
  signal?: AbortSignal
) {
  return httpClient.put<ReportTemplateDetailApi>(`/api/reports/templates/${reportTemplateId}`, payload, signal);
}

export async function publishReportTemplate(reportTemplateId: number, signal?: AbortSignal) {
  return httpClient.post<ReportTemplateDetailApi>(`/api/reports/templates/${reportTemplateId}/publish`, undefined, signal);
}

export async function archiveReportTemplate(reportTemplateId: number, signal?: AbortSignal) {
  return httpClient.post<ReportTemplateDetailApi>(`/api/reports/templates/${reportTemplateId}/archive`, undefined, signal);
}
