import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import type { ColumnDef } from "@tanstack/react-table";
import {
  Activity,
  Layers,
  List,
  MonitorSmartphone,
  Power,
  ShieldCheck,
} from "lucide-react";
import apiClient from "@/api/client";
import { FilterBar } from "@/components/ui/FilterBar";
import { DataGrid } from "@/components/ui/DataGrid";
import { PageAction, PageHeader } from "@/components/ui/PageHeader";
import { StatusBadge } from "@/components/ui/StatusBadge";
import { useDebounce } from "@/hooks/use-debounce";
import { useAuthStore } from "@/store/auth-store";
import type { Session } from "@/types/session";
import { toast } from "sonner";

function getSessionStatus(session: Session): "active" | "expired" | "revoked" {
  if (session.isRevoked) {
    return "revoked";
  }

  if (new Date(session.expiresAt) <= new Date()) {
    return "expired";
  }

  return "active";
}

type SessionRow = Session & { userCode: string };
type ViewMode = "group" | "list";
type SortDir = "asc" | "desc";
type SessionRevokeScope = 1 | 2 | 3 | 4;

export default function SessionsPage() {
  const accessToken = useAuthStore((state) => state.accessToken);
  const user = useAuthStore((state) => state.user);
  const [viewMode, setViewMode] = useState<ViewMode>("group");
  const [search, setSearch] = useState("");
  const [userIdInput, setUserIdInput] = useState("");
  const [appliedUserId, setAppliedUserId] = useState<number | null>(null);
  const [activeOnly, setActiveOnly] = useState(false);
  const [selectedSessionIds, setSelectedSessionIds] = useState<number[]>([]);
  const [isRevoking, setIsRevoking] = useState(false);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [sortBy, setSortBy] = useState("startedAt");
  const [sortDir, setSortDir] = useState<SortDir>("desc");

  const debouncedSearch = useDebounce(search, 300);
  const currentSessionId = useMemo(() => extractSessionIdFromAccessToken(accessToken), [accessToken]);
  const isPrivilegedActor = user?.roles?.some((role) => role === "SYS_ADMIN" || role === "SYS_OPERATOR") ?? false;

  const sessionsQuery = useQuery({
    queryKey: ["sessions", activeOnly, appliedUserId],
    queryFn: async (): Promise<Session[]> => {
      const params: Record<string, string | number | boolean> = {
        onlyActive: activeOnly,
      };

      if (appliedUserId) {
        params.userId = appliedUserId;
      }

      const { data } = await apiClient.get<Session[]>("/api/sessions", { params });
      return data;
    },
  });

  const filtered = useMemo(() => {
    const q = debouncedSearch.trim().toLowerCase();
    const sourceRows = sessionsQuery.data ?? [];

    if (!q) {
      return sourceRows;
    }

    return sourceRows.filter((session) => {
      const haystack = [
        String(session.id),
        String(session.userId),
        session.userCode,
        session.sessionKey,
        session.clientIpAddress,
        session.userAgent,
        session.revokedBy,
      ]
        .filter(Boolean)
        .join(" ")
        .toLowerCase();

      return haystack.includes(q);
    });
  }, [sessionsQuery.data, debouncedSearch]);

  const rows: SessionRow[] = useMemo(
    () => filtered.map((session) => ({
      ...session,
      userCode: session.userCode,
    })),
    [filtered],
  );

  const grouped = useMemo(() => {
    const map = new Map<number, SessionRow[]>();

    for (const row of rows) {
      if (!map.has(row.userId)) {
        map.set(row.userId, []);
      }
      map.get(row.userId)?.push(row);
    }

    return Array.from(map.entries())
      .map(([userId, sessions]) => {
        const revokable = sessions.filter((session) => !session.isRevoked).map((session) => session.id);
        const activeSessions = sessions.filter((session) => getSessionStatus(session) === "active");
        const expiredSessions = sessions.filter((session) => getSessionStatus(session) === "expired");
        const revokedCount = sessions.filter((session) => session.isRevoked).length;
        const sourceSessions = activeSessions.length > 0 ? activeSessions : sessions;
        const referenceSession = [...sourceSessions].sort(
          (a, b) => new Date(b.lastSeenAt ?? b.startedAt).getTime() - new Date(a.lastSeenAt ?? a.startedAt).getTime(),
        )[0];

        return {
          userId,
          userCode: sessions[0]?.userCode ?? `U${userId}`,
          sessions,
          revokable,
          activeCount: activeSessions.length,
          expiredCount: expiredSessions.length,
          revokedCount,
          referenceLoginAt: referenceSession?.startedAt ?? null,
          activeOpenDuration: referenceSession && getSessionStatus(referenceSession) === "active"
            ? formatOpenDuration(referenceSession.startedAt)
            : "Aktif oturum yok",
        };
      })
      .sort((a, b) => a.userCode.localeCompare(b.userCode));
  }, [rows]);

  const sortedRows = useMemo(() => {
    const sorted = [...rows];

    sorted.sort((a, b) => {
      const aVal = sortValue(a, sortBy);
      const bVal = sortValue(b, sortBy);

      if (aVal < bVal) {
        return sortDir === "asc" ? -1 : 1;
      }

      if (aVal > bVal) {
        return sortDir === "asc" ? 1 : -1;
      }

      return 0;
    });

    return sorted;
  }, [rows, sortBy, sortDir]);

  const pagedRows = useMemo(() => {
    const start = (page - 1) * pageSize;
    return sortedRows.slice(start, start + pageSize);
  }, [sortedRows, page, pageSize]);

  const stats = useMemo(() => {
    const total = rows.length;
    const revoked = rows.filter((row) => row.isRevoked).length;
    const active = rows.filter((row) => getSessionStatus(row) === "active").length;
    const lastSession = [...rows].sort(
      (a, b) => new Date(b.lastSeenAt ?? b.startedAt).getTime() - new Date(a.lastSeenAt ?? a.startedAt).getTime(),
    )[0];

    return {
      total,
      revoked,
      active,
      lastDevice: lastSession?.userAgent ? compactUserAgent(lastSession.userAgent) : "-",
      lastActivity: formatDate(lastSession?.lastSeenAt ?? lastSession?.startedAt ?? null),
    };
  }, [rows]);

  async function revokeSessions(sessionIds: number[], successLabel: string, reason: string) {
    if (sessionIds.length === 0) {
      toast.error("Sonlandirilacak oturum bulunamadi");
      return;
    }

    setIsRevoking(true);

    try {
      const results = await Promise.allSettled(
        sessionIds.map((sessionId) => apiClient.post(`/api/sessions/${sessionId}/revoke`, { reason })),
      );

      const successCount = results.filter((result) => result.status === "fulfilled").length;
      const failedCount = results.length - successCount;

      if (failedCount === 0) {
        toast.success(`${successLabel} (${successCount})`);
      } else {
        toast.error(`Kismi islem: ${successCount} basarili, ${failedCount} basarisiz`);
      }

      setSelectedSessionIds((prev) => prev.filter((id) => !sessionIds.includes(id)));
      await sessionsQuery.refetch();
    } catch {
      toast.error("Session sonlandirma islemi basarisiz");
    } finally {
      setIsRevoking(false);
    }
  }

  async function revokeBulk(scope: SessionRevokeScope, successLabel: string, options?: { sessionIds?: number[]; userId?: number | null }) {
    const reason = askRevokeReason(successLabel);
    if (!reason) {
      return;
    }

    setIsRevoking(true);
    try {
      await apiClient.post("/api/sessions/revoke-bulk", {
        scope,
        sessionIds: options?.sessionIds,
        userId: options?.userId ?? null,
        reason,
      });
      toast.success(successLabel);
      setSelectedSessionIds([]);
      await sessionsQuery.refetch();
    } catch {
      toast.error("Toplu sonlandirma islemi basarisiz");
    } finally {
      setIsRevoking(false);
    }
  }

  function toggleSelected(sessionId: number) {
    setSelectedSessionIds((prev) => (prev.includes(sessionId)
      ? prev.filter((id) => id !== sessionId)
      : [...prev, sessionId]));
  }

  function handleSort(column: string) {
    if (sortBy === column) {
      setSortDir((prev) => (prev === "asc" ? "desc" : "asc"));
    } else {
      setSortBy(column);
      setSortDir("asc");
    }
    setPage(1);
  }

  const columns = useMemo<ColumnDef<SessionRow>[]>(() => [
    {
      id: "select",
      header: "",
      cell: ({ row }) => {
        const disabled = row.original.isRevoked;
        const checked = selectedSessionIds.includes(row.original.id);

        return (
          <input
            type="checkbox"
            checked={checked}
            disabled={disabled}
            onChange={() => toggleSelected(row.original.id)}
          />
        );
      },
    },
    {
      accessorKey: "userCode",
      header: "Kullanici Kodu",
      cell: ({ row }) => {
        const isCurrent = currentSessionId === row.original.id;
        return (
          <div className="flex items-center gap-1.5">
            <span style={{ fontFamily: "'JetBrains Mono', monospace" }}>{row.original.userCode}</span>
            {isCurrent && (
              <span
                className="rounded px-1.5 py-0.5 text-[10px] font-semibold"
                style={{
                  fontFamily: "'JetBrains Mono', monospace",
                  background: "color-mix(in srgb, var(--ui-primary) 15%, transparent)",
                  border: "1px solid color-mix(in srgb, var(--ui-primary) 35%, transparent)",
                  color: "var(--ui-primary)",
                }}
              >
                Current
              </span>
            )}
          </div>
        );
      },
    },
    {
      accessorKey: "userId",
      header: "Kullanici ID",
      cell: ({ row }) => (
        <span style={{ fontFamily: "'JetBrains Mono', monospace" }}>{row.original.userId}</span>
      ),
    },
    {
      accessorKey: "sessionKey",
      header: "Session Key",
      cell: ({ row }) => (
        <span className="block max-w-[220px] truncate" style={{ fontFamily: "'JetBrains Mono', monospace" }}>
          {row.original.sessionKey}
        </span>
      ),
    },
    {
      accessorKey: "clientIpAddress",
      header: "IP Adresi",
      cell: ({ row }) => (
        <span style={{ fontFamily: "'JetBrains Mono', monospace" }}>
          {row.original.clientIpAddress ?? "-"}
        </span>
      ),
    },
    {
      accessorKey: "userAgent",
      header: "Cihaz",
      cell: ({ row }) => (
        <span className="block max-w-[200px] truncate">{compactUserAgent(row.original.userAgent)}</span>
      ),
    },
    {
      id: "status",
      header: "Durum",
      cell: ({ row }) => {
        const status = getSessionStatus(row.original);
        const isCurrent = currentSessionId === row.original.id;

        if (status === "revoked") {
          return <StatusBadge status="revoked" variant="danger" />;
        }

        if (status === "expired") {
          return <StatusBadge status="inactive" variant="muted" />;
        }

        return (
          <div className="flex items-center gap-1.5">
            <StatusBadge status="active" variant="success" />
            {isCurrent && (
              <span
                className="rounded px-1.5 py-0.5 text-[10px] font-semibold"
                style={{
                  fontFamily: "'JetBrains Mono', monospace",
                  background: "color-mix(in srgb, var(--ui-primary) 15%, transparent)",
                  border: "1px solid color-mix(in srgb, var(--ui-primary) 35%, transparent)",
                  color: "var(--ui-primary)",
                }}
              >
                Bu cihaz
              </span>
            )}
          </div>
        );
      },
    },
    {
      accessorKey: "startedAt",
      header: "Giris Zamani",
      cell: ({ row }) => formatDate(row.original.startedAt),
    },
    {
      accessorKey: "lastSeenAt",
      header: "Son Aktivite",
      cell: ({ row }) => formatDate(row.original.lastSeenAt),
    },
    {
      accessorKey: "expiresAt",
      header: "Bitis",
      cell: ({ row }) => formatDate(row.original.expiresAt),
    },
    {
      id: "actions",
      header: "Islem",
      cell: ({ row }) => {
        const status = getSessionStatus(row.original);
        const disabled = status === "revoked" || isRevoking;

        return (
          <button
            type="button"
            disabled={disabled}
            onClick={() => {
              if (!window.confirm(`Session ${row.original.id} sonlandirilsin mi?`)) {
                return;
              }

              const reason = askRevokeReason(`Session ${row.original.id}`);
              if (!reason) {
                return;
              }

              void revokeSessions([row.original.id], "Oturum sonlandirildi", reason);
            }}
            className="rounded-md px-2.5 py-1 text-[11px] font-medium disabled:opacity-40"
            style={{
              fontFamily: "'JetBrains Mono', monospace",
              border: "1px solid color-mix(in srgb, var(--ui-danger) 35%, transparent)",
              background: "var(--ui-danger-bg)",
              color: "var(--ui-danger)",
            }}
          >
            Revoke
          </button>
        );
      },
    },
  ], [currentSessionId, isRevoking, revokeSessions, selectedSessionIds]);

  return (
    <div className="flex flex-col gap-4">
      <PageHeader
        title="Oturumlar"
        subtitle="SYS_ADMIN ve SYS_OPERATOR icin kullanici bazli gruplu veya detayli oturum yonetimi"
        actions={(
          <div className="flex items-center gap-2">
            <PageAction variant={viewMode === "group" ? "primary" : "ghost"} onClick={() => setViewMode("group")}>
              <Layers size={14} /> Gruplu
            </PageAction>
            <PageAction variant={viewMode === "list" ? "primary" : "ghost"} onClick={() => setViewMode("list")}>
              <List size={14} /> Liste
            </PageAction>
            <PageAction variant="ghost" onClick={() => { void sessionsQuery.refetch(); }}>
              Yenile
            </PageAction>
          </div>
        )}
      />

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
        <div className="rounded-[10px] px-4 py-3 ui-card" style={{ borderTop: "3px solid var(--ui-success)" }}>
          <div className="flex items-center justify-between">
            <span className="text-[22px] font-bold ui-text">{stats.active}</span>
            <ShieldCheck size={18} style={{ color: "var(--ui-success)", opacity: 0.7 }} />
          </div>
          <span className="text-[11px] ui-text-muted">Aktif Session</span>
        </div>

        <div className="rounded-[10px] px-4 py-3 ui-card" style={{ borderTop: "3px solid var(--ui-primary)" }}>
          <div className="flex items-center justify-between">
            <span className="text-[13px] font-semibold ui-text truncate max-w-[80%]">{stats.lastDevice}</span>
            <MonitorSmartphone size={18} style={{ color: "var(--ui-primary)", opacity: 0.7 }} />
          </div>
          <span className="text-[11px] ui-text-muted">Son Cihaz</span>
        </div>

        <div className="rounded-[10px] px-4 py-3 ui-card" style={{ borderTop: "3px solid var(--ui-warning, #C0841A)" }}>
          <div className="flex items-center justify-between">
            <span className="text-[13px] font-semibold ui-text">{stats.lastActivity}</span>
            <Activity size={18} style={{ color: "var(--ui-warning, #C0841A)", opacity: 0.7 }} />
          </div>
          <span className="text-[11px] ui-text-muted">Son Aktivite</span>
        </div>

        <div className="rounded-[10px] px-4 py-3 ui-card" style={{ borderTop: "3px solid var(--ui-danger)" }}>
          <div className="flex items-center justify-between">
            <span className="text-[22px] font-bold ui-text">{stats.total}</span>
            <Power size={18} style={{ color: "var(--ui-danger)", opacity: 0.7 }} />
          </div>
          <div className="mt-2">
            <button
              type="button"
              disabled={isRevoking}
              className="rounded-md px-2.5 py-1.5 text-[11px] font-semibold disabled:opacity-40"
              style={{
                fontFamily: "'JetBrains Mono', monospace",
                border: "1px solid color-mix(in srgb, var(--ui-danger) 35%, transparent)",
                background: "var(--ui-danger-bg)",
                color: "var(--ui-danger)",
              }}
              onClick={() => {
                if (!window.confirm("Tum session'lar sonlandirilsin mi?")) {
                  return;
                }
                void revokeBulk(3, "Tum sessionlar sonlandirildi", { userId: appliedUserId });
              }}
            >
              ALL Terminate
            </button>
          </div>
        </div>
      </div>

      <FilterBar
        search={{
          value: search,
          onChange: (value) => {
            setSearch(value);
            setPage(1);
          },
          placeholder: "Kullanıcı kodu, session key, kullanıcı ID veya IP ara...",
        }}
        filters={[
          {
            key: "status",
            label: "Durum",
            type: "select",
            value: activeOnly ? "active" : "all",
            options: [
              { value: "all", label: "Tümü" },
              { value: "active", label: "Aktif" },
            ],
            onChange: (value) => {
              setActiveOnly(String(value) === "active");
              setPage(1);
            },
          },
        ]}
        onReset={() => {
          setSearch("");
          setActiveOnly(false);
          setUserIdInput("");
          setAppliedUserId(null);
          setPage(1);
        }}
        activeCount={(search ? 1 : 0) + (activeOnly ? 1 : 0) + (appliedUserId ? 1 : 0)}
      />

      <div className="flex items-center gap-2">
        <input
          value={userIdInput}
          onChange={(e) => setUserIdInput(e.target.value.replace(/[^0-9]/g, ""))}
          placeholder="Kullanıcı ID (opsiyonel)"
          className="rounded-lg px-3 py-2 text-[13px] outline-none transition-colors"
          style={{
            background: "var(--ui-surface-alt)",
            border: "1px solid var(--ui-border)",
            color: "var(--ui-text)",
            fontFamily: "'Plus Jakarta Sans', sans-serif",
          }}
        />
        <button
          type="button"
          className="rounded-md px-2.5 py-1.5 text-[11px] font-medium"
          style={{
            fontFamily: "'JetBrains Mono', monospace",
            border: "1px solid var(--ui-border)",
            color: "var(--ui-text-muted)",
          }}
          onClick={() => {
            setAppliedUserId(userIdInput ? Number(userIdInput) : null);
            setPage(1);
          }}
        >
          Uygula
        </button>
      </div>

      {isPrivilegedActor && (
        <p className="text-[12px] ui-text-muted">
          Kullanici ID bos birakilirsa tum kullanicilarin oturumlari listelenir.
        </p>
      )}

      {sessionsQuery.isError && (
        <div
          className="rounded-[10px] px-4 py-3 flex items-center justify-between"
          style={{ background: "var(--ui-danger-bg)", border: "1px solid color-mix(in srgb, var(--ui-danger) 35%, transparent)", color: "var(--ui-danger)" }}
        >
          <span className="text-[13px]">Oturumlar yüklenemedi.</span>
          <button
            className="rounded-md px-3 py-1.5 text-[12px]"
            style={{ border: "1px solid color-mix(in srgb, var(--ui-danger) 35%, transparent)", background: "var(--ui-card-bg)", color: "var(--ui-danger)" }}
            onClick={() => { void sessionsQuery.refetch(); }}
          >
            Tekrar Dene
          </button>
        </div>
      )}

      {viewMode === "group" ? (
        <div className="grid grid-cols-1 gap-3">
          {grouped.map((group) => (
            <div
              key={group.userId}
              className="rounded-[10px] px-4 py-3 ui-card"
            >
              <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                <div className="flex flex-wrap items-center gap-2 sm:gap-2.5">
                  <p className="text-[13px] font-semibold ui-text" style={{ fontFamily: "'JetBrains Mono', monospace" }}>
                    {group.userCode}
                  </p>
                  <p className="text-[11px] rounded-md px-2 py-1 leading-5 ui-text-muted" style={{ background: "var(--ui-surface-alt)", border: "1px solid var(--ui-border)" }}>
                    User ID: {group.userId} · Toplam Session: {group.sessions.length}
                  </p>
                  <p className="text-[11px] rounded-md px-2 py-1 leading-5 ui-text-muted" style={{ background: "var(--ui-surface-alt)", border: "1px solid var(--ui-border)" }}>
                    Baz Giriş: {formatDate(group.referenceLoginAt)}
                  </p>
                  <p className="text-[11px] rounded-md px-2 py-1 leading-5 ui-text-muted" style={{ background: "var(--ui-surface-alt)", border: "1px solid var(--ui-border)" }}>
                    Açık Kalma: {group.activeOpenDuration}
                  </p>
                  <p className="text-[11px] rounded-md px-2 py-1 leading-5 ui-text-muted" style={{ background: "var(--ui-surface-alt)", border: "1px solid var(--ui-border)" }}>
                    Aktif: {group.activeCount} · Suresi Dolmus: {group.expiredCount} · Revoked: {group.revokedCount}
                  </p>
                </div>
                <button
                  type="button"
                  disabled={group.revokable.length === 0 || isRevoking}
                  onClick={() => {
                    if (!window.confirm(`${group.userCode} için ${group.revokable.length} aktif/revokable session sonlandırılsın mı?`)) {
                      return;
                    }
                    setActiveOnly(true);
                    void revokeBulk(2, `${group.userCode} icin toplu sonlandirma tamamlandi`, { sessionIds: group.revokable });
                  }}
                  className="w-full sm:w-auto rounded-md px-3 py-1.5 text-[11px] font-semibold disabled:opacity-40 whitespace-nowrap"
                  style={{
                    fontFamily: "'JetBrains Mono', monospace",
                    border: "1px solid color-mix(in srgb, var(--ui-danger) 35%, transparent)",
                    background: "linear-gradient(180deg, color-mix(in srgb, var(--ui-danger-bg) 88%, var(--ui-card-bg)) 0%, var(--ui-danger-bg) 100%)",
                    color: "var(--ui-danger)",
                    boxShadow: "0 1px 2px color-mix(in srgb, var(--ui-danger) 20%, transparent)",
                  }}
                >
                  {group.revokable.length === 0 ? "Aktif Yok" : "Tümünü Sonlandır"}
                </button>
              </div>
            </div>
          ))}

          {!sessionsQuery.isLoading && grouped.length === 0 && (
            <p className="text-[13px] ui-text-muted">
              Gruplanacak oturum bulunamadı.
            </p>
          )}
        </div>
      ) : (
        <>
          <div className="flex flex-wrap items-center gap-2">
            <button
              type="button"
              onClick={() => {
                const ids = rows.filter((session) => !session.isRevoked).map((session) => session.id);
                setSelectedSessionIds(ids);
              }}
              className="rounded-md px-2.5 py-1.5 text-[11px] font-medium"
              style={{ fontFamily: "'JetBrains Mono', monospace", border: "1px solid var(--ui-border)", color: "var(--ui-text-muted)" }}
            >
              Görüneni Seç
            </button>
            <button
              type="button"
              onClick={() => setSelectedSessionIds([])}
              className="rounded-md px-2.5 py-1.5 text-[11px] font-medium"
              style={{ fontFamily: "'JetBrains Mono', monospace", border: "1px solid var(--ui-border)", color: "var(--ui-text-muted)" }}
            >
              Seçimi Temizle
            </button>
            <button
              type="button"
              disabled={selectedSessionIds.length === 0 || isRevoking}
              onClick={() => {
                if (!window.confirm(`${selectedSessionIds.length} seçili session sonlandırılsın mı?`)) {
                  return;
                }
                setActiveOnly(true);
                void revokeBulk(2, "Secili oturumlar sonlandirildi", { sessionIds: selectedSessionIds });
              }}
              className="rounded-md px-2.5 py-1.5 text-[11px] font-medium disabled:opacity-40"
              style={{
                fontFamily: "'JetBrains Mono', monospace",
                border: "1px solid color-mix(in srgb, var(--ui-danger) 35%, transparent)",
                background: "var(--ui-danger-bg)",
                color: "var(--ui-danger)",
              }}
            >
              Seçilileri Sonlandır ({selectedSessionIds.length})
            </button>
          </div>

          <DataGrid
            columns={columns}
            data={pagedRows}
            isLoading={sessionsQuery.isLoading}
            totalCount={sortedRows.length}
            pagination={{
              page,
              pageSize,
              onPageChange: setPage,
              onPageSizeChange: (size) => {
                setPageSize(size);
                setPage(1);
              },
            }}
            sorting={{ sortBy, sortDir, onSort: handleSort }}
            emptyMessage="Oturum bulunamadı"
          />
        </>
      )}
    </div>
  );
}

