import { useState, useEffect, useRef, useCallback, useMemo } from "react";
import { Link, Outlet, useLocation, useNavigate } from "react-router-dom";
import {
  Users, UserCog, MonitorDot, Shield, KeyRound,
  LayoutDashboard, FileText as LogIcon, ShieldAlert, Globe, Database,
  Inbox, Lock, GitBranch, ArrowRightLeft, ClipboardCheck, FileText,
  FileBarChart, Server, Terminal, LogOut, Menu,
  ChevronLeft, ChevronRight, Search, Bell, ChevronDown,
  User, Key, AlertTriangle, CheckCircle2, Info, XCircle,
  Maximize2, Minimize2, X, ArrowRight,
  type LucideIcon,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { useAuthStore } from "@/store/auth-store";
import { ProfileImageDisplay } from "@/components/ui/ProfileImage";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu, DropdownMenuContent, DropdownMenuItem,
  DropdownMenuSeparator, DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Sheet, SheetContent, SheetTrigger } from "@/components/ui/sheet";
import {
  Tooltip, TooltipContent, TooltipTrigger,
} from "@/components/ui/tooltip";
import type { UserRole } from "@/types/auth";

/* ═══════════════════════════════════════ */
/*  NAV DATA                               */
/* ═══════════════════════════════════════ */

interface NavItem {
  to: string; label: string; icon: LucideIcon; dot: string;
  badge?: number; adminOnly?: boolean;
}
interface NavGroup { label: string; items: NavItem[]; }

const navGroups: NavGroup[] = [
  { label: "Kimlik & Erişim", items: [
    { to: "/users", label: "Kullanıcı Yönetimi", icon: Users, dot: "#5B9BD5" },
    { to: "/roles", label: "Rol Yönetimi", icon: UserCog, dot: "#5B9BD5", adminOnly: true },
    { to: "/sessions", label: "Oturum Yönetimi", icon: MonitorDot, dot: "#5B9BD5" },
  ]},
  { label: "Yetkilendirme", items: [
    { to: "/permissions", label: "Yetki Yönetimi", icon: Shield, dot: "#D4891A", adminOnly: true },
    { to: "/tcode-test", label: "TCode Test Aracı", icon: KeyRound, dot: "#D4891A", adminOnly: true },
  ]},
  { label: "Operasyonlar", items: [
    { to: "/dashboard", label: "Audit Dashboard", icon: LayoutDashboard, dot: "#2E6DA4" },
    { to: "/logs/system", label: "Sistem Logları", icon: LogIcon, dot: "#2E6DA4" },
    { to: "/logs/security", label: "Güvenlik Olayları", icon: ShieldAlert, dot: "#E05252" },
    { to: "/logs/requests", label: "İstek Logları", icon: Globe, dot: "#2E6DA4" },
    { to: "/logs/entity-changes", label: "Varlık Değişiklikleri", icon: Database, dot: "#2E6DA4" },
    { to: "/outbox", label: "Outbox Yönetimi", icon: Inbox, dot: "#D4891A" },
    { to: "/password-policy", label: "Şifre Politikası", icon: Lock, dot: "#2E6DA4", adminOnly: true },
  ]},
  { label: "İş Süreçleri", items: [
    { to: "/approvals/workflows", label: "Onay Workflow", icon: GitBranch, dot: "#1E8A6E" },
    { to: "/approvals/pending", label: "Bekleyen Onaylar", icon: ClipboardCheck, dot: "#1E8A6E" },
    { to: "/approvals/delegations", label: "Delegasyon Yönetimi", icon: ArrowRightLeft, dot: "#1E8A6E" },
    { to: "/documents", label: "Doküman Yönetimi", icon: FileText, dot: "#1E8A6E" },
    { to: "/reports/templates", label: "Rapor Şablonları", icon: FileBarChart, dot: "#1E8A6E" },
  ]},
  { label: "ERP Entegrasyon", items: [
    { to: "/erp/services", label: "Servis Kataloğu", icon: Server, dot: "#1E8A6E" },
    { to: "/erp/runner", label: "Sorgu Çalıştırıcı", icon: Terminal, dot: "#1E8A6E" },
  ]},
];

/* ═══════════════════════════════════════ */
/*  NOTIFICATIONS DATA                     */
/* ═══════════════════════════════════════ */

type NType = "error" | "warning" | "success" | "info";
interface Notification { id: number; type: NType; title: string; body: string; time: string; unread: boolean; }

