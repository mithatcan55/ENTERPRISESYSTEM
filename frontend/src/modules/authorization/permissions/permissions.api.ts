import { httpClient } from "../../../core/api/httpClient";

export type UserActionPermissionItem = {
  id: number;
  userId: number;
  subModulePageId: number;
  transactionCode: string;
  actionCode: string;
  isAllowed: boolean;
  createdAt: string;
  modifiedAt: string | null;
};

export type UserActionPermissionQuery = {
  userId: number;
  subModulePageId?: number;
  transactionCode?: string;
};

export type UpsertPermissionPayload = {
  userId: number;
  subModulePageId?: number;
  transactionCode?: string;
  actionCode: string;
  isAllowed: boolean;
};

export async function listActionPermissions(query: UserActionPermissionQuery, signal?: AbortSignal) {
  // Permission listeleme ileride matriks gorunume gecse bile ayni query modeliyle yasayacak.
  const searchParams = new URLSearchParams();
  searchParams.set("userId", String(query.userId));

  if (query.subModulePageId) {
    searchParams.set("subModulePageId", String(query.subModulePageId));
  }

  if (query.transactionCode) {
    searchParams.set("transactionCode", query.transactionCode);
  }

  return httpClient.get<UserActionPermissionItem[]>(`/api/permissions/actions?${searchParams.toString()}`, signal);
}

export async function upsertActionPermission(payload: UpsertPermissionPayload, signal?: AbortSignal) {
  return httpClient.post<UserActionPermissionItem>("/api/permissions/actions", payload, signal);
}

export async function deleteActionPermission(permissionId: number, signal?: AbortSignal) {
  return httpClient.delete<void>(`/api/permissions/actions/${permissionId}`, signal);
}
