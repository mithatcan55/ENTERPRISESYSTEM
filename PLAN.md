# EnterpriseSystem - Sifirdan Yeniden Tasarim Yol Haritasi

## Alinan Kararlar

| Karar | Secim |
|-------|-------|
| **Konum** | `C:\Users\mithat.can\Desktop\Project\EnterpriseSystem` (mevcut uzerine) |
| **Framework** | .NET 9 |
| **Mimari** | Hibrit: CQRS (karmasik islemler) + Service-based (basit CRUD) |
| **Frontend** | React + TypeScript + Vite + Ant Design 5.x + Ant Design Charts |
| **Veritabani** | PostgreSQL (2 ayri DB: Business + Log) |
| **API Docs** | Scalar (OpenAPI) |
| **Kod Tasima** | Auth (6 seviye yetkilendirme + JWT + Session) + Log (7 tablo + interceptor) |
| **DDD** | YOK - domain event, aggregate root jargonu kullanilmayacak |
| **Egitim Modu** | Her adim aciklamali, kullanici kodu yazar, asistan rehberlik eder |

---

## Proje Yapisi (Hedef)

```
EnterpriseSystem/
├── src/
│   ├── Host.Api/                              # Web API giris noktasi
│   │   ├── Controllers/                       # API controller'lari
│   │   ├── Middleware/                         # CorrelationId, RequestLifecycle, ExceptionHandler
│   │   ├── Security/                          # JWT servisi, CurrentUserContext, AuthHandlers
│   │   ├── Extensions/                        # ServiceCollection extension'lari
│   │   ├── Program.cs                         # Pipeline kurulumu
│   │   ├── appsettings.json
│   │   └── Host.Api.csproj
│   │
│   ├── BuildingBlocks/
│   │   ├── SharedKernel/                      # Base entity'ler, interface'ler
│   │   │   ├── Entity.cs
│   │   │   ├── AuditableEntity.cs
│   │   │   ├── IAuditableEntity.cs
│   │   │   ├── ISoftDeletable.cs
│   │   │   └── IDomainEvent.cs
│   │   │
│   │   ├── Application/                       # Uygulama katmani ortakliklari
│   │   │   ├── Exceptions/
│   │   │   └── Application.csproj
│   │   │
│   │   └── Infrastructure/                    # Altyapi ortakliklari
│   │       ├── Persistence/
│   │       │   ├── BusinessDbContext.cs
│   │       │   ├── LogDbContext.cs
│   │       │   └── IAuditActorAccessor.cs
│   │       ├── Interceptors/
│   │       │   └── DatabaseCommandLoggingInterceptor.cs
│   │       ├── Extensions/
│   │       │   └── InfrastructureServiceCollectionExtensions.cs
│   │       └── Infrastructure.csproj
│   │
│   ├── Modules/
│   │   ├── Identity/                          # Kimlik dogrulama modulu (TASIMA)
│   │   │   ├── Identity.Application/
│   │   │   ├── Identity.Domain/
│   │   │   ├── Identity.Infrastructure/
│   │   │   └── Identity.Presentation/
│   │   │
│   │   ├── Authorization/                     # Dinamik yetkilendirme modulu (YENI)
│   │   │   ├── Authorization.Application/
│   │   │   │   ├── Contracts/
│   │   │   │   ├── Commands/                  # CQRS - karmasik islemler
│   │   │   │   ├── Queries/                   # CQRS - karmasik sorgular
│   │   │   │   └── Services/                  # Service-based - basit CRUD
│   │   │   ├── Authorization.Domain/
│   │   │   ├── Authorization.Infrastructure/
│   │   │   └── Authorization.Presentation/
│   │   │
│   │   ├── Approval/                          # Dinamik onay/workflow modulu (YENI)
│   │   │   ├── Approval.Application/
│   │   │   ├── Approval.Domain/
│   │   │   ├── Approval.Infrastructure/
│   │   │   └── Approval.Presentation/
│   │   │
│   │   ├── Notification/                      # Bildirim modulu (YENI)
│   │   │   ├── Notification.Application/
│   │   │   ├── Notification.Domain/
│   │   │   ├── Notification.Infrastructure/
│   │   │   └── Notification.Presentation/
│   │   │
│   │   ├── FileManagement/                    # Dosya yonetimi modulu (YENI)
│   │   │   ├── FileManagement.Application/
│   │   │   ├── FileManagement.Domain/
│   │   │   ├── FileManagement.Infrastructure/
│   │   │   └── FileManagement.Presentation/
│   │   │
│   │   ├── Reporting/                         # Raporlama + Excel/PDF export (YENI)
│   │   │   ├── Reporting.Application/
│   │   │   ├── Reporting.Domain/
│   │   │   ├── Reporting.Infrastructure/
│   │   │   └── Reporting.Presentation/
│   │   │
│   │   ├── Localization/                      # Coklu dil modulu (YENI)
│   │   │   ├── Localization.Application/
│   │   │   ├── Localization.Domain/
│   │   │   ├── Localization.Infrastructure/
│   │   │   └── Localization.Presentation/
│   │   │
│   │   └── [Gelecek Is Modulleri: TeknikServis, Cari, Depo...]
│   │
│   └── frontend/                              # React uygulamasi
│       ├── public/
│       ├── src/
│       │   ├── api/                           # Axios client, endpoint tanimlari
│       │   ├── components/                    # Paylasilan componentler
│       │   │   ├── StandardTable/             # Sort+Filter+Pagination kalip tablo
│       │   │   ├── StandardForm/              # Kalip form
│       │   │   ├── PageHeader/
│       │   │   ├── KpiCard/
│       │   │   └── ApprovalStatus/            # Onay durumu gosterge componenti
│       │   ├── contexts/                      # AuthContext, PermissionContext, LocaleContext
│       │   ├── hooks/                         # usePermission, useMenu, useFetch, useLocale
│       │   ├── layouts/                       # MainLayout (sidebar + header + content)
│       │   ├── guards/                        # RouteGuard, ButtonGuard, ColumnGuard
│       │   ├── locales/                       # Dil dosyalari (tr-TR, en-US, de-DE)
│       │   ├── pages/
│       │   │   ├── Login/
│       │   │   ├── Dashboard/                 # KPI + Chart dolu
│       │   │   ├── Admin/
│       │   │   │   ├── Modules/
│       │   │   │   ├── Users/
│       │   │   │   ├── Roles/
│       │   │   │   ├── Permissions/           # Yetki matrix ekrani
│       │   │   │   ├── Conditions/
│       │   │   │   ├── ApprovalFlows/         # Onay akisi yonetimi
│       │   │   │   ├── Languages/             # Dil yonetimi
│       │   │   │   └── Notifications/         # Bildirim ayarlari
│       │   │   ├── Logs/
│       │   │   ├── Files/                     # Dosya yonetimi
│       │   │   └── Reports/                   # Raporlama
│       │   ├── theme/
│       │   ├── types/
│       │   └── utils/
│       ├── package.json
│       ├── vite.config.ts
│       └── tsconfig.json
│
├── tests/
│   ├── UnitTests/
│   └── IntegrationTests/
│
├── docs/                                      # Egitim dokumantasyonu
│   ├── 00-mimari-genel-bakis.md
│   ├── 01-proje-kurulumu.md
│   ├── 02-shared-kernel.md
│   ├── 03-veritabani-tasarimi.md
│   ├── 04-auth-sistemi.md
│   ├── 05-yetkilendirme-sistemi.md
│   ├── 06-loglama-sistemi.md
│   ├── 07-onay-sistemi.md
│   ├── 08-bildirim-sistemi.md
│   ├── 09-dosya-yonetimi.md
│   ├── 10-raporlama-sistemi.md
│   ├── 11-coklu-dil-sistemi.md
│   ├── 12-frontend-kurulumu.md
│   ├── 13-dinamik-menu-sistemi.md
│   ├── 14-yetki-matrix-ekrani.md
│   ├── 15-dashboard-tasarimi.md
│   └── 16-yeni-modul-ekleme-rehberi.md
│
├── EnterpriseSystem.sln
├── Directory.Packages.props
├── global.json
└── .gitignore
```

