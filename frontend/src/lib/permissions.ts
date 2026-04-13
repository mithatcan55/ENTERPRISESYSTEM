import type { AuthUser } from "@/types/auth";

export const AppPermission = {
  UsersListView: "Users.View",
  UsersDetailView: "Users.View",
  UsersCreate: "Users.Create",
  UsersUpdate: "Users.Update",
  UsersDelete: "Users.Delete",
  UsersDeactivate: "Users.Update",
  UsersReactivate: "Users.Update",
  UsersAssignRoles: "Roles.Update",
  UsersAssignPermissions: "Permissions.Update",
} as const;

export type AppPermission = typeof AppPermission[keyof typeof AppPermission];

const aliasMap: Record<AppPermission, string[]> = {
  "Users.View": ["Users.View", "USERS_LIST_VIEW", "USERS_DETAIL_VIEW", "SYS03:READ", "SYS04:READ"],
  "Users.Create": ["Users.Create", "USERS_CREATE", "SYS01:CREATE"],
  "Users.Update": ["Users.Update", "USERS_UPDATE", "USERS_DEACTIVATE", "USERS_REACTIVATE", "SYS01:UPDATE", "SYS01:DEACTIVATE", "SYS01:REACTIVATE"],
  "Users.Delete": ["Users.Delete", "USERS_DELETE", "SYS01:DELETE"],
  "Roles.Update": ["Roles.Update", "USERS_ASSIGN_ROLES", "SYS05:MANAGE", "SYS01:UPDATE"],
  "Permissions.Update": ["Permissions.Update", "USERS_ASSIGN_PERMISSIONS", "SYS06:MANAGE", "SYS06:PERMISSIONS_READ", "SYS01:UPDATE"],
};

function normalize(value: string) {
  return value.trim().toUpperCase();
}

export function hasPermission(user: AuthUser | null, permission: AppPermission | string) {
  if (!user) return false;
  if (user.roles.includes("SYS_ADMIN")) return true;

  const expected = normalize(permission);
  const expectedValues = (aliasMap[expected as AppPermission] ?? [expected]).map(normalize);
  const granted = new Set((user.permissions ?? []).map(normalize));

  return expectedValues.some((value) => granted.has(value));
}

export function hasAnyPermission(user: AuthUser | null, permissions: Array<AppPermission | string>) {
  return permissions.some((permission) => hasPermission(user, permission));
}

export function hasAllPermissions(user: AuthUser | null, permissions: Array<AppPermission | string>) {
  return permissions.every((permission) => hasPermission(user, permission));
}
