export type BrandThemeKey = "hm-aygun";

export type BrandTheme = {
  name: string;
  logos: {
    compact: string;
    wide: string;
  };
  colors: {
    primary: string;
    primaryStrong: string;
    primarySoft: string;
    accent: string;
    neutralBrand: string;
    surfaceTint: string;
    textStrong: string;
    textMuted: string;
    border: string;
    success: string;
    warning: string;
    danger: string;
    info: string;
  };
};

export const brandThemes: Record<BrandThemeKey, BrandTheme> = {
  "hm-aygun": {
    name: "Hermann Muller | Aygun",
    logos: {
      compact: "/brands/hm-aygun-compact.png",
      wide: "/brands/hm-aygun-wide.png"
    },
    colors: {
      primary: "#0C446D",
      primaryStrong: "#083555",
      primarySoft: "#EAF1F6",
      accent: "#E00008",
      neutralBrand: "#6E6B6B",
      surfaceTint: "#F3F5F7",
      textStrong: "#17222D",
      textMuted: "#5B6672",
      border: "#D6DDE4",
      success: "#1F8A4C",
      warning: "#C98900",
      danger: "#C62828",
      info: "#1F6FB2"
    }
  }
};
