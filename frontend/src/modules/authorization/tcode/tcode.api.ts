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
