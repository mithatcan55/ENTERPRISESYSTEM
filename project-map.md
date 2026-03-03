# EnterpriseSystem Proje Haritası (Canlı Doküman)

> Bu dosya, projenin en başından bugüne kadar oluşan teknik akışı, sınıf ilişkilerini, katman bağımlılıklarını ve neden-sonuç kararlarını tek yerde tutmak için hazırlanmıştır.
> 
> Kural: Mimaride veya akışta her değişiklik olduğunda bu dosya güncellenir.

> Detaylı ana kitap dosyası: `project-book.md`

---

## 1) Hedef ve Kapsam

Bu proje, **.NET 9 tabanlı modular monolith** yaklaşımı ile kurulan ve aşağıdaki hedefleri taşıyan bir altyapıdır:

1. Çok güçlü ve denetlenebilir loglama (request başından sonuna kadar izlenebilirlik)
2. Ayrı log veritabanı + immutable log tabloları
3. Kullanıcı/rol/izin altyapısı (role + permission + condition)
4. SAP/CANIAS benzeri **T-Code** ile ekran erişimi (ör. SYS01, SYS02)
5. 6 seviyeli yetkilendirme modeli
6. Her kritik tabloda "kimin ne yaptığı" bilgisinin kaçmaması

---

## 2) Şu Anki Çalışan Mimarinin Özeti

### 2.1 Katmanlar

- `src/Host.Api` → Uygulamanın giriş noktası, middleware zinciri, DI, OpenAPI/Scalar
- `src/BuildingBlocks/SharedKernel` → Ortak kontratlar (audit, soft-delete vb.)
- `src/BuildingBlocks/Application` → Uygulama seviyesinde paylaşılacak soyutlamalar (ileride genişletilecek)
- `src/BuildingBlocks/Infrastructure` → EF Core context'leri, entity'ler, migration'lar, persistence yardımcıları
- `src/Modules/Identity/*` → Modül klasörleri (şu an iskelet)

### 2.2 Veritabanları

- `BusinessDb` → İş verisi + yetkilendirme modeli
- `LogDb` → Log verisi (immutable kurallı)

---

## 3) Zaman Akışı (Baştan Bugüne)

### Aşama 1: İskelet
- Solution ve proje yapısı kuruldu.
- Clean/Modular yapı için temel referans zinciri oluştu.

### Aşama 2: Shared çekirdek
- Ortak audit kontratları eklendi (`IAuditableEntity`, `ISoftDeletable`).

### Aşama 3: Log modeli
- Log için EF context (`LogDbContext`) ve 7 log tablosu modeli oluşturuldu:
  - `database_query_logs`
  - `entity_change_logs`
  - `http_request_logs`
  - `page_visit_logs`
  - `performance_logs`
  - `security_event_logs`
  - `system_logs`

### Aşama 4: Request yaşam döngüsü loglama
- Correlation middleware ve request lifecycle middleware eklendi.
- File log + DB log akışı birlikte çalışacak şekilde bağlandı.

### Aşama 5: Immutable log
- Log tablolarına `UPDATE/DELETE` engeli migration ile eklendi.
- Trigger tabanlı koruma aktif edildi.

### Aşama 6: Yetkilendirme refactor
- Yetki modeli İngilizce tablo/kolon/sınıf adlarına dönüştürüldü.
- T-Code odaklı sayfa modeli eklendi (SYS01..SYS04 seed).
- Ara tablolar dahil tüm yetki entity'lerine audit alanı eklendi.

---

## 4) Ana Sınıflar ve Bağlantı Haritası

## 4.1 Uygulama Başlatma

### `Host.Api/Program.cs`
Bu dosya sistemin composition root noktasıdır.

Yaptıkları:
1. Serilog host entegrasyonu
2. OpenAPI + Scalar
3. `LogDbContext` kaydı
4. `BusinessDbContext` kaydı
5. `IAuditActorAccessor` servis kaydı
6. Middleware zinciri:
   - CorrelationId
   - Serilog request logging
   - Request lifecycle DB logging

Neden önemli:
- Bu dosya altyapının "nerede birleştiğini" gösterir.
- Sistem davranışını merkezi şekilde kontrol etmemizi sağlar.

---

## 4.2 Loglama Sınıfları

### `Infrastructure/Persistence/LogDbContext.cs`
- `logs` şemasını varsayılan yapar.
- 7 log tablosunun EF mapping'ini içerir.

### `Host.Api/Middleware/CorrelationIdMiddleware.cs`
- Her isteğe `X-Correlation-Id` verir.
- Header'dan geliyorsa korur, yoksa üretir.
- Log context'e korelasyon bilgisi ekler.

### `Host.Api/Middleware/RequestLifecycleLoggingMiddleware.cs`
- Request ve response body/headers bilgilerini toplar.
- Süre ölçümü yapar.
- Hata bilgilerini yakalar.
- `http_request_logs` ve `system_logs` tablolarına yazar.

Neden önemli:
- Denetim ve olay analizi için uçtan uca iz bırakır.
- "Hangi kullanıcı, hangi endpoint, ne kadar sürede, hangi sonuçla" sorusunu yanıtlar.

---

## 4.3 Yetkilendirme Sınıfları (6 Seviye)

### Seviye-1: Module
- Sınıf: `Module`
- Tablo: `Modules`
- Amaç: Üst modül görünürlüğü/erişimi

### Seviye-2: SubModule
- Sınıf: `SubModule`
- Tablo: `SubModules`
- Amaç: Modül altı iş alanı ayrımı

### Seviye-3: SubModulePage (T-Code)
- Sınıf: `SubModulePage`
- Tablo: `SubModulePages`
- Alan: `TransactionCode` (SYS01, SYS02...)
- Amaç: Doğrudan ekran/işlev çağırma

