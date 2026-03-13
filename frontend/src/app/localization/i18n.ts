import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import enCommon from "./locales/en/common.json";
import enMenu from "./locales/en/menu.json";
import deCommon from "./locales/de/common.json";
import deMenu from "./locales/de/menu.json";
import trCommon from "./locales/tr/common.json";
import trMenu from "./locales/tr/menu.json";

void i18n.use(initReactI18next).init({
  lng: "tr",
  fallbackLng: "en",
  defaultNS: "common",
  ns: ["common", "menu"],
  interpolation: {
    escapeValue: false
  },
  resources: {
    tr: {
      common: trCommon,
      menu: trMenu
    },
    en: {
      common: enCommon,
      menu: enMenu
    },
    de: {
      common: deCommon,
      menu: deMenu
    }
  }
});

export default i18n;
