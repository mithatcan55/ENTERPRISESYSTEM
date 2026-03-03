# EnterpriseSystem Proje Kitabı (Canlı Ana Dosya)

Bu doküman, projenin teknik kaynağıdır.

- Neyi kurduk?
- Neden kurduk?
- Sınıflar nasıl bağlı?
- Hangi dosya neye hizmet ediyor?
- Değişiklik gelirse hangi şablonla güncellenecek?

Bu dosya, her mimari değişiklikte güncellenir.

---

## 1. Vizyon ve Kırmızı Çizgiler

1. Mimari: Clean + Vertical Slice + Modular Monolith
2. Altyapı: .NET 9 + PostgreSQL + EF Core
3. Loglama: Dosya + LogDb (ayrı DB), immutable log tabloları
4. Yetki: 6 seviye + T-Code erişimi (SYS01 vb.)
5. Denetim: Ara tablolar dahil her kritik kayıtta Created/Modified/Deleted izi
6. İsim standardı: Tablo/kolon/sınıf İngilizce, açıklamalar Türkçe

---

## 2. Klasör ve Katman Haritası

### 2.1 Kök
- EnterpriseSystem.sln
- Directory.Packages.props
- global.json
- project-map.md
- project-book.md
- learning-log.md

### 2.2 Kaynak
- src/Host.Api
  - Program.cs
  - Middleware/
  - Services/
- src/BuildingBlocks/SharedKernel
- src/BuildingBlocks/Application
- src/BuildingBlocks/Infrastructure
  - Persistence/
    - BusinessDbContext.cs
    - LogDbContext.cs
    - Entities/
    - Migrations/

---

## 3. Proje Bağımlılık Haritası

1. Host.Api
   - Infrastructure
   - Identity.Infrastructure
   - Identity.Presentation
2. Infrastructure
   - Application
   - SharedKernel
3. Identity.Infrastructure
   - Identity.Application
   - Identity.Domain
   - Infrastructure

Yorum:
- Host sadece composition root.
- Persistence ve cross-cutting davranışlar Infrastructure altında.

---

## 4. Runtime Akışı (Request -> DB -> Log)

1. İstek Host.Api'ye gelir.
2. CorrelationId middleware isteğe id atar.
3. Serilog request logging çalışır.
4. RequestLifecycle middleware request/response bilgisini toplar.
5. DB'ye iki log atılır:
   - http_request_logs
   - system_logs
6. Sonuç kullanıcıya döner.

Neden:
- Olay adli takibi için tek bir correlation zinciri oluşur.

---

## 5. Ana Sınıflar (Detaylı)

## 5.1 Program.cs
Dosya: src/Host.Api/Program.cs

Görevler:
- Serilog host entegrasyonu
- OpenAPI + Scalar endpointleri
- LogDbContext kaydı
- BusinessDbContext kaydı
- Audit actor accessor DI kaydı
- Middleware sıralaması

Hizmeti:
- Uygulamanın teknik omurgasını başlatır.

## 5.2 LogDbContext
Dosya: src/BuildingBlocks/Infrastructure/Persistence/LogDbContext.cs

Görevler:
- logs şeması altında log tablolarını map eder.
- Tablolar:
  - database_query_logs
  - entity_change_logs
  - http_request_logs
  - page_visit_logs
  - performance_logs
  - security_event_logs
  - system_logs

Hizmeti:
- Log verisinin standart ve sorgulanabilir şekilde saklanması.

## 5.3 BusinessDbContext
Dosya: src/BuildingBlocks/Infrastructure/Persistence/BusinessDbContext.cs

Görevler:
- Yetki modelinin 6 seviyesini map eder.
- SYS01-SYS04 seed kayıtlarını üretir.
- SaveChanges sırasında audit alanlarını otomatik doldurur.
- Soft-delete dönüşümünü yönetir.

Hizmeti:
- Yetki güvenliği + denetlenebilir veri değişimi.

## 5.4 CorrelationIdMiddleware
Dosya: src/Host.Api/Middleware/CorrelationIdMiddleware.cs

Görevler:
- Header varsa kullanır, yoksa üretir.
- Response header'a geri yazar.
- Serilog context'e property push eder.

Hizmeti:
- Tüm log kayıtlarının tek işlem kimliği altında toplanması.

## 5.5 RequestLifecycleLoggingMiddleware
Dosya: src/Host.Api/Middleware/RequestLifecycleLoggingMiddleware.cs

Görevler:
- Request body/header okuma
- Response body/header okuma
- Süre ölçümü
- Kullanıcı/ip/session/correlation toplama
- http_request_logs + system_logs yazımı

Hizmeti:
- Denetleyicilere karşı güçlü ve güvenilir log hattı.

## 5.6 HttpContextAuditActorAccessor
Dosya: src/Host.Api/Services/HttpContextAuditActorAccessor.cs

Görevler:
- Aktif kullanıcı kimliğini döndürür.
- Kullanıcı yoksa system fallback döner.

Hizmeti:
- CreatedBy/ModifiedBy/DeletedBy alanlarının kaçmaması.

