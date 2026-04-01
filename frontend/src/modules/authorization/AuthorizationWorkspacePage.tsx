import { Navigate, Route, Routes } from "react-router-dom";
import { PermissionsPage } from "./permissions/PermissionsPage";
import { RolesListPage } from "./roles/RolesListPage";
import { TCodeResolverPage } from "./tcode/TCodeResolverPage";

export function AuthorizationWorkspacePage() {
  return (
    <Routes>
      <Route path="roles" element={<RolesListPage />} />
      <Route path="permissions/actions" element={<PermissionsPage />} />
      <Route path="tcode" element={<TCodeResolverPage />} />
      <Route path="" element={<Navigate to="roles" replace />} />
      <Route path="*" element={<Navigate to="roles" replace />} />
    </Routes>
  );
}
