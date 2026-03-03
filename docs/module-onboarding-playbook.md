# Module Onboarding Playbook (Core-First, Uçtan Uca Uygulama Rehberi)

Bu rehberin amacı sadece "liste vermek" değil, seni karar yorgunluğundan kurtarmaktır.

Okuduktan sonra şu cümleleri kurabilmelisin:
- "Yeni modül açarken önce hangi altyapıyı hazırlarım biliyorum."
- "Controller içinde ne yazarım ne yazmam biliyorum."
- "Frontend ekibi hangi sırayla entegrasyon yapar biliyorum."

Bu projede ana prensip:
**İş kodu sadece iş akışını anlatır; çapraz teknik yükleri altyapı taşır.**

---

## 1) Önce Neyi Kurarız? (Doğru Öncelik Sırası)

Bir kütüphaneyi kullanmadan önce onu indirmek nasıl zorunluysa, modül geliştirmede de bir öncelik sırası vardır.

### 1.1 Zorunlu Altyapı (Önce)
1. DI ve servis kayıt yapısı (Scrutor + extension yapısı)
2. Global hata yönetimi (ProblemDetails standardı)
3. Request/Audit log altyapısı
4. Kimlik/actor çözümleme standardı
5. Yetki motoru (T-Code + level modeli)

### 1.2 İş Modülü (Sonra)
6. Modül varlıkları ve endpoint'leri
7. Frontend ekran bağlantısı
8. Uçtan uca test/deneme

Bu sırayı bozarsan her modülde aynı teknik sorunları tekrar çözmek zorunda kalırsın.

---

## 2) WinForms Event Zihniyetiyle Eşleştirme

Sana en pratik gelen model bu olduğu için birebir eşleştiriyoruz.

### Event-1: Form_Load (Ekran açılırken)
Backend karşılığı:
- `GET /api/tcode/{transactionCode}`

Ne döner:
- `isAllowed`: ekran açılabilir mi
- `actions`: butonlar aktif/pasif
- `conditions`: veri görünürlüğü ve filtre kuralları

Frontend görevi:
- Yetkiye göre ekranı aç/kapat
- Butonları policy'e göre gizle/göster

### Event-2: Button_Click (Kaydet/Sil/Güncelle)
Backend karşılığı:
- command endpoint (örn `POST /api/users`)

Backend ne yapar:
- İş kuralını çalıştırır
- DbContext ile transaction scope'ta kaydeder
- Hata olursa global handler tek formatta yanıt üretir

### Event-3: Grid_Load (Listeleme)
Backend karşılığı:
- list endpoint (örn `GET /api/users?page=1&pageSize=20`)

Backend ne yapar:
- Pagination, sorting, filtre uygular
- Teknik log otomatik toplanır

Bu eşleştirme sayesinde "hangi event'te ne yapacağım" sorusu netleşir.

---

## 3) Katman Sorumluluk Matrisi (Neyi Nereye Yazarız?)

### 3.1 Program.cs
Yapar:
- Sadece uygulama pipeline'ını başlatır
- Extension metodlarını çağırır

Yapmaz:
- Satır satır servis kaydı şişkinliği
- Modül detay bilgisi

### 3.2 DependencyInjection Extensions
Yapar:
- Katman/modül servis kayıtlarını toplar
- Host yükünü düşürür

Yapmaz:
- İş kuralı

### 3.3 Controller
Yapar:
- Request alır
- Servise delege eder
- Response döner

Yapmaz:
- Elle try/catch tekrarı
- Teknik log kodu
- Claim parse karmaşası

### 3.4 Service
Yapar:
- İş davranışı
- Yetki kararına göre işlem
- Veri erişim koordinasyonu

Yapmaz:
- HTTP detay yönetimi

### 3.5 Middleware/Global Handler
Yapar:
- Teknik log, exception yönetimi, correlation zinciri

Yapmaz:
- İş kararı üretmez

---

## 4) "Bunu Yazmalı mıyım?" Karar Tablosu

### 4.1 "Burada log yazmalı mıyım?"
- Teknik request logu ise: Hayır (otomatik).
- İş olayı ise: Evet (anlamlı business event olarak).

### 4.2 "Burada yetki kontrolü yapmalı mıyım?"
- Ekran policy kontrolü ise: T-Code resolver merkezi akış.
- İşlem bazlı ek kural ise: Servis içinde policy sonucuna göre.

### 4.3 "Burada UnitOfWork yazmalı mıyım?"
- Hayır. DbContext scope zaten UoW davranışını sağlar.

