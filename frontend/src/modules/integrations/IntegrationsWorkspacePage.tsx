import { Navigate, Route, Routes } from "react-router-dom";
import { OutboxPage } from "./outbox/OutboxPage";
import { ErpQueryPage } from "./erp/ErpQueryPage";

export function IntegrationsWorkspacePage() {
  return (
    <Routes>
      <Route path="outbox" element={<OutboxPage />} />
      <Route path="erp" element={<ErpQueryPage />} />
      <Route path="" element={<Navigate to="erp" replace />} />
      <Route path="*" element={<Navigate to="erp" replace />} />
    </Routes>
  );
}
