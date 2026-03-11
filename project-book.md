# EnterpriseSystem Ana Proje Kitabi

Bu dosya projenin ana teknik rehberidir.

Amaci:

- bugunku gercek mimariyi kayda gecirmek
- neden bu yapiya geldigimizi aciklamak
- yeni gelisen bir kisinin sistemi tek basina okuyup ilerleyebilmesini saglamak
- moduller, katmanlar, guvenlik, loglama ve egitim dokumanlarini tek merkezden baglamak

Kural:

- mimariye dokunan her buyuk degisiklikte bu dosya guncellenir
- bu dosya guncellenmeden mimari is "tamam" sayilmaz

---

## 1. Cekirdek Karar Ozetleri

### 1.1 Mimari Karar

Proje bugun su kararlarla ilerler:

- teknoloji: `.NET 9`, `ASP.NET Core`, `EF Core`, `PostgreSQL`
- dagitim modeli: `modular monolith`
- uygulama stili: `service + CQRS`
- host rolu: sadece startup, middleware, auth host config, module composition
- cross-cutting alanlar: `BuildingBlocks`

### 1.2 Bilincli Olarak Terk Edilenler

Bu proje icin bilincli olarak terk edilenler:

- vertical slice agirlikli dosya organizasyonu
- host icinde daginik business service tutma
- her serviste manuel log yazma aliskanligi
- tek `BusinessDbContext` icinde tum bounded context'leri eritme yaklasimi

### 1.3 Bilincli Olarak Benimsenenler

- moduller: `Identity`, `Authorization`, `Operations`, `Integrations`
- her modulde `Application / Infrastructure / Presentation`
- command/query pipeline
- ikinci savunma hatti olarak request pre-check'leri
- event-driven observability + notification backbone
- Turkce egitim dokumanlari ve kod ici aciklama standardi

---

## 2. Cozumun Bugunku Haritasi

### 2.1 Ana Klasorler

- `src/Host.Api`
- `src/BuildingBlocks/SharedKernel`
- `src/BuildingBlocks/Application`
- `src/BuildingBlocks/Infrastructure`
- `src/Modules/Identity`
- `src/Modules/Authorization`
- `src/Modules/Operations`
- `src/Modules/Integrations`
- `tests`
- `docs/architecture`

### 2.2 Host.Api'nin Bugunku Gercek Rolü

`Host.Api` artik business mantik tasimak icin degil, sistemi birlestirmek icin vardir.

Gorevleri:

- `Program.cs`
- middleware zinciri
- authentication / authorization host configuration
- exception handling registration
- MVC assembly discovery
- module registration

`Host.Api` icinde kalmasi kabul edilmeyen seyler:

- modül business service'leri
- modül controller'lari
- yetki karar motoru
- operasyon sorgu servisleri
- integration orchestration servisleri

### 2.3 BuildingBlocks Katmanlari

#### SharedKernel

Ortak domain ve audit tabani:

- entity tabani
- domain event kontratlari
- audit / soft-delete kontratlari

#### Application

Ortak uygulama sozlesmeleri:

- exception tipleri
- security kontratlari
- observability event kontratlari
- pipeline arayuzleri

#### Infrastructure

Ortak teknik omurga:

- `DbContext` altyapisi
- interceptor'lar
- merkezi log event writer
- operational event publisher
- notification routing
- request execution pipeline implementasyonu

---

## 3. Moduller

### 3.1 Identity

Sorumluluklar:

- auth lifecycle
- user CRUD
- role CRUD
- role assignment
- permission management
- password policy
- session lifecycle

Detayli egitim rehberi:

- `docs/architecture/identity-module-training.md`

### 3.2 Authorization

Sorumluluklar:

- T-Code evaluation
- permission authorization
- action/condition enforcement
- security policy bridge

Detayli egitim rehberi:

- `docs/architecture/authorization-module-training.md`

### 3.3 Operations

Sorumluluklar:

- log query servisleri
- audit dashboard
- export endpoint'leri
- session admin sorgulari

Detayli egitim rehberi:

- `docs/architecture/operations-module-training.md`

### 3.4 Integrations

Sorumluluklar:

- outbox
- dispatcher
- email/excel entegrasyon akislari
- external gateway
- notification channel entegrasyonlari

Detayli egitim rehberi:

- `docs/architecture/integrations-module-training.md`

---

## 4. Service + CQRS Modeli Bu Projede Nasil Kullaniliyor

Bu projede CQRS bir slogan olarak degil, sorumluluk ayirici arac olarak kullaniliyor.

### 4.1 Temel Kural

- basit ve tek amacli operasyonlar: ayri command/query handler
- tekrar eden ortak davranislar: pipeline
- teknik altyapi: BuildingBlocks

### 4.2 Neden Boylesi Secildi

