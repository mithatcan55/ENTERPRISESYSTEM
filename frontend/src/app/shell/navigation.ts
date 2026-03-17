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
    titleKey: "dashboard",
    items: [{ key: "dashboard", titleKey: "dashboard", to: "/dashboard" }]
  },
  {
    key: "identity",
    titleKey: "identity",
    items: [
      { key: "users", titleKey: "users", to: "/identity/users", access: { anyTransactionCode: ["SYS03"] } },
      { key: "sessions", titleKey: "sessions", to: "/identity/sessions", access: { anyRole: ["SYS_ADMIN"] } },
      {
        key: "passwordPolicy",
        titleKey: "passwordPolicy",
        to: "/identity/password-policy",
        access: { anyRole: ["SYS_ADMIN"] }
      }
    ]
  },
  {
    key: "authorization",
    titleKey: "authorization",
    items: [
      { key: "roles", titleKey: "roles", to: "/authorization/roles", access: { anyRole: ["SYS_ADMIN"] } },
      {
        key: "permissions",
        titleKey: "permissions",
        to: "/authorization/permissions/actions",
        access: { anyPermission: ["USER_ACTION_VIEW"] }
      },
      { key: "tcode", titleKey: "tcode", to: "/authorization/tcode", access: { anyRole: ["SYS_ADMIN"] } }
    ]
  },
  {
    key: "operations",
    titleKey: "operations",
    items: [
      { key: "audit", titleKey: "audit", to: "/operations/audit", access: { anyRole: ["SYS_ADMIN"] } },
      { key: "logs", titleKey: "logs", to: "/operations/logs/system", access: { anyRole: ["SYS_ADMIN"] } }
    ]
  },
  {
    key: "integrations",
    titleKey: "integrations",
    items: [{ key: "outbox", titleKey: "outbox", to: "/integrations/outbox", access: { anyRole: ["SYS_ADMIN"] } }]
  },
  {
    key: "approvals",
    titleKey: "approvals",
    items: [
      { key: "approvalWorkflows", titleKey: "approvalWorkflows", to: "/approvals/workflows", access: { anyRole: ["SYS_ADMIN", "SYS_OPERATOR"] } },
      { key: "approvalInbox", titleKey: "approvalInbox", to: "/approvals/inbox", access: { anyRole: ["SYS_ADMIN", "SYS_OPERATOR"] } },
      { key: "delegations", titleKey: "delegations", to: "/approvals/delegations", access: { anyRole: ["SYS_ADMIN", "SYS_OPERATOR"] } }
    ]
  },
  {
    key: "reports",
    titleKey: "reports",
    items: [{ key: "reports", titleKey: "reports", to: "/reports" }]
  }
];
