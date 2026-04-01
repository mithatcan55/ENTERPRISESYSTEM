const LOCALE_MAP: Record<string, string> = {
  tr: "tr-TR",
  en: "en-GB",
  de: "de-DE",
};

export function formatDateTime(isoString: string, lang = "tr"): string {
  const locale = LOCALE_MAP[lang] ?? "tr-TR";
  return new Intl.DateTimeFormat(locale, {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  }).format(new Date(isoString));
}

export function formatDate(isoString: string, lang = "tr"): string {
  const locale = LOCALE_MAP[lang] ?? "tr-TR";
  return new Intl.DateTimeFormat(locale, {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  }).format(new Date(isoString));
}

export function formatTime(isoString: string, lang = "tr"): string {
  const locale = LOCALE_MAP[lang] ?? "tr-TR";
  return new Intl.DateTimeFormat(locale, {
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(isoString));
}
