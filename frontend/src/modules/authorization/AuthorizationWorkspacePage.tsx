import { Route, Routes } from "react-router-dom";
import { PermissionsPage } from "./permissions/PermissionsPage";
import { RolesListPage } from "./roles/RolesListPage";
import { PlaceholderModulePage } from "../shared/PlaceholderModulePage";

export function AuthorizationWorkspacePage() {
  return (
    <Routes>
      <Route path="roles" element={<RolesListPage />} />
      <Route path="permissions/actions" element={<PermissionsPage />} />
      <Route
        path="tcode"
        element={
          <PlaceholderModulePage
            title="T-Code Workspace"
            description="T-Code cozumleme ve seviye bazli sonuc ekrani sonraki adimda gercek modulle baglanacak."
          />
        }
      />
      <Route
        path="*"
        element={
          <PlaceholderModulePage
            title="Authorization Workspace"
            description="Rol, yetki ve T-Code ekranlari bu modulde toplanir."
          />
        }
      />
    </Routes>
  );
}
