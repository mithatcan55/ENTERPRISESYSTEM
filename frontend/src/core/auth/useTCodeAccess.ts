import { useQuery } from "@tanstack/react-query";
import { checkTCodeAccess } from "../../modules/authorization/tcode/tcode.api";

/**
 * Verilen T-Code için kullanıcının erişim hakkını sorgular.
 *
 * - `isAllowed`: sayfaya girme hakkı var mı (Level 1-4 tüm kontroller)
 * - `can(actionCode)`: spesifik bir aksiyon izni var mı (Level 5)
 * - `isLoading`: ilk yükleme devam ediyor mu
 *
 * Sonuçlar TanStack Query cache'inde 5 dakika tutulur; aynı T-Code için
 * bileşen ağacı boyunca tek bir istek gider.
 */
export function useTCodeAccess(tcode: string) {
  const { data, isLoading } = useQuery({
    queryKey: ["tcode-access", tcode.toUpperCase()],
    queryFn: ({ signal }) => checkTCodeAccess(tcode, signal),
    staleTime: 5 * 60 * 1000,   // 5 dakika
    gcTime: 10 * 60 * 1000,     // 10 dakika
    retry: false,
  });

  const isAllowed = data?.isAllowed ?? false;
  const actions: Record<string, boolean> = data?.actions ?? {};

  /**
   * Belirtilen aksiyon için izin var mı?
   * Aksiyon kaydı hiç yoksa (boş dict) → varsayılan olarak true döner
   * (geriye dönük uyumluluk — backend ile aynı mantık).
   */
  function can(actionCode: string): boolean {
    if (isLoading) return false;
    if (Object.keys(actions).length === 0) return isAllowed;
    return actions[actionCode.toUpperCase()] === true;
  }

  return { isAllowed, can, isLoading, raw: data };
}
