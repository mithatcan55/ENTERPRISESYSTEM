import { cn } from "@/lib/utils";
import { RefreshCw } from "lucide-react";

interface WindowOption { value: string; label: string; }
interface TopBarProps {
  title: string; subtitle?: string;
  windowOptions?: WindowOption[]; activeWindow?: string; onWindowChange?: (v: string) => void;
  onRefresh?: () => void; isRefreshing?: boolean; showLive?: boolean; children?: React.ReactNode;
}

export default function TopBar({ title, subtitle, windowOptions, activeWindow, onWindowChange, onRefresh, isRefreshing, showLive, children }: TopBarProps) {
  return (
    <div className="mb-6 flex flex-wrap items-center justify-between gap-3 pb-4" style={{ borderBottom: "1px solid #E2EBF3" }}>
      <div>
        <h1 className="text-[16px] font-semibold tracking-[-0.01em]" style={{ color: "#1B3A5C" }}>{title}</h1>
        {subtitle && <p className="mt-0.5 text-[11px]" style={{ color: "#7A96B0" }}>{subtitle}</p>}
      </div>
      <div className="flex items-center gap-2">
        {windowOptions && onWindowChange && (
          <div className="flex items-center gap-1">
            {windowOptions.map((opt) => {
              const active = opt.value === activeWindow;
              return (
                <button key={opt.value} onClick={() => onWindowChange(opt.value)}
                  className="rounded-md px-2.5 py-1 text-[11px] font-medium transition-all"
                  style={{
                    fontFamily: "'JetBrains Mono', monospace",
                    border: active ? "1px solid #5B9BD5" : "1px solid #D6E4F0",
                    background: active ? "#EAF1FA" : "transparent",
                    color: active ? "#2E6DA4" : "#7A96B0",
                  }}>{opt.label}</button>
              );
            })}
          </div>
        )}
        {onRefresh && (
          <button onClick={onRefresh} disabled={isRefreshing}
            className="flex items-center gap-1.5 rounded-md px-2.5 py-1 text-[11px] font-medium transition-all disabled:opacity-50"
            style={{ fontFamily: "'JetBrains Mono', monospace", border: "1px solid #D6E4F0", color: "#7A96B0" }}>
            <RefreshCw className={cn("h-3 w-3", isRefreshing && "animate-spin")} />Yenile
          </button>
        )}
        {showLive && (
          <div className="flex items-center gap-1.5 rounded-md px-2.5 py-1" style={{ border: "1px solid #C3E6D0", background: "#E8F5EE" }}>
            <span className="inline-block h-1.5 w-1.5 rounded-full" style={{ backgroundColor: "#1E8A6E", animation: "topbar-pulse 2s ease-in-out infinite" }} />
            <span className="text-[11px] font-medium" style={{ color: "#1E8A6E", fontFamily: "'JetBrains Mono', monospace" }}>Canlı</span>
          </div>
        )}
        {children}
      </div>
    </div>
  );
}
