import { httpClient } from "../../../core/api/httpClient";

// ── Types ───────────────────────────────────────────────────────────

export type ErpRunRequest = {
  endpoint: string;
  params?: Record<string, string>;
};

export type ErpQueryResult = {
  rows: Array<Record<string, unknown>>;
  rowCount: number;
  duration: string | null;
  endpoint: string;
};

export type ErpServiceParam = {
  name: string;
  label: string | null;
  type: string;
  required: boolean;
  default: string | null;
  options: Array<{ value: string; label: string }> | null;
};

export type ErpServiceInfo = {
  endpoint: string;
  serviceName: string | null;
  parameters: ErpServiceParam[];
};

export type ErpServiceListItem = {
  endpoint: string;
  name: string;
  description: string | null;
  category: string;
  isActive: boolean;
  parameters: ErpServiceParam[];
};

export type ErpExcelExportRequest = {
  endpoint: string;
  params?: Record<string, string>;
  sheetName?: string;
};

// ── API Functions ───────────────────────────────────────────────────

export async function listErpServices(signal?: AbortSignal): Promise<ErpServiceListItem[]> {
  return httpClient.get<ErpServiceListItem[]>("/api/erp/services", signal);
}

export async function runErpQuery(request: ErpRunRequest, signal?: AbortSignal): Promise<ErpQueryResult> {
  return httpClient.post<ErpQueryResult>("/api/erp/run", request, signal);
}

export async function exportErpExcel(request: ErpExcelExportRequest): Promise<void> {
  const token = localStorage.getItem("auth_session");
  const session = token ? JSON.parse(token) : null;
  const accessToken = session?.accessToken;

  const response = await fetch(
    `${import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5279"}/api/erp/export-excel`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
      },
      body: JSON.stringify(request),
    }
  );

  if (!response.ok) throw new Error(`Excel export failed: ${response.status}`);

  const blob = await response.blob();
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = `${request.endpoint}-${new Date().toISOString().slice(0, 10)}.xlsx`;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

export async function getErpServiceParams(endpoint: string, signal?: AbortSignal): Promise<ErpServiceInfo> {
  return httpClient.get<ErpServiceInfo>(`/api/erp/params/${endpoint}`, signal);
}
