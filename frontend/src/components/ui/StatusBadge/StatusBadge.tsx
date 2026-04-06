type Variant = "success" | "danger" | "warning" | "info" | "muted";
export interface StatusBadgeProps { status: string; variant?: "auto" | Variant; }

const autoMap: Record<string, Variant> = {
  active: "success", success: "success", granted: "success", published: "success", approved: "success", sent: "success",
  error: "danger", failed: "danger", denied: "danger", rejected: "danger", revoked: "danger",
  pending: "warning", warning: "warning",
  inactive: "muted", archived: "muted", deleted: "muted", muted: "muted",
  info: "info", processing: "info",
};

/** Map raw status strings to Turkish display labels */
const labelMap: Record<string, string> = {
  active: "Aktif", inactive: "Pasif", success: "Başarılı", failed: "Başarısız",
  error: "Hata", pending: "Bekliyor", sent: "Gönderildi", approved: "Onaylı",
  denied: "Reddedildi", rejected: "Reddedildi", granted: "Verildi", revoked: "İptal",
  published: "Yayında", archived: "Arşiv", deleted: "Silindi",
  warning: "Uyarı", info: "Bilgi", processing: "İşleniyor", muted: "Pasif",
};

function resolveVariant(status: string, variant?: "auto" | Variant): Variant {
  if (variant && variant !== "auto") return variant;
  return autoMap[status.toLowerCase()] ?? "muted";
}

const styles: Record<Variant, { bg: string; color: string; border: string }> = {
  success: { bg: "#E8F5EE", color: "#1E8A6E", border: "#C3E6D0" },
  danger:  { bg: "#FDECEA", color: "#C0392B", border: "#F5C6C2" },
  warning: { bg: "#FEF3E2", color: "#D4891A", border: "#F5D99A" },
  info:    { bg: "#EAF1FA", color: "#2E6DA4", border: "#BDD5EC" },
  muted:   { bg: "#F0F4F8", color: "#7A96B0", border: "#D6E4F0" },
};

export default function StatusBadge({ status, variant = "auto" }: StatusBadgeProps) {
  const s = styles[resolveVariant(status, variant)];
  const displayLabel = labelMap[status.toLowerCase()] ?? status;
  return (
    <span className="inline-flex items-center rounded-md px-2 py-0.5 text-[11px] font-medium"
      style={{ background: s.bg, color: s.color, border: `1px solid ${s.border}`, fontFamily: "'Plus Jakarta Sans', sans-serif" }}>
      {displayLabel}
    </span>
  );
}
