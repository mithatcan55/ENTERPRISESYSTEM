# EnterpriseSystem — Ana Proje Dosyası (Canlı Kitap)

Bu dosya projenin ana teknik kaynağıdır.

Amaç:
- Baştan bugüne yapılanları kaybetmemek
- Sınıf ilişkilerini görünür tutmak
- Neyi neden yaptığımızı açık bırakmak
- Yeni değişikliklerde aynı şablonla ilerlemek

Kural:
- Mimariye dokunan her değişiklikte bu dosya güncellenir.
- Bu dosya değişmeden mimari commit tamamlanmış sayılmaz.

---

## Cilt 1 — Mimari Çerçeve

### 1.1 Hedef Mimari
- Teknoloji: .NET 9, ASP.NET Core API, EF Core, PostgreSQL
- Mimari: Clean + Vertical Slice + Modular Monolith
- Loglama: Dosya + ayrı LogDb
- Yetki: Role + Permission + Condition + T-Code
- Denetim: Ara tablolar dahil kim/ne/zaman izi

### 1.2 Neden Modular Monolith
- Tek deploy kolaylığı
- Modül bazlı büyüme
- Mikroservise geçişte hazır ayrışma
- Test ve bakım maliyetini düşürme

### 1.3 Kırmızı Çizgiler
- Tablolar/kolonlar İngilizce
- Kod açıklamaları Türkçe
- Log tabloları immutable
- Yetki modeli 6 seviyeden aşağı düşmez

---

## Cilt 2 — Repo ve Katman Haritası

### 2.1 Dizinler
- src/Host.Api
- src/BuildingBlocks/SharedKernel
- src/BuildingBlocks/Application
- src/BuildingBlocks/Infrastructure
- src/Modules/Identity
- tests

### 2.2 Katman Görevleri

**Host.Api**
- Composition root
- Middleware zinciri
- OpenAPI + Scalar
- DI wiring

**SharedKernel**
- Ortak kontratlar (audit/soft delete)

**Infrastructure**
- DbContext'ler
- Entity mapping
- Migrations
- Audit actor abstraction

**Modules/Identity**
- Şu an iskelet
- User FK bağları için bir sonraki adım

### 2.3 Proje Referans Mantığı
- Host.Api -> Infrastructure + Module Infra/Presentation
- Infrastructure -> Application + SharedKernel
- Identity.Infrastructure -> Identity.Application + Identity.Domain + Infrastructure

---

## Cilt 3 — Baştan Bugüne Teknik Akış

### Aşama A — Skeleton
- Solution ve proje yapısı kuruldu
- Host API başlangıcı hazırlandı

### Aşama B — Shared Audit Temeli
- IAuditableEntity / ISoftDeletable ile temel izlilik sözleşmeleri

### Aşama C — Log Modeli
- LogDbContext oluşturuldu
- 7 log tablosu EF modeli tanımlandı

### Aşama D — Request Yaşam Döngüsü Loglama
- CorrelationId middleware
- RequestLifecycleLogging middleware
- File + DB log birlikte

### Aşama E — Immutable Log
- LogDb migration ile UPDATE/DELETE bloklandı
- Trigger tabanlı güvenlik aktif edildi

### Aşama F — 6 Seviye Yetki + T-Code
- BusinessDbContext kuruldu
- İngilizce tablo/kolon standardına geçildi
- SYS01/SYS02/SYS03/SYS04 seed eklendi
- Ara tablolar dahil audit abstraction zorunlu kılındı

### Aşama G — Dokümantasyon Standardı
- project-map.md özet harita
- project-book.md ana kitap
- learning-log.md oturum şablonu

---

## Cilt 4 — Runtime Akışı (Uçtan Uca)

### 4.1 API İsteği Akışı
1. Request gelir
2. CorrelationId atanır/taşınır
3. Serilog request pipeline işler
4. RequestLifecycle middleware request/response bilgisi toplar
5. LogDb'ye yazar
6. Yanıt döner

### 4.2 Audit Akışı
1. DbContext SaveChanges çağrılır
2. ApplyAuditRules çalışır
3. Added -> CreatedBy/CreatedAt
4. Modified -> ModifiedBy/ModifiedAt
5. Deleted -> soft delete + DeletedBy/DeletedAt

### 4.3 Yetki Akışı (Hedeflenen)
1. T-Code çözülür (SYS01 -> page)
2. Level 1-2-3 erişim doğrulanır
3. Level 4 company scope uygulanır
4. Level 5 action permission uygulanır
5. Level 6 condition permission query’ye yansıtılır

