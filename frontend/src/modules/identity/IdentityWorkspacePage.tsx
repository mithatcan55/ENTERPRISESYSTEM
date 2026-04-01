import { Navigate, Route, Routes } from "react-router-dom";
import { TCodeGuard } from "../../app/shell/TCodeGuard";
import { PasswordPolicyPage } from "./password-policy/PasswordPolicyPage";
import { SessionsPage } from "./sessions/SessionsPage";
import { UserCreatePage } from "./users/pages/UserCreatePage";
import { UserDetailPage } from "./users/pages/UserDetailPage";
import { UserEditPage } from "./users/pages/UserEditPage";
import { UserListPage } from "./users/pages/UserListPage";
import { UserPermissionsPage } from "./users/pages/UserPermissionsPage";
import { UserRolesPage } from "./users/pages/UserRolesPage";
import { PlaceholderModulePage } from "../shared/PlaceholderModulePage";

export function IdentityWorkspacePage() {
  return (
    <Routes>
      {/* SU04 — Kullanıcı Listesi */}
      <Route
        path="users"
        element={
          <TCodeGuard tcode="SYS04">
            <UserListPage />
          </TCodeGuard>
        }
      />
      {/* SU01 — Yeni Kullanıcı */}
      <Route
        path="users/create"
        element={
          <TCodeGuard tcode="SYS01">
            <UserCreatePage />
          </TCodeGuard>
        }
      />
      {/* SU03 — Kullanıcı Detay */}
      <Route
        path="users/:userId"
        element={
          <TCodeGuard tcode="SYS03">
            <UserDetailPage />
          </TCodeGuard>
        }
      />
      {/* SU02 — Kullanıcı Düzenle */}
      <Route
        path="users/:userId/edit"
        element={
          <TCodeGuard tcode="SYS02">
            <UserEditPage />
          </TCodeGuard>
        }
      />
      {/* SU05 — Kullanıcı Rolleri */}
      <Route
        path="users/:userId/roles"
        element={
          <TCodeGuard tcode="SYS05">
            <UserRolesPage />
          </TCodeGuard>
        }
      />
      {/* SU06 — Kullanıcı İzinleri */}
      <Route
        path="users/:userId/permissions"
        element={
          <TCodeGuard tcode="SYS06">
            <UserPermissionsPage />
          </TCodeGuard>
        }
      />

      <Route path="sessions" element={<SessionsPage />} />
      <Route path="password-policy" element={<PasswordPolicyPage />} />
      {/* /identity veya bilinmeyen path → /identity/users'a yonlendir */}
      <Route path="" element={<Navigate to="users" replace />} />
      <Route path="*" element={<Navigate to="users" replace />} />
    </Routes>
  );
}

