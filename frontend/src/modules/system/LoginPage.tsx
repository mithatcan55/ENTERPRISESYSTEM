import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Navigate, useNavigate } from "react-router-dom";
import { useAuth } from "../../core/auth/AuthProvider";
import type { ApiError } from "../../core/api/httpClient";

export function LoginPage() {
  const { t } = useTranslation(["common"]);
  const navigate = useNavigate();
  const { isAuthenticated, isLoading, login } = useAuth();
  const [identifier, setIdentifier] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<ApiError | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (isLoading) {
    return null;
  }

  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  async function handleSubmit() {
    setIsSubmitting(true);
    setError(null);

    try {
      await login({ identifier, password });
      navigate("/dashboard", { replace: true });
    } catch (requestError) {
      setError(requestError as ApiError);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="login-page">
      <section className="login-card">
        <span className="login-card__eyebrow">HM | AYGUN</span>
        <h1>{t("appTitle")}</h1>
        <p>{t("loginDescription")}</p>
        <form className="login-form">
          <input
            type="text"
            value={identifier}
            placeholder={t("loginIdentifierPlaceholder")}
            onChange={(event) => setIdentifier(event.target.value)}
          />
          <input
            type="password"
            value={password}
            placeholder={t("loginPasswordPlaceholder")}
            onChange={(event) => setPassword(event.target.value)}
          />
          <button type="button" onClick={() => void handleSubmit()} disabled={isSubmitting}>
            {isSubmitting ? t("loginSubmitting") : t("loginAction")}
          </button>
        </form>
        {error ? <p className="form-feedback form-feedback--error">{error.detail ?? error.title}</p> : null}
      </section>
    </main>
  );
}
