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

  if (isLoading) return null;
  if (isAuthenticated) return <Navigate to="/dashboard" replace />;

  async function handleSubmit() {
    setIsSubmitting(true);
    setError(null);
    try {
      await login({ identifier: identifier.trim(), password });
      navigate("/dashboard", { replace: true });
    } catch (err) {
      setError(err as ApiError);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="skeleton-placeholder" style={{ minHeight: "100vh", flexDirection: "column", gap: 16 }}>
      <h2>Login — Bootstrap tema bekleniyor</h2>
      <form onSubmit={(e) => { e.preventDefault(); void handleSubmit(); }} style={{ display: "flex", flexDirection: "column", gap: 8, width: 300 }}>
        <input type="text" value={identifier} onChange={(e) => setIdentifier(e.target.value)} placeholder="Kullanici adi" style={{ padding: 8 }} />
        <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Sifre" style={{ padding: 8 }} />
        <button type="submit" disabled={isSubmitting} style={{ padding: 8 }}>{isSubmitting ? "..." : "Giris Yap"}</button>
        {error && <p style={{ color: "red" }}>{error.detail ?? error.title}</p>}
        <small>core.admin / 123456</small>
      </form>
    </div>
  );
}
