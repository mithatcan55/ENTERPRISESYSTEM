export type NavGroup = {
  key: string;
  titleKey: string;
  items: Array<{
    key: string;
    titleKey: string;
    to: string;
  }>;
};

export const navigationGroups: NavGroup[] = [
  {
    key: "dashboard",
    titleKey: "menu.dashboard",
    items: [{ key: "dashboard", titleKey: "menu.dashboard", to: "/dashboard" }]
  },
  {
    key: "identity",
    titleKey: "menu.identity",
    items: [
      { key: "users", titleKey: "menu.users", to: "/identity/users" },
      { key: "sessions", titleKey: "menu.sessions", to: "/identity/sessions" },
      { key: "passwordPolicy", titleKey: "menu.passwordPolicy", to: "/identity/password-policy" }
    ]
  },
  {
    key: "authorization",
    titleKey: "menu.authorization",
    items: [
      { key: "roles", titleKey: "menu.roles", to: "/authorization/roles" },
      { key: "permissions", titleKey: "menu.permissions", to: "/authorization/permissions/actions" },
      { key: "tcode", titleKey: "menu.tcode", to: "/authorization/tcode" }
    ]
  },
  {
    key: "operations",
    titleKey: "menu.operations",
    items: [
      { key: "audit", titleKey: "menu.audit", to: "/operations/audit" },
      { key: "logs", titleKey: "menu.logs", to: "/operations/logs/system" }
    ]
  },
  {
    key: "integrations",
    titleKey: "menu.integrations",
    items: [{ key: "outbox", titleKey: "menu.outbox", to: "/integrations/outbox" }]
  },
  {
    key: "reports",
    titleKey: "menu.reports",
    items: [{ key: "reports", titleKey: "menu.reports", to: "/reports" }]
  }
];
