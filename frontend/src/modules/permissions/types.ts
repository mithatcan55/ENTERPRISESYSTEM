export interface UserActionPermission {
  id: number;
  userId: number;
  subModulePageId: number;
  transactionCode: string;
  actionCode: string;
  isAllowed: boolean;
  createdAt: string;
  modifiedAt: string | null;
}

export interface ListPermissionsParams {
  userId: number;
  subModulePageId?: number;
  transactionCode?: string;
}

export interface UpsertPermissionPayload {
  userId: number;
  subModulePageId?: number;
  transactionCode?: string;
  actionCode: string;
  isAllowed: boolean;
}

export interface PermissionFormState {
  userId: string;
  subModulePageId: string;
  transactionCode: string;
  actionCode: string;
  isAllowed: boolean;
}