const initialNotifications: Notification[] = [
  { id: 1, type: "error", title: "Başarısız giriş denemesi", body: "core.admin kullanıcısı 3 dakika içinde 5 başarısız giriş yaptı.", time: "2 dk önce", unread: true },
  { id: 2, type: "warning", title: "Outbox kuyruğu doldu", body: "Pending durumundaki 23 mesaj 1 saatten uzun süredir işlenmiyor.", time: "14 dk önce", unread: true },
  { id: 3, type: "success", title: "Yeni kullanıcı oluşturuldu", body: "mithat.can kullanıcısı başarıyla oluşturuldu ve SYS_OPERATOR rolü atandı.", time: "1 sa önce", unread: true },
  { id: 4, type: "info", title: "Şifre politikası güncellendi", body: "Minimum şifre uzunluğu 8'den 12 karaktere çıkarıldı.", time: "3 sa önce", unread: false },
  { id: 5, type: "warning", title: "Session süresi doldu", body: "14 aktif oturum son 24 saat içinde otomatik olarak sonlandırıldı.", time: "5 sa önce", unread: false },
];

const nIcons: Record<NType, { icon: LucideIcon; bg: string; color: string }> = {
  error:   { icon: XCircle, bg: "#FDECEA", color: "#E05252" },
  warning: { icon: AlertTriangle, bg: "#FEF3E2", color: "#D4891A" },
  success: { icon: CheckCircle2, bg: "#E8F5EE", color: "#1E8A6E" },
  info:    { icon: Info, bg: "#EAF1FA", color: "#2E6DA4" },
};

/* ═══════════════════════════════════════ */
/*  HELPERS                                */
/* ═══════════════════════════════════════ */

const sans = "'Plus Jakarta Sans', sans-serif";
const mono = "'JetBrains Mono', monospace";

function useHasRole() {
  const roles = useAuthStore((s) => s.user?.roles);
  return (role: UserRole) => roles?.includes(role) ?? false;
}

function useSidebarCollapsed() {
  const [collapsed, setCollapsed] = useState(() => localStorage.getItem("sidebar-collapsed") === "true");
  useEffect(() => { localStorage.setItem("sidebar-collapsed", String(collapsed)); }, [collapsed]);
  return [collapsed, setCollapsed] as const;
}

/** Find group + label for current route (breadcrumb) */
function useBreadcrumb() {
  const { pathname } = useLocation();
  for (const g of navGroups) {
    for (const item of g.items) {
      if (pathname === item.to || pathname.startsWith(item.to + "/")) {
        return { group: g.label, page: item.label };
      }
    }
  }
  return { group: "", page: "" };
}

/* ═══════════════════════════════════════ */
/*  SIDEBAR NAV                            */
/* ═══════════════════════════════════════ */

function SidebarNavContent({ collapsed, onNavigate }: { collapsed: boolean; onNavigate?: () => void }) {
  const { pathname } = useLocation();
  const hasRole = useHasRole();
  const isAdmin = hasRole("SYS_ADMIN");

  return (
    <nav className="flex flex-col gap-4 px-2">
      {navGroups.map((group) => {
        const visible = group.items.filter((i) => !i.adminOnly || isAdmin);
        if (!visible.length) return null;
        return (
          <div key={group.label}>
            {/* Group label — hidden when collapsed */}
            {!collapsed && (
              <div className="mb-1.5 px-2 text-[9px] font-medium uppercase tracking-[0.1em]"
                style={{ color: "rgba(255,255,255,0.28)", fontFamily: sans }}>{group.label}</div>
            )}
            <div className="flex flex-col gap-0.5">
              {visible.map(({ to, label, icon: Icon, dot, badge }) => {
                const active = pathname === to || pathname.startsWith(to + "/");
                const navLink = (
                  <Link key={to} to={to} onClick={onNavigate}
                    className={cn("group flex items-center rounded-md transition-all duration-150",
                      collapsed ? "justify-center px-0 py-2" : "gap-2.5 px-2.5 py-[7px]")}
                    style={{
                      color: active ? "#FFFFFF" : "rgba(255,255,255,0.55)",
                      background: active ? "rgba(255,255,255,0.10)" : "transparent",
                      borderLeft: collapsed ? "none" : active ? `2px solid ${dot}` : "2px solid transparent",
                      fontFamily: sans, fontSize: 13, fontWeight: 500,
                    }}
                    onMouseEnter={(e) => { if (!active) { e.currentTarget.style.background = "rgba(255,255,255,0.05)"; e.currentTarget.style.color = "rgba(255,255,255,0.85)"; }}}
                    onMouseLeave={(e) => { if (!active) { e.currentTarget.style.background = "transparent"; e.currentTarget.style.color = "rgba(255,255,255,0.55)"; }}}>
                    <Icon className={cn("h-4 w-4 shrink-0", collapsed && "mx-auto")} style={{ opacity: active ? 1 : 0.6 }} />
                    {!collapsed && (
                      <>
                        <span className="flex-1 truncate">{label}</span>
                        {badge !== undefined && badge > 0 && (
                          <span className="rounded px-1.5 py-0.5 text-[10px] font-medium leading-none"
                            style={{ background: "rgba(220,50,50,0.25)", color: "#FF8080", fontFamily: mono }}>{badge}</span>
                        )}
                      </>
                    )}
                  </Link>
                );

                if (collapsed) {
                  return (
                    <Tooltip key={to} delayDuration={0}>
                      <TooltipTrigger asChild>{navLink}</TooltipTrigger>
                      <TooltipContent side="right" className="text-[12px]" style={{ fontFamily: sans }}>
                        {label}
                      </TooltipContent>
                    </Tooltip>
                  );
                }
                return navLink;
              })}
            </div>
          </div>
        );
      })}
    </nav>
  );
}

