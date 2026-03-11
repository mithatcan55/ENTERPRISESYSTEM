# Enterprise Backbone Master Plan (Kurumsal Omurga Planı)

Bu planın amacı: proje yönünü netleştirmek, "önce ne yapılır" sorusunu bitirmek ve her adımı test edilebilir hale getirmek.

---

## 1) Bugün Nerede Duruyoruz? (Gerçek Durum)

### Tamamlananlar
- Core DI yapısı (extension + Scrutor)
- Merkezi log akışı (request lifecycle + security/system logs)
- Immutable log tabloları
- Global exception handling + ProblemDetails standardı
- Validation response standardı
- 6 seviyeli yetki modeli (T-Code dahil)
- T-Code resolve endpoint (Scalar ile denenebilir)
- Modül scaffold şablonları + otomasyon scripti

### Eksik Olan Kritik Omurga
- Identity domain gerçek user modeli (şu an iskelet)
- User CRUD (ekle/güncelle/sil/listele) gerçek implementasyon
- Role/Permission yönetim API'leri
- Session yönetimi (listele/sonlandır)
- Şifre politikası + periyodik değişim kuralı
- Rate limiting (global + endpoint bazlı)
- Yönetim arayüzü için operasyon endpoint'leri (log/audit/session)
- Dış servis veri çekme politikası (resilience + denetim)

---

## 2) UnitOfWork Kararı (Kesin)

Bu projede şu an **ayrı UnitOfWork sınıfı kullanılmıyor**.

Neden:
- EF Core DbContext request scope içinde doğal UnitOfWork davranışı verir.
- Ek UnitOfWork katmanı şu aşamada değerden çok karmaşıklık üretiyor.

Ne zaman eklenir:
- Birden fazla bounded context / birden fazla transaction kaynağı tek işte koordinasyon gerektirirse.

---

## 3) Öncelik Sırası (Bozulmaz Akış)

### Faz-1 — Identity Core ve User Yönetimi (En Acil)
Hedef: Gerçek kullanıcı yaşam döngüsü hazır olsun.

Teslimatlar:
1. Identity User entity + migration
2. User CRUD API
3. Soft delete + audit izleri
4. Scalar test senaryoları

### Faz-2 — Role/Permission Yönetimi
Hedef: Yetkiyi yönetim ekranından sürdürülebilir yapmak.

Teslimatlar:
1. Role, Permission, UserRole, RolePermission API'leri
2. T-Code ile role policy bağları
3. Seed yönetim komutları

### Faz-3 — Oturum ve Şifre Güvenliği
Hedef: Kurumsal hesap güvenliği.

Teslimatlar:
1. Session tablosu ve active session tracking
2. Session listeleme/sonlandırma endpoint'leri
3. Şifre doğrulama politikası (karmaşıklık + geçmiş kontrol)
4. Şifre periyot kuralı (örn 90 gün)
5. Zorunlu şifre değişim akışı

### Faz-4 — Rate Limit + Abuse Koruması
Hedef: API dayanıklılığı ve güvenlik.

Teslimatlar:
1. Global rate limit policy
2. Auth endpoint özel policy
3. IP + user bazlı limit stratejisi
4. Limit ihlali loglaması

### Faz-5 — Yönetim Odaklı Operasyon API Katmanı
Hedef: UI'dan yönetilebilir kurumsal operasyon.

Teslimatlar:
1. Log sorgulama endpoint'leri (system/security/http)
2. Audit/denetim sorgu endpoint'leri
3. Session admin endpoint'leri
4. Hata izleme endpoint'leri (correlation bazlı)

### Faz-6 — Dış Servis Entegrasyon Omurgası
Hedef: Harici kaynaklardan veri çekerken kontrol ve güvenlik.

Teslimatlar:
1. Outbound HTTP client standardı
2. Retry/timeout/circuit breaker politikası
3. Dış servis çağrı logları + denetim izi
4. Veri sözleşmesi ve mapping standardı

---

## 4) Roller, Yetkiler, Kullanıcılar — Net Durum

### Roller/Yetkiler
- Model altyapısı var.
- Yönetim API'si ve gerçek role lifecycle henüz yok.

### Kullanıcılar
- User ID claim çözümü var.
- Identity gerçek user tablosu ve CRUD henüz yok.

### T-Code
- Resolve ve 6-seviye kontrol var.
- Role yönetim paneli bağlantısı henüz yok.

---

## 5) Scalar'da Ne Zaman Ne Deneceksin?

### Faz-1 sonrası
- User create/update/delete/list endpoint'leri
- Validation error formatı
- NotFound/Forbidden hata formatı

### Faz-2 sonrası
- Role atama/geri alma
- Permission etkisi (T-Code allowed/denied)

### Faz-3 sonrası
- Session listeleme
- Session sonlandırma
- Şifre değişim ve expiration senaryosu

### Faz-4 sonrası
- Rate limit threshold testi
- 429 yanıt sözleşmesi

### Faz-5 sonrası
- Log/audit endpoint filtre testleri
- CorrelationId ile hata izleme

---

## 6) Yönetim Ekranı İçin API Paketleri

UI'da görmek istediğin her şey için backend paketleri:

1. Identity Management API
   - users, roles, permissions

2. Session Management API
   - active sessions, revoke session

3. Security & Audit API
   - security_event_logs, entity_change_logs

4. Operations API
   - system_logs, http_request_logs, correlation search

---

## 7) Denetim (Audit/Compliance) Kapıları

Her faz bitmeden önce zorunlu kontrol:

- Build başarılı
- Migration uygulanabilir
- Log izleri doğrulandı
- Hata sözleşmesi bozulmadı
- Scalar senaryoları geçti
- Dokümantasyon güncel

Kapı geçilmezse faz "tamam" sayılmaz.

---

## 8) Hemen Sonraki İş (Implementasyon Başlangıcı)

Bir sonraki kod adımı:

**Faz-1 / Adım-1:** Identity User entity + migration + minimal User CRUD (list/create).

Bununla birlikte:
- Scalar koleksiyonu için ilk test senaryoları hazırlanır.
- FE contract stub bu endpoint'lere bağlanır.
