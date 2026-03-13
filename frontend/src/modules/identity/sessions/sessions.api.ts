import { httpClient } from "../../../core/api/httpClient";

export type SessionListItem = {
  id: number;
  userId: number;
  sessionKey: string;
  startedAt: string;
  expiresAt: string;
  lastSeenAt: string | null;
  isRevoked: boolean;
  revokedAt: string | null;
  revokedBy: string | null;
  clientIpAddress: string | null;
  userAgent: string | null;
};

export type RevokeSessionPayload = {
  reason?: string;
};

export async function listSessions(userId?: number, onlyActive = true, signal?: AbortSignal) {
  // Query string kurulumunu burada topluyoruz ki ekran kodu sadece filtre niyetiyle ilgilensin.
  const searchParams = new URLSearchParams();

  if (userId) {
    searchParams.set("userId", String(userId));
  }

  searchParams.set("onlyActive", String(onlyActive));

  return httpClient.get<SessionListItem[]>(`/api/sessions?${searchParams.toString()}`, signal);
}

export async function revokeSession(sessionId: number, payload?: RevokeSessionPayload, signal?: AbortSignal) {
  return httpClient.post<void>(`/api/sessions/${sessionId}/revoke`, payload, signal);
}
