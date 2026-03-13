import { Navigate, Route, Routes } from "react-router-dom";
import { PermissionGuard } from "../../core/auth/guards";
import { AuthorizationWorkspacePage } from "../../modules/authorization/AuthorizationWorkspacePage";
import { DashboardPage } from "../../modules/dashboard/DashboardPage";
import { IdentityWorkspacePage } from "../../modules/identity/IdentityWorkspacePage";
import { IntegrationsWorkspacePage } from "../../modules/integrations/IntegrationsWorkspacePage";
import { OperationsWorkspacePage } from "../../modules/operations/OperationsWorkspacePage";
import { ReportsWorkspacePage } from "../../modules/reports/ReportsWorkspacePage";
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
        <Route path="identity/*" element={<IdentityWorkspacePage />} />
        <Route path="authorization/*" element={<AuthorizationWorkspacePage />} />
        <Route path="operations/*" element={<OperationsWorkspacePage />} />
        <Route path="integrations/*" element={<IntegrationsWorkspacePage />} />
        <Route path="reports/*" element={<ReportsWorkspacePage />} />
      </Route>
    </Routes>
  );
}