---

## Cilt 5 — Sınıf Haritası (Ana ve Yardımcı)

## 5.1 Ana Sınıflar

### Program
Görev:
- Tüm bileşenleri ayağa kaldırma

Neden:
- Composition root tek olmalı

Hizmet ettiği konu:
- Uygulama başlatma, middleware sıralaması

### LogDbContext
Görev:
- Log şemasını map etmek

Neden:
- Log tabloları tek context altında yönetilsin

Hizmet ettiği konu:
- İzlenebilirlik, olay analizi

### BusinessDbContext
Görev:
- Yetki modelini map etmek
- Seed üretmek
- Audit kurallarını otomatik uygulamak

Neden:
- Yetki + denetim tek yerde tutarlı yönetilsin

Hizmet ettiği konu:
- ERP benzeri erişim kontrolü

## 5.2 Yardımcı Sınıflar

### IAuditActorAccessor
Görev:
- Aktör bilgisini soyutlamak

Neden:
- Persistence katmanı HTTP’ye direkt bağımlı olmasın

### HttpContextAuditActorAccessor
Görev:
- HTTP user’dan aktör bilgisini almak

Neden:
- CreatedBy/ModifiedBy güvenilir dolsun

### AuditableIntEntity
Görev:
- Tüm entity'lerde ortak audit alanları

Neden:
- Ara tablolar dahil iz kaçmasın

### CorrelationIdMiddleware
Görev:
- Correlation zinciri kurmak

Neden:
- Dağınık log yerine tek işlem izi

### RequestLifecycleLoggingMiddleware
Görev:
- Request/response detaylarını loglamak

Neden:
- Denetleyici ve operasyonel takip güvenliği

---

## Cilt 6 — 6 Seviye Yetkilendirme Modeli

### Level 1: Module
- Table: Modules
- Amaç: Üst modül erişimi

### Level 2: SubModule
- Table: SubModules
- Amaç: Alt modül erişimi

### Level 3: SubModulePage
- Table: SubModulePages
- Amaç: Ekran erişimi
- Kritik alan: TransactionCode

### Level 4: UserCompanyPermission
- Table: UserCompanyPermissions
- Amaç: Şirket kapsamı

### Level 5: UserPageActionPermission
- Table: UserPageActionPermissions
- Amaç: Buton/kolon/işlem kontrolü

### Level 6: UserPageConditionPermission
- Table: UserPageConditionPermissions
- Amaç: Veri filtresi (örn price <= 10000)

Destek eşleme tabloları:
- UserModulePermissions
- UserSubModulePermissions
- UserPagePermissions

---

## Cilt 7 — T-Code Haritası

Başlangıç seed:
- SYS01 -> Create User
- SYS02 -> Update User
- SYS03 -> View User
- SYS04 -> User Report

Bu harita BusinessDbContext seed içinde tanımlıdır.

---

## Cilt 8 — Loglama ve Denetim Güvenliği

### 8.1 Log Tabloları
- database_query_logs
- entity_change_logs
- http_request_logs
- page_visit_logs
- performance_logs
- security_event_logs
- system_logs

### 8.2 İmmutability
- Update/Delete trigger ile engellenir

### 8.3 Denetim İlkeleri
- CorrelationId zorunlu
- Error mesaj + stack saklanır
- Actor bilgisi her kritik tabloda tutulur

---

## Cilt 9 — İlişki Matrisi

1. Module (1) -> (N) SubModule
2. SubModule (1) -> (N) SubModulePage
3. User -> UserModulePermission -> Module
4. User -> UserSubModulePermission -> SubModule
5. User -> UserPagePermission -> SubModulePage
6. User -> UserCompanyPermission -> Company scope
7. User -> UserPageActionPermission -> action rights
8. User -> UserPageConditionPermission -> data filter rights

Not:
- User entity FK bağları Identity modülüne sonraki adımda taşınacak.

---

## Cilt 10 — Kod Ekleri (Çekirdek Dosyalar)

Aşağıdaki dosyalar bu projede ana teknik omurgadır:
- src/Host.Api/Program.cs
- src/Host.Api/Middleware/CorrelationIdMiddleware.cs
- src/Host.Api/Middleware/RequestLifecycleLoggingMiddleware.cs
- src/Host.Api/Services/HttpContextAuditActorAccessor.cs
- src/BuildingBlocks/Infrastructure/Persistence/LogDbContext.cs
- src/BuildingBlocks/Infrastructure/Persistence/BusinessDbContext.cs

