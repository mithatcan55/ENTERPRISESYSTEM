import { BrowserRouter, Routes, Route, Navigate, useParams } from "react-router-dom";
import { useAuthStore } from "@/store/auth-store";
import Layout from "@/components/Layout";
import { AppPermission, hasPermission } from "@/lib/permissions";
import { resolveDefaultRoute } from "@/lib/default-route";
import type { UserRole } from "@/types/auth";

// Auth
import LoginPage from "@/pages/LoginPage";
import ForbiddenPage from "@/pages/ForbiddenPage";

// Kimlik & Erişim
import { UsersListPage } from "@/modules/users";
import RolesListPage from "@/modules/roles/ListPage";
import AssignUserRolePage from "@/modules/roles/AssignUserRolePage";
import SessionsPage from "@/pages/SessionsPage";

// Yetkilendirme
import PermissionsListPage from "@/modules/permissions/ListPage";
import AssignUserPermissionPage from "@/modules/permissions/AssignUserPermissionPage";
import TCodeTestPage from "@/modules/tcode/TestPage";
import LocalizationListPage from "@/modules/localization/ListPage";

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
  const mustChangePassword = useAuthStore((s) => s.user?.mustChangePassword ?? false);
  const hasAnyAccess = useAuthStore((s) => {
    if (!s.user) return true;
    return (s.user.roles?.length ?? 0) > 0
      || (s.user.permissions?.length ?? 0) > 0;
  });
  const currentPath = window.location.pathname;

  if (!token) return <Navigate to="/login" replace />;
  if (mustChangePassword && currentPath !== "/change-password") {
    return <Navigate to="/change-password" replace />;
  }
  if (!mustChangePassword && !hasAnyAccess) {
    return <Navigate to="/403" replace />;
  }
  return <>{children}</>;
}

function AdminRoute({ children }: { children: React.ReactNode }) {
  const token = useAuthStore((s) => s.accessToken);
  const hasRole = useAuthStore((s) => s.hasRole);
  if (!token) return <Navigate to="/login" replace />;
  if (!hasRole("SYS_ADMIN")) return <Navigate to="/403" replace />;
  return <>{children}</>;
}

function RoleRoute({
  roles,
  children,
}: {
  roles: UserRole[];
  children: React.ReactNode;
}) {
  const token = useAuthStore((s) => s.accessToken);
  const hasRole = useAuthStore((s) => s.hasRole);

  if (!token) return <Navigate to="/login" replace />;
  if (!roles.some((role) => hasRole(role))) return <Navigate to="/403" replace />;
  return <>{children}</>;
}

function PermissionRoute({
  permission,
  children,
}: {
  permission: AppPermission | string;
  children: React.ReactNode;
}) {
  const token = useAuthStore((s) => s.accessToken);
  const user = useAuthStore((s) => s.user);

  if (!token) return <Navigate to="/login" replace />;
  if (!hasPermission(user, permission)) return <Navigate to="/403" replace />;
  return <>{children}</>;
}

function DefaultRouteRedirect() {
  const user = useAuthStore((s) => s.user);
  const token = useAuthStore((s) => s.accessToken);

  if (!token) return <Navigate to="/login" replace />;
  return <Navigate to={resolveDefaultRoute(user)} replace />;
}

function UsersLegacyRedirect({ mode }: { mode: "create" | "detail" | "edit" }) {
  const { id } = useParams<{ id: string }>();
  if (mode === "create") {
    return <Navigate to="/users?mode=create" replace />;
  }

  if (!id) {
    return <Navigate to="/users" replace />;
  }

  return <Navigate to={`/users?mode=${mode}&id=${id}`} replace />;
}

/* ─── Router ─── */

