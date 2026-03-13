import type { PropsWithChildren } from "react";
import { Navigate } from "react-router-dom";
import { canAccess, type AccessRule } from "./access";
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
  const rule: AccessRule = { anyRole, anyPermission, anyTransactionCode };
  if (!canAccess(user, rule)) {
    return <Navigate to="/forbidden" replace />;
  }

  return children;
}