Not:
- Kaynak kod tek doğru referanstır; bu bölüm yönlendirme amaçlıdır.

---

## Cilt 11 — Değişiklik Kayıt Formatı (Zorunlu)

Her mimari değişiklikten sonra aşağıdaki başlıklar doldurulur.

### 11.1 Change Record
- Tarih:
- Başlık:
- Neyi değiştirdik:
- Neden:
- Etkilenen dosyalar:
- Migration:
- Build sonucu:
- Risk:

### 11.2 Architecture Delta
- Yeni ana sınıf:
- Yeni yardımcı sınıf:
- Değişen ilişki:
- Kaldırılan yapı:
- Geri uyumluluk notu:

### 11.3 Security Delta
- Audit alanları etkisi:
- Log güvenliği etkisi:
- Yetki seviyeleri etkisi:

---

## Cilt 12 — Bu Şablon Nasıl Güncellenecek?

Kural zinciri:
1. Kod değişikliği
2. Build
3. Migration (gerekirse)
4. project-map güncelleme
5. project-book güncelleme
6. learning-log güncelleme
7. commit

Eğer araya yeni bir aşama girerse:
- Bu dosyada yeni Cilt açılır
- Önceki Ciltte referans verilir
- Değişen akış açıkça yazılır

---

## Cilt 13 — Mevcut Commit Akışı

- chore: initialize clean modular monolith skeleton
- feat: add shared kernel entity, domain event, and auditing contracts
- feat: add EF log schema and request lifecycle logging
- feat: enforce immutable log tables and update db credentials
- feat: add 6-level authorization model with audit abstraction and project map
- docs: add live project book and map synchronization guide

---

## Cilt 14 — Sonraki Yol Haritası

1. Identity modülü ile gerçek User FK bağlantısı
2. Condition parser sonucu query filter projection katmanına yansıtma
3. Outbox + notification ile audit olay senkronizasyonu

Not: T-Code Resolver + authorization engine ilk sürümü tamamlandı.
Not: Claim fallback + security_event_logs allow/deny kayıtları tamamlandı.
Not: Condition Parser v2 tamamlandı (number/date/string operator desteği).

Aktif endpoint:
- GET /api/tcode/{transactionCode}?amount={decimal}
- Opsiyonel query: userId, companyId
- userId/companyId query verilmezse claim'den çözülür.

---

## Cilt 15 — Kısa Operasyon Rehberi

### Build
- dotnet build EnterpriseSystem.sln

### BusinessDb Migration
- dotnet ef migrations add <Name> --project src/BuildingBlocks/Infrastructure/Infrastructure.csproj --startup-project src/Host.Api/Host.Api.csproj --context Infrastructure.Persistence.BusinessDbContext --output-dir Persistence/Migrations/BusinessDb
- dotnet ef database update --project src/BuildingBlocks/Infrastructure/Infrastructure.csproj --startup-project src/Host.Api/Host.Api.csproj --context Infrastructure.Persistence.BusinessDbContext

### LogDb Migration
- dotnet ef migrations add <Name> --project src/BuildingBlocks/Infrastructure/Infrastructure.csproj --startup-project src/Host.Api/Host.Api.csproj --context Infrastructure.Persistence.LogDbContext --output-dir Persistence/Migrations/LogDb
- dotnet ef database update --project src/BuildingBlocks/Infrastructure/Infrastructure.csproj --startup-project src/Host.Api/Host.Api.csproj --context Infrastructure.Persistence.LogDbContext

---

## Cilt 16 — Görsel Mimari Dosyaları

Bu proje artık görsel harita dosyaları da içerir:

- docs/architecture/solution-tree.md
- docs/architecture/solution-dependency.mmd
- docs/architecture/request-log-flow.mmd
- docs/architecture/authorization-6-level.mmd
- docs/architecture/class-diagram-authz.puml
- docs/architecture/sequence-tcode-access.puml

Not:
- Mermaid dosyaları akış/bağımlılık görselleri içindir.
- PlantUML dosyaları sınıf ve sequence diyagramları içindir.
- Yeni modül eklendiğinde önce bu dosyalar güncellenir, sonra proje-map/project-book güncellenir.

---

Bu dosya canlıdır. Değişiklik oldukça revize edilir.
