import apiClient from "@/api/client";
import type { ListPermissionsParams, UpsertPermissionPayload, UserActionPermission } from "./types";

export const permissionsApi = {
  list: (params: ListPermissionsParams) =>
    apiClient.get<UserActionPermission[]>("/api/permissions/actions", { params }).then((r) => r.data),

  upsert: (payload: UpsertPermissionPayload) =>
    apiClient.post<UserActionPermission>("/api/permissions/actions", payload).then((r) => r.data),

  delete: (permissionId: number) =>
    apiClient.delete(`/api/permissions/actions/${permissionId}`),
};
