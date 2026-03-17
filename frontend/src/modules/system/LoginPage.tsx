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
      <section className="login-layout">
        <article className="login-hero">
          <span className="login-card__eyebrow">HM | AYGUN</span>
          <strong className="login-hero__meta">{t("loginEnterpriseLabel")}</strong>
          <h1>{t("loginHeroHeadline")}</h1>
          <p>{t("loginDescription")}</p>
          <div className="login-hero__summary">
            <div>
              <span>{t("loginHeroSummaryLabel")}</span>
              <strong>{t("loginHeroSummaryValue")}</strong>
            </div>
            <div>
              <span>{t("loginHeroSecurityLabel")}</span>
              <strong>{t("loginHeroSecurityValue")}</strong>
            </div>
          </div>
          <div className="login-hero__highlights">
            <div>
              <span>{t("loginHighlightUsers")}</span>
              <strong>{t("loginHighlightUsersValue")}</strong>
            </div>
            <div>
              <span>{t("loginHighlightApprovals")}</span>
              <strong>{t("loginHighlightApprovalsValue")}</strong>
            </div>
            <div>
              <span>{t("loginHighlightAudit")}</span>
              <strong>{t("loginHighlightAuditValue")}</strong>
            </div>
          </div>
        </article>

        <section className="login-card login-card--elevated">
          <div className="login-card__header">
            <span className="login-card__eyebrow">{t("loginFormEyebrow")}</span>
            <h2>{t("loginFormTitle")}</h2>
            <p>{t("loginFormDescription")}</p>
          </div>
          <form
            className="login-form"
            onSubmit={(event) => {
              event.preventDefault();
              void handleSubmit();
            }}
          >
            <label className="login-form__field">
              <span>{t("loginIdentifierLabel")}</span>
              <input
                type="text"
                value={identifier}
                placeholder={t("loginIdentifierPlaceholder")}
                onChange={(event) => setIdentifier(event.target.value)}
              />
            </label>
            <label className="login-form__field">
              <span>{t("loginPasswordLabel")}</span>
              <input
                type="password"
                value={password}
                placeholder={t("loginPasswordPlaceholder")}
                onChange={(event) => setPassword(event.target.value)}
              />
            </label>
            <button type="submit" disabled={isSubmitting}>
              {isSubmitting ? t("loginSubmitting") : t("loginAction")}
            </button>
          </form>
          <div className="login-card__credentials">
            <div>
              <span>{t("loginDemoUserLabel")}</span>
              <strong>core.admin</strong>
            </div>
            <div>
              <span>{t("loginDemoPasswordLabel")}</span>
              <strong>123456</strong>
            </div>
          </div>
          <div className="login-card__footer">
            <div>
              <span>{t("loginSupportLabel")}</span>
              <strong>{t("loginSupportValue")}</strong>
            </div>
            <div>
              <span>{t("loginSecurityLabel")}</span>
              <strong>{t("loginSecurityValue")}</strong>
            </div>
          </div>
          {error ? <p className="form-feedback form-feedback--error">{error.detail ?? error.title}</p> : null}
        </section>
      </section>
    </main>
  );
}
