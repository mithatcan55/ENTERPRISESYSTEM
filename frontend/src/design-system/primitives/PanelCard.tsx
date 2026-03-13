import type { PropsWithChildren } from "react";

export function PanelCard({
  title,
  subtitle,
  children
}: PropsWithChildren<{ title: string; subtitle?: string }>) {
  return (
    <section className="panel-card">
      <header className="panel-card__header">
        <div>
          <h2>{title}</h2>
          {subtitle ? <span>{subtitle}</span> : null}
        </div>
      </header>
      <div className="panel-card__body">{children}</div>
    </section>
  );
}
