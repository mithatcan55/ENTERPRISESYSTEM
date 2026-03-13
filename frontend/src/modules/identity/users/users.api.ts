import { httpClient } from "../../../core/api/httpClient";

export type UserListItem = {
  id: number;
  userCode: string;
  username: string;
  email: string;
  isActive: boolean;
  mustChangePassword: boolean;
  passwordExpiresAt: string | null;
  createdAt: string;
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

export async function listUsers(signal?: AbortSignal) {
  return httpClient.get<UserListItem[]>("/api/users", signal);
}

export async function createUser(payload: CreateUserPayload, signal?: AbortSignal) {
  return httpClient.post<UserListItem>("/api/users", payload, signal);
}
