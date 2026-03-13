import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import enIdentity from "./locales/en/identity.json";
import enCommon from "./locales/en/common.json";
import enMenu from "./locales/en/menu.json";
import deIdentity from "./locales/de/identity.json";
import deCommon from "./locales/de/common.json";
import deMenu from "./locales/de/menu.json";
import trIdentity from "./locales/tr/identity.json";
import trCommon from "./locales/tr/common.json";
import trMenu from "./locales/tr/menu.json";

void i18n.use(initReactI18next).init({
  lng: "tr",
  fallbackLng: "en",
  defaultNS: "common",
  ns: ["common", "menu", "identity"],
  interpolation: {
    escapeValue: false
  },
  resources: {
    tr: {
      common: trCommon,
      menu: trMenu,
      identity: trIdentity
    },
    en: {
      common: enCommon,
      menu: enMenu,
      identity: enIdentity
    },
    de: {
      common: deCommon,
      menu: deMenu,
      identity: deIdentity
    }
  }
});

export default i18n;
