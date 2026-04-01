import { Navigate, Route, Routes } from "react-router-dom";
import { ApprovalInboxPage } from "./ApprovalInboxPage";
import { ApprovalWorkflowsPage } from "./ApprovalWorkflowsPage";
import { DelegationsPage } from "./DelegationsPage";

export function ApprovalsWorkspacePage() {
  return (
    <Routes>
      <Route path="workflows" element={<ApprovalWorkflowsPage />} />
      <Route path="inbox" element={<ApprovalInboxPage />} />
      <Route path="delegations" element={<DelegationsPage />} />
      <Route path="" element={<Navigate to="inbox" replace />} />
      <Route path="*" element={<Navigate to="inbox" replace />} />
    </Routes>
  );
}
