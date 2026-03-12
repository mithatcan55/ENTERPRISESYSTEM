# Observability Deep Dive

Bu dokuman projenin observability omurgasini derinlemesine anlatir.

Odak noktasi:

- `RequestLifecycleLoggingMiddleware`
- `EntityChangeLoggingInterceptor`
- `LogEventWriter`
- `OperationalEventPublisher`

Bu dort parca birlikte okundugunda sistemin "nasil izlenebilir hale getirildigi" daha net anlasilir.

## 1. Buyuk Resim

Observability bu projede tek bir log tablosu meselesi degildir.

Sistem su kanallardan iz birakir:

- HTTP request lifecycle
- business operation events
- SQL command loglari
- entity change loglari
- performance kayitlari
- page visit kayitlari
- security eventleri
- notification routing

## 2. RequestLifecycleLoggingMiddleware

Dosya:

- `src/Host.Api/Middleware/RequestLifecycleLoggingMiddleware.cs`

Bu middleware HTTP seviyesinde ilk buyuk gozlem katmanidir.

Yaptigi isler:

1. request body'yi buffer'lar
2. response body'yi memory stream ile yakalar
3. hassas alanlari redact eder
4. correlation id, user, ip, user-agent gibi baglami toplar
5. `HttpRequestLog` yazar
6. `SystemLog` yazar
7. `PerformanceLog` yazar
8. uygun GET isteklerinde `PageVisitLog` yazar

### Neden Onemli

Bu katman olmadan:

- HTTP davranisini uçtan uca izlemek zorlasir
- request/response bazli denetim zorlasir
- correlation zinciri kaybolur

### Bu Turda Yapilan Teknik Duzeltme

`PerformanceLog` tarafinda artik gercek `memoryBefore`, `memoryAfter` ve `memoryUsed` kaydi uretilecek sekilde duzeltme yapildi.

## 3. EntityChangeLoggingInterceptor

Dosya:

- `src/BuildingBlocks/Infrastructure/Logging/EntityChangeLoggingInterceptor.cs`

Bu interceptor EF `SaveChanges` akisinda devreye girer.

Amaci:

- degisen entity'leri otomatik yakalamak
- bunlari `EntityChangeLog` olarak yazmak

### Akis

1. `SavingChanges` aninda degisecek entry'ler toplanir
2. gecici bir pending listeye konur
3. `SavedChanges` sonrasinda kalici log modeline donusturulur
4. `LogEventWriter` ile `LogDb`'ye yazilir

### Neden Onemli

Bu desen sayesinde her handler veya service icinde:

- "hangi alan degisti" logunu elle yazmak gerekmez

### Bu Turda Yapilan Teknik Duzeltme

Entity diff mantigi daha dogru hale getirildi:

- `Modified` durumda sadece gercekten degisen alanlar loglanir
- `Added/Deleted/Modified` ayrimi daha temiz yapilir
- kayitlar artik once pending modelde tutulup sonra `EntityChangeLog`'a cevrilir

Bu, eski genis ve gürültülü change log davranisini iyilestirir.

## 4. LogEventWriter

Dosya:

- `src/BuildingBlocks/Infrastructure/Observability/LogEventWriter.cs`

Bu sinif tek tek log tiplerini `LogDbContext` icine yazan merkezi yazicidir.

Destekledigi tipler:

- database query
- system
- security
- http
- performance
- page visit
- entity changes

### Neden Onemli

Log yazan her sinif `LogDbContext` bilmek zorunda kalmaz.

Bu su faydayi saglar:

- log persistence detaylari tek yerde toplanir
- write hatalari kontrollu yakalanir
- log yazan siniflar sade kalir

## 5. OperationalEventPublisher

Dosya:

- `src/BuildingBlocks/Infrastructure/Observability/OperationalEventPublisher.cs`

Bu publisher log yazici degildir.
Asil isi routing karari vermektir.

Akis:

1. event gelir
2. route bulunur
3. system log yazilacak mi karar verilir
4. security log yazilacak mi karar verilir
5. performance log yazilacak mi karar verilir
6. notification gonderilecek mi karar verilir

### Neden Onemli

Bu katman sayesinde:

- event ureten kod hedef sink'leri bilmez
- routing config ile degistirilebilir

## 6. Bir Olay Nasil Akar

### Ornek: Business Operation

```text
Controller
  -> OperationLoggingFilter
    -> OperationalEventPublisher
      -> SystemLog
      -> PerformanceLog
      -> gerekirse Notification
```

### Ornek: DB Degisimi

```text
DbContext.SaveChanges
  -> EntityChangeLoggingInterceptor
    -> LogEventWriter
      -> EntityChangeLog
```

### Ornek: HTTP Islem

```text
Request
  -> RequestLifecycleLoggingMiddleware
    -> HttpRequestLog
    -> SystemLog
    -> PerformanceLog
    -> PageVisitLog
```

## 7. Bu Tasarimin Faydasi

1. az kodla daha cok izlenebilirlik
2. denetim izi kaybetmeme
3. log, event ve notification ayrimini netlestirme
4. yeni modul eklerken ayni omurgayi tekrar kurmama

## 8. Soru-Cevap

### Soru: Request log, operation event ve entity change log neden ayri?

Cevap:
Cunku her biri farkli katmanin gercegini temsil eder:

- HTTP katmani
- business operation katmani
- persistence katmani

### Soru: Entity change niye handler icinde yazilmiyor?

Cevap:
Cunku unutulur ve tekrar eder. Interceptor bu isi merkezi ve otomatik yapar.

### Soru: LogEventWriter neden kendi SaveChanges'ini yapiyor?

Cevap:
Cunku log yazimi ana business context'ten ayrik LogDb baglaminda, bagimsiz ve dayanıklı yurumelidir.

## 9. Sonuc

Bu projede observability sonradan eklenmis bir log paketi degildir.
Mimariye yerlestirilmis bir omurgadir.

Bu omurgayi anlamak, sistemin neden denetlenebilir ve operasyonel olarak guclu oldugunu anlamak demektir.
