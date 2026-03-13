import { Route, Routes } from "react-router-dom";
import { PasswordPolicyPage } from "./password-policy/PasswordPolicyPage";
import { UsersListPage } from "./users/UsersListPage";
import { SessionsPage } from "./sessions/SessionsPage";
import { PlaceholderModulePage } from "../shared/PlaceholderModulePage";

export function IdentityWorkspacePage() {
  return (
    <Routes>
      <Route path="users" element={<UsersListPage />} />
      <Route path="sessions" element={<SessionsPage />} />
      <Route path="password-policy" element={<PasswordPolicyPage />} />
      <Route
        path="*"
        element={
          <PlaceholderModulePage
            title="Identity Workspace"
            description="Kullanici, oturum ve sifre politikasi ekranlari bu modulde toplanir."
          />
        }
      />
    </Routes>
  );
}
