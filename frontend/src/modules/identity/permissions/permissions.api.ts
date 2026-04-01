import { httpClient } from "../../../core/api/httpClient";

export type UserActionPermission = {
  id: number;
  userId: number;
  subModulePageId: number;
  transactionCode: string;
  actionCode: string;
  isAllowed: boolean;
  createdAt: string;
  modifiedAt: string | null;
};

export type UpsertPermissionPayload = {
  userId: number;
  transactionCode?: string;
  subModulePageId?: number;
  actionCode: string;
  isAllowed: boolean;
};

export async function listUserActionPermissions(
  userId: number,
  transactionCode?: string,
  signal?: AbortSignal
) {
  const params = new URLSearchParams({ userId: String(userId) });
  if (transactionCode) params.set("transactionCode", transactionCode);
  return httpClient.get<UserActionPermission[]>(`/api/permissions/actions?${params.toString()}`, signal);
}

export async function upsertUserActionPermission(
  payload: UpsertPermissionPayload,
  signal?: AbortSignal
) {
  return httpClient.post<UserActionPermission>("/api/permissions/actions", payload, signal);
}

export async function deleteUserActionPermission(permissionId: number, signal?: AbortSignal) {
  return httpClient.delete<void>(`/api/permissions/actions/${permissionId}`, signal);
}
