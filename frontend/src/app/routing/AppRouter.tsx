import { Navigate, Route, Routes } from "react-router-dom";
import { PermissionGuard } from "../../core/auth/guards";
import { AuthorizationPlaceholderPage } from "../../modules/authorization/AuthorizationPlaceholderPage";
import { DashboardPage } from "../../modules/dashboard/DashboardPage";
import { IdentityPlaceholderPage } from "../../modules/identity/IdentityPlaceholderPage";
import { IntegrationsPlaceholderPage } from "../../modules/integrations/IntegrationsPlaceholderPage";
import { OperationsPlaceholderPage } from "../../modules/operations/OperationsPlaceholderPage";
import { ReportsPlaceholderPage } from "../../modules/reports/ReportsPlaceholderPage";
import { ForbiddenPage } from "../../modules/system/ForbiddenPage";
import { LoginPage } from "../../modules/system/LoginPage";
import { AppShell } from "../shell/AppShell";

export function AppRouter() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/forbidden" element={<ForbiddenPage />} />
      <Route
        path="/"
        element={
          <PermissionGuard anyRole={["SYS_ADMIN", "SYS_OPERATOR"]}>
            <AppShell />
          </PermissionGuard>
        }
      >
        <Route index element={<Navigate to="/dashboard" replace />} />
        <Route path="dashboard" element={<DashboardPage />} />
        <Route path="identity/*" element={<IdentityPlaceholderPage />} />
        <Route path="authorization/*" element={<AuthorizationPlaceholderPage />} />
        <Route path="operations/*" element={<OperationsPlaceholderPage />} />
        <Route path="integrations/*" element={<IntegrationsPlaceholderPage />} />
        <Route path="reports/*" element={<ReportsPlaceholderPage />} />
      </Route>
    </Routes>
  );
}
