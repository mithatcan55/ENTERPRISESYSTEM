import type { PropsWithChildren } from "react";
import { createContext, useContext, useEffect, useMemo, useState } from "react";
import { configureHttpClientRuntime } from "../api/httpClientRuntime";
import { login, refresh, type LoginPayload } from "./auth.api";
import type { AuthUser } from "./access";
import {
  clearStoredAuthSession,
  isTokenExpired,
  readStoredAuthSession,
  writeStoredAuthSession,
  type AuthSession
} from "./authSession";

type AuthContextValue = {
  user: AuthUser | null;
  session: AuthSession | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (payload: LoginPayload) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] = useState<AuthSession | null>(() => readStoredAuthSession());
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    configureHttpClientRuntime({
      getAccessToken: () => session?.accessToken ?? null
      ,
      refreshSession: async () => {
        const currentSession = readStoredAuthSession();

        if (!currentSession || isTokenExpired(currentSession.refreshTokenExpiresAt)) {
          clearStoredAuthSession();
          setSession(null);
          return false;
        }

        try {
          const refreshedSession = await refresh(currentSession.refreshToken, currentSession);
          writeStoredAuthSession(refreshedSession);
          setSession(refreshedSession);
          return true;
        } catch {
          clearStoredAuthSession();
          setSession(null);
          return false;
        }
      }
    });
  }, [session]);

  useEffect(() => {
    let cancelled = false;

    async function bootstrapSession() {
      const storedSession = readStoredAuthSession();

      if (!storedSession) {
        if (!cancelled) {
          setSession(null);
          setIsLoading(false);
        }

        return;
      }

      if (!isTokenExpired(storedSession.accessTokenExpiresAt)) {
        if (!cancelled) {
          setSession(storedSession);
          setIsLoading(false);
        }

        return;
      }

      if (isTokenExpired(storedSession.refreshTokenExpiresAt)) {
        clearStoredAuthSession();

        if (!cancelled) {
          setSession(null);
          setIsLoading(false);
        }

        return;
      }

      try {
        const refreshedSession = await refresh(storedSession.refreshToken, storedSession);
        writeStoredAuthSession(refreshedSession);

        if (!cancelled) {
          setSession(refreshedSession);
        }
      } catch {
        clearStoredAuthSession();

        if (!cancelled) {
          setSession(null);
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    void bootstrapSession();

    return () => {
      cancelled = true;
    };
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      user: session?.user ?? null,
      session,
      isAuthenticated: !!session?.accessToken,
      isLoading,
      login: async (payload) => {
        const nextSession = await login(payload);
        writeStoredAuthSession(nextSession);
        setSession(nextSession);
      },
      logout: () => {
        clearStoredAuthSession();
        setSession(null);
      }
    }),
    [isLoading, session]
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
