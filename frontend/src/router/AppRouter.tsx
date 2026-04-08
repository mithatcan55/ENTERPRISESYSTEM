import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { useAuthStore } from "@/store/auth-store";
import Layout from "@/components/Layout";

// Auth
import LoginPage from "@/pages/LoginPage";
import ForbiddenPage from "@/pages/ForbiddenPage";

// Kimlik & Erişim
import { UsersListPage, UserDetailPage } from "@/modules/users";
import UserCreateEditPage from "@/modules/users/UserCreateEditPage";
import RolesListPage from "@/modules/roles/ListPage";
import SessionsPage from "@/pages/SessionsPage";

// Yetkilendirme
import PermissionsListPage from "@/modules/permissions/ListPage";
import TCodeTestPage from "@/modules/tcode/TestPage";

// Operasyonlar
import DashboardPage from "@/pages/DashboardPage";
import SystemLogsPage from "@/modules/logs/SystemLogsPage";
import SecurityEventsPage from "@/modules/logs/SecurityEventsPage";
import HttpRequestLogsPage from "@/modules/logs/HttpRequestLogsPage";
import EntityChangesPage from "@/modules/logs/EntityChangesPage";
import OutboxPage from "@/pages/OutboxPage";
import PasswordPolicyPage from "@/pages/PasswordPolicyPage";

// İş Süreçleri
import WorkflowListPage from "@/modules/approvals/WorkflowListPage";
import PendingApprovalsPage from "@/modules/approvals/PendingApprovalsPage";
import DelegationsPage from "@/modules/approvals/DelegationsPage";
import DocumentsListPage from "@/modules/documents/ListPage";
import ReportsListPage from "@/modules/reports/ListPage";

// ERP Entegrasyon
import ErpServicesPage from "@/modules/erp/ServicesPage";
import ErpRunnerPage from "@/modules/erp/RunnerPage";

// Profile
import ProfilePage from "@/pages/ProfilePage";
import ChangePasswordPage from "@/pages/ChangePasswordPage";

/* ─── Route guards ─── */

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const token = useAuthStore((s) => s.accessToken);
  if (!token) return <Navigate to="/login" replace />;
  return <>{children}</>;
}

function AdminRoute({ children }: { children: React.ReactNode }) {
  const token = useAuthStore((s) => s.accessToken);
  const hasRole = useAuthStore((s) => s.hasRole);
  if (!token) return <Navigate to="/login" replace />;
  if (!hasRole("SYS_ADMIN")) return <Navigate to="/403" replace />;
  return <>{children}</>;
}

/* ─── Router ─── */

export default function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/403" element={<ForbiddenPage />} />

        {/* Protected layout */}
        <Route
          element={
            <ProtectedRoute>
              <Layout />
            </ProtectedRoute>
          }
        >
          {/* ── Kimlik & Erişim ── */}
          <Route path="/users" element={<UsersListPage />} />
          <Route path="/users/new" element={<UserCreateEditPage />} />
          <Route path="/users/:id" element={<UserDetailPage />} />
          <Route path="/users/:id/edit" element={<UserCreateEditPage />} />
          <Route path="/roles" element={<AdminRoute><RolesListPage /></AdminRoute>} />
          <Route path="/sessions" element={<SessionsPage />} />

          {/* ── Yetkilendirme ── */}
          <Route path="/permissions" element={<AdminRoute><PermissionsListPage /></AdminRoute>} />
          <Route path="/tcode-test" element={<AdminRoute><TCodeTestPage /></AdminRoute>} />

          {/* ── Operasyonlar ── */}
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/logs/system" element={<SystemLogsPage />} />
          <Route path="/logs/security" element={<SecurityEventsPage />} />
          <Route path="/logs/requests" element={<HttpRequestLogsPage />} />
          <Route path="/logs/entity-changes" element={<EntityChangesPage />} />
          <Route path="/outbox" element={<OutboxPage />} />
          <Route path="/password-policy" element={<AdminRoute><PasswordPolicyPage /></AdminRoute>} />

          {/* ── İş Süreçleri ── */}
          <Route path="/approvals/workflows" element={<WorkflowListPage />} />
          <Route path="/approvals/pending" element={<PendingApprovalsPage />} />
          <Route path="/approvals/delegations" element={<DelegationsPage />} />
          <Route path="/documents" element={<DocumentsListPage />} />
          <Route path="/reports/templates" element={<ReportsListPage />} />

          {/* ── ERP Entegrasyon ── */}
          <Route path="/erp/services" element={<ErpServicesPage />} />
          <Route path="/erp/runner" element={<ErpRunnerPage />} />

          {/* ── Profile ── */}
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/change-password" element={<ChangePasswordPage />} />
        </Route>

        {/* Catch-all */}
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
