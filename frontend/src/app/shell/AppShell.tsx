import { Bell, Globe2, Menu, PanelLeftOpen, Search } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { NavLink, Outlet } from "react-router-dom";
import { useBrandTheme } from "../providers/BrandProvider";
import { canAccess } from "../../core/auth/access";
import { useAuth } from "../../core/auth/AuthProvider";
import { navigationGroups } from "./navigation";

export function AppShell() {
  const { t, i18n } = useTranslation(["common", "menu"]);
  const { theme } = useBrandTheme();
  const { user } = useAuth();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const visibleGroups = navigationGroups
    .map((group) => ({
      ...group,
      items: group.items.filter((item) => canAccess(user, item.access))
    }))
    .filter((group) => group.items.length > 0);

  return (
    <div className="app-shell">
      <aside className={`sidebar ${sidebarOpen ? "sidebar--open" : ""}`}>
        <div className="sidebar__brand">
          <div className="sidebar__logo-mark" />
          <div>
            <strong>{theme.name}</strong>
            <span>{t("common:appTitle")}</span>
          </div>
        </div>

        <nav className="sidebar__nav">
          {visibleGroups.map((group) => (
            <section key={group.key} className="sidebar__group">
              <h3>{t(group.titleKey)}</h3>
              {group.items.map((item) => (
                <NavLink key={item.key} to={item.to} className="sidebar__link">
                  {t(item.titleKey)}
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
            onClick={() => setSidebarOpen((current) => !current)}
            type="button"
          >
            <Menu size={18} />
          </button>

          <div className="topbar__search">
            <Search size={18} />
            <input placeholder={t("common:searchPlaceholder")} />
          </div>

          <div className="topbar__actions">
            <button className="topbar__icon-button" type="button" aria-label="workspace">
              <PanelLeftOpen size={18} />
            </button>

            <label className="topbar__locale">
              <Globe2 size={16} />
              <select value={i18n.language} onChange={(event) => void i18n.changeLanguage(event.target.value)}>
                <option value="tr">TR</option>
                <option value="en">EN</option>
                <option value="de">DE</option>
              </select>
            </label>

            <button className="topbar__icon-button" type="button" aria-label="notifications">
              <Bell size={18} />
            </button>

            <div className="topbar__profile">
              <span className="topbar__tenant">HM | AYGUN</span>
              <strong>{user.displayName}</strong>
            </div>
          </div>
        </header>

        <main className="content-area">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
