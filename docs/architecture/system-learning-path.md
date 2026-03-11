# System Learning Path

Bu dokuman projeyi ilk kez okuyacak veya uzun aradan sonra geri donecek biri icin "hangi dosyayi hangi sirayla okumaliyim?" sorusuna cevap verir.

Amac:

- baglam kaybini azaltmak
- ayni konulari daginik okumayi onlemek
- egitim dokumanlari ile kod okumayi birlikte planlamak

## 1. Hizli Baslangic Yolu

Eger 20-30 dakikada genel resmi almak istiyorsan su sirayla ilerle:

1. `project-map.md`
2. `project-book.md`
3. `docs/architecture/README.md`
4. `docs/architecture/current-state-target-state-guide.md`

## 2. Mimari Omurga Yolu

Sonra ortak omurgayi oku:

1. `docs/architecture/command-query-pipeline-training.md`
2. `src/BuildingBlocks/Infrastructure/Pipeline/RequestExecutionPipeline.cs`
3. `src/BuildingBlocks/Infrastructure/Observability/OperationalEventPublisher.cs`
4. `src/Host.Api/Exceptions/GlobalExceptionHandler.cs`
5. `src/Host.Api/Localization/ApiTextLocalizer.cs`

## 3. Modul Bazli Okuma Sirasi

### Identity

1. `docs/architecture/identity-module-training.md`
2. `src/Modules/Identity/Identity.Presentation/Controllers/AuthController.cs`
3. `src/Modules/Identity/Identity.Presentation/Controllers/UsersController.cs`
4. `src/Modules/Identity/Identity.Infrastructure/Services/AuthLifecycleService.cs`
5. `src/Modules/Identity/Identity.Infrastructure/Services/PasswordPolicyService.cs`

### Authorization

1. `docs/architecture/authorization-module-training.md`
2. `src/Modules/Authorization/Authorization.Presentation/Controllers/TCodeController.cs`
3. `src/Modules/Authorization/Authorization.Infrastructure/Security/TCodeAuthorizationHandler.cs`
4. `src/Modules/Authorization/Authorization.Infrastructure/Services/TCodeAuthorizationService.cs`
5. `src/Modules/Authorization/Authorization.Infrastructure/Services/PermissionAuthorizationService.cs`

### Operations

1. `docs/architecture/operations-module-training.md`
2. `src/Modules/Operations/Operations.Presentation/Controllers/OperationsLogsController.cs`
3. `src/Modules/Operations/Operations.Infrastructure/Services/OperationsLogQueryService.cs`
4. `src/Modules/Operations/Operations.Infrastructure/Services/AuditDashboardService.cs`

### Integrations

1. `docs/architecture/integrations-module-training.md`
2. `src/Modules/Integrations/Integrations.Presentation/Controllers/OutboxController.cs`
3. `src/Modules/Integrations/Integrations.Infrastructure/Services/ExternalOutboxService.cs`
4. `src/Modules/Integrations/Integrations.Infrastructure/Services/ExternalOutboxDispatcherService.cs`

## 4. Konu Bazli Okuma

### Guvenlik

- `docs/architecture/authorization-module-training.md`
- `docs/authorization-policy-matrix.md`
- `src/Modules/Authorization/...`
- `src/Modules/Identity/Identity.Infrastructure/*/PreChecks/...`

### Loglama ve Notification

- `docs/architecture/error-policy-and-notification-training.md`
- `src/BuildingBlocks/Infrastructure/Observability/...`
- `src/Host.Api/Middleware/RequestLifecycleLoggingMiddleware.cs`

### Dil ve Hata Mesajlari

- `docs/architecture/localization-training.md`
- `src/Host.Api/Localization/...`
- `src/Host.Api/Exceptions/GlobalExceptionHandler.cs`

### Canli Endpoint Ogrenimi

- `docs/architecture/manual-api-learning-guide.md`
- `src/Host.Api/Host.Api.http`

## 5. Kod Okurken Sorulacak Sorular

1. Bu sinifin tek sorumlulugu ne?
2. Bu sinif neden bu katmanda?
3. Bu sinif pipeline / log / notification ile nasil baglaniyor?
4. Bu davranis tekrar ediyor mu?
5. Bu karar guvenlik veya denetim tarafini etkiliyor mu?

## 6. Uygulamali Ogrenme Plani

Gun 1:

- genel haritalar
- current state / target state
- pipeline ve observability

Gun 2:

- Identity modulu
- auth ve password policy
- users / roles / permissions akislari

Gun 3:

- Authorization modulu
- T-Code 6 seviye modeli
- permission ve pre-check mantigi

Gun 4:

- Operations ve Integrations
- outbox, dashboard, log query
- `Host.Api.http` ile manuel denemeler

## 7. Sonuc

Bu proje tek dosya okuyarak anlasilacak kadar kucuk degil.
Ama dogru sirayla okunursa baglam kaybi olmadan ogrenilebilir.

Bu dosya o dogru sirayi vermek icin vardir.
