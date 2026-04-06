export interface ActionPermission {
  actionCode: string;
  isAllowed: boolean;
}

export interface ActionPermissionsResponse {
  userId: string;
  transactionCode: string;
  actions: ActionPermission[];
}

export interface UpsertPermissionRequest {
  userId: string;
  transactionCode: string;
  actionCode: string;
  isAllowed: boolean;
}

export interface TCodeAccessResult {
  isAllowed: boolean;
  deniedAtLevel: string | null;
  deniedReason: string | null;
  actions: Record<string, boolean>;
  missingContextFields: string[];
}
