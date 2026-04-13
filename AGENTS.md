# AGENTS

Bu dosya, Codex'in EnterpriseSystem reposunda nasil calisacagini belirleyen kalici kilavuzdur.

## 1) Proje Amaci
- Kurumsal admin/ERP benzeri sistem.
- Guvenlik, UX'ten once gelir.
- Backend source of truth'tur.

## 2) Mimari Kurallar
- User = Role + DirectPermission.
- Permission yetkidir.
- T-Code navigasyon/kisayoldur, yetki degildir.
- Session DB kontrolludur.
- Refresh token DB'de tutulur.
- Her request'te session dogrulamasi yapilir.
- MustChangePassword hem backend hem frontend'de enforce edilir.
- Kritik olaylarda tum session'lar revoke edilir.
- CRUD ayri sayfa degil, feature + mode yaklasimi ile ele alinir.

## 3) Session Politikasi
- Session DB'de tutulur.
- Revoke reason tutulur.
- Current / Selected / All / AllExceptCurrent desteklenir.
- Password change -> ALL revoke.
- Role/permission kritik degisim -> ALL revoke (opsiyonel, onerilir).

## 4) Calisma Kurallari
- Branch acma; mevcut branch uzerinde ilerle.
- Once analiz yap, sonra kod yaz.
- Minimal degisiklik yap.
- Mevcut mimariyi bozma.
- Her gorev sonunda commit olustur.
- Conventional Commit kullan.
- Push oncesi degisiklikleri ozetle.
- Secret/config risklerini kontrol et.

## 5) Kodlama Kurallari
- Acik isimlendirme kullan.
- Defensive coding uygula.
- Mevcut stil ile uyumlu kal.
- Gereksiz refactor yapma.
- Gerekirse TODO birak; kisa ve aciklamali yaz.

## 6) Done Kriteri
- Kod derlenebilir olmali.
- Ilgili akis bozulmamis olmali.
- Degisen dosyalar listelenmis olmali.
- Kisa test/dogrulama adimlari yazilmis olmali.

## 7) Commit Standardi
- `feat(scope): ...`
- `fix(scope): ...`
- `refactor(scope): ...`