/* ═══════════════════════════════════════ */
/*  SIDEBAR USER FOOTER                    */
/* ═══════════════════════════════════════ */

function SidebarUserFooter({ collapsed }: { collapsed: boolean }) {
  const { user, clear } = useAuthStore();
  const navigate = useNavigate();

  if (collapsed) {
    return (
      <div className="flex justify-center py-3" style={{ borderTop: "1px solid rgba(255,255,255,0.07)" }}>
        <Tooltip delayDuration={0}>
          <TooltipTrigger asChild>
            <div className="cursor-pointer" onClick={() => { clear(); navigate("/login", { replace: true }); }}>
              <ProfileImageDisplay src={null} displayName={user?.displayName ?? "U"} size={32} />
            </div>
          </TooltipTrigger>
          <TooltipContent side="right">{user?.displayName ?? "Kullanıcı"}</TooltipContent>
        </Tooltip>
      </div>
    );
  }

  const roleLabel = user?.roles?.includes("SYS_ADMIN" as UserRole) ? "SYS_ADMIN" : user?.roles?.[0] ?? "user";

  return (
    <div className="flex items-center gap-2.5 px-3 py-3" style={{ borderTop: "1px solid rgba(255,255,255,0.07)" }}>
      <ProfileImageDisplay src={null} displayName={user?.displayName ?? "U"} size={32} />
      <div className="flex-1 min-w-0">
        <div className="truncate text-[12px] font-medium" style={{ color: "rgba(255,255,255,0.8)", fontFamily: sans }}>{user?.displayName ?? "Kullanıcı"}</div>
        <div className="truncate text-[10px]" style={{ color: "rgba(255,255,255,0.35)", fontFamily: mono }}>{roleLabel}</div>
      </div>
      <button onClick={() => { clear(); navigate("/login", { replace: true }); }}
        className="shrink-0 rounded p-1 transition-colors hover:bg-[rgba(255,255,255,0.07)]"
        style={{ color: "rgba(255,255,255,0.30)" }}>
        <LogOut className="h-3.5 w-3.5" />
      </button>
    </div>
  );
}

/* ═══════════════════════════════════════ */
/*  NOTIFICATION PANEL                     */
/* ═══════════════════════════════════════ */

function NotificationPanel({ notifications, onMarkAllRead }: { notifications: Notification[]; onMarkAllRead: () => void }) {
  const unreadCount = notifications.filter((n) => n.unread).length;

  return (
    <div style={{ width: 340 }}>
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-3" style={{ borderBottom: "1px solid #E2EBF3" }}>
        <span className="text-[14px] font-semibold" style={{ color: "#1B3A5C", fontFamily: sans }}>Bildirimler</span>
        {unreadCount > 0 && (
          <button onClick={onMarkAllRead} className="text-[11px] font-medium transition-colors hover:underline"
            style={{ color: "#2E6DA4", fontFamily: sans }}>Tümünü okundu işaretle</button>
        )}
      </div>

      {/* Items */}
      <div className="max-h-[350px] overflow-y-auto">
        {notifications.map((n, i) => {
          const ni = nIcons[n.type];
          const NIcon = ni.icon;
          return (
            <div key={n.id}>
              <div className="flex gap-3 px-4 py-3 transition-colors hover:bg-[#F7FAFD] cursor-pointer">
                <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full" style={{ background: ni.bg }}>
                  <NIcon className="h-4 w-4" style={{ color: ni.color }} />
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-start justify-between gap-2">
                    <span className="text-[13px] font-medium leading-tight" style={{ color: "#1B3A5C", fontFamily: sans }}>{n.title}</span>
                    <div className="flex items-center gap-1.5 shrink-0">
                      <span className="text-[11px]" style={{ color: "#B0BEC5", fontFamily: mono }}>{n.time}</span>
                      {n.unread && <span className="h-2 w-2 rounded-full" style={{ background: "#2E6DA4" }} />}
                    </div>
                  </div>
                  <p className="mt-0.5 text-[12px] leading-relaxed line-clamp-2" style={{ color: "#7A96B0", fontFamily: sans }}>{n.body}</p>
                </div>
              </div>
              {i < notifications.length - 1 && <div style={{ borderBottom: "1px solid #F0F4F8" }} />}
            </div>
          );
        })}
      </div>

      {/* Footer */}
      <div className="flex justify-center py-2.5" style={{ borderTop: "1px solid #E2EBF3" }}>
        <Link to="/notifications" className="text-[12px] font-medium transition-colors hover:underline"
          style={{ color: "#2E6DA4", fontFamily: sans }}>Tüm bildirimleri gör</Link>
      </div>
    </div>
  );
}