export default function AppRouter() {
  const opsRoles: UserRole[] = ["SYS_ADMIN", "SYS_OPERATOR"];

  return (
    <BrowserRouter>
      <Routes>
        {/* Public */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/403" element={<ForbiddenPage />} />

        {/* Password change is protected but rendered without Layout to avoid unrelated API calls */}
        <Route
          path="/change-password"
          element={
            <ProtectedRoute>
              <ChangePasswordPage />
            </ProtectedRoute>
          }
        />

        {/* Protected layout */}
        <Route
          element={
            <ProtectedRoute>
              <Layout />
            </ProtectedRoute>
          }
        >
          {/* ── Kimlik & Erişim ── */}
          <Route path="/users" element={<PermissionRoute permission={AppPermission.UsersListView}><UsersListPage /></PermissionRoute>} />
          <Route path="/users/new" element={<PermissionRoute permission={AppPermission.UsersCreate}><UsersLegacyRedirect mode="create" /></PermissionRoute>} />
          <Route path="/users/:id" element={<PermissionRoute permission={AppPermission.UsersDetailView}><UsersLegacyRedirect mode="detail" /></PermissionRoute>} />
          <Route path="/users/:id/edit" element={<PermissionRoute permission={AppPermission.UsersUpdate}><UsersLegacyRedirect mode="edit" /></PermissionRoute>} />
          <Route path="/roles" element={<AdminRoute><RolesListPage /></AdminRoute>} />
          <Route path="/roles/assign-user" element={<AdminRoute><AssignUserRolePage /></AdminRoute>} />
          <Route path="/sessions" element={<RoleRoute roles={opsRoles}><SessionsPage /></RoleRoute>} />

          {/* ── Yetkilendirme ── */}
          <Route path="/permissions" element={<AdminRoute><PermissionsListPage /></AdminRoute>} />
          <Route path="/permissions/assign-user" element={<AdminRoute><AssignUserPermissionPage /></AdminRoute>} />
          <Route path="/tcode-test" element={<AdminRoute><TCodeTestPage /></AdminRoute>} />
          <Route path="/localization/languages" element={<AdminRoute><LocalizationListPage /></AdminRoute>} />

          {/* ── Operasyonlar ── */}
          <Route path="/dashboard" element={<RoleRoute roles={opsRoles}><DashboardPage /></RoleRoute>} />
          <Route path="/logs/system" element={<RoleRoute roles={opsRoles}><SystemLogsPage /></RoleRoute>} />
          <Route path="/logs/security" element={<RoleRoute roles={opsRoles}><SecurityEventsPage /></RoleRoute>} />
          <Route path="/logs/requests" element={<RoleRoute roles={opsRoles}><HttpRequestLogsPage /></RoleRoute>} />
          <Route path="/logs/entity-changes" element={<RoleRoute roles={opsRoles}><EntityChangesPage /></RoleRoute>} />
          <Route path="/outbox" element={<RoleRoute roles={opsRoles}><OutboxPage /></RoleRoute>} />
          <Route path="/password-policy" element={<AdminRoute><PasswordPolicyPage /></AdminRoute>} />

          {/* ── İş Süreçleri ── */}
          <Route path="/approvals/workflows" element={<RoleRoute roles={opsRoles}><WorkflowListPage /></RoleRoute>} />
          <Route path="/approvals/pending" element={<PendingApprovalsPage />} />
          <Route path="/approvals/delegations" element={<RoleRoute roles={opsRoles}><DelegationsPage /></RoleRoute>} />
          <Route path="/documents" element={<RoleRoute roles={opsRoles}><DocumentsListPage /></RoleRoute>} />
          <Route path="/reports/templates" element={<RoleRoute roles={opsRoles}><ReportsListPage /></RoleRoute>} />

          {/* ── ERP Entegrasyon ── */}
          <Route path="/erp/services" element={<RoleRoute roles={opsRoles}><ErpServicesPage /></RoleRoute>} />
          <Route path="/erp/runner" element={<RoleRoute roles={opsRoles}><ErpRunnerPage /></RoleRoute>} />

          {/* ── Profile ── */}
          <Route path="/profile" element={<ProfilePage />} />
        </Route>

        {/* Catch-all */}
        <Route path="*" element={<DefaultRouteRedirect />} />
      </Routes>
    </BrowserRouter>
  );
}