---

## 6. Yardımcı Sınıflar

## 6.1 IAuditActorAccessor
Dosya: src/BuildingBlocks/Infrastructure/Persistence/Auditing/IAuditActorAccessor.cs

Amaç:
- DbContext ile kullanıcı kaynağını soyutlamak.

## 6.2 AuditableIntEntity
Dosya: src/BuildingBlocks/Infrastructure/Persistence/Entities/Abstractions/AuditableIntEntity.cs

Amaç:
- Tüm yetki entity'lerinde ortak audit alanlarını zorunlu kılmak.

---

## 7. Yetkilendirme Modeli (6 Seviye)

## Seviye 1: Module
- Entity: Module
- Table: Modules
- Ne işe yarar: Üst modül erişimi

## Seviye 2: SubModule
- Entity: SubModule
- Table: SubModules
- Ne işe yarar: Modül alt kırılımı

## Seviye 3: SubModulePage
- Entity: SubModulePage
- Table: SubModulePages
- Kritik alan: TransactionCode (SYS01...)
- Ne işe yarar: Ekran/fonksiyon erişimi

## Seviye 4: UserCompanyPermission
- Table: UserCompanyPermissions
- Ne işe yarar: Şirket kapsamı

## Seviye 5: UserPageActionPermission
- Table: UserPageActionPermissions
- Ne işe yarar: Buton/kolon/aksiyon yetkisi

## Seviye 6: UserPageConditionPermission
- Table: UserPageConditionPermissions
- Ne işe yarar: Veri filtresi (örn price <= 10000)

Destek tabloları:
- UserModulePermissions
- UserSubModulePermissions
- UserPagePermissions

---

## 8. T-Code Haritası (Başlangıç)

- SYS01 -> Create User
- SYS02 -> Update User
- SYS03 -> View User
- SYS04 -> User Report

Bu kayıtlar BusinessDbContext içindeki seed ile gelir.

---

## 9. Log Güvenilirlik Politikası

1. CorrelationId zorunlu
2. Request+response metadata saklanır
3. Hata stack bilgisi saklanır
4. Log tabloları immutable migration ile korunur
5. Runtime log dosyaları git'e alınmaz

---

## 10. Şu Ana Kadarki Commit Akışı

- chore: initialize clean modular monolith skeleton
- feat: add shared kernel entity, domain event, and auditing contracts
- feat: add EF log schema and request lifecycle logging
- feat: enforce immutable log tables and update db credentials
- feat: add 6-level authorization model with audit abstraction and project map

---

## 11. Değişiklik Gelince Güncelleme Protokolü

Her değişiklikte aşağıdaki 4 dosya kontrol edilir:
1. project-book.md (ana açıklama)
2. project-map.md (özet harita)
3. learning-log.md (oturum kaydı)
4. ilgili migration dosyası (varsa)

Güncelleme adımı:
1. Değişikliği yap
2. Build al
3. Migration gerekliyse üret/uygula
4. project-book.md bölümünü güncelle
5. learning-log.md oturum kaydını gir
6. Commit at

---

## 12. Şablonlar

## 12.1 Architecture Delta Şablonu
- Değişiklik adı:
- Etkilenen katman:
- Yeni sınıf(lar):
- Silinen sınıf(lar):
- Yeni ilişki:
- Risk:
- Test/Build sonucu:

## 12.2 Migration Şablonu
- Context:
- Migration adı:
- Eklenen tablo/alan:
- Geri alma notu:
- DB update sonucu:

## 12.3 Security/Audit Şablonu
- CreatedBy doluyor mu:
- ModifiedBy doluyor mu:
- DeletedBy doluyor mu:
- CorrelationId uçtan uca mı:
- Log immutability korunuyor mu:

---

## 13. Kritik Kod Ekleri

Bu bölümde çekirdek dosyaların kodu örnek olarak tutulur. Tam kaynak her zaman ilgili dosyalardadır.

### 13.1 Program.cs (özet)
- Serilog + OpenAPI + 2 DbContext + middleware zinciri.

### 13.2 BusinessDbContext (özet)
- 6 level yetki tabloları
- SYS01-SYS04 seed
- ApplyAuditRules

### 13.3 RequestLifecycleLoggingMiddleware (özet)
- request/response body yakalama
- duration ölçümü
- http_request_logs + system_logs yazımı

Not:
- Kod değişirse bu bölümdeki özet satırları güncellenir; asıl kaynak dosyalar tek doğru kaynaktır.

---

## 14. Sonraki Teknik Yol

1. T-Code Resolver servisi (SYSxx -> page + permission pipeline)
2. Authorization Engine (seviye 1-6 karar birleştirme)
3. Condition Parser (Field/Operator/Value -> query filter)
4. SecurityEventLog zenginleştirme (deny reason)
5. Identity modülü ile gerçek User FK bağlantısı

---

## 15. Not

Bu dosya bir “tek seferlik rapor” değil, canlı teknik kitaptır.
Her mimari değişiklikte revize edilmelidir.