/* ═══════════════════════════════════════ */
/*  SEARCH OVERLAY                         */
/* ═══════════════════════════════════════ */

const iconMap: Record<string, LucideIcon> = {
  Users, UserCog, MonitorDot, Shield, KeyRound, LayoutDashboard,
  FileText: LogIcon, ShieldAlert, Globe, Database, Inbox, Lock,
  GitBranch, ClipboardCheck, ArrowRightLeft, FileBarChart, Server, Terminal,
};

const searchData = [
  { label: "Kullanıcı Yönetimi", sub: "Sistem kullanıcıları", route: "/users", icon: "Users", color: "#2E6DA4", bg: "#EAF1FA" },
  { label: "Rol Yönetimi", sub: "Sistem rol tanımları", route: "/roles", icon: "UserCog", color: "#2E6DA4", bg: "#EAF1FA" },
  { label: "Oturum Yönetimi", sub: "Aktif oturumlar", route: "/sessions", icon: "MonitorDot", color: "#2E6DA4", bg: "#EAF1FA" },
  { label: "Yetki Yönetimi", sub: "Action izinleri", route: "/permissions", icon: "Shield", color: "#D4891A", bg: "#FEF3E2" },
  { label: "TCode Test Aracı", sub: "6 seviyeli erişim kontrolü", route: "/tcode-test", icon: "KeyRound", color: "#D4891A", bg: "#FEF3E2" },
  { label: "Audit Dashboard", sub: "Güvenlik KPI özeti", route: "/dashboard", icon: "LayoutDashboard", color: "#2E6DA4", bg: "#EAF1FA" },
  { label: "Sistem Logları", sub: "Uygulama logları", route: "/logs/system", icon: "FileText", color: "#2E6DA4", bg: "#EAF1FA" },
  { label: "Güvenlik Olayları", sub: "Güvenlik olay kayıtları", route: "/logs/security", icon: "ShieldAlert", color: "#E05252", bg: "#FDECEA" },
  { label: "İstek Logları", sub: "HTTP istek kayıtları", route: "/logs/requests", icon: "Globe", color: "#2E6DA4", bg: "#EAF1FA" },
  { label: "Varlık Değişiklikleri", sub: "Entity değişiklik geçmişi", route: "/logs/entity-changes", icon: "Database", color: "#2E6DA4", bg: "#EAF1FA" },
  { label: "Outbox Yönetimi", sub: "Mail ve rapor kuyruğu", route: "/outbox", icon: "Inbox", color: "#D4891A", bg: "#FEF3E2" },
  { label: "Şifre Politikası", sub: "Parola kuralları", route: "/password-policy", icon: "Lock", color: "#2E6DA4", bg: "#EAF1FA" },
  { label: "Onay Workflow", sub: "İş akışı şablonları", route: "/approvals/workflows", icon: "GitBranch", color: "#1E8A6E", bg: "#E8F5EE" },
  { label: "Bekleyen Onaylar", sub: "Onay bekleyen süreçler", route: "/approvals/pending", icon: "ClipboardCheck", color: "#1E8A6E", bg: "#E8F5EE" },
  { label: "Delegasyon Yönetimi", sub: "Onay yetki devri", route: "/approvals/delegations", icon: "ArrowRightLeft", color: "#1E8A6E", bg: "#E8F5EE" },
  { label: "Doküman Yönetimi", sub: "Sistem dokümanları", route: "/documents", icon: "FileText", color: "#1E8A6E", bg: "#E8F5EE" },
  { label: "Rapor Şablonları", sub: "Rapor oluşturma", route: "/reports/templates", icon: "FileBarChart", color: "#1E8A6E", bg: "#E8F5EE" },
  { label: "Servis Kataloğu", sub: "ERP servis tanımları", route: "/erp/services", icon: "Server", color: "#1E8A6E", bg: "#E8F5EE" },
  { label: "Sorgu Çalıştırıcı", sub: "ERP sorgu aracı", route: "/erp/runner", icon: "Terminal", color: "#1E8A6E", bg: "#E8F5EE" },
];