---

## Faz 0: Proje Iskeleti + Kod Tasima

### Adim 0.1: Mevcut klasoru yedekle ve temizle
- Mevcut EnterpriseSystem klasorunun yedegini al
- Sifirdan solution olustur

### Adim 0.2: Solution + Proje yapisi
- `EnterpriseSystem.sln` olustur
- Tum `.csproj` dosyalarini olustur (17 proje)
- `Directory.Packages.props` (merkezi paket yonetimi)
- `global.json` (.NET 9)
- `.gitignore`
- Proje referanslarini bagla
- `dotnet build` ile dogrula

### Adim 0.3: SharedKernel tasima
- `Entity.cs`, `AuditableEntity.cs`, `IAuditableEntity.cs`, `ISoftDeletable.cs`, `IDomainEvent.cs`

### Adim 0.4: Application katmani tasima
- `AppException.cs`, `ValidationAppException.cs`, `NotFoundAppException.cs`, `ForbiddenAppException.cs`

### Adim 0.5: Infrastructure tasima
- `BusinessDbContext.cs`, `LogDbContext.cs` (7 log tablosu, immutable)
- `DatabaseCommandLoggingInterceptor.cs`, `IAuditActorAccessor.cs`
- DI extension'lari

### Adim 0.6: Identity modulu tasima
- Domain: User, Role, UserRole, UserSession, UserPasswordHistory, UserRefreshToken
- Application: Servis interface'leri + DTO'lar
- Infrastructure: Servis implementasyonlari
- Presentation: DI kaydi

