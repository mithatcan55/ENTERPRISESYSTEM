import { httpClient } from "../../core/api/httpClient";

export type AuditDashboardSummary = {
  generatedAt: string;
  windowHours: number;
  systemErrorCount: number;
  failedLoginCount: number;
  sessionRevokeRatePercent: number;
  failedLoginTrend: Array<{
    hour: string;
    count: number;
  }>;
  topCriticalEvents: Array<{
    eventType: string;
    count: number;
  }>;
};

export type LogQuery = {
  page?: number;
  pageSize?: number;
  search?: string;
  correlationId?: string;
  startAt?: string;
  endAt?: string;
};

export type PagedResult<TItem> = {
  items: TItem[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type SystemLogListItem = {
  id: number;
  timestamp: string;
  level: string | null;
  category: string | null;
  source: string | null;
  message: string | null;
  correlationId: string | null;
  httpStatusCode: number | null;
  userId: string | null;
  username: string | null;
};

export type SecurityEventListItem = {
  id: number;
  timestamp: string;
  eventType: string | null;
  severity: string | null;
  userId: string | null;
  username: string | null;
  resource: string | null;
  action: string | null;
  isSuccessful: boolean;
  failureReason: string | null;
  ipAddress: string | null;
};

export type HttpRequestLogListItem = {
  id: number;
  timestamp: string;
  method: string | null;
  path: string | null;
  statusCode: number;
  durationMs: number;
  correlationId: string | null;
  isError: boolean;
  userId: string | null;
  username: string | null;
  ipAddress: string | null;
};

export type EntityChangeLogListItem = {
  id: number;
  timestamp: string;
  correlationId: string | null;
  userId: string | null;
  username: string | null;
  entityType: string | null;
  entityId: string | null;
  action: string | null;
  tableName: string | null;
  schemaName: string | null;
  changedProperties: string | null;
};

function toQueryString(query: LogQuery) {
  const searchParams = new URLSearchParams();

  if (query.page) {
    searchParams.set("page", String(query.page));
  }

  if (query.pageSize) {
    searchParams.set("pageSize", String(query.pageSize));
  }

  if (query.search) {
    searchParams.set("search", query.search);
  }

  if (query.correlationId) {
    searchParams.set("correlationId", query.correlationId);
  }

  if (query.startAt) {
    searchParams.set("startAt", query.startAt);
  }

  if (query.endAt) {
    searchParams.set("endAt", query.endAt);
  }

  return searchParams.toString();
}

export async function getAuditDashboardSummary(windowHours = 24, signal?: AbortSignal) {
  return httpClient.get<AuditDashboardSummary>(`/api/ops/audit/dashboard/summary?windowHours=${windowHours}`, signal);
}

export async function getSystemLogs(query: LogQuery, signal?: AbortSignal) {
  return httpClient.get<PagedResult<SystemLogListItem>>(`/api/ops/logs/system?${toQueryString(query)}`, signal);
}

export async function getSecurityLogs(query: LogQuery, signal?: AbortSignal) {
  return httpClient.get<PagedResult<SecurityEventListItem>>(`/api/ops/logs/security?${toQueryString(query)}`, signal);
}

export async function getHttpLogs(query: LogQuery, signal?: AbortSignal) {
  return httpClient.get<PagedResult<HttpRequestLogListItem>>(`/api/ops/logs/http?${toQueryString(query)}`, signal);
}

export async function getEntityChangeLogs(query: LogQuery, signal?: AbortSignal) {
  return httpClient.get<PagedResult<EntityChangeLogListItem>>(
    `/api/ops/logs/entity-changes?${toQueryString(query)}`,
    signal
  );
}

export async function exportEntityChangeLogsCsv(query: LogQuery): Promise<void> {
  const token = localStorage.getItem("auth_session");
  const session = token ? JSON.parse(token) : null;
  const accessToken = session?.accessToken;

  const response = await fetch(
    `${import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5279"}/api/ops/logs/entity-changes/export?${toQueryString(query)}`,
    {
      headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
    }
  );

  if (!response.ok) {
    throw new Error(`Export failed: ${response.status}`);
  }

  const blob = await response.blob();
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = "entity-change-logs.csv";
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}
