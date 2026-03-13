import { Route, Routes } from "react-router-dom";
import { AuditDashboardPage } from "./AuditDashboardPage";
import { LogsPage } from "./LogsPage";
import { PlaceholderModulePage } from "../shared/PlaceholderModulePage";

export function OperationsWorkspacePage() {
  return (
    <Routes>
      <Route path="audit" element={<AuditDashboardPage />} />
      <Route path="logs/system" element={<LogsPage />} />
      <Route path="logs/security" element={<LogsPage />} />
      <Route path="logs/http" element={<LogsPage />} />
      <Route path="logs/entity-changes" element={<LogsPage />} />
      <Route
        path="*"
        element={
          <PlaceholderModulePage
            title="Operations Workspace"
            description="Audit dashboard ve log explorer ekranlari burada toplanir."
          />
        }
      />
    </Routes>
  );
}
