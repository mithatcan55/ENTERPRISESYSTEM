# Authorization Policy Matrix

Bu doküman, API endpoint'lerinin hangi kimlik doğrulama/yetkilendirme kuralı ile korunduğunu tek yerden gösterir.

## Kurallar

- Varsayılan: Session bearer authentication zorunludur.
- T-Code tabanlı enforcement: `[TCodeAuthorize("...")]`
- Role tabanlı enforcement: `[Authorize(Roles = "...")]`
- Anonymous istisnası: sadece login endpoint'i.

## Endpoint Matrisi

| Endpoint | HTTP | Koruma | Zorunlu Kural |
|---|---|---|---|
| `/api/auth/login` | POST | AllowAnonymous + `auth-strict` rate limit | Yok (anonymous) |
| `/api/auth/change-password` | POST | Authorize | Authenticated user |
| `/api/users` | GET | Authorize + T-Code | `SYS03` |
| `/api/users` | POST | Authorize + T-Code | `SYS01` |
| `/api/roles` | GET | Authorize(Roles) | `SYS_ADMIN` |
| `/api/roles` | POST | Authorize(Roles) | `SYS_ADMIN` |
| `/api/roles/{roleId}/assign/{userId}` | POST | Authorize(Roles) | `SYS_ADMIN` |
| `/api/roles/users/{userId}` | GET | Authorize(Roles) | `SYS_ADMIN` |
| `/api/permissions/actions` | GET | Authorize(Roles) | `SYS_ADMIN` |
| `/api/permissions/actions` | POST | Authorize(Roles) | `SYS_ADMIN` |
| `/api/sessions` | GET | Authorize | Authenticated user |
| `/api/sessions/{sessionId}/revoke` | POST | Authorize | Authenticated user |
| `/api/tcode/{transactionCode}` | GET | Authorize | Authenticated user |
| `/api/ops/logs/system` | GET | Authorize(Roles) | `SYS_ADMIN` or `SYS_OPERATOR` |
| `/api/ops/logs/security` | GET | Authorize(Roles) | `SYS_ADMIN` or `SYS_OPERATOR` |
| `/api/ops/logs/http` | GET | Authorize(Roles) | `SYS_ADMIN` or `SYS_OPERATOR` |
| `/api/ops/logs/entity-changes` | GET | Authorize(Roles) | `SYS_ADMIN` or `SYS_OPERATOR` |
| `/api/ops/logs/entity-changes/export` | GET | Authorize(Roles) | `SYS_ADMIN` or `SYS_OPERATOR` |
| `/api/ops/logs/sessions` | GET | Authorize(Roles) | `SYS_ADMIN` or `SYS_OPERATOR` |
| `/api/ops/security/password-policy` | GET | Authorize(Roles) | `SYS_ADMIN` |
| `/api/ops/security/password-policy` | PUT | Authorize(Roles) | `SYS_ADMIN` |
| `/api/integrations/reference/company/{externalId}` | GET | Authorize | Authenticated user |

## Operasyon Notları

- Yeni endpoint açılırken bu matrise satır eklenmesi zorunludur.
- Endpoint koruması değişirse aynı PR içinde bu doküman güncellenmelidir.
- Yönetim endpoint'lerinde öncelik role bazlı, iş akışı endpoint'lerinde öncelik T-Code bazlıdır.

## CI Guard Kuralları

- Reflection tabanlı CI guard testi, tüm controller action'larını tarar.
- `AllowAnonymous` sadece allow-list endpoint'lerde kabul edilir.
- Tüm endpoint'lerde `[Authorize]` zorunludur (anonymous allow-list hariç).
- Yazma işlemlerinde (`POST/PUT/PATCH/DELETE`) ek enforcement gerekir:
	- `[Authorize(Roles = ...)]` veya `[TCodeAuthorize("...")]` veya `[PermissionAuthorize("...")]`
	- İstisna: self-service allow-list endpoint'ler.

Self-service allow-list:

- `POST /api/auth/change-password`
- `POST /api/sessions/{sessionId:int}/revoke`

Not:

- Self-service endpoint'lerde zorlayıcı yetki kontrolü servis katmanında uygulanır.
	- Şifre değişimi: sadece kendi hesabı
	- Session revoke: sadece kendi session'ı (SYS_ADMIN/SYS_OPERATOR istisnası)
- Bu kurallar CI'da `.github/workflows/ci.yml` üzerinden otomatik doğrulanır.
