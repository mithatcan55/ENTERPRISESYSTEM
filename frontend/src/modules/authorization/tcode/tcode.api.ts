import { httpClient } from "../../../core/api/httpClient";

export type TCodeAccessResult = {
  transactionCode: string;
  moduleCode: string | null;
  subModuleCode: string | null;
  pageCode: string | null;
  routeLink: string | null;
  isAllowed: boolean;
  deniedAtLevel: number | null;
  deniedReason: string | null;
  requiredActionCode: string | null;
  amountVisible: boolean | null;
  actions: Record<string, boolean>;
  conditions: Array<{
    fieldName: string;
    operator: string;
    expectedValue: string;
    actualValue: string | null;
    isSatisfied: boolean;
  }>;
  missingContextFields: string[];
};

export type TCodeResolveQuery = {
  transactionCode: string;
  userId: number;
  companyId: number;
  actionCode?: string;
  amount?: number;
  denyOnUnsatisfiedConditions?: boolean;
};

// Navigasyon için: userId/companyId JWT claim'inden çözülür, manuel gerekmez.
export async function navigateByTCode(transactionCode: string, signal?: AbortSignal) {
  return httpClient.get<TCodeAccessResult>(
    `/api/tcode/${encodeURIComponent(transactionCode.trim().toUpperCase())}`,
    signal
  );
}

// UI yetki kontrolü için: denyOnUnsatisfiedConditions=false → eksik context alanı deny etmez,
// sadece sayfa/seviye erişimi ve action izinleri döner.
// Backend yetkisiz durumlarda da 403 + TCodeAccessResult body döndürür — bunu yakalarız.
export async function checkTCodeAccess(transactionCode: string, signal?: AbortSignal): Promise<TCodeAccessResult> {
  const tcode = transactionCode.trim().toUpperCase();
  const runtime = await import("../../../core/api/httpClientRuntime").then(m => m.getHttpClientRuntime());
  const apiBase = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, "") ?? (import.meta.env.DEV ? "http://localhost:5279" : "");
  const token = runtime.getAccessToken();

  const response = await fetch(
    `${apiBase}/api/tcode/${encodeURIComponent(tcode)}?denyOnUnsatisfiedConditions=false`,
    {
      headers: {
        "Content-Type": "application/json",
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
      signal,
    }
  );

  // 200 veya 403 → her ikisinde de TCodeAccessResult body geliyor
  if (response.ok || response.status === 403) {
    return (await response.json()) as TCodeAccessResult;
  }

  // 401 → isAllowed: false olarak dön, sayfayı koru
  if (response.status === 401) {
    return { transactionCode: tcode, isAllowed: false, actions: {}, conditions: [], missingContextFields: [] } as unknown as TCodeAccessResult;
  }

  throw { status: response.status, title: "T-Code access check failed." };
}

export async function resolveTCode(query: TCodeResolveQuery, signal?: AbortSignal) {
  const searchParams = new URLSearchParams();
  searchParams.set("userId", String(query.userId));
  searchParams.set("companyId", String(query.companyId));
  searchParams.set("denyOnUnsatisfiedConditions", String(query.denyOnUnsatisfiedConditions ?? true));

  if (query.actionCode) {
    searchParams.set("actionCode", query.actionCode);
  }

  if (query.amount !== undefined) {
    searchParams.set("amount", String(query.amount));
  }

  return httpClient.get<TCodeAccessResult>(`/api/tcode/${query.transactionCode}?${searchParams.toString()}`, signal);
}
