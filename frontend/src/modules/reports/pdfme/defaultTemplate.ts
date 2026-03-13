import { BLANK_PDF, type Template } from "@pdfme/common";

type TemplateBrandColors = {
  primary: string;
  neutralBrand: string;
  accent: string;
};

const defaultBrandColors: TemplateBrandColors = {
  primary: "#0C446D",
  neutralBrand: "#6E6B6B",
  accent: "#E00008"
};

// Ilk designer acildiginda kullanicinin bos bir tuval yerine neye benzer bir
// rapor duzeni gorecegini belirleyen temel sablon burada tutulur.
export function createDefaultPdfmeTemplate(colors: Partial<TemplateBrandColors> = {}): Template {
  const palette = { ...defaultBrandColors, ...colors };

  return {
    basePdf: BLANK_PDF,
    schemas: [
      [
        {
          name: "reportTitle",
          type: "text",
          position: { x: 20, y: 20 },
          width: 110,
          height: 14,
          fontSize: 20,
          fontColor: palette.primary,
          content: "HM | AYGUN RAPOR BASLIGI"
        },
        {
          name: "reportAccent",
          type: "text",
          position: { x: 140, y: 21 },
          width: 20,
          height: 14,
          fontSize: 20,
          fontColor: palette.accent,
          content: "II"
        },
        {
          name: "reportSubtitle",
          type: "text",
          position: { x: 20, y: 38 },
          width: 120,
          height: 8,
          fontSize: 10,
          fontColor: palette.neutralBrand,
          content: "Dinamik alanlar ve marka uyumlu rapor tasarimi"
        },
        {
          name: "reportOwnerLabel",
          type: "text",
          position: { x: 20, y: 58 },
          width: 36,
          height: 7,
          fontSize: 9,
          fontColor: palette.neutralBrand,
          content: "Hazirlayan:"
        },
        {
          name: "reportOwner",
          type: "text",
          position: { x: 58, y: 58 },
          width: 48,
          height: 7,
          fontSize: 9,
          fontColor: palette.primary,
          content: "CORE_ADMIN"
        },
        {
          name: "reportDateLabel",
          type: "text",
          position: { x: 118, y: 58 },
          width: 24,
          height: 7,
          fontSize: 9,
          fontColor: palette.neutralBrand,
          content: "Tarih:"
        },
        {
          name: "reportDate",
          type: "text",
          position: { x: 144, y: 58 },
          width: 30,
          height: 7,
          fontSize: 9,
          fontColor: palette.primary,
          content: "2026-03-13"
        },
        {
          name: "reportFooter",
          type: "text",
          position: { x: 20, y: 280 },
          width: 150,
          height: 8,
          fontSize: 9,
          fontColor: palette.neutralBrand,
          content: "Sayfa {{page}} / {{totalPages}}"
        }
      ]
    ]
  };
}

export const defaultPdfmeTemplate: Template = createDefaultPdfmeTemplate();
