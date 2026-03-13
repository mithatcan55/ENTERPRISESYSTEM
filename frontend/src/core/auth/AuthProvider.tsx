import type { PropsWithChildren } from "react";
import { createContext, useContext, useMemo } from "react";

type AuthContextValue = {
  user: {
    id: number;
    username: string;
    roles: string[];
    permissions: string[];
    transactionCodes: string[];
  };
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: PropsWithChildren) {
  const value = useMemo<AuthContextValue>(
    () => ({
      user: {
        id: 1,
        username: "core.admin",
        roles: ["SYS_ADMIN"],
        permissions: ["USER_ACTION_VIEW", "USER_ACTION_EDIT"],
        transactionCodes: ["SYS01", "SYS02", "SYS03", "SYS04"]
      }
    }),
    []
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used inside AuthProvider.");
  }

  return context;
}
