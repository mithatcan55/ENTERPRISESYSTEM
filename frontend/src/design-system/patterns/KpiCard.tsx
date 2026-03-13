export function KpiCard({
  title,
  value,
  delta,
  tone
}: {
  title: string;
  value: string;
  delta: string;
  tone: "primary" | "accent" | "neutral" | "danger";
}) {
  return (
    <article className={`kpi-card kpi-card--${tone}`}>
      <span>{title}</span>
      <strong>{value}</strong>
      <small>{delta}</small>
    </article>
  );
}
