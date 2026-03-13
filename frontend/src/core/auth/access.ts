export type AccessRule = {
  anyRole?: string[];
  anyPermission?: string[];
  anyTransactionCode?: string[];
};

export type AuthUser = {
  id: number;
  userCode: string;
  username: string;
  displayName: string;
  roles: string[];
  permissions: string[];
  transactionCodes: string[];
  companyId?: number;
};

export function canAccess(user: AuthUser | null, rule?: AccessRule) {
  // Menu, route ve buton gorunurlugu ayni kural fonksiyonundan beslensin ki
  // frontend tarafinda farkli yetki yorumlari olusmasin.
  if (!user) {
    return false;
  }

  if (!rule) {
    return true;
  }

  const roleAllowed = !rule.anyRole || rule.anyRole.some((item) => user.roles.includes(item));
  const permissionAllowed =
    !rule.anyPermission || rule.anyPermission.some((item) => user.permissions.includes(item));
  const tcodeAllowed =
    !rule.anyTransactionCode || rule.anyTransactionCode.some((item) => user.transactionCodes.includes(item));

  return roleAllowed && permissionAllowed && tcodeAllowed;
}
