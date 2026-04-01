import type { PropsWithChildren, ReactNode } from "react";

export function PanelCard({
  title,
  subtitle,
  action,
  children
}: PropsWithChildren<{ title: string; subtitle?: string; action?: ReactNode }>) {
  return (
    <section className="panel-card">
      <header className="panel-card__header">
        <div style={{ display: "flex", alignItems: "flex-start", justifyContent: "space-between", gap: 12 }}>
          <div>
            <h2>{title}</h2>
            {subtitle ? <span>{subtitle}</span> : null}
          </div>
          {action ? <div style={{ flexShrink: 0 }}>{action}</div> : null}
        </div>
      </header>
      <div className="panel-card__body">{children}</div>
    </section>
  );
}