### Seviye-4: Company Scope
- Sınıf: `UserCompanyPermission`
- Tablo: `UserCompanyPermissions`
- Amaç: Kullanıcının şirket kapsamını sınırlama

### Seviye-5: Action/Operation
- Sınıf: `UserPageActionPermission`
- Tablo: `UserPageActionPermissions`
- Alan: `ActionCode`, `IsAllowed`
- Amaç: Buton/kolon/işlem bazlı yetki

### Seviye-6: Data Condition
- Sınıf: `UserPageConditionPermission`
- Tablo: `UserPageConditionPermissions`
- Alanlar: `FieldName`, `Operator`, `Value`
- Amaç: Veri filtresi (örn `price <= 10000`)

Ek ilişki tabloları:
- `UserModulePermission`
- `UserSubModulePermission`
- `UserPagePermission`

Neden önemli:
- Kullanıcıya sadece "sayfayı gör" değil, "hangi veriyi nasıl görecek" seviyesinde kontrol verir.

---

## 5) Yardımcı Sınıflar ve Ana Sınıflar

## 5.1 Yardımcı/Altyapı Sınıfları

### `IAuditActorAccessor`
- Amaç: Mevcut aktör bilgisini persistence katmanına taşımak
- Neden: DbContext'in HttpContext'e direkt bağımlı olmaması

### `HttpContextAuditActorAccessor`
- Amaç: Aktör bilgisini HTTP kullanıcı context'inden toplamak
- Fallback: kullanıcı yoksa `system`

### `AuditableIntEntity`
- Amaç: Tüm entity'lerde ortak audit alanlarını zorunlu kılmak
- Alanlar: `CreatedBy`, `CreatedAt`, `ModifiedBy`, `ModifiedAt`, `DeletedBy`, `DeletedAt`, `IsDeleted`

## 5.2 Ana Sınıflar

### `BusinessDbContext`
- Yetki tablolarının tamamını yönetir
- SaveChanges sırasında audit alanlarını otomatik doldurur
- Soft-delete mantığı içerir
- SYS01..SYS04 başlangıç verisi üretir

### `LogDbContext`
- Log depolama modelini yönetir
- Immutable migration ile birlikte çalışır

---

## 6) Sınıflar Arası İlişkiler

1. `Module (1) -> (N) SubModule`
2. `SubModule (1) -> (N) SubModulePage`
3. `User -> UserModulePermission -> Module`
4. `User -> UserSubModulePermission -> SubModule`
5. `User -> UserPagePermission -> SubModulePage`
6. `User -> UserPageActionPermission -> SubModulePage`
7. `User -> UserPageConditionPermission -> SubModulePage`

Not:
- Kullanıcı tablosu henüz bu projede ayrıca modellenmediği için `UserId` alanları int olarak tutulur.
- İleride Identity modülünden FK bağlanacak.

---

## 7) Neden Bu Yapı Seçildi?

1. **Modular monolith**
   - Tek deploy kolaylığı + modüler büyüme
2. **EF ile tam şema yönetimi**
   - SQL script dağınıklığını azaltır
   - Migration geçmişi ile geri alınabilirlik
3. **Ayrı log DB + immutable kural**
   - Denetim güvenilirliği artar
4. **6 level yetki**
   - Kurumsal ERP ihtiyaçlarına uygun esneklik
5. **Audit abstraction**
   - Kimin ne yaptığı bilgisi ara tablolar dahil kaçmaz

---

## 8) Güvenilir Log Prensipleri (Denetim Odaklı)

1. Her istekte `CorrelationId` zorunlu
2. Request + response + status + duration kaydı
3. Hata durumunda mesaj + stack bilgisi kaydı
4. Log tabloları immutable
5. Uygulama log dosyaları git'e girmemeli (`.gitignore` aktif)

---

## 9) İş Akışı Şablonu (Canlı Güncellenecek)

Bu bölüm yeni değişikliklerde güncellenecek standarttır.

### 9.1 Change Record
- Tarih:
- Değişiklik başlığı:
- Neyi değiştirdik:
- Neden değiştirdik:
- Etkilenen dosyalar:
- Migration adı:
- Build sonucu:
- Risk/Not:

### 9.2 Architecture Delta
- Yeni sınıf eklendi mi?
- İlişki değişti mi?
- Yeni tablo eklendi mi?
- Yetki seviyesi etkilendi mi?
- Log güvenliği etkilendi mi?

### 9.3 Eğitim Notu
- Bu adımda öğrenilen kritik konu:
- Sonraki adım:

---

## 10) Sonraki Net Adımlar

1. Identity tablosunu tanımlayıp `UserId` alanlarına FK bağlamak
2. T-Code çözümleyici servis eklemek (`SYS01 -> page route`) 
3. Yetki karar motoru yazmak:
   - önce page permission
   - sonra action permission
   - sonra condition permission
4. Condition parser ile query filtre üretmek (örn `price <= 10000`)
5. Authorization sonucu ve deny nedeni için `security_event_logs` zenginleştirmesi

---

## 11) Bu Dokümanın Güncelleme Kuralı

- Mimaride her kırılma değişikliğinde (yeni context, yeni tablo, yeni middleware, yeni policy), bu dosya güncellenecek.
- Migration eklenirse ilgili bölümde adı ve amacı yazılacak.
- Yeni class eklenirse "Yardımcı sınıf mı, ana sınıf mı" mutlaka belirtilecek.
- Yeni yetki seviyesi veya T-Code senaryosu gelirse 4.3 ve 6. bölüm revize edilecek.