### Adim 0.7: Host.Api tasima
- Program.cs pipeline
- Middleware: CorrelationId, RequestLifecycleLogging, GlobalExceptionHandler
- Security: JWT, CurrentUserContext, TCodeAuthorizationService
- Controllers: Auth, Users, Roles, Sessions, PasswordPolicy
- Extensions: HostServiceCollectionExtensions

### Adim 0.8: Build + Veritabani + Test
- `dotnet build` basarili olmali
- Migration olustur + DB update
- Scalar'dan login test

---

## Faz 1: Dinamik Yetkilendirme Modulu (YENI KOD)

### Adim 1.1: Authorization Domain entity'leri
- `Module.cs` - Ana modul (Id, Kod, Ikon, SiraNo, IsActive)
- `SubModule.cs` - Alt modul (Id, ModuleId, Kod, Ikon, SiraNo, IsActive)
- `SubModulePage.cs` - Sayfa (Id, SubModuleId, Kod, RouterLink, TransactionCode, SiraNo, IsActive)
- 6 seviye yetki entity'leri (modul, altmodul, sayfa, sirket, buton, kosul)

### Adim 1.2: Basit CRUD servisleri (Service-based)
- Modul CRUD, Alt modul CRUD, Sayfa CRUD

### Adim 1.3: Karmasik islemler (CQRS)
- Yetki atama command'leri (modul, altmodul, sayfa, buton, kosul, toplu)

### Adim 1.4: Karmasik sorgular (CQRS)
- GetPermissionMatrix, GetDynamicMenuTree, GetEffectivePermissions, GetPageAccessDetail

### Adim 1.5: Controller'lar + Seed Data

---

## Faz 2: Coklu Dil (i18n) Modulu (YENI KOD)

### Adim 2.1: Localization Domain entity'leri
- `Language.cs` - Desteklenen diller (Id, Code, Name, NativeName, IsActive, IsDefault)
- `TranslationKey.cs` - Ceviri anahtarlari (Id, GroupCode, Key)
- `TranslationValue.cs` - Ceviri degerleri (Id, TranslationKeyId, LanguageId, Value)
- `ModuleTranslation.cs` - Modul ad cevirileri (ModuleId, LanguageId, Name, Description)
- `SubModuleTranslation.cs` - Alt modul ad cevirileri
- `PageTranslation.cs` - Sayfa ad cevirileri

### Adim 2.2: Servisler
- `ILanguageCrudService` - Dil CRUD
- `ITranslationService` - Ceviri CRUD + toplu import/export
- `ILocalizationProvider` - Aktif dile gore ceviri saglayan servis

### Adim 2.3: Controller'lar
- `LanguagesController` - Dil yonetimi API
- `TranslationsController` - Ceviri yonetimi API

### Adim 2.4: Seed Data
- Varsayilan diller: tr-TR, en-US, de-DE
- Sistem metinleri cevirileri

---

## Faz 3: Dinamik Onay (Approval) Modulu (YENI KOD)

### Adim 3.1: Approval Domain entity'leri
- `ApprovalFlowDefinition.cs` - Onay akisi tanimi
  - Id, ModuleCode, EntityType, Name, StepCount, IsActive
- `ApprovalFlowStep.cs` - Onay adimi
  - Id, FlowDefinitionId, StepOrder, ApproverType (Role/User/Department)
  - ApproverValue, Condition (opsiyonel: "Tutar > 50000"), TimeoutHours, CanReject
