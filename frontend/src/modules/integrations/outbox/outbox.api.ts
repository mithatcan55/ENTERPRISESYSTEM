import { httpClient } from "../../../core/api/httpClient";

export type OutboxMessageListItem = {
  id: number;
  createdAt: string;
  eventType: string;
  status: string;
  attemptCount: number;
  maxAttempts: number;
  nextAttemptAt: string;
  processedAt: string | null;
  lastError: string | null;
  correlationId: string | null;
  deduplicationKey: string | null;
};

export type OutboxPagedResult<TItem> = {
  items: TItem[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type OutboxMessageQuery = {
  page?: number;
  pageSize?: number;
  status?: string;
  eventType?: string;
  search?: string;
};

export type QueueMailPayload = {
  to: string;
  subject: string;
  body: string;
};

export type QueueExcelPayload = {
  reportName: string;
  headers: string[];
  rows: string[][];
  notifyEmail?: string;
};

function toQueryString(query: OutboxMessageQuery) {
  const searchParams = new URLSearchParams();

  if (query.page) {
    searchParams.set("page", String(query.page));
  }

  if (query.pageSize) {
    searchParams.set("pageSize", String(query.pageSize));
  }

  if (query.status) {
    searchParams.set("status", query.status);
  }

  if (query.eventType) {
    searchParams.set("eventType", query.eventType);
  }

  if (query.search) {
    searchParams.set("search", query.search);
  }

  return searchParams.toString();
}

export async function listOutboxMessages(query: OutboxMessageQuery, signal?: AbortSignal) {
  return httpClient.get<OutboxPagedResult<OutboxMessageListItem>>(
    `/api/ops/outbox/messages?${toQueryString(query)}`,
    signal
  );
}

export async function queueMail(payload: QueueMailPayload, signal?: AbortSignal) {
  return httpClient.post<{ id: number; eventType: string; status: string; createdAt: string }>(
    "/api/ops/outbox/mail",
    payload,
    signal
  );
}

export async function queueExcel(payload: QueueExcelPayload, signal?: AbortSignal) {
  return httpClient.post<{ id: number; eventType: string; status: string; createdAt: string }>(
    "/api/ops/outbox/excel",
    payload,
    signal
  );
}
