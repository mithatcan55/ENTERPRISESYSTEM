import { httpClient } from "../api/httpClient";
import type { AuthUser } from "./access";
import type { AuthSession } from "./authSession";

export type LoginPayload = {
  identifier: string;
  password: string;
};

type LoginResponse = {
  userId: number;
  userCode: string;
  username: string;
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
  tokenType: string;
  mustChangePassword: boolean;
  passwordExpiresAt: string | null;
  effectiveAuthorization: {
    roles: string[];
    transactionCodes: string[];
    permissions: string[];
  };
};

type RefreshResponse = {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
  tokenType: string;
};

type JwtPayload = Record<string, string | string[] | number | undefined>;

export async function login(payload: LoginPayload, signal?: AbortSignal) {
  const response = await httpClient.post<LoginResponse>("/api/auth/login", payload, signal, false);

  return toAuthSession(response);
}

export async function refresh(refreshToken: string, currentSession: AuthSession, signal?: AbortSignal) {
  const response = await httpClient.post<RefreshResponse>(
    "/api/auth/refresh",
    { refreshToken },
    signal,
    false
  );

  return {
    ...currentSession,
    accessToken: response.accessToken,
    accessTokenExpiresAt: response.accessTokenExpiresAt,
    refreshToken: response.refreshToken,
    refreshTokenExpiresAt: response.refreshTokenExpiresAt,
    tokenType: response.tokenType,
    user: {
      ...currentSession.user,
      ...readUserFromJwt(response.accessToken, currentSession.user)
    }
  } satisfies AuthSession;
}

function toAuthSession(response: LoginResponse): AuthSession {
  const userFromToken = readUserFromJwt(response.accessToken);

  const user: AuthUser = {
    id: response.userId,
    userCode: response.userCode,
    username: response.username,
    displayName: response.username,
    roles: response.effectiveAuthorization.roles,
    permissions: response.effectiveAuthorization.permissions,
    transactionCodes: response.effectiveAuthorization.transactionCodes,
    companyId: userFromToken.companyId
  };

  return {
    accessToken: response.accessToken,
    accessTokenExpiresAt: response.accessTokenExpiresAt,
    refreshToken: response.refreshToken,
    refreshTokenExpiresAt: response.refreshTokenExpiresAt,
    tokenType: response.tokenType,
    mustChangePassword: response.mustChangePassword,
    passwordExpiresAt: response.passwordExpiresAt,
    user
  };
}

function readUserFromJwt(token: string, fallback?: Partial<AuthUser>) {
  const payload = decodeJwtPayload(token);

  return {
    id: Number(payload.user_id ?? fallback?.id ?? 0),
    userCode: String(payload.user_code ?? fallback?.userCode ?? ""),
    username: String(payload.username ?? fallback?.username ?? ""),
    displayName: String(payload.username ?? fallback?.displayName ?? ""),
    roles: readClaimArray(payload, "role", fallback?.roles),
    permissions: readClaimArray(payload, "permission", fallback?.permissions),
    transactionCodes: fallback?.transactionCodes ?? [],
    companyId: payload.company_id ? Number(payload.company_id) : fallback?.companyId
  } satisfies AuthUser;
}

function decodeJwtPayload(token: string): JwtPayload {
  const [, payloadPart] = token.split(".");

  if (!payloadPart) {
    return {};
  }

  const normalized = payloadPart.replace(/-/g, "+").replace(/_/g, "/");
  const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, "=");
  const json = window.atob(padded);

  return JSON.parse(json) as JwtPayload;
}

function readClaimArray(payload: JwtPayload, key: string, fallback: string[] = []) {
  const value = payload[key];

  if (Array.isArray(value)) {
    return value.map(String);
  }

  if (typeof value === "string") {
    return [value];
  }

  return fallback;
}
