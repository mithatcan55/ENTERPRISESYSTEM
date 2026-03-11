# EnterpriseSystem Proje Haritasi

Bu dosya projenin hizli ama dogru ust seviye haritasidir.

Detayli teknik kitap:

- `project-book.md`

Mimari diyagram ve modul egitim seti:

- `docs/architecture/README.md`

---

## 1. Tek Cumlelik Ozet

EnterpriseSystem, `.NET 9` tabanli, `service + CQRS` yaklasimi kullanan, modullere ayrilmis, guvenlik ve denetim omurgasi guclu bir modular monolith backend projesidir.

---

## 2. Bugunku Ana Moduller

### Host.Api

Rolu:

- uygulamayi ayaga kaldirmak
- middleware zincirini kurmak
- modulleri compose etmek

Business logic tasimaz.

### Identity

Rolu:

- auth lifecycle
- users
- roles
- permissions
- sessions
- password policy

### Authorization

Rolu:

- T-Code enforcement
- permission enforcement
- action / condition kontrolu

### Operations

Rolu:

- log query
- audit dashboard
- operasyonel export ve izleme

### Integrations

Rolu:

- outbox
- dispatcher
- email / excel akislari
- external service adaptasyonlari

---

## 3. Katmanlar

### BuildingBlocks.SharedKernel

- ortak domain tabani
- audit / soft-delete kontratlari

### BuildingBlocks.Application

- exception tipleri
- security kontratlari
- observability kontratlari
- request pipeline arayuzleri

### BuildingBlocks.Infrastructure

- ortak `DbContext` altyapisi
- interceptor'lar
- event publisher
- log writer
- request pipeline implementasyonu

---

## 4. Mimari Kurallar

1. `Host.Api` host'tur, modul degildir
2. controller kendi modulunun `Presentation` katmaninda durur
3. HTTP bilgisi `Application` katmanina tasinmaz
4. ortak davranislar pipeline / interceptor / middleware ile cozulur
5. her business kisminin logunu gelistirici elle yazmak zorunda kalmamali
6. Turkce egitim ve aciklama standardi korunur

---

## 5. Service + CQRS Ozet Karari

Bu projede:

- buyuk tek servisler kirildi
- command ve query sorumluluklari ayrildi
- ama vertical slice tercih edilmedi

Yani:

- modul bazli yapi korunuyor
- modul icinde command/query ayrimi var

Bu fark kritiktir.

---

## 6. Guvenlik Omurgasi

### Kimlik

- session-aware auth
- current user context
- central claim types

### Yetki

- role
- permission
- T-Code
- company scope
- action
- condition

### Ikinci Savunma Hatti

Request modelleri gerekirse:

- `ITCodeProtectedRequest`
- `IPermissionProtectedRequest`

arayuzlerini uygular.

Pipeline icinde pre-check calisir.

---

## 7. Loglama ve Denetim Omurgasi

Sistemde loglama daginik degil, omurga bazlidir.

Temel bilesenler:

- request lifecycle middleware
- SQL command logging interceptor
- entity change logging interceptor
- operation logging filter
- operational event publisher
- notification channels

Log tipleri:

- HTTP
- system
- security
- entity change
- performance
- page visit
- database query

---

## 8. Veritabani Mantigi

Ana context yonu:

- `IdentityDbContext`
- `AuthorizationDbContext`
- `IntegrationsDbContext`
- `LogDbContext`

Bu ayrim su sebeple var:

- bounded context sahipligi netlessin
- migration yonetimi sade olsun
- moduller buyurken birbirine yapismasin

---

## 9. Egitim Haritasi

Bu projeyi ogrenmek icin okunma sirasi:

1. `project-map.md`
2. `project-book.md`
3. `docs/architecture/current-state-target-state-guide.md`
4. `docs/architecture/command-query-pipeline-training.md`
5. `docs/architecture/identity-module-training.md`
6. `docs/architecture/authorization-module-training.md`
7. `docs/architecture/operations-module-training.md`
8. `docs/architecture/integrations-module-training.md`

---

## 10. Hangi Sorunun Cevabi Hangi Dokumanda

### "Genel resim nedir?"

- `project-map.md`

### "Tum mimariyi detayli anlat."

- `project-book.md`

### "Current state / target state nedir?"

- `docs/architecture/current-state-target-state-guide.md`

### "Pipeline nasil calisiyor?"

- `docs/architecture/command-query-pipeline-training.md`

### "Identity modulunu ogret."

- `docs/architecture/identity-module-training.md`

### "Authorization modulunu ogret."

- `docs/architecture/authorization-module-training.md`

### "Outbox ve entegrasyon akislarini ogret."

- `docs/architecture/integrations-module-training.md`

### "Log query ve audit dashboard nasil calisiyor?"

- `docs/architecture/operations-module-training.md`

---

## 11. Kisa Soru-Cevap

### Soru: Bu proje hala vertical slice mi?

Cevap:
Hayir. Modul bazli yapi korunuyor, command/query ayrimi ise service + CQRS ihtiyacina gore kullaniliyor.

### Soru: Host.Api neden zayif tutuluyor?

Cevap:
Cunku host buyudukce modul sahipligi kaybolur. Business mantik ilgili modulde kalmalidir.

### Soru: Neden bu kadar log altyapisi var?

Cevap:
Kurumsal sistemlerde sadece "calisti" demek yetmez; ne oldu, neden oldu, kim yapti, tekrar denemek gerekir mi, bunlar da bilinmelidir.

### Soru: Neden kod icinde Turkce aciklama istiyoruz?

Cevap:
Bu proje egitim ve devir teslim odakli. Bilgi sadece kafada kalmamali.

---

## 12. Sonraki Adimlar

Yakin hedefler:

- localization egitim rehberi
- error policy engine dokumani
- daha fazla modul ici Turkce aciklama
- project-book icinde daha cok ornek akiş ve soru-cevap

---

Bu dosya hizli referans icindir.
Derinlik gerekirse `project-book.md` ve `docs/architecture/*` rehberlerine inilir.
