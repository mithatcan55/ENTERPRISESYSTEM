import { cn } from "@/lib/utils";

const accentColors = { red: "#E05252", amber: "#D4891A", blue: "#2E6DA4", teal: "#1E8A6E" } as const;
type Variant = keyof typeof accentColors;
type DeltaType = "up" | "ok" | "warn" | "muted";

const deltaStyles: Record<DeltaType, { color: string; prefix: string }> = {
  up: { color: "#C0392B", prefix: "↑ " },
  ok: { color: "#1E8A6E", prefix: "✓ " },
  warn: { color: "#D4891A", prefix: "~ " },
  muted: { color: "#B0BEC5", prefix: "" },
};

interface KpiCardProps {
  variant: Variant;
  label: string;
  value: string | number;
  unit?: string;
  delta?: { type: DeltaType; text: string };
  className?: string;
}

export default function KpiCard({ variant, label, value, unit, delta, className }: KpiCardProps) {
  return (
    <div className={cn("relative overflow-hidden rounded-[10px] px-5 py-4", className)}
      style={{ background: "#FFFFFF", border: "1px solid #E2EBF3", borderTop: `3px solid ${accentColors[variant]}` }}>
      <div className="mb-2 text-[11px] font-medium tracking-[0.02em]" style={{ color: "#7A96B0" }}>{label}</div>
      <div className="flex items-baseline gap-1">
        <span className="text-[26px] font-light tracking-[-0.03em]" style={{ color: "#1B3A5C" }}>{value}</span>
        {unit && <span className="text-[14px]" style={{ color: "#7A96B0" }}>{unit}</span>}
      </div>
      {delta && (
        <div className="mt-1.5 text-[11px] font-medium" style={{ color: deltaStyles[delta.type].color, fontFamily: "'JetBrains Mono', monospace" }}>
          {deltaStyles[delta.type].prefix}{delta.text}
        </div>
      )}
    </div>
  );
}
