import { httpClient } from "../../../core/api/httpClient";

export type RoleListItem = {
  id: number;
  code: string;
  name: string;
  description: string | null;
  isSystemRole: boolean;
  createdAt: string;
};

export type CreateRolePayload = {
  code: string;
  name: string;
  description?: string;
};

export async function listRoles(signal?: AbortSignal) {
  return httpClient.get<RoleListItem[]>("/api/roles", signal);
}

export async function createRole(payload: CreateRolePayload, signal?: AbortSignal) {
  return httpClient.post<RoleListItem>("/api/roles", payload, signal);
}

export async function deleteRole(roleId: number, signal?: AbortSignal) {
  return httpClient.delete<void>(`/api/roles/${roleId}`, signal);
}
