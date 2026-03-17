import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import enAuthorization from "./locales/en/authorization.json";
import enApprovals from "./locales/en/approvals.json";
import enIdentity from "./locales/en/identity.json";
import enIntegrations from "./locales/en/integrations.json";
import enOperations from "./locales/en/operations.json";
import enReports from "./locales/en/reports.json";
import enCommon from "./locales/en/common.json";
import enMenu from "./locales/en/menu.json";
import deAuthorization from "./locales/de/authorization.json";
import deApprovals from "./locales/de/approvals.json";
import deIdentity from "./locales/de/identity.json";
import deIntegrations from "./locales/de/integrations.json";
import deOperations from "./locales/de/operations.json";
import deReports from "./locales/de/reports.json";
import deCommon from "./locales/de/common.json";
import deMenu from "./locales/de/menu.json";
import trAuthorization from "./locales/tr/authorization.json";
import trApprovals from "./locales/tr/approvals.json";
import trIdentity from "./locales/tr/identity.json";
import trIntegrations from "./locales/tr/integrations.json";
import trOperations from "./locales/tr/operations.json";
import trReports from "./locales/tr/reports.json";
import trCommon from "./locales/tr/common.json";
import trMenu from "./locales/tr/menu.json";

void i18n.use(initReactI18next).init({
  lng: "tr",
  fallbackLng: "en",
  defaultNS: "common",
  ns: ["common", "menu", "identity", "authorization", "operations", "integrations", "reports", "approvals"],
  interpolation: {
    escapeValue: false
  },
  resources: {
    tr: {
      common: trCommon,
      menu: trMenu,
      approvals: trApprovals,
      identity: trIdentity,
      authorization: trAuthorization,
      operations: trOperations,
      integrations: trIntegrations,
      reports: trReports
    },
    en: {
      common: enCommon,
      menu: enMenu,
      approvals: enApprovals,
      identity: enIdentity,
      authorization: enAuthorization,
      operations: enOperations,
      integrations: enIntegrations,
      reports: enReports
    },
    de: {
      common: deCommon,
      menu: deMenu,
      approvals: deApprovals,
      identity: deIdentity,
      authorization: deAuthorization,
      operations: deOperations,
      integrations: deIntegrations,
      reports: deReports
    }
  }
});

export default i18n;
