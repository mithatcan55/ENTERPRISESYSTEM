import { Route, Routes } from "react-router-dom";
import { PlaceholderModulePage } from "../shared/PlaceholderModulePage";
import { ApprovalInboxPage } from "./ApprovalInboxPage";
import { ApprovalWorkflowsPage } from "./ApprovalWorkflowsPage";
import { DelegationsPage } from "./DelegationsPage";

export function ApprovalsWorkspacePage() {
  return (
    <Routes>
      <Route path="workflows" element={<ApprovalWorkflowsPage />} />
      <Route path="inbox" element={<ApprovalInboxPage />} />
      <Route path="delegations" element={<DelegationsPage />} />
      <Route
        path="*"
        element={
          <PlaceholderModulePage
            title="Approvals Workspace"
            description="Approval workflow, inbox ve delegation ekranlari bu modulde toplanir."
          />
        }
      />
    </Routes>
  );
}
