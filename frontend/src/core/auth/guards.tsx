import type { PropsWithChildren } from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "./AuthProvider";

export function PermissionGuard({
  anyRole,
  anyPermission,
  anyTransactionCode,
  children
}: PropsWithChildren<{
  anyRole?: string[];
  anyPermission?: string[];
  anyTransactionCode?: string[];
}>) {
  const { user } = useAuth();

  const roleAllowed = !anyRole || anyRole.some((item) => user.roles.includes(item));
  const permissionAllowed = !anyPermission || anyPermission.some((item) => user.permissions.includes(item));
  const tcodeAllowed =
    !anyTransactionCode || anyTransactionCode.some((item) => user.transactionCodes.includes(item));

  if (!roleAllowed || !permissionAllowed || !tcodeAllowed) {
    return <Navigate to="/forbidden" replace />;
  }

  return children;
}