- `ApprovalRequest.cs` - Onay talebi
  - Id, FlowDefinitionId, EntityType, EntityId, CurrentStepOrder
  - Status (Draft/Pending/Approved/Rejected/Cancelled), CreatedBy, CreatedAt
- `ApprovalStepResult.cs` - Adim sonucu
  - Id, RequestId, StepOrder, ApproverId, Decision, Comment, DecisionAt

### Adim 3.2: Servisler (Hibrit)
- **CQRS:**
  - `SubmitForApprovalCommand` - Kaydi onaya gonder
  - `ApproveStepCommand` - Adimi onayla
  - `RejectStepCommand` - Adimi reddet
  - `GetPendingApprovalsQuery` - Bekleyen onaylarim
  - `GetApprovalHistoryQuery` - Onay gecmisi
- **Service-based:**
  - `IApprovalFlowCrudService` - Akis tanimi CRUD
  - `IApprovalFlowStepCrudService` - Adim CRUD

### Adim 3.3: Controller'lar
- `ApprovalFlowsController` - Akis tanimi yonetimi
- `ApprovalsController` - Onay islemleri (gonder, onayla, reddet, listele)

---

## Faz 4: Bildirim Modulu (YENI KOD)

### Adim 4.1: Notification Domain entity'leri
- `NotificationTemplate.cs` - Bildirim sablonu
  - Id, Code, Channel (InApp/Email/Both), Subject, Body, IsActive
- `Notification.cs` - Bildirim kaydi
  - Id, TemplateId, RecipientUserId, Title, Body, IsRead, ReadAt, CreatedAt
- `NotificationPreference.cs` - Kullanici tercihleri
  - Id, UserId, Channel, IsEnabled

### Adim 4.2: Servisler
- `INotificationDispatcher` - Bildirim gonderme (in-app + email)
- `INotificationCrudService` - Bildirim listele, okundu isaretle
- Otomatik tetikleyiciler: onay geldi, reddedildi, suresi doldu, yeni atama, sistem uyarisi
- **SignalR Hub** - Gercek zamanli bildirim push (bildirim zili aninda guncellenir)

### Adim 4.3: Controller'lar + Hub
- `NotificationsController` - Bildirimlerim, okundu isaretle, tercihleri guncelle
- `NotificationHub` (SignalR) - Canli bildirim push, okunmamis sayisi guncelleme

---

## Faz 5: Dosya Yonetimi Modulu (YENI KOD)

### Adim 5.1: FileManagement Domain entity'leri
- `FileAttachment.cs` - Dosya kaydi
  - Id, OriginalFileName, StoredFileName, ContentType, FileSize
  - EntityType, EntityId (hangi kayda bagli: "TeknikServisKayit", 42)
  - UploadedBy, UploadedAt, IsDeleted

### Adim 5.2: Servisler
- `IFileStorageService` - Dosya kaydet/oku/sil (disk veya cloud)
- `IFileAttachmentService` - Entity'ye dosya bagla/listele/sil

### Adim 5.3: Controller'lar
- `FilesController` - Upload, download, listele, sil

---

## Faz 6: Raporlama + Excel/PDF Export Modulu (YENI KOD)

### Adim 6.1: Reporting Domain entity'leri
- `ReportDefinition.cs` - Rapor tanimi
  - Id, ModuleCode, Name, SqlQuery, Parameters (JSON), IsActive
  - OutputFormats (Excel/PDF/Both), CreatedBy
- `ReportExecution.cs` - Rapor calisma gecmisi
  - Id, ReportDefinitionId, ExecutedBy, ExecutedAt, DurationMs, RowCount, Status

### Adim 6.2: Servisler
- `IReportDefinitionCrudService` - Rapor tanimi CRUD
- `IReportExecutionService` - Rapor calistirma
- `IExcelExportService` - Excel ciktisi olusturma
- `IPdfExportService` - PDF ciktisi olusturma

### Adim 6.3: Controller'lar
- `ReportsController` - Rapor tanimi CRUD + calistirma + export

---

## Faz 7: React Frontend Temeli

