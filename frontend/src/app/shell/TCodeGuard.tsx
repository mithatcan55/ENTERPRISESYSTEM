import type { PropsWithChildren } from "react";
import { Navigate } from "react-router-dom";
import { useTCodeAccess } from "../../core/auth/useTCodeAccess";

type TCodeGuardProps = PropsWithChildren<{
  tcode: string;
  /** Yetkisiz durumda yönlendirilecek yol. Varsayılan: "/forbidden" */
  redirectTo?: string;
}>;

/**
 * Verilen T-Code için kullanıcının Level 1-4 erişim hakkı yoksa
 * `redirectTo` (varsayılan /forbidden) adresine yönlendirir.
 *
 * Yükleme sırasında boş render döner — layout flash önlemek için.
 */
export function TCodeGuard({ tcode, redirectTo = "/forbidden", children }: TCodeGuardProps) {
  const { isAllowed, isLoading } = useTCodeAccess(tcode);

  if (isLoading) {
    // Yükleme tamamlanana kadar hiçbir şey gösterme (flash önle)
    return null;
  }

  if (!isAllowed) {
    return <Navigate to={redirectTo} replace />;
  }

  return <>{children}</>;
}