Eskiden buyuk servisler birden fazla sorumluluk tasiyordu:

- list
- create
- update
- deactivate
- reactivate
- delete

Bu sorunlari doguruyordu:

- servis sisligi
- test zorlugu
- bir degisikligin baska akislari etkilemesi

Su anki ayrim:

- `Users` akisi handler bazli
- `Roles` akisi handler bazli
- `Permissions` akisi handler bazli

Ama yine de bu bir `Vertical Slice` degil.

Sebep:

- modul siniri korunuyor
- feature feature daginik mini proje yok
- modul icinde command/query ayrimi var

### 4.3 Pipeline'in Rolü

Ortak `IRequestExecutionPipeline` su davranislari tek yerde toplar:

- validation
- pre-check
- event uretimi
- standart hata akislarini koruma

Detay:

- `docs/architecture/command-query-pipeline-training.md`

---

## 5. Guvenlik Omurgasi

### 5.1 Kimlik Dogrulama

Sistem session-aware auth kullanir.

Ana noktalar:

- token valid mi?
- session aktif mi?
- user aktif mi?
- gerekli claim'ler var mi?

### 5.2 Yetkilendirme

Iki ayri model beraber yasar:

- role/policy bazli
- T-Code / permission bazli

### 5.3 Ikinci Savunma Hatti

Controller attribute tek basina yeterli kabul edilmez.

Bu nedenle request modelleri:

- `ITCodeProtectedRequest`
- `IPermissionProtectedRequest`

arayuzlerini uygulayabilir.

Pipeline'da calisan pre-check'ler:

- `TCodeProtectedRequestPreCheck`
- `PermissionProtectedRequestPreCheck`

Bu sayede ayni command ileride farkli bir transport katmanindan cagrilsa bile guvenlik davranisi korunur.

### 5.4 T-Code 6 Seviye Modeli

1. module
2. submodule
3. page
4. company scope
5. action
6. condition

Bu model bugun deny seviyesini ve nedenini acik sekilde raporlayacak olgunluga getirilmis durumda.

Action mantigi:

- action kaydi yoksa geriye donuk uyumluluk korunur
- action kaydi varsa istenen action acikca izinli olmalidir

Condition mantigi:

- request context ile veritabanindaki kosullar karsilastirilir
- tatmin olmayan kosullar deny uretebilir

---

## 6. Veritabani Tasarimi

### 6.1 Eski Problem

Eskiden tek `BusinessDbContext` cok fazla domain tasiyordu.

Bu bounded context sinirlarini bozuyordu.

### 6.2 Bugunku Durum

Context ayrisma yonu acildi:

- `IdentityDbContext`
- `AuthorizationDbContext`
- `IntegrationsDbContext`
- `LogDbContext`

Bu ayrim simdiden moduller arasi sahipligi netlestiriyor.

### 6.3 Genel Sema

#### Identity tarafi

- `Users`
- `Roles`
- `UserRoles`
- `UserSessions`
- `UserPasswordHistories`
- `UserRefreshTokens`

#### Authorization tarafi

- `Modules`
- `SubModules`
- `SubModulePages`
- `UserModulePermissions`
- `UserSubModulePermissions`
- `UserPagePermissions`
- `UserCompanyPermissions`
- `UserPageActionPermissions`
- `UserPageConditionPermissions`

#### Integrations tarafi

- `ExternalOutboxMessages`

#### Log tarafi

- `database_query_logs`
- `entity_change_logs`
- `http_request_logs`
- `page_visit_logs`
- `performance_logs`
- `security_event_logs`
- `system_logs`

---

## 7. Loglama, Hata ve Notification Omurgasi

### 7.1 Amaç

Hedef su:

- geliştirici her serviste "su logu da yazayim" diye dusunmesin
- hata, log ve notification ayni olay omurgasindan aksin

### 7.2 Bugunku Yapı

Eklenen omurga:

- `OperationalEvent`
- `IOperationalEventPublisher`
- `INotificationChannel`
- routing options
- merkezi log writer
- request log middleware
- entity change interceptor
- operation logging filter

### 7.3 Neler Otomatik

- HTTP request log
- performance log
- page visit log
- entity change log
- SQL query log
- command/query basari ve hata eventleri
- global exception eventleri

### 7.4 Notification Altyapisi

Notification kanallari omurgasi acildi.

Su an:

- email
- webhook

kanallari mevcut.

Bu tasarim yeni kanallara aciktir:

- teams
- slack
- sms
- in-app

### 7.5 Hassas Veri Koruma

Request log tarafinda merkezi redaction vardir.

Maskelenen tipik alanlar:

- `Authorization`
- `Cookie`
- `Set-Cookie`
- `password`
- `refreshToken`
- `token`
- `secret`

Amaç:

