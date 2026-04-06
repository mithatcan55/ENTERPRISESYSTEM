import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { AuthUser, UserRole } from "@/types/auth";

interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  user: AuthUser | null;
  setTokens: (accessToken: string, refreshToken: string) => void;
  setUser: (user: AuthUser) => void;
  clear: () => void;
  hasRole: (role: UserRole) => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      accessToken: null,
      refreshToken: null,
      user: null,
      setTokens: (accessToken, refreshToken) =>
        set({ accessToken, refreshToken }),
      setUser: (user) => set({ user }),
      clear: () => set({ accessToken: null, refreshToken: null, user: null }),
      hasRole: (role) => get().user?.roles?.includes(role) ?? false,
    }),
    { name: "auth-storage" },
  ),
);
