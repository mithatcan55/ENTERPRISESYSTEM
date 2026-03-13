import type { AccessRule } from "../../core/auth/access";

export type NavGroup = {
  key: string;
  titleKey: string;
  items: Array<{
    key: string;
    titleKey: string;
    to: string;
    access?: AccessRule;
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
      { key: "users", titleKey: "menu.users", to: "/identity/users", access: { anyTransactionCode: ["SYS03"] } },
      { key: "sessions", titleKey: "menu.sessions", to: "/identity/sessions", access: { anyRole: ["SYS_ADMIN"] } },
      {
        key: "passwordPolicy",
        titleKey: "menu.passwordPolicy",
        to: "/identity/password-policy",
        access: { anyRole: ["SYS_ADMIN"] }
      }
    ]
  },
  {
    key: "authorization",
    titleKey: "menu.authorization",
    items: [
      { key: "roles", titleKey: "menu.roles", to: "/authorization/roles", access: { anyRole: ["SYS_ADMIN"] } },
      {
        key: "permissions",
        titleKey: "menu.permissions",
        to: "/authorization/permissions/actions",
        access: { anyPermission: ["USER_ACTION_VIEW"] }
      },
      { key: "tcode", titleKey: "menu.tcode", to: "/authorization/tcode", access: { anyRole: ["SYS_ADMIN"] } }
    ]
  },
  {
    key: "operations",
    titleKey: "menu.operations",
    items: [
      { key: "audit", titleKey: "menu.audit", to: "/operations/audit", access: { anyRole: ["SYS_ADMIN"] } },
      { key: "logs", titleKey: "menu.logs", to: "/operations/logs/system", access: { anyRole: ["SYS_ADMIN"] } }
    ]
  },
  {
    key: "integrations",
    titleKey: "menu.integrations",
    items: [{ key: "outbox", titleKey: "menu.outbox", to: "/integrations/outbox", access: { anyRole: ["SYS_ADMIN"] } }]
  },
  {
    key: "reports",
    titleKey: "menu.reports",
    items: [{ key: "reports", titleKey: "menu.reports", to: "/reports" }]
  }
];