const quickItems = [
  { icon: "Users", bg: "#EAF1FA", color: "#2E6DA4", label: "Kullanıcı Yönetimi", sub: "/users", route: "/users" },
  { icon: "UserCog", bg: "#EAF1FA", color: "#2E6DA4", label: "Rol Yönetimi", sub: "/roles", route: "/roles" },
  { icon: "LayoutDashboard", bg: "#EAF1FA", color: "#2E6DA4", label: "Audit Dashboard", sub: "/dashboard", route: "/dashboard" },
  { icon: "ShieldAlert", bg: "#FDECEA", color: "#E05252", label: "Güvenlik Olayları", sub: "/logs/security", route: "/logs/security" },
  { icon: "Inbox", bg: "#FEF3E2", color: "#D4891A", label: "Outbox Yönetimi", sub: "/outbox", route: "/outbox" },
  { icon: "Terminal", bg: "#E8F5EE", color: "#1E8A6E", label: "Sorgu Çalıştırıcı", sub: "/erp/runner", route: "/erp/runner" },
];

const kbdCls: React.CSSProperties = {
  background: "#FFFFFF", border: "1px solid #E2EBF3", borderRadius: 4,
  padding: "1px 5px", fontSize: 11, color: "#7A96B0", fontFamily: mono, marginRight: 3, display: "inline-block",
};

