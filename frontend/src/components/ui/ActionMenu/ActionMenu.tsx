import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuSeparator, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { MoreHorizontal } from "lucide-react";
import type { LucideIcon } from "lucide-react";

export interface ActionItem { label: string; icon?: LucideIcon; onClick: () => void; variant?: "default" | "danger"; }
export interface ActionMenuProps { actions: ActionItem[]; }

export default function ActionMenu({ actions }: ActionMenuProps) {
  if (!actions.length) return null;
  const normal = actions.filter((a) => a.variant !== "danger");
  const danger = actions.filter((a) => a.variant === "danger");

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <button className="flex h-7 w-7 items-center justify-center rounded-full transition-colors" style={{ color: "#7A96B0" }}
          onMouseEnter={(e) => (e.currentTarget.style.background = "#F0F4F8")} onMouseLeave={(e) => (e.currentTarget.style.background = "transparent")}
          onClick={(e) => e.stopPropagation()}><MoreHorizontal className="h-4 w-4" /></button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="min-w-[160px] p-1"
        style={{ background: "#FFFFFF", border: "1px solid #E2EBF3", borderRadius: 8, boxShadow: "0 4px 16px rgba(27,58,92,0.12)" }}>
        {normal.map((a) => (
          <DropdownMenuItem key={a.label} onClick={(e) => { e.stopPropagation(); a.onClick(); }}
            className="flex items-center gap-2 rounded-md px-2.5 py-1.5 text-[13px] outline-none cursor-pointer" style={{ color: "#1B3A5C" }}
            onMouseEnter={(e) => (e.currentTarget.style.background = "#F7FAFD")} onMouseLeave={(e) => (e.currentTarget.style.background = "transparent")}>
            {a.icon && <a.icon className="h-3.5 w-3.5 shrink-0" style={{ color: "#7A96B0" }} />}{a.label}
          </DropdownMenuItem>
        ))}
        {normal.length > 0 && danger.length > 0 && <DropdownMenuSeparator className="my-1" style={{ background: "#E2EBF3" }} />}
        {danger.map((a) => (
          <DropdownMenuItem key={a.label} onClick={(e) => { e.stopPropagation(); a.onClick(); }}
            className="flex items-center gap-2 rounded-md px-2.5 py-1.5 text-[13px] outline-none cursor-pointer" style={{ color: "#C0392B" }}
            onMouseEnter={(e) => (e.currentTarget.style.background = "#FDECEA")} onMouseLeave={(e) => (e.currentTarget.style.background = "transparent")}>
            {a.icon && <a.icon className="h-3.5 w-3.5 shrink-0" />}{a.label}
          </DropdownMenuItem>
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
