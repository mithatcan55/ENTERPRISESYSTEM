import type { PropsWithChildren } from "react";
import { createContext, useContext } from "react";
import type { AuthUser } from "./access";

type AuthContextValue = {
  user: AuthUser;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: PropsWithChildren) {
  const value: AuthContextValue = {
    user: {
      id: 1,
      username: "core.admin",
      displayName: "Core Platform Admin",
      roles: ["SYS_ADMIN"],
      permissions: ["USER_ACTION_VIEW", "USER_ACTION_EDIT", "USER_ACTION_ASSIGN"],
      transactionCodes: ["SYS01", "SYS02", "SYS03", "SYS04"]
    }
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used inside AuthProvider.");
  }

  return context;
}
