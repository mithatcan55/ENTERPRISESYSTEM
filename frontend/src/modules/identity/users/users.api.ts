import { httpClient } from "../../../core/api/httpClient";

// ── Tip tanımları (Backend DTO'ları ile birebir eşleşir) ──────────────────────

export type PagedResult<T> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
};

export type UserListItem = {
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
  deletedBy: string | null;
  profileImageUrl: string | null;
  roleCount: number;
  primaryRoleName: string | null;
};

export type UserDetail = {
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
  deletedBy: string | null;
  profileImageUrl: string | null;
};

export type UserListQuery = {
  page?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortDirection?: "asc" | "desc";
  isActive?: boolean;
  includeDeleted?: boolean;
};

export type CreateUserPayload = {
  userCode: string;
  username: string;
  email: string;
  password: string;
  companyId: number;
  notifyAdminByMail: boolean;
  adminEmail?: string;
};

export type CreatedUser = {
  id: number;
  userCode: string;
  username: string;
  email: string;
  mustChangePassword: boolean;
  passwordExpiresAt: string | null;
};

export type UpdateUserPayload = {
  username: string;
  email: string;
  isActive: boolean;
  profileImageUrl?: string | null;
  mustChangePassword: boolean;
  passwordExpiresAt?: string | null;
};

// ── API fonksiyonları ─────────────────────────────────────────────────────────

function buildQuery(params: Record<string, unknown>): string {
  const qs = Object.entries(params)
    .filter(([, v]) => v !== undefined && v !== null && v !== "")
    .map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(String(v))}`)
    .join("&");
  return qs ? `?${qs}` : "";
}

export async function listUsers(query: UserListQuery = {}, signal?: AbortSignal) {
  const qs = buildQuery(query as Record<string, unknown>);
  return httpClient.get<PagedResult<UserListItem>>(`/api/users${qs}`, signal);
}

export async function getUserById(userId: number, signal?: AbortSignal) {
  return httpClient.get<UserDetail>(`/api/users/${userId}`, signal);
}

export async function createUser(payload: CreateUserPayload, signal?: AbortSignal) {
  return httpClient.post<CreatedUser>("/api/users", payload, signal);
}

export async function updateUser(userId: number, payload: UpdateUserPayload, signal?: AbortSignal) {
  return httpClient.put<UserListItem>(`/api/users/${userId}`, payload, signal);
}

export async function deactivateUser(userId: number, signal?: AbortSignal) {
  return httpClient.post<void>(`/api/users/${userId}/deactivate`, undefined, signal);
}

export async function reactivateUser(userId: number, signal?: AbortSignal) {
  return httpClient.post<void>(`/api/users/${userId}/reactivate`, undefined, signal);
}

export async function deleteUser(userId: number, signal?: AbortSignal) {
  return httpClient.delete<void>(`/api/users/${userId}`, signal);
}

