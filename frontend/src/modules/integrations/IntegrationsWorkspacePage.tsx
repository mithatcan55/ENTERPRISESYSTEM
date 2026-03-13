import { Route, Routes } from "react-router-dom";
import { OutboxPage } from "./outbox/OutboxPage";
import { PlaceholderModulePage } from "../shared/PlaceholderModulePage";

export function IntegrationsWorkspacePage() {
  return (
    <Routes>
      <Route path="outbox" element={<OutboxPage />} />
      <Route
        path="*"
        element={
          <PlaceholderModulePage
            title="Integrations Workspace"
            description="Outbox ve entegrasyon operasyon ekranlari burada toplanir."
          />
        }
      />
    </Routes>
  );
}
