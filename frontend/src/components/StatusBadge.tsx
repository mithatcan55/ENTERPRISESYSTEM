type Variant = "success" | "danger" | "warning" | "muted";

const styles: Record<Variant, { bg: string; color: string; border: string }> = {
  success: { bg: "#E8F5EE", color: "#1E8A6E", border: "#C3E6D0" },
  danger:  { bg: "#FDECEA", color: "#C0392B", border: "#F5C6C2" },
  warning: { bg: "#FEF3E2", color: "#D4891A", border: "#F5D99A" },
  muted:   { bg: "#F0F4F8", color: "#7A96B0", border: "#D6E4F0" },
};

interface StatusBadgeProps { variant: Variant; children: React.ReactNode; }

export default function StatusBadge({ variant, children }: StatusBadgeProps) {
  const s = styles[variant];
  return (
    <span className="inline-flex items-center rounded-md px-2 py-0.5 text-[11px] font-medium"
      style={{ background: s.bg, color: s.color, border: `1px solid ${s.border}`, fontFamily: "'JetBrains Mono', monospace" }}>
      {children}
    </span>
  );
}
