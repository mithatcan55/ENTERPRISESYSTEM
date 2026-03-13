import { Route, Routes } from "react-router-dom";
import { UsersListPage } from "./users/UsersListPage";
import { PlaceholderModulePage } from "../shared/PlaceholderModulePage";

export function IdentityWorkspacePage() {
  return (
    <Routes>
      <Route path="users" element={<UsersListPage />} />
      <Route
        path="sessions"
        element={
          <PlaceholderModulePage
            title="Sessions Workspace"
            description="Oturum izleme ve revoke ekranlari bir sonraki adimda gercek module baglanacak."
          />
        }
      />
      <Route
        path="password-policy"
        element={
          <PlaceholderModulePage
            title="Password Policy Workspace"
            description="Sifre politikasi yonetimi cekirdek shell icinde yerini aldi; detay form ekranlari sonraki adimda eklenecek."
          />
        }
      />
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
