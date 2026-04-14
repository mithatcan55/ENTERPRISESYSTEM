import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { AuthUser, UserRole } from "@/types/auth";
import { hasPermission } from "@/lib/permissions";
import type { AppPermission } from "@/lib/permissions";

interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  user: AuthUser | null;
  syncAuth: (accessToken: string, refreshToken: string, user: AuthUser) => void;
  setTokens: (accessToken: string, refreshToken: string) => void;
  setUser: (user: AuthUser) => void;
  markPasswordChanged: () => void;
  clear: () => void;
  hasRole: (role: UserRole) => boolean;
  hasPermission: (permission: AppPermission | string) => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      accessToken: null,
      refreshToken: null,
      user: null,
      syncAuth: (accessToken, refreshToken, user) =>
        set({ accessToken, refreshToken, user }),
      setTokens: (accessToken, refreshToken) =>
        set({ accessToken, refreshToken }),
      setUser: (user) => set({ user }),
      markPasswordChanged: () => set((state) => ({
        user: state.user
          ? { ...state.user, mustChangePassword: false }
          : state.user,
      })),
      clear: () => set({ accessToken: null, refreshToken: null, user: null }),
      hasRole: (role) => get().user?.roles?.includes(role) ?? false,
      hasPermission: (permission) => hasPermission(get().user, permission),
    }),
    { name: "auth-storage" },
  ),
);
