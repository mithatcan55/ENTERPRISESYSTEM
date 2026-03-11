# Operations Modulu Egitim Rehberi

Bu dokuman `Operations` modülünü anlatir.

Bu modülun temel amaci:

- loglari sorgulanabilir hale getirmek
- denetim dashboard'u uretmek
- operasyon ekiplerine uygulamanin calisma durumunu okunabilir sunmak

## 1. Operations Modulu Neden Var

Uygulamada log toplamak tek basina yeterli degildir.
Asil ihtiyac su sorulari cevaplayabilmektir:

- son 24 saatte hangi security olaylari oldu?
- hangi correlation id hangi hatayla baglantili?
- hangi entity'lerde degisiklik oldu?
- hangi session'lar revoke edildi?
- hata trendi artiyor mu?

Bu modül bu gorunurlugu saglar.

## 2. Ana Dosyalar

- `Operations.Infrastructure/Services/OperationsLogQueryService.cs`
- `Operations.Infrastructure/Services/AuditDashboardService.cs`
- `Operations.Presentation/Controllers/OperationsLogsController.cs`

## 3. OperationsLogQueryService Ne Yapar

Bu servis farkli log tipleri icin ortak bir sorgulama deseni uygular.

Desteklenen akışlar:

- system logs
- security events
- http request logs
- entity change logs
- session admin listesi

Her metotta tekrar eden standart sunlardir:

- paging normalize edilir
- filtreler uygulanir
- `AsNoTracking` ile okuma optimizasyonu yapilir
- DTO projection ile sonuc hazirlanir

### Neden DTO Projection?

Cunku log entity'lerini dogrudan API'ye acmak istemiyoruz.
Hem gereksiz alan sızdirma riski olur, hem de persistence yapisi API'ye baglanir.

### Entity Change Export

`ExportEntityChangeLogsCsvAsync` ayni sorgu mantigini kullanir ama ciktisini CSV'ye cevirir.

Bu ilk surum icin operasyon ekiplerine yeterlidir.
Buyuk veri hacminde streaming export veya job tabanli export dusunulebilir.

### Session Admin Query

Bu metod farkli bir ozellige sahiptir:

- sadece `LogDb` degil
- `IdentityDbContext` icindeki session ve user verisini de kullanir

Yani `Operations` modulu bazen "log oku", bazen de "operasyonel gorunum uret" islevi gorur.

## 4. AuditDashboardService Ne Yapar

Bu servis dashboard ozetini uretir.

Hesapladigi temel metrikler:

- system error count
- failed login count
- failed login trendi
- session revoke rate
- top critical events

Buradaki deger sadece veri cekmek degil, veri yorumlamaktir.

Ornek:

- `revokeRate` bir ham veri degil, turetilmis metriktir

Bu tip metrikler dashboard'un karar destek tarafini guclendirir.

## 5. OperationsLogsController Ne Yapar

Bu controller is kurali yazmaz.

Gorevleri:

- operasyonel sorgu servislerini HTTP'ye acmak
- uygun response tipi donmek
- export endpoint'ini dosya sonucuna cevirmek

Endpoint ornekleri:

- `GET /api/ops/logs/system`
- `GET /api/ops/logs/security`
- `GET /api/ops/logs/http`
- `GET /api/ops/logs/entity-changes`
- `GET /api/ops/logs/entity-changes/export`
- `GET /api/ops/logs/sessions`

## 6. Uctan Uca Entity Change Export Akisi

```text
OperationsLogsController.ExportEntityChanges
  -> OperationsLogQueryService.ExportEntityChangeLogsCsvAsync
    -> QueryEntityChangeLogsAsync
      -> LogDbContext.EntityChangeLogs
    -> CSV string olusturulur
  -> File(byte[], "text/csv")
```

Bu desen sade ama etkilidir:

- sorgu mantigi tekrar edilmez
- export mantigi ayri yerde toparlanir

## 7. Neden Operations Ayri Modül

Bu kodlari host altinda tutmak ilk bakista kolay gorunebilir.
Ama buyuk projede su sorunlari yaratir:

- host sismanlar
- operasyon kodlari business akislardan ayrismaz
- sahiplik net olmaz

Ayri modül su faydayi getirir:

- operasyon ekiplerinin ihtiyaclari tek modülde toplanir
- dashboard ve log sorgu mantigi buyurken business moduller kirlenmez

## 8. Soru-Cevap

### Soru: Operations modulu sadece log mu sorgular?

Cevap:
Hayir. Audit dashboard gibi turetilmis operasyonel gorunumler de olusturur.

### Soru: Neden `AsNoTracking` kullaniliyor?

Cevap:
Cunku bunlar okuma akislaridir. Change tracking gereksiz maliyet olur.

### Soru: Neden export icin ayrica endpoint var?

Cevap:
Operasyon ekipleri bazen ekran goruntusunden fazlasini ister. CSV alma ihtiyaci denetim ve raporlama icin cok yaygindir.

## 9. Sonuc

`Operations` modulu uygulamanin "kendini izleme" kasidir.

Bu modül sayesinde sistem sadece calisan bir backend degil, ayni zamanda:

- okunabilir
- sorgulanabilir
- denetlenebilir

bir platform haline gelir.
