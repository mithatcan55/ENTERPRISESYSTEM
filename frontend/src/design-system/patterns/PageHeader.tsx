export function PageHeader({ title, description }: { title: string; description: string }) {
  return (
    <header className="page-header">
      <div>
        <span className="page-header__eyebrow">Enterprise Platform</span>
        <h1>{title}</h1>
        <p>{description}</p>
      </div>
    </header>
  );
}
