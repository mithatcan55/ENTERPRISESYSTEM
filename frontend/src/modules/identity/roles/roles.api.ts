import { httpClient } from "../../../core/api/httpClient";

export type RoleListItem = {
  id: number;
  code: string;
  name: string;
  description: string | null;
  isSystemRole: boolean;
  createdAt: string;
};

export type UserRoleItem = {
  roleId: number;
  roleCode: string;
  roleName: string;
  isSystemRole: boolean;
};

export async function listRoles(signal?: AbortSignal) {
  return httpClient.get<RoleListItem[]>("/api/roles", signal);
}

export async function listUserRoles(userId: number, signal?: AbortSignal) {
  return httpClient.get<UserRoleItem[]>(`/api/roles/users/${userId}`, signal);
}

export async function assignRole(roleId: number, userId: number, signal?: AbortSignal) {
  return httpClient.post<void>(`/api/roles/${roleId}/assign/${userId}`, undefined, signal);
}

export async function unassignRole(roleId: number, userId: number, signal?: AbortSignal) {
  return httpClient.delete<void>(`/api/roles/${roleId}/assign/${userId}`, signal);
}
