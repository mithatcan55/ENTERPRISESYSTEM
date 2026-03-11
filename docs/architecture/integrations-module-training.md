# Integrations Modulu Egitim Rehberi

Bu dokuman `Integrations` modülünün neden var oldugunu, outbox desenini nasil kullandigini ve HTTP isteginden asenkron islemeye giden yolu anlatir.

## 1. Modülun Ana Amaci

`Integrations` modulu su alanlardan sorumludur:

- external outbox
- e-posta gonderimi
- rapor olusturma ve dagitma
- dis sistem veri gecitleri
- notification kanallarinin entegrasyon seviyesi

Bu modülun ana degeri su soruyu cevaplamasidir:

- "Bir yan etkiyi ana islemden koparmak ve daha dayanikli hale getirmek istiyorsak bunu nasil yapariz?"

## 2. Outbox Neden Gerekli

Dogrudan mail gondermek veya dosya uretmek cazip gorunur.
Ama bu su riskleri getirir:

- HTTP istegi uzun surer
- gecici bir dis servis hatasi ana islemi bozabilir
- retry mantigi daginik hale gelir
- denetim kaydi zorlasir

Outbox deseni bu sorunu cozer.

Akis:

1. API istegi gelir
2. islem dogrudan dis servisi cagirmak yerine bir outbox mesaji olusturur
3. mesaj veritabanina yazilir
4. arka plandaki dispatcher uygun zamanda bu mesaji isler
5. basariliysa `Succeeded`, degilse `Failed` veya `DeadLetter` olur

## 3. Ana Dosyalar

- `Integrations.Infrastructure/Services/ExternalOutboxService.cs`
- `Integrations.Infrastructure/Services/ExternalOutboxDispatcherService.cs`
- `Integrations.Presentation/Controllers/OutboxController.cs`

## 4. ExternalOutboxService Ne Yapar

Bu servis API tarafindaki giris noktalarina yakindir.

Gorevleri:

- mail kuyruklama
- excel rapor kuyruklama
- mevcut outbox mesajlarini listeleme

Onemli nokta:

- burada dis servise gitmeyiz
- sadece dayanıklı bir mesaj kaydi yazariz

### QueueMail

`QueueMailAsync`:

- `to`, `subject`, `body` alanlarini kontrol eder
- payload olusturur
- `EnqueueAsync` ile `ExternalOutboxMessage` kaydi acar

### QueueExcelReport

`QueueExcelReportAsync`:

- rapor adini kontrol eder
- satir ve baslik payload'ini olusturur
- ayni kuyruklama desenini kullanir

### EnqueueAsync

Bu metod kritik bir yerdir.

Burada:

- payload JSON'a cevrilir
- status `Pending` olur
- correlation id tasinir
- deduplication key uretilir

### Soru

Neden correlation id outbox kaydina yaziliyor?

### Cevap

Cunku daha sonra asenkron islenen bir mesaji ilk HTTP istegiyle baglamak isteriz. Bu, hata analizi ve denetim icin cok degerlidir.

## 5. ExternalOutboxDispatcherService Ne Yapar

Bu sinif bir `BackgroundService` olarak calisir.

Sonsuz dongu mantigi:

1. bekleyen mesajlari bul
2. uygun parti boyutuyla cek
3. islemeye al
4. event type'a gore uygun servisi cagir
5. basariliysa `Succeeded`
6. hata varsa retry planla veya `DeadLetter` yap

### EventType Ayrimi

Su an iki temel event tipi var:

- `MailNotification`
- `ExcelReport`

Bu desen buyuyebilir:

- webhook
- sms
- external ERP callback

### Retry Mantigi

Hata halinde:

- `AttemptCount` artar
- exponential backoff uygulanir
- hala hak varsa `Failed`
- tavan dolduysa `DeadLetter`

Bu ne saglar?

- gecici hatalara karsi dayaniklilik
- sonsuz retry cehennemini engelleme
- operasyon ekibine temiz durum gorunumu

## 6. OutboxController Ne Yapar

Bu controller operasyon ekipleri icin API kapisidir.

Endpoint'ler:

- `GET /api/ops/outbox/messages`
- `POST /api/ops/outbox/mail`
- `POST /api/ops/outbox/excel`

Onemli ayrim:

- `POST` endpoint'leri 202 Accepted doner

Sebep:

- istek kabul edilmistir
- ama gercek islem daha sonra dispatcher tarafindan yapilacaktir

## 7. Uctan Uca Mail Kuyruklama Akisi

```text
OutboxController.QueueMail
  -> ExternalOutboxService.QueueMailAsync
    -> EnqueueAsync
      -> IntegrationsDbContext.ExternalOutboxMessages
  -> 202 Accepted

Background:
ExternalOutboxDispatcherService
  -> pending message alir
  -> payload deserialize eder
  -> IEmailDeliveryService cagirir
  -> status gunceller
```

## 8. Bu Modül Neden Ayri

Eskiden bu tarz kodlar host icinde daginik olabilir.
Ama ayri modüle almak su faydayi saglar:

- dis servis entegre kodlari tek yerde toplanir
- retry ve outbox mantigi business modullerden ayrilir
- operasyon ekipleri bu modülü tek basina takip edebilir

## 9. Soru-Cevap

### Soru: Neden mail direkt gonderilmiyor?

Cevap:
Ana islem dis servis hatasi yuzunden bozulmasin diye.

### Soru: Neden `Accepted` donuyoruz?

Cevap:
Cunku gercek islem daha sonra dispatcher tarafinda yapiliyor.

### Soru: Neden `DeadLetter` durumu var?

Cevap:
Ayni mesaji sonsuza kadar denemek sistemde gürültü uretir. Bir noktada operasyonel mudahale gerekir.

## 10. Sonuc

`Integrations` modulu su problemi cozer:

- dis dunya ile konusurken ana is akisini kirmadan, izlenebilir ve tekrar denenebilir bir altyapi saglamak

Bu modülü iyi anlamak, sistemin "dayaniklilik" tarafini anlamak demektir.
