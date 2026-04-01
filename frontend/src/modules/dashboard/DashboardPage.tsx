import { useAuth } from "../../core/auth/AuthProvider";

export function DashboardPage() {
  const { user } = useAuth();
  return (
    <div className="skeleton-placeholder" style={{ flexDirection: "column", gap: 8 }}>
      <h2>Dashboard — Bootstrap tema bekleniyor</h2>
      <p>Hos geldiniz, {user?.displayName ?? user?.username ?? "Admin"}</p>
    </div>
  );
}
