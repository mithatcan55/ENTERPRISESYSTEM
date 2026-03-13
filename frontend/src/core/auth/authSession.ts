import type { AuthUser } from "./access";

const storageKey = "enterprise-system.auth-session";

export type AuthTokens = {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
  tokenType: string;
};

export type AuthSession = AuthTokens & {
  user: AuthUser;
  mustChangePassword: boolean;
  passwordExpiresAt: string | null;
};

export function readStoredAuthSession() {
  const serialized = window.localStorage.getItem(storageKey);

  if (!serialized) {
    return null;
  }

  try {
    return JSON.parse(serialized) as AuthSession;
  } catch {
    window.localStorage.removeItem(storageKey);
    return null;
  }
}

export function writeStoredAuthSession(session: AuthSession) {
  window.localStorage.setItem(storageKey, JSON.stringify(session));
}

export function clearStoredAuthSession() {
  window.localStorage.removeItem(storageKey);
}

export function isTokenExpired(expiresAt: string) {
  return new Date(expiresAt).getTime() <= Date.now();
}
