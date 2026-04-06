import { Loader2 } from "lucide-react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";

type CrudMode = "create" | "edit" | "delete";
export interface CrudModalProps {
  mode: CrudMode; title: string; isOpen: boolean; onClose: () => void;
  onSubmit: () => void; isLoading?: boolean; children: React.ReactNode;
}

const mono = "'JetBrains Mono', monospace";
const modeConfig: Record<CrudMode, { label: string; bg: string; border: string; color: string; hoverBg: string }> = {
  create: { label: "Oluştur", bg: "#1B3A5C", border: "#1B3A5C", color: "#FFFFFF", hoverBg: "#2E6DA4" },
  edit:   { label: "Güncelle", bg: "#1B3A5C", border: "#1B3A5C", color: "#FFFFFF", hoverBg: "#2E6DA4" },
  delete: { label: "Sil", bg: "#FDECEA", border: "#F5C6C2", color: "#C0392B", hoverBg: "#FCE0DD" },
};

export default function CrudModal({ mode, title, isOpen, onClose, onSubmit, isLoading, children }: CrudModalProps) {
  const cfg = modeConfig[mode];
  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent
        className="gap-0 overflow-hidden p-0 w-full h-full sm:h-auto sm:max-w-lg rounded-none sm:rounded-xl [&>button:last-child]:hidden"
        style={{ background: "#FFFFFF", border: "1px solid #E2EBF3" }}
      >
        {/* Header — no manual X, shadcn's DialogClose is hidden via [&>button:last-child]:hidden */}
        <DialogHeader className="flex flex-row items-center justify-between px-5 sm:px-6 py-4"
          style={{ borderBottom: "1px solid #E2EBF3" }}>
          <DialogTitle className="text-[15px] font-medium" style={{ color: "#1B3A5C" }}>{title}</DialogTitle>
          <button onClick={onClose} className="rounded-md p-1.5 transition-colors hover:bg-[#F0F4F8]"
            style={{ color: "#7A96B0" }} aria-label="Kapat">
            <svg width="14" height="14" viewBox="0 0 14 14" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round">
              <line x1="1" y1="1" x2="13" y2="13" /><line x1="13" y1="1" x2="1" y2="13" />
            </svg>
          </button>
        </DialogHeader>

        {/* Body — scrollable */}
        <div className="flex-1 overflow-y-auto px-5 sm:px-6 py-5" style={{ maxHeight: "70vh" }}>
          {children}
        </div>

        {/* Footer */}
        <div className="flex items-center justify-end gap-2 px-5 sm:px-6 py-4"
          style={{ borderTop: "1px solid #E2EBF3" }}>
          <button onClick={onClose} disabled={isLoading}
            className="rounded-md px-3.5 py-2 text-[12px] font-medium transition-all disabled:opacity-50 min-h-[36px]"
            style={{ fontFamily: mono, border: "1px solid #D6E4F0", color: "#7A96B0" }}
            onMouseEnter={(e) => (e.currentTarget.style.background = "#F7FAFD")}
            onMouseLeave={(e) => (e.currentTarget.style.background = "transparent")}>
            İptal
          </button>
          <button onClick={onSubmit} disabled={isLoading}
            className="flex items-center gap-1.5 rounded-md px-3.5 py-2 text-[12px] font-medium transition-all disabled:opacity-60 min-h-[36px]"
            style={{ fontFamily: mono, background: cfg.bg, border: `1px solid ${cfg.border}`, color: cfg.color }}
            onMouseEnter={(e) => (e.currentTarget.style.background = cfg.hoverBg)}
            onMouseLeave={(e) => (e.currentTarget.style.background = cfg.bg)}>
            {isLoading && <Loader2 className="h-3 w-3 animate-spin" />}{cfg.label}
          </button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