function sortValue(session: SessionRow, sortBy: string) {
  switch (sortBy) {
    case "userCode":
      return session.userCode.toLowerCase();
    case "userId":
      return session.userId;
    case "sessionKey":
      return session.sessionKey.toLowerCase();
    case "clientIpAddress":
      return (session.clientIpAddress ?? "").toLowerCase();
    case "userAgent":
      return (session.userAgent ?? "").toLowerCase();
    case "startedAt":
      return session.startedAt;
    case "lastSeenAt":
      return session.lastSeenAt ?? "";
    case "expiresAt":
      return session.expiresAt;
    default:
      return session.startedAt;
  }
}

function formatDate(value?: string | null) {
  if (!value) {
    return "-";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "-";
  }

  return date.toLocaleString("tr-TR");
}

function formatOpenDuration(startedAt?: string | null) {
  if (!startedAt) {
    return "-";
  }

  const started = new Date(startedAt);
  if (Number.isNaN(started.getTime())) {
    return "-";
  }

  const elapsedMs = Date.now() - started.getTime();
  if (elapsedMs <= 0) {
    return "0dk";
  }

  const totalMinutes = Math.floor(elapsedMs / (1000 * 60));
  const days = Math.floor(totalMinutes / (60 * 24));
  const hours = Math.floor((totalMinutes % (60 * 24)) / 60);
  const minutes = totalMinutes % 60;

  if (days > 0) {
    return `${days}g ${hours}sa`;
  }

  if (hours > 0) {
    return `${hours}sa ${minutes}dk`;
  }

  return `${minutes}dk`;
}

