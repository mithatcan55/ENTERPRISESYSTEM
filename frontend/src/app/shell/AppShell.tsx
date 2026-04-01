import {
  Activity,
  BarChart2,
  Bell,
  CheckSquare,
  ChevronLeft,
  ChevronRight,
  FileSearch,
  Globe2,
  Inbox,
  KeyRound,
  LayoutDashboard,
  Lock,
  LogOut,
  Menu,
  Monitor,
  Plug,
  ScrollText,
  Send,
  Shield,
  Terminal,
  UserCheck,
  Users,
} from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { NavLink, Outlet } from "react-router-dom";
import { useBrandTheme } from "../providers/BrandProvider";
import { canAccess } from "../../core/auth/access";
import { useAuth } from "../../core/auth/AuthProvider";
import { navigationGroups } from "./navigation";
import { TCodeNavigator } from "./TCodeNavigator";

// Icon map by nav item key
const NAV_ICONS: Record<string, React.ReactNode> = {
  dashboard:       <LayoutDashboard size={16} />,
  users:           <Users size={16} />,
  sessions:        <Monitor size={16} />,
  passwordPolicy:  <KeyRound size={16} />,
  roles:           <Shield size={16} />,
  permissions:     <Lock size={16} />,
  tcode:           <Terminal size={16} />,
  audit:           <FileSearch size={16} />,
  logs:            <ScrollText size={16} />,
  outbox:          <Send size={16} />,
  approvalWorkflows: <CheckSquare size={16} />,
  approvalInbox:   <Inbox size={16} />,
  delegations:     <UserCheck size={16} />,
  reports:         <BarChart2 size={16} />,
};

function getInitials(name?: string) {
  if (!name) return "U";
  const parts = name.trim().split(/[\s._-]+/).filter(Boolean);
  if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
  return name.slice(0, 2).toUpperCase();
}

export function AppShell() {
  const { t, i18n } = useTranslation(["common", "menu"]);
  const { theme } = useBrandTheme();
  const { user, logout } = useAuth();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);

  const visibleGroups = navigationGroups
    .map((group) => ({
      ...group,
      items: group.items.filter((item) => canAccess(user, item.access))
    }))
    .filter((group) => group.items.length > 0);

  return (
    <div className={`app-shell ${sidebarCollapsed ? "app-shell--collapsed" : ""}`}>
      <aside className={`sidebar ${sidebarOpen ? "sidebar--open" : ""}`}>
        <div className="sidebar__brand">
          <div className="sidebar__logo-mark">E</div>
          <div className="sidebar__brand-text">
            <strong>{theme.name}</strong>
            <span>{t("common:appTitle")}</span>
          </div>
          <button
            className="sidebar__collapse-btn"
            type="button"
            onClick={() => setSidebarCollapsed((c) => !c)}
            title={sidebarCollapsed ? "Genişlet" : "Daralt"}
          >
            {sidebarCollapsed ? <ChevronRight size={14} /> : <ChevronLeft size={14} />}
          </button>
        </div>

        <nav className="sidebar__nav">
          {visibleGroups.map((group) => (
            <section key={group.key} className="sidebar__group">
              <div className="sidebar__group-title">{t(group.titleKey, { ns: "menu" })}</div>
              {group.items.map((item) => (
                <NavLink
                  key={item.key}
                  to={item.to}
                  className="sidebar__link"
                  data-tooltip={sidebarCollapsed ? t(item.titleKey, { ns: "menu" }) : undefined}
                >
                  <span className="sidebar__link-icon">
                    {NAV_ICONS[item.key] ?? <LayoutDashboard size={16} />}
                  </span>
                  <span className="sidebar__link-text">
                    {t(item.titleKey, { ns: "menu" })}
                  </span>
                </NavLink>
              ))}
            </section>
          ))}
        </nav>
      </aside>

      <div className="workspace">
        <header className="topbar">
          <button
            className="topbar__icon-button topbar__mobile-toggle"
            onClick={() => setSidebarOpen((c) => !c)}
            type="button"
          >
            <Menu size={16} />
          </button>

          <TCodeNavigator />

          <div className="topbar__actions">
            <label className="topbar__locale">
              <Globe2 size={14} />
              <select
                value={i18n.language}
                onChange={(event) => void i18n.changeLanguage(event.target.value)}
              >
                <option value="tr">TR</option>
                <option value="en">EN</option>
                <option value="de">DE</option>
              </select>
            </label>

            <button className="topbar__icon-button" type="button" aria-label="notifications">
              <Bell size={15} />
            </button>

            <div className="topbar__profile">
              <div>
                <div className="topbar__tenant">HM | AYGUN</div>
                <strong>{user?.displayName ?? "-"}</strong>
              </div>
              <div className="topbar__profile-avatar">
                {getInitials(user?.displayName)}
              </div>
            </div>

            <button className="topbar__logout-button" type="button" onClick={logout}>
              <LogOut size={13} style={{ marginRight: 4 }} />
              {t("logout")}
            </button>
          </div>
        </header>

        <main className="content-area">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
