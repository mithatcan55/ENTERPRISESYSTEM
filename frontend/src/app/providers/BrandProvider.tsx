import type { PropsWithChildren } from "react";
import { createContext, useContext, useEffect, useMemo, useState } from "react";
import { brandThemes, type BrandTheme, type BrandThemeKey } from "../theme/brandThemes";

type BrandContextValue = {
  activeBrand: BrandThemeKey;
  theme: BrandTheme;
  setActiveBrand: (brand: BrandThemeKey) => void;
};

const BrandContext = createContext<BrandContextValue | null>(null);

export function BrandProvider({ children }: PropsWithChildren) {
  const [activeBrand, setActiveBrand] = useState<BrandThemeKey>("hm-aygun");
  const theme = useMemo(() => brandThemes[activeBrand], [activeBrand]);

  useEffect(() => {
    const root = document.documentElement;

    Object.entries(theme.colors).forEach(([key, value]) => {
      root.style.setProperty(`--brand-${key}`, value);
    });
  }, [theme]);

  return (
    <BrandContext.Provider value={{ activeBrand, theme, setActiveBrand }}>
      {children}
    </BrandContext.Provider>
  );
}

export function useBrandTheme() {
  const context = useContext(BrandContext);

  if (!context) {
    throw new Error("useBrandTheme must be used inside BrandProvider.");
  }

  return context;
}
