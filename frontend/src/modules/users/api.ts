import apiClient from "@/api/client";

/* ─── Types ─── */

export interface UserListItem {
  id: number;
  userCode: string;

  firstName: string | null;
  lastName: string | null;
  displayName: string;
  email: string;
  isActive: boolean;
  mustChangePassword: boolean;
  passwordExpiresAt: string | null;
  createdAt: string;
  createdBy: string | null;
  modifiedBy: string | null;
  modifiedAt: string | null;
  isDeleted: boolean;
  deletedAt: string | null;
  profileImageUrl: string | null;
  roleCount: number;
  primaryRoleName: string | null;
}

export interface UserRoleDto {
  roleId: number;
  roleCode: string;
  roleName: string;
}

export interface UserDirectPermissionDto {
  id: number;
  subModulePageId: number;
  transactionCode: string;
  actionCode: string;
  isAllowed: boolean;
}

export interface UserDetail {
  id: number;
  userCode: string;

  firstName: string | null;
  lastName: string | null;
  displayName: string;
  email: string;
  isActive: boolean;
  mustChangePassword: boolean;
  passwordExpiresAt: string | null;
  createdAt: string;
  createdBy: string | null;
  modifiedBy: string | null;
  modifiedAt: string | null;
  isDeleted: boolean;
  deletedAt: string | null;
  deletedBy: string | null;
  profileImageUrl: string | null;
  roles: UserRoleDto[];
  directPermissions: UserDirectPermissionDto[];
}

export interface UserListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortDirection?: "asc" | "desc";
  isActive?: boolean;
  includeDeleted?: boolean;
}

export interface CreateUserPayload {
  userCode: string;

  firstName?: string | null;
  lastName?: string | null;
  email: string;
  password: string;
  companyId: number;
  notifyAdminByMail: boolean;
  adminEmail?: string;
  roleIds?: number[];
  permissionIds?: number[];
}

export interface UpdateUserPayload {
  firstName?: string | null;
  lastName?: string | null;
  email: string;
  isActive: boolean;
  mustChangePassword: boolean;
  profileImageUrl?: string | null;
  roleIds?: number[];
  permissionIds?: number[];
}

export interface LookupItem {
  id: number;
  name: string;
}

export interface PermissionLookupItem {
  id: number;
  transactionCode: string;
  actionCode: string;
  displayName: string;
  permissionCode: string;
  storedKey: string;
  navigationCode: string;
}

export interface UserLookupsResponse {
  roles: LookupItem[];
  permissions: PermissionLookupItem[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

/* ─── API ─── */

export const usersApi = {
  list: (params: UserListParams) =>
    apiClient.get<PagedResult<UserListItem>>("/api/users", { params }).then((r) => r.data),

  getById: (id: number) =>
    apiClient.get<UserDetail>(`/api/users/${id}`).then((r) => r.data),

  create: (data: CreateUserPayload) =>
    apiClient.post("/api/users", data).then((r) => r.data),

  update: (id: number, data: UpdateUserPayload) =>
    apiClient.put(`/api/users/${id}`, data).then((r) => r.data),

  lookups: () =>
    apiClient.get<UserLookupsResponse>("/api/users/lookups").then((r) => r.data),

  deactivate: (id: number) => apiClient.post(`/api/users/${id}/deactivate`),
  reactivate: (id: number) => apiClient.post(`/api/users/${id}/reactivate`),
  delete: (id: number) => apiClient.delete(`/api/users/${id}`),
};
