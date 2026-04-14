import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { toast } from "sonner";
import apiClient from "@/api/client";
import { PageHeader } from "@/components/ui/PageHeader";
import type { TCodeNavigationItem } from "@/types/tcode";

export default function TCodeTestPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState("");
  const [codeInput, setCodeInput] = useState("");
  const normalizedCodeInput = codeInput.trim().toUpperCase();

  const searchQuery = useQuery({
    queryKey: ["tcode-navigation", search],
    queryFn: async () => {
      const { data } = await apiClient.get<TCodeNavigationItem[]>("/api/tcode/navigation", {
        params: { query: search, take: 30 },
      });
      return data;
    },
  });

  const resolveQuery = useQuery({
    queryKey: ["tcode-navigation-resolve", normalizedCodeInput],
    queryFn: async () => {
      const { data } = await apiClient.get<TCodeNavigationItem>(`/api/tcode/navigation/${normalizedCodeInput}`);
      return data;
    },
    enabled: false,
  });

  const items = useMemo(() => searchQuery.data ?? [], [searchQuery.data]);

  async function openByCode() {
    if (!normalizedCodeInput) {
      toast.error("T-Code gerekli");
      return;
    }

    try {
      const result = await resolveQuery.refetch();
      if (!result.data) {
        toast.error("T-Code bulunamadi veya erisim yok");
        return;
      }
      navigate(result.data.routeLink);
    } catch {
      toast.error("T-Code resolve islemi basarisiz");
    }
  }

  return (
    <div className="flex flex-col gap-4">
      <PageHeader title="TCode Navigation" subtitle="T-Code permission degil; sadece navigation kisayoludur" />

      <div className="rounded-[10px] p-4 ui-card flex flex-col gap-3">
        <p className="text-[12px] ui-text-muted">Ornek: USRT01 - create, USRT02 - edit</p>
        <div className="flex flex-wrap gap-2">
          <input
            value={codeInput}
            onChange={(e) => setCodeInput(e.target.value)}
            placeholder="T-Code yaz (USRT01)"
            className="rounded-lg px-3 py-2 text-[13px] outline-none min-w-[240px]"
            style={{
              background: "var(--ui-surface-alt)",
              border: "1px solid var(--ui-border)",
              color: "var(--ui-text)",
              fontFamily: "'JetBrains Mono', monospace",
            }}
          />
          <button
            type="button"
            onClick={() => { void openByCode(); }}
            className="rounded-md px-3 py-2 text-[12px] font-semibold"
            style={{
              fontFamily: "'JetBrains Mono', monospace",
              border: "1px solid var(--ui-primary)",
              color: "var(--ui-primary)",
            }}
          >
            Go
          </button>
        </div>
      </div>

      <div className="rounded-[10px] p-4 ui-card flex flex-col gap-3">
        <div className="flex flex-wrap gap-2 items-center">
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Isimle veya kodla ara (users, USRT, SYS...)"
            className="rounded-lg px-3 py-2 text-[13px] outline-none min-w-[300px]"
            style={{
              background: "var(--ui-surface-alt)",
              border: "1px solid var(--ui-border)",
              color: "var(--ui-text)",
              fontFamily: "'Plus Jakarta Sans', sans-serif",
            }}
          />
          <span className="text-[12px] ui-text-muted">{items.length} kayit</span>
        </div>

        {searchQuery.isLoading && <p className="text-[12px] ui-text-muted">Yukleniyor...</p>}

        {!searchQuery.isLoading && items.length === 0 && (
          <p className="text-[12px] ui-text-muted">Kayit yok</p>
        )}

        <div className="grid grid-cols-1 gap-2">
          {items.map((item) => (
            <div
              key={`${item.transactionCode}-${item.routeLink}`}
              className="rounded-lg p-3 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between"
              style={{ border: "1px solid var(--ui-border)", background: "var(--ui-surface-alt)" }}
            >
              <div className="flex flex-col">
                <span className="text-[12px] font-semibold ui-text" style={{ fontFamily: "'JetBrains Mono', monospace" }}>
                  {item.transactionCode}
                </span>
                <span className="text-[13px] ui-text">{item.name}</span>
                <span className="text-[11px] ui-text-muted">{item.routeLink}</span>
              </div>
              <button
                type="button"
                onClick={() => navigate(item.routeLink)}
                className="rounded-md px-3 py-1.5 text-[11px] font-semibold"
                style={{
                  fontFamily: "'JetBrains Mono', monospace",
                  border: "1px solid var(--ui-border)",
                  color: "var(--ui-text)",
                }}
              >
                Ac
              </button>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}