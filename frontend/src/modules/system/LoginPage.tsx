export function LoginPage() {
  return (
    <main className="login-page">
      <section className="login-card">
        <span className="login-card__eyebrow">HM | AYGUN</span>
        <h1>Enterprise System</h1>
        <p>Tek frontend shell uzerinden kullanici, yetki, audit ve operasyon yonetimi.</p>
        <form className="login-form">
          <input type="text" placeholder="Kullanici kodu veya email" />
          <input type="password" placeholder="Sifre" />
          <button type="button">Giris</button>
        </form>
      </section>
    </main>
  );
}