- denetim izi olsun
- ama hassas veri sizmasin

---

## 8. Localization

Localization hedef dilleri:

- `tr-TR`
- `en-US`
- `de-DE`

Bugunku uygulama:

- JSON resource tabanli text localizer

Temel hedef:

- API mesajlarini standardize etmek
- error code -> localized text akisini desteklemek
- frontend ile ortak anahtar diline gecmek

---

## 9. Egitim ve Dokumantasyon Sistemi

Bu proje sadece kodu degil, kodun anlatimini da ciddiye alir.

### 9.1 Katmanlar

#### Ust Seviye Haritalar

- `project-map.md`
- `project-book.md`

#### Mimari Set

- `docs/architecture/README.md`
- `docs/architecture/current-state-target-state-guide.md`
- diyagram dosyalari

#### Modul Bazli Rehberler

- `identity-module-training.md`
- `authorization-module-training.md`
- `operations-module-training.md`
- `integrations-module-training.md`

#### Ozel Konu Rehberleri

- `command-query-pipeline-training.md`

### 9.2 Kod Ici Aciklama Kuralı

Her satir yorumlanmaz.

Yorum eklenecek yerler:

- kritik karar noktasi
- akisin neden o sirayla kuruldugu
- ikinci savunma hatti gibi niyet aciklayan alanlar
- transaction ve retry gibi yan etki riski tasiyan kisimlar

Amaç:

- kodu gürültüyle doldurmamak
- kritik karar mantigini gizlememek

---

## 10. Runtime Akislari

### 10.1 Users.Create

```text
UsersController
  -> CreateUserCommand
  -> RequestExecutionPipeline
    -> validator
    -> TCode pre-check
    -> CreateUserCommandHandler
      -> Identity / Authorization tablolari
    -> OperationalEventPublisher
```

### 10.2 T-Code Yetki Karari

```text
TCodeAuthorizationHandler
  -> route/query/action bilgisi toplar
  -> TCodeAuthorizationService
    -> page/submodule/module cozer
    -> level 1-6 kontrol eder
    -> deny reason ve level uretir
    -> security event publish eder
```

### 10.3 Outbox

```text
OutboxController
  -> ExternalOutboxService
    -> ExternalOutboxMessage kaydi
  -> Background Dispatcher
    -> dis servis
    -> retry / dead-letter
```

### 10.4 Operations Dashboard

```text
Operations controller
  -> query service
    -> LogDb
    -> IdentityDb
  -> paged result / csv / dashboard summary
```

---

## 11. Guncel Teknik Kazanimlar

Bugune kadar tamamlanan ana kazanimlar:

- host icinden modullere tasima
- identity presentation gerceklestirme
- authorization module extraction
- operations module extraction
- integrations module extraction
- context ayrisma yonu
- action + condition enforcement
- merkezi observability backbone
- notification backbone
- common request execution pipeline
- users / roles / permissions CQRS ayrimi
- request pre-check ikinci savunma hatti
- Turkce egitim dokumanlari ve kod ici aciklamalar

---

## 12. Halen Bilincli Olarak Acik Birakilan Noktalar

Bu proje olgunlasti ama bitmedi.

Acik noktalar:

- daha fazla request validator standardizasyonu
- daha fazla pre-check kullanimi
- project-book ve project-map'in soru-cevap / diyagram seviyesini daha da buyutmek
- localization ile exception mesajlari arasindaki tam bag
- T-Code condition tarafinda filter/deny ayriminin daha da zenginlestirilmesi
- outbox operasyonlari icin daha ileri notification policy'leri

---

## 13. Bir Sonraki Mantikli Adimlar

1. `project-map.md` dosyasini yeni yapinin kisa ama gorsel ust seviye haritasina donusturmek
2. localization egitim rehberi cikarmak
3. error policy engine davranisini daha acik dokumante etmek
4. yeni gelecek moduller icin standart onboarding sablonu cikarmak

---

## 14. Operasyonel Komutlar

### Build

- `dotnet build`

### Git Akisi

Bu ortamda pratik karar:

1. kod degisikligi
2. `dotnet build`
3. `git add .`
4. `git commit -m "..."`
5. `git push`

### Migration Yonu

Context bazli migration komutlari ilgili context'e gore calistirilir.
Yeni context ayrisimi nedeniyle her migration'in sahibi modül/context bazli dusunulmelidir.

---

## 15. Son Soz

Bu kitap tek basina "dosya listesi" olmak icin yazilmadi.
Amaci su:

- sistemi bugunku haliyle dogru anlatmak
- yarin buyurken ayni dili korumak
- yeni giren gelistiricinin projeyi ayakta tutabilecek kadar baglam kazanmasini saglamak

Bu dosya canlidir.
Kod degistikce bu kitap da degisir.
