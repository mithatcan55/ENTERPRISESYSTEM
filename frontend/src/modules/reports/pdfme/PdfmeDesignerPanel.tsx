import type { Template } from "@pdfme/common";
import { generate } from "@pdfme/generator";
import { image, table, text } from "@pdfme/schemas";
import { Designer } from "@pdfme/ui";
import { useEffect, useRef } from "react";

type PdfmeDesignerPanelProps = {
  template: Template;
  sampleInput: Record<string, string>;
  primaryColor: string;
  onTemplateChange: (template: Template) => void;
  onPreviewReady: (url: string) => void;
};

const plugins = {
  text,
  image,
  table
};

export function PdfmeDesignerPanel({
  template,
  sampleInput,
  primaryColor,
  onTemplateChange,
  onPreviewReady
}: PdfmeDesignerPanelProps) {
  const containerRef = useRef<HTMLDivElement | null>(null);
  const designerRef = useRef<Designer | null>(null);

  useEffect(() => {
    if (!containerRef.current) {
      return;
    }

    // pdfme designer React component gibi degil, DOM container uzerine kuruluyor.
    // Bu nedenle mount ve update yasam donguleri ayri ele aliniyor.
    designerRef.current = new Designer({
      domContainer: containerRef.current,
      template,
      plugins,
      options: {
        lang: "en",
        sidebarOpen: true,
        theme: {
          token: {
            colorPrimary: primaryColor
          }
        }
      }
    });

    designerRef.current.onChangeTemplate((nextTemplate: Template) => {
      onTemplateChange(nextTemplate);
    });

    return () => {
      designerRef.current?.destroy();
      designerRef.current = null;
    };
  }, [onTemplateChange, primaryColor]);

  useEffect(() => {
    designerRef.current?.updateTemplate(template);
  }, [template]);

  async function createPdfBlob() {
    const activeTemplate = designerRef.current?.getTemplate() ?? template;
    const pdf = await generate({
      template: activeTemplate,
      inputs: [sampleInput],
      plugins
    });

    return new Blob([pdf.buffer], { type: "application/pdf" });
  }

  async function handlePreview() {
    const blob = await createPdfBlob();
    const url = URL.createObjectURL(blob);
    onPreviewReady(url);
  }

  async function handleDownloadPdf() {
    const blob = await createPdfBlob();
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = "hm-aygun-report-preview.pdf";
    anchor.click();
    URL.revokeObjectURL(url);
  }

  function handleExportTemplate() {
    const activeTemplate = designerRef.current?.getTemplate() ?? template;
    const blob = new Blob([JSON.stringify(activeTemplate, null, 2)], { type: "application/json" });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = "hm-aygun-report-template.json";
    anchor.click();
    URL.revokeObjectURL(url);
  }

  return (
    <div className="pdfme-designer-panel">
      <div className="pdfme-designer-panel__toolbar">
        <button type="button" className="secondary-button" onClick={() => void handleExportTemplate()}>
          Template JSON
        </button>
        <button type="button" className="secondary-button" onClick={() => void handleDownloadPdf()}>
          PDF indir
        </button>
        <button type="button" className="secondary-button" onClick={() => void handlePreview()}>
          Preview PDF
        </button>
      </div>
      <div ref={containerRef} className="pdfme-designer-panel__canvas" />
    </div>
  );
}