### Adim 7.1: Proje kurulumu
- Vite + React + TypeScript
- Ant Design 5.x + @ant-design/pro-components + @ant-design/charts
- Axios + React Router + react-i18next + dayjs
- Tema ozellestirme (renkler, fontlar)
- Proxy ayari (API'ye yonlendirme)

### Adim 7.2: Auth + Permission + Locale altyapisi
- `AuthContext` - Login, logout, token yonetimi
- `PermissionContext` - Kullanicinin yetkileri (API'den)
- `LocaleContext` - Dil secimi + ceviri saglayici
- `useAuth`, `usePermission`, `useLocale` hook'lari

### Adim 7.3: Layout sistemi
- `MainLayout` - Ant Design ProLayout
  - Dinamik sidebar (API'den gelen menu tree)
  - Header (kullanici, dil secici, bildirim ikonu, cikis)
  - Breadcrumb (otomatik)
  - Responsive (mobile drawer menu)

### Adim 7.4: Standart kalip componentler
- `StandardTable` - ProTable wrapper (sort + filter + pagination + export + search)
- `StandardForm` - ProForm wrapper (standart form layout)
- `StandardPage` - Sayfa sablonu (header + content + breadcrumb)
- `KpiCard` - Dashboard KPI karti
- `ChartCard` - Dashboard grafik karti
- `ApprovalStatus` - Onay durumu gostergesi
- `FileUploader` - Dosya yukleme componenti
- `NotificationBell` - Bildirim ikonu + dropdown listesi

### Adim 7.5: Guard'lar
- `RouteGuard` - Yetkisiz sayfaya erisimi engelle
- `ButtonGuard` - Yetkisiz butonu gizle/disable et
- `ColumnGuard` - Yetkisiz kolonu tabloda gizle
- `ConditionGuard` - Deger kisitlamasini uygula

### Adim 7.6: Login sayfasi
- Kullanici adi + sifre formu + dil secimi
- JWT token alma + yetki bilgisi cekme
- Dinamik yonlendirme (ilk erisilebilir sayfaya)

---

## Faz 8: Admin Panel Sayfalari

### Adim 8.1: Dashboard
- KPI kartlari (toplam kullanici, aktif oturum, hata orani, ortalama response suresi)
- Cizgi grafik: Son 7 gun istek sayisi
- Pasta grafik: Modul bazli kullanim dagilimi
- Bar grafik: En cok hata alan endpoint'ler
- Gauge grafik: Sistem sagligi skoru
- Tablo: Son 10 guvenlik olayi
- Tablo: Bekleyen onaylarim

### Adim 8.2: Modul Yonetimi
- Modul listesi (StandardTable ile)
- Modul ekle/duzenle formu (coklu dil destegi)
- Alt modul yonetimi (tree view)
- Sayfa yonetimi (her alt modulun altinda)
- Siralama (drag & drop)

### Adim 8.3: Kullanici Yonetimi
- Kullanici listesi (StandardTable)
- Kullanici ekle/duzenle formu
- Rol atama (multi-select)
- Sifre sifirlama + aktif/pasif toggle

### Adim 8.4: Rol Yonetimi
- Rol listesi + CRUD

### Adim 8.5: Yetki Matrix Ekrani (EN KRITIK EKRAN)
- Satir: Kullanicilar | Sutun: Moduller > Alt Moduller > Sayfalar
- Hucre: Checkbox (erisim var/yok)
- Alt panel: Buton yetkileri + Kosul yetkileri
- Toplu atama

### Adim 8.6: Kosul Yonetimi
- Alan + Operator + Deger (ornek: Tutar <= 10000)
- Kolon gorunurlugu + Onizleme

### Adim 8.7: Onay Akisi Yonetimi
- Akis tanimi listesi (modul bazli)
- Adim ekleme/cikarma (drag & drop siralama)
- Onaylayici atama (rol/kullanici/departman)
- Kosul ekleme (opsiyonel)

### Adim 8.8: Dil Yonetimi
- Dil listesi + aktif/pasif
- Ceviri editoru (key-value tablo, dile gore filtreleme)
- Toplu import/export (JSON veya Excel)

### Adim 8.9: Bildirim Ayarlari
- Bildirim sablon yonetimi
- Kullanici bildirim tercihleri

### Adim 8.10: Oturum Yonetimi
- Aktif oturumlar + sonlandirma + gecmis

---

## Faz 9: Loglama & Monitoring Paneli

### Adim 9.1: HTTP Request Log Viewer
- StandardTable + filtreler + detay gorunumu + CorrelationId takibi

### Adim 9.2: Entity Change Log Viewer
- OldValues vs NewValues diff gorunumu

### Adim 9.3: Security Event Log Viewer
- Guvenlik olaylari + severity filtreleme

### Adim 9.4: Performance Log Viewer
- Yavas sorgular + memory + response suresi

### Adim 9.5: Monitoring Dashboard
- Canli istatistikler + grafikler

---

## Faz 10: Raporlama Paneli

### Adim 10.1: Rapor Tanimi Yonetimi
- Rapor CRUD (admin panelden SQL + parametre tanimi)
- Parametre formu otomatik olusturma

### Adim 10.2: Rapor Calistirma
- Parametre gir + calistir + sonuclari tablo olarak gor
- Excel export + PDF export butonlari

---

## Faz 11: Is Modulu Sablonu + Ornek Modul

### Adim 11.1: Modul sablonu dokumantasyonu
- Yeni modul ekleme rehberi (adim adim)
- Backend: Domain > Application > Infrastructure > Presentation
- Frontend: pages/ altinda yeni klasor
- DB'de modul + alt modul + sayfa tanimi
- Onay akisi tanimi + yetki atamasi

### Adim 11.2: Ornek modul: Teknik Servis
- Backend: 4 katmanli yapi
- Frontend: Kayit listele, olustur, duzenle, sil sayfalari
- Onay akisi: 2 seviyeli (teknisyen sefi + mudur)
- Dosya ekleme: Fatura, fotograf
- Tam yetkilendirme + loglama entegrasyonu

---

## Teknoloji Listesi (Kesinlesmis)

### Backend (.NET 9)
| Paket | Amac |
|-------|------|
| Npgsql.EntityFrameworkCore.PostgreSQL | PostgreSQL ORM |
| Microsoft.AspNetCore.Authentication.JwtBearer | JWT auth |
| Scalar.AspNetCore | API dokumantasyonu |
| Serilog.AspNetCore + Sinks.File + Sinks.PostgreSQL | Structured logging |
| Mapster | DTO mapping |
| FluentValidation | Input validation |
| Scrutor | Auto DI registration |
| BCrypt.Net-Next | Sifre hashleme |
| Polly | Resilience (retry, circuit breaker) |
| MediatR | CQRS pipeline (karmasik islemler icin) |
| Microsoft.AspNetCore.SignalR | Gercek zamanli bildirim (bildirim zili) |
| ClosedXML | Excel export |
| QuestPDF | PDF export |
| MailKit | Email gonderme |

### Frontend (React)
| Paket | Amac |
|-------|------|
| React 19 + TypeScript | UI framework |
| Vite | Build tool |
| Ant Design 5.x | UI component kutuphanesi |
| @ant-design/pro-components | ProTable, ProForm, ProLayout |
| @ant-design/charts | Grafik/chart kutuphanesi |
| Axios | HTTP client |
| React Router 7 | Routing |
| react-i18next | Frontend coklu dil |
| dayjs | Tarih islemleri |
| @microsoft/signalr | SignalR client (gercek zamanli bildirim zili) |

### Veritabani (PostgreSQL)
| DB | Amac |
|----|------|
| enterprise_system_db | Is veritabani (yetkilendirme + identity + onay + bildirim + dosya + rapor + is modulleri) |
| enterprise_system_logdb | Log veritabani (immutable, 7 tablo) |

---

## Calisma Kurallari
1. Her adim egitim dokumantasyonu ile birlikte yapilir
2. Kullanici kodu yazar, asistan rehberlik eder
3. Asistan otomatik kod degisikligi YAPMAZ
4. Her fazin sonunda build + test
5. Her faz onaylanmadan bir sonrakine gecilmez
6. DDD jargonu KULLANILMAZ
7. Her dosyada aciklama satirlari bol yazilir (ne, nicin, neden)

---

## Ozet: 12 Faz, 11 Modul

| Faz | Icerik | Tip |
|-----|--------|-----|
| 0 | Proje iskeleti + Auth/Log tasima | TASIMA |
| 1 | Dinamik yetkilendirme modulu | YENI |
| 2 | Coklu dil (i18n) modulu | YENI |
| 3 | Dinamik onay (workflow) modulu | YENI |
| 4 | Bildirim modulu | YENI |
| 5 | Dosya yonetimi modulu | YENI |
| 6 | Raporlama + Excel/PDF export | YENI |
| 7 | React frontend temeli | YENI |
| 8 | Admin panel sayfalari | YENI |
| 9 | Loglama & monitoring paneli | YENI |
| 10 | Raporlama paneli | YENI |
| 11 | Is modulu sablonu + ornek modul | YENI |