function compactUserAgent(userAgent?: string | null) {
  if (!userAgent) {
    return "-";
  }

  const normalized = userAgent.toLowerCase();
  if (normalized.includes("edg/")) {
    return "Edge";
  }
  if (normalized.includes("opr/") || normalized.includes("opera")) {
    return "Opera";
  }
  if (normalized.includes("firefox/")) {
    return "Firefox";
  }
  if (normalized.includes("chrome/")) {
    return "Chrome";
  }
  if (normalized.includes("safari/")) {
    return "Safari";
  }
  return userAgent.slice(0, 28);
}

function askRevokeReason(target: string) {
  const reason = window.prompt(`${target} icin revoke reason giriniz (min 3):`, "admin_action");
  const normalized = reason?.trim();

  if (!normalized) {
    return null;
  }

  if (normalized.length < 3) {
    toast.error("Reason en az 3 karakter olmali");
    return null;
  }

  return normalized;
}

function extractSessionIdFromAccessToken(token: string | null) {
  if (!token) {
    return null;
  }

  const parts = token.split(".");
  if (parts.length < 2) {
    return null;
  }

  try {
    const base64 = parts[1].replace(/-/g, "+").replace(/_/g, "/");
    const padded = base64 + "=".repeat((4 - (base64.length % 4)) % 4);
    const decoded = JSON.parse(atob(padded)) as { session_id?: string | number };
    const raw = decoded.session_id;
    if (typeof raw === "number") {
      return raw;
    }
    if (typeof raw === "string" && raw.length > 0) {
      const parsed = Number(raw);
      return Number.isFinite(parsed) ? parsed : null;
    }
    return null;
  } catch {
    return null;
  }
}