---

## 5) Yeni Modül Açma Reçetesi (Adım Adım, Atlanmaz)

### Adım-A: Kimliklendirme
- Module ekle
- SubModule ekle
- SubModulePage + TransactionCode ekle

Çıktı: Sistem bu ekranı artık tanır.

### Adım-B: Yetki Matrisi
- UserModulePermission
- UserSubModulePermission
- UserPagePermission
- UserCompanyPermission

Opsiyonel:
- Action permission
- Condition permission

Çıktı: "Kim, neyi, hangi kapsamda" sorusunun cevabı oluşur.

### Adım-C: API Sözleşmesi
- DTO'lar
- Endpoint route
- Validation kuralları
- Response formatı

Çıktı: Frontend ile entegrasyon dili netleşir.

### Adım-D: Frontend Bağlantısı
- Sayfa açılışında T-Code resolve çağrısı
- `actions` ile buton policy
- `conditions` ile alan davranışı

Çıktı: UI backend policy'e göre davranır.

### Adım-E: Kabul Testi
- Allowed senaryo
- Denied senaryo
- Hata senaryo (ProblemDetails)
- Build temiz

Çıktı: Modül prod'a taşınabilir seviyeye gelir.

---

## 6) Controller Şablonu (Zihinsel Kalıp)

Controller yazarken şu kalıbı takip et:

1. Input al
2. Kimlik/şirket bilgisini context'ten çöz
3. Servis çağır
4. Sonucu standard response ile dön

Yazma:
- Gereksiz cross-cutting kodu
- Çok katmanlı if-else yetki dağınıklığı

---

## 7) Frontend Şablonu (Zihinsel Kalıp)

Sayfa mount olduğunda:
1. T-Code resolve çağrısı
2. `isAllowed` false ise route guard
3. `actions` ile buton enable/gizle
4. `conditions` ile kolon/alan policy uygula
5. Liste/form endpoint çağrılarına geç

Bu kalıp bozulursa UI davranışı ekipten ekibe değişir.

---

## 8) Anti-Pattern Listesi (Yapma)

- Her endpoint'e farklı hata formatı yazmak
- Her servis içinde claim parse etmek
- Program.cs içine tüm servisleri yığmak
- Modül eklerken yetki seed adımını atlamak
- Frontend'de T-Code resolve yapmadan ekran açmak

---

## 9) DoD (Definition of Done) — Gerçek Bitti Kriteri

Bir modül aşağıdakiler tamam değilse bitmiş sayılmaz:

1. Kimliklendirme (Module/SubModule/Page/T-Code)
2. Yetki matrisi
3. API sözleşmesi
4. Frontend policy bağlantısı
5. Allowed/Denied/hata testleri
6. Build başarılı
7. Dokümantasyon güncel

---

## 10) 10 Dakikalık Hızlı Özet

"Yeni modül açarken önce T-Code kimliğini ve permission matrisini kurarım. Controller'da sadece iş akışını yazarım; log/hata/audit altyapıdadır. Frontend sayfa açılışında T-Code resolve çağrısı yapar; dönen actions/conditions ile UI policy uygulanır. Her şey build + denied/allowed/hata testi ile doğrulanır." 

Bu özet doğru uygulanırsa modül eklemek çocuk oyuncağına döner.

---

## 11) Hazır Scaffold Nerede?

Bu rehberin birebir kopyalanabilir şablonları burada:

- `templates/module-scaffold/README.md`
- `templates/module-scaffold/backend/module-template/...`
- `templates/module-scaffold/checklists/seed-checklist.md.template`
- `templates/module-scaffold/frontend/contract/module.contract.stub.json.template`

Mini kullanım örneği:
1. `{{ModuleName}}` yerine `Inventory` yaz.
2. `{{module-route}}` yerine `inventory` yaz.
3. `{{TCODE_PREFIX}}` yerine `INV` yaz.
4. Seed checklist'i sırayla tamamla.

Scaffold'ı otomatik üretmek için script:

- Dry run:
	- `./scripts/new-module.ps1 -ModuleName Inventory -TCodePrefix INV -DryRun`
- Gerçek üretim:
	- `./scripts/new-module.ps1 -ModuleName Inventory -TCodePrefix INV`

Çıktı klasörü:
- `scaffolds/generated/Inventory`

Not:
- Bu script mevcut çalışma koduna dokunmaz.
- Altyapı tamamlanana kadar sadece şablon üretip kenarda bekletmek için tasarlanmıştır.

