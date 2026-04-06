import apiClient from "@/api/client";

export interface UserListItem {
  id: number;
  userCode: string;
  username: string;
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

export interface UserDetail {
  id: number;
  userCode: string;
  username: string;
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
  username: string;
  email: string;
  password: string;
  companyId: number;
  notifyAdminByMail: boolean;
  adminEmail?: string;
}

export interface UpdateUserPayload {
  username: string;
  email: string;
  isActive: boolean;
  mustChangePassword: boolean;
  passwordExpiresAt?: string | null;
  profileImageUrl?: string | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const usersApi = {
  list: (params: UserListParams) =>
    apiClient.get<PagedResult<UserListItem>>("/api/users", { params }).then((r) => r.data),

  getById: (id: number) =>
    apiClient.get<UserDetail>(`/api/users/${id}`).then((r) => r.data),

  create: (data: CreateUserPayload) =>
    apiClient.post("/api/users", data).then((r) => r.data),

  update: (id: number, data: UpdateUserPayload) =>
    apiClient.put(`/api/users/${id}`, data).then((r) => r.data),

  deactivate: (id: number) => apiClient.post(`/api/users/${id}/deactivate`),

  reactivate: (id: number) => apiClient.post(`/api/users/${id}/reactivate`),

  delete: (id: number) => apiClient.delete(`/api/users/${id}`),
};