function SearchOverlay({ open, onClose }: { open: boolean; onClose: () => void }) {
  const navigate = useNavigate();
  const inputRef = useRef<HTMLInputElement>(null);
  const [query, setQuery] = useState("");
  const [selectedIdx, setSelectedIdx] = useState(0);

  const results = useMemo(() => {
    if (!query.trim()) return [];
    const q = query.toLowerCase();
    return searchData.filter(
      (d) => d.label.toLowerCase().includes(q) || d.sub.toLowerCase().includes(q) || d.route.toLowerCase().includes(q),
    );
  }, [query]);

  const items = query.trim() ? results : quickItems.map((q) => ({ ...q, sub: q.sub }));
  const itemCount = items.length;

  // Reset on open/close
  useEffect(() => {
    if (open) { setQuery(""); setSelectedIdx(0); setTimeout(() => inputRef.current?.focus(), 50); }
  }, [open]);

  // Clamp selectedIdx
  useEffect(() => { if (selectedIdx >= itemCount) setSelectedIdx(Math.max(0, itemCount - 1)); }, [itemCount, selectedIdx]);

  const go = useCallback((route: string) => { navigate(route); onClose(); }, [navigate, onClose]);

  // Key handler
  useEffect(() => {
    if (!open) return;
    function handleKey(e: KeyboardEvent) {
      if (e.key === "Escape") { onClose(); return; }
      if (e.key === "ArrowDown") { e.preventDefault(); setSelectedIdx((i) => Math.min(i + 1, itemCount - 1)); return; }
      if (e.key === "ArrowUp") { e.preventDefault(); setSelectedIdx((i) => Math.max(i - 1, 0)); return; }
      if (e.key === "Enter" && items[selectedIdx]) { e.preventDefault(); go(items[selectedIdx].route); }
    }
    document.addEventListener("keydown", handleKey);
    return () => document.removeEventListener("keydown", handleKey);
  }, [open, itemCount, selectedIdx, items, go, onClose]);

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center backdrop-blur-sm"
      style={{ background: "rgba(15,39,68,0.6)" }}
      onClick={(e) => { if (e.target === e.currentTarget) onClose(); }}>

      <div className="mt-[15vh] sm:mt-[20vh] overflow-hidden" onClick={(e) => e.stopPropagation()}
        style={{ width: "min(640px, 90vw)", background: "#FFFFFF", borderRadius: 16, border: "1px solid #E2EBF3", boxShadow: "0 24px 64px rgba(27,58,92,0.18)" }}>

        {/* Search input bar */}
        <div className="flex items-center gap-3 px-5 py-4" style={{ borderBottom: "1px solid #F0F4F8" }}>
          <Search className="shrink-0" size={20} style={{ color: "#5B9BD5" }} />
          <input ref={inputRef} value={query} onChange={(e) => { setQuery(e.target.value); setSelectedIdx(0); }}
            placeholder="Kullanıcı, rol, modül veya sayfa ara..."
            className="flex-1 h-10 bg-transparent border-none outline-none text-[16px]"
            style={{ color: "#1B3A5C", fontFamily: sans }} />
          {query && (
            <button onClick={() => { setQuery(""); inputRef.current?.focus(); }}
              className="p-1 rounded hover:bg-[#F0F4F8] transition-colors">
              <X size={16} style={{ color: "#B0BEC5" }} />
            </button>
          )}
          <span style={kbdCls}>ESC</span>
        </div>

        {/* Content */}
        <div className="max-h-[360px] overflow-y-auto">
          {/* Section title */}
          {!query.trim() && (
            <div className="px-5 pt-3 pb-1.5 text-[11px] uppercase tracking-[0.08em]" style={{ color: "#B0BEC5" }}>
              Hızlı Erişim
            </div>
          )}

          {/* Items */}
          {itemCount > 0 ? items.map((item, i) => {
            const Icon = iconMap[item.icon] ?? Search;
            const isSelected = i === selectedIdx;
            return (
              <div key={item.route} onClick={() => go(item.route)}
                className="flex items-center gap-3 px-5 py-2.5 cursor-pointer transition-colors"
                style={{
                  background: isSelected ? "#EAF1FA" : "transparent",
                  borderLeft: isSelected ? "2px solid #2E6DA4" : "2px solid transparent",
                }}
                onMouseEnter={() => setSelectedIdx(i)}>
                <div className="flex items-center justify-center shrink-0"
                  style={{ width: 28, height: 28, borderRadius: 8, background: item.bg }}>
                  <Icon size={14} style={{ color: item.color }} />
                </div>
                <div className="flex-1 min-w-0">
                  <div className="text-[13px] font-medium" style={{ color: "#1B3A5C" }}>{item.label}</div>
                  <div className="text-[11px]" style={{ color: "#7A96B0" }}>{item.sub}</div>
                </div>
                <ArrowRight size={12} style={{ color: "#D6E4F0" }} />
              </div>
            );
          }) : query.trim() && (
            <div className="flex flex-col items-center gap-2 py-12">
              <Search size={32} style={{ color: "#D6E4F0" }} />
              <span className="text-[14px]" style={{ color: "#7A96B0" }}>Sonuç bulunamadı</span>
              <span className="text-[12px]" style={{ color: "#B0BEC5" }}>Farklı bir anahtar kelime deneyin</span>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="flex items-center justify-between px-5 py-2.5" style={{ background: "#F7FAFD", borderTop: "1px solid #F0F4F8" }}>
          <div className="flex items-center gap-1">
            <span style={kbdCls}>↑</span><span style={kbdCls}>↓</span>
            <span className="text-[11px] mr-3" style={{ color: "#B0BEC5" }}>gezin</span>
            <span style={kbdCls}>↵</span>
            <span className="text-[11px] mr-3" style={{ color: "#B0BEC5" }}>seç</span>
            <span style={kbdCls}>ESC</span>
            <span className="text-[11px]" style={{ color: "#B0BEC5" }}>kapat</span>
          </div>
          <span className="text-[11px]" style={{ color: "#B0BEC5" }}>{searchData.length} sayfa</span>
        </div>
      </div>
    </div>
  );
}

/* ═══════════════════════════════════════ */
/*  MAIN LAYOUT                            */
/* ═══════════════════════════════════════ */

export default function Layout() {
  const { user } = useAuthStore();
  const navigate = useNavigate();
  const [collapsed, setCollapsed] = useSidebarCollapsed();
  const breadcrumb = useBreadcrumb();
  const [notifications, setNotifications] = useState(initialNotifications);
  const [isFullscreen, setIsFullscreen] = useState(false);

  useEffect(() => {
    const handler = () => setIsFullscreen(!!document.fullscreenElement);
    document.addEventListener("fullscreenchange", handler);
    return () => document.removeEventListener("fullscreenchange", handler);
  }, []);

  const toggleFullscreen = () => {
    if (!document.fullscreenElement) document.documentElement.requestFullscreen();
    else document.exitFullscreen();
  };

  const [searchOpen, setSearchOpen] = useState(false);

  // Ctrl+K / Cmd+K global shortcut
  useEffect(() => {
    function handleGlobalKey(e: KeyboardEvent) {
      if ((e.ctrlKey || e.metaKey) && e.key === "k") { e.preventDefault(); setSearchOpen(true); }
    }
    document.addEventListener("keydown", handleGlobalKey);
    return () => document.removeEventListener("keydown", handleGlobalKey);
  }, []);

  const unreadCount = notifications.filter((n) => n.unread).length;
  const markAllRead = () => setNotifications((prev) => prev.map((n) => ({ ...n, unread: false })));

  const initials = user?.displayName
    ? user.displayName.split(" ").map((n) => n[0]).join("").toUpperCase().slice(0, 2)
    : "U";
  const roleLabel = user?.roles?.includes("SYS_ADMIN" as UserRole) ? "SYS_ADMIN" : user?.roles?.[0] ?? "user";

  return (
    <div className="flex h-screen overflow-hidden" style={{ background: "#F0F4F8", fontFamily: "'Plus Jakarta Sans', ui-sans-serif, system-ui, sans-serif" }}>

      {/* ═══ Desktop Sidebar ═══ */}
      <aside className={cn("hidden md:flex shrink-0 flex-col relative transition-all duration-200 ease-in-out")}
        style={{ width: collapsed ? 56 : 220, background: "#1B3A5C", borderRight: "1px solid rgba(255,255,255,0.07)" }}>

        {/* Logo area */}
        <div className={cn("flex items-center py-4 transition-all", collapsed ? "justify-center px-2" : "px-4")}>
          {collapsed ? (
            <span className="text-[14px] font-semibold" style={{ color: "rgba(255,255,255,0.85)", fontFamily: sans }}>HM</span>
          ) : (
            <img src="/hm-aygun.png" alt="HM AYGÜN" className="brightness-0 invert" style={{ maxHeight: 26 }} />
          )}
        </div>

        <div className="mx-2 mb-2" style={{ borderBottom: "1px solid rgba(255,255,255,0.07)" }} />

        {/* Nav */}
        <div className="flex-1 overflow-y-auto">
          <SidebarNavContent collapsed={collapsed} />
        </div>

        {/* User footer */}
        <SidebarUserFooter collapsed={collapsed} />

        {/* Collapse toggle button */}
        <button onClick={() => setCollapsed(!collapsed)}
          className="absolute flex items-center justify-center rounded-full shadow-sm transition-all hover:shadow-md"
          style={{
            width: 24, height: 24, top: 20, right: -12,
            background: "#FFFFFF", border: "1px solid #E2EBF3",
          }}>
          {collapsed
            ? <ChevronRight className="h-3 w-3" style={{ color: "#7A96B0" }} />
            : <ChevronLeft className="h-3 w-3" style={{ color: "#7A96B0" }} />}
        </button>
      </aside>

      {/* ═══ Main area ═══ */}
      <div className="flex flex-1 flex-col overflow-hidden">

        {/* ═══ Header ═══ */}
        <header className="flex h-[52px] items-center justify-between px-5"
          style={{ background: "#FFFFFF", borderBottom: "1px solid #E2EBF3" }}>

          {/* Left: Mobile menu + Breadcrumb */}
          <div className="flex items-center gap-3">
            <Sheet>
              <SheetTrigger asChild>
                <Button variant="ghost" size="icon" className="md:hidden h-8 w-8"><Menu className="h-4 w-4" /></Button>
              </SheetTrigger>
              <SheetContent side="left" className="w-[220px] p-0" style={{ background: "#1B3A5C", border: "none" }}>
                <div className="flex items-center px-4 py-4">
                  <img src="/hm-aygun.png" alt="HM AYGÜN" className="brightness-0 invert" style={{ maxHeight: 26 }} />
                </div>
                <div className="mx-2 mb-2" style={{ borderBottom: "1px solid rgba(255,255,255,0.07)" }} />
                <SidebarNavContent collapsed={false} />
                <SidebarUserFooter collapsed={false} />
              </SheetContent>
            </Sheet>

            {/* Breadcrumb */}
            {breadcrumb.group && (
              <div className="hidden md:flex items-center gap-1.5">
                <span className="text-[12px]" style={{ color: "#B0BEC5", fontWeight: 400, fontFamily: sans }}>{breadcrumb.group}</span>
                <span className="text-[12px]" style={{ color: "#B0BEC5" }}>/</span>
                <span className="text-[12px]" style={{ color: "#2C4A6B", fontWeight: 500, fontFamily: sans }}>{breadcrumb.page}</span>
              </div>
            )}
          </div>

          {/* Right: Fullscreen + Search + Bell + Divider + User */}
          <div className="flex items-center gap-1.5">
            {/* Fullscreen */}
            <Tooltip delayDuration={0}>
              <TooltipTrigger asChild>
                <button onClick={toggleFullscreen}
                  className="hidden sm:flex h-[34px] w-[34px] items-center justify-center rounded-lg transition-all duration-150 hover:bg-[#F0F4F8]"
                  style={{ color: "#7A96B0" }}
                  onMouseEnter={(e) => (e.currentTarget.style.color = "#2E6DA4")}
                  onMouseLeave={(e) => (e.currentTarget.style.color = "#7A96B0")}>
                  {isFullscreen ? <Minimize2 className="h-4 w-4" /> : <Maximize2 className="h-4 w-4" />}
                </button>
              </TooltipTrigger>
              <TooltipContent>{isFullscreen ? "Küçült" : "Tam ekran"}</TooltipContent>
            </Tooltip>

            {/* Search */}
            <Tooltip delayDuration={0}>
              <TooltipTrigger asChild>
                <button onClick={() => setSearchOpen(true)}
                  className="flex h-[34px] w-[34px] items-center justify-center rounded-full transition-colors hover:bg-[#F7FAFD]">
                  <Search className="h-4 w-4" style={{ color: "#7A96B0" }} />
                </button>
              </TooltipTrigger>
              <TooltipContent>Ara (Ctrl+K)</TooltipContent>
            </Tooltip>

            {/* Notifications */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <button className="relative flex h-[34px] w-[34px] items-center justify-center rounded-full transition-colors hover:bg-[#F7FAFD]">
                  <Bell className="h-4 w-4" style={{ color: "#7A96B0" }} />
                  {unreadCount > 0 && (
                    <span className="absolute top-1 right-1.5 h-2 w-2 rounded-full" style={{ background: "#E05252" }} />
                  )}
                </button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="p-0" style={{ width: 340, background: "#FFFFFF", border: "1px solid #E2EBF3", borderRadius: 10, boxShadow: "0 8px 32px rgba(27,58,92,0.12)" }}>
                <NotificationPanel notifications={notifications} onMarkAllRead={markAllRead} />
              </DropdownMenuContent>
            </DropdownMenu>

            {/* Divider */}
            <div className="mx-1.5 h-6" style={{ width: 1, background: "#E2EBF3" }} />

            {/* User dropdown */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <button className="flex items-center gap-2 rounded-lg px-2 py-1 transition-colors hover:bg-[#F7FAFD]">
                  <div className="flex h-8 w-8 items-center justify-center rounded-full text-[11px] font-semibold"
                    style={{ background: "rgba(91,155,213,0.15)", color: "#2E6DA4" }}>{initials}</div>
                  <span className="hidden sm:inline text-[13px] font-medium" style={{ color: "#2C4A6B", fontFamily: sans }}>{user?.displayName ?? "Kullanıcı"}</span>
                  <ChevronDown className="hidden sm:block h-3.5 w-3.5" style={{ color: "#B0BEC5" }} />
                </button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="p-0" style={{ width: 220, background: "#FFFFFF", border: "1px solid #E2EBF3", borderRadius: 10, boxShadow: "0 8px 32px rgba(27,58,92,0.12)" }}>
                {/* User info header */}
                <div className="px-4 py-3">
                  <div className="flex items-center gap-3">
                    <div className="flex h-10 w-10 items-center justify-center rounded-full text-[13px] font-semibold"
                      style={{ background: "rgba(91,155,213,0.15)", color: "#2E6DA4" }}>{initials}</div>
                    <div className="min-w-0">
                      <div className="truncate text-[13px] font-semibold" style={{ color: "#1B3A5C", fontFamily: sans }}>{user?.displayName ?? "Kullanıcı"}</div>
                      <span className="inline-flex items-center rounded px-1.5 py-0.5 text-[9px] font-medium uppercase tracking-wider"
                        style={{ background: "#EAF1FA", color: "#2E6DA4", fontFamily: mono }}>{roleLabel}</span>
                    </div>
                  </div>
                  <div className="mt-1 truncate text-[11px]" style={{ color: "#B0BEC5", fontFamily: mono }}>{user?.userName ?? ""}</div>
                </div>
                <DropdownMenuSeparator style={{ background: "#E2EBF3" }} />

                <div className="py-1">
                  <DropdownMenuItem className="flex items-center gap-2.5 px-4 py-2 cursor-pointer text-[13px]"
                    style={{ color: "#2C4A6B", fontFamily: sans }}
                    onClick={() => navigate("/profile")}>
                    <User className="h-4 w-4" style={{ color: "#7A96B0" }} />Profil Ayarları
                  </DropdownMenuItem>
                  <DropdownMenuItem className="flex items-center gap-2.5 px-4 py-2 cursor-pointer text-[13px]"
                    style={{ color: "#2C4A6B", fontFamily: sans }}>
                    <Bell className="h-4 w-4" style={{ color: "#7A96B0" }} />Bildirimler
                    {unreadCount > 0 && (
                      <span className="ml-auto rounded-full px-1.5 py-0.5 text-[10px] font-semibold"
                        style={{ background: "#EAF1FA", color: "#2E6DA4", fontFamily: mono }}>{unreadCount}</span>
                    )}
                  </DropdownMenuItem>
                  <DropdownMenuItem className="flex items-center gap-2.5 px-4 py-2 cursor-pointer text-[13px]"
                    style={{ color: "#2C4A6B", fontFamily: sans }}
                    onClick={() => navigate("/change-password")}>
                    <Key className="h-4 w-4" style={{ color: "#7A96B0" }} />Şifre Değiştir
                  </DropdownMenuItem>
                </div>
                <DropdownMenuSeparator style={{ background: "#E2EBF3" }} />

                <div className="py-1">
                  <DropdownMenuItem className="flex items-center gap-2.5 px-4 py-2 cursor-pointer text-[13px]"
                    style={{ color: "#E05252", fontFamily: sans }}
                    onMouseEnter={(e) => (e.currentTarget.style.background = "#FDECEA")}
                    onMouseLeave={(e) => (e.currentTarget.style.background = "transparent")}
                    onClick={() => { useAuthStore.getState().clear(); window.location.href = "/login"; }}>
                    <LogOut className="h-4 w-4" />Çıkış Yap
                  </DropdownMenuItem>
                </div>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </header>

        {/* ═══ Page content ═══ */}
        <main className="flex-1 overflow-y-auto px-3 py-3 sm:px-5 sm:py-5" style={{ background: "#F0F4F8" }}>
          <Outlet />
        </main>
      </div>

      {/* Search overlay */}
      <SearchOverlay open={searchOpen} onClose={() => setSearchOpen(false)} />
    </div>
  );
}
