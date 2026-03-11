# Error Policy ve Notification Routing Egitim Rehberi

Bu dokuman hata yonetimi, operational event yayini ve notification routing mantigini anlatir.

Amac:

- exception ile log arasindaki bagi gostermek
- event backbone'in neden kuruldugunu aciklamak
- notification routing mantigini ogretmek

## 1. Eski Problem Neydi

Klasik projelerde su daginiklik olur:

- bir yerde logger'a yazilir
- baska yerde mail atilir
- bir hata sadece response'e doner
- baska hata hic bildirilmez

Bu durumda sistem davranisi standardize edilemez.

## 2. Yeni Yaklasim

Bu projede ana fikir su:

- once olay tanimlanir
- sonra bu olay route edilir

Yani:

`Exception / Handler / Middleware`
-> `OperationalEvent`
-> `OperationalEventPublisher`
-> `Log + Security Log + Performance Log + Notification`

## 3. Ana Dosyalar

- `src/Host.Api/Exceptions/GlobalExceptionHandler.cs`
- `src/BuildingBlocks/Infrastructure/Observability/OperationalEventPublisher.cs`
- `src/BuildingBlocks/Infrastructure/Observability/ObservabilityRoutingOptions.cs`

## 4. GlobalExceptionHandler Nasil Calisir

Bu sinif iki farkli cikti uretir:

1. istemciye `ProblemDetails`
2. sistemin icine `OperationalEvent`

Yani hata sadece HTTP cevabi degildir.
Ayni zamanda log, audit ve notification akislarinin da girisidir.

### MapException Mantigi

Eger hata `AppException` ise:

- status code bellidir
- error code bellidir
- validation errors olabilir

Eger beklenmeyen exception ise:

- `500`
- `internal_error`

doner.

## 5. OperationalEventPublisher Nasil Calisir

Publisher'in gorevi log yazmak degil, route karari vermektir.

Akis:

1. gelen event icin route bulunur
2. route uygunsa system log yazilir
3. gerekiyorsa security log yazilir
4. gerekiyorsa performance log yazilir
5. gerekiyorsa notification uretilir

Bu katmanin degeri su:

- event ureten kod hedef sink'leri bilmek zorunda kalmaz

## 6. Routing Mantigi

`ObservabilityRoutingOptions` icinde route listesi vardir.

Her route:

- hangi event adina uygulanacagini
- hangi minimum severity'de aktif olacagini
- system log yazip yazmayacagini
- security log yazip yazmayacagini
- notification gonderip gondermeyecegini

belirler.

### Yildiz Route

`EventName = "*"` ise fallback route gibi davranir.

Bu tum event'leri kapsayan genel rota icindir.

## 7. Notification Nasil Uretiliyor

Publisher event'i dogrudan email formatiyla tasimaz.

Once `NotificationMessage` olusturur.

Icerik:

- event name
- severity
- subject
- body
- metadata

Sonra uygun notification kanallarina dagitir.

Bu sayede ayni event:

- email
- webhook
- ileride teams/slack

kanallarina ayni mantikla yayilabilir.

## 8. Log ile Notification Neden Ayri

Loglama amaci:

- kalici kayit
- denetim
- arama ve filtreleme

Notification amaci:

- dikkat cekme
- operasyona sinyal verme

Her kritik olay loglanir ama her olay notification olmak zorunda degildir.

Bu ayrim routing policy ile yonetilir.

## 9. Hangi Hata Hangi Davranisa Gitmeli

Tipik kural ornekleri:

- validation hatasi
  - warning log
  - notification yok

- forbidden / security deny
  - security log
  - gerekli ise notification

- unhandled exception
  - system log
  - error severity
  - ops notification

- slow operation
  - performance log
  - kritik eşiği asarsa notification

## 10. Soru-Cevap

### Soru: Neden exception dogrudan logger'a yazilmiyor?

Cevap:
Cunku o zaman log bir hedefe baglanmis olur. Event backbone ise birden fazla hedefi ayni anda besler.

### Soru: Her event notification olmali mi?

Cevap:
Hayir. Aksi halde operasyon ekipleri alarm yorgunlugu yasar.

### Soru: Neden route'larda severity filtresi var?

Cevap:
Cunku ayni event tipi farkli siddette gelebilir. Her varyasyonu ayni sekilde ele almak istemeyiz.

## 11. Sonuc

Bu proje icin hata yonetimi artik yalnizca "catch edip cevap donmek" degil:

- standard event uretmek
- dogru loglara yazmak
- gerekiyorsa notification cikarabilmek

anlamina gelir.

Bu omurga buyudukce sistemin operasyonel olgunlugu da artar.
