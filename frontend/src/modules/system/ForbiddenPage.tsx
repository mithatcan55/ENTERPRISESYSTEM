import { Link } from "react-router-dom";

export function ForbiddenPage() {
  return (
    <main className="login-page">
      <section className="login-card">
        <span className="login-card__eyebrow">403</span>
        <h1>Erisim Engellendi</h1>
        <p>Rol, permission veya T-Code kisiti nedeniyle bu ekrana erisemiyorsunuz.</p>
        <Link className="quick-action-card" to="/dashboard">
          Ana panele don
        </Link>
      </section>
    </main>
  );
}
