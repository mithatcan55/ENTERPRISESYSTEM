# System Log Pipeline Training

Bu notun amaci, `Host.Api` baslarken ve istekler calisirken loglarin hangi kanallara gittigini netlestirmektir.

## Neden ozel sink kullandik?

Projede zaten `LogDbContext` altinda `logs.system_logs` tablosu var. Hazir bir PostgreSQL sink kullansaydik,
genellikle ikinci bir generic log tablosu olusacakti. Bunun yerine `PostgreSqlSystemLogSink` ile
Serilog olaylarini mevcut `logs.system_logs` tablosuna yazdik.

Dosya:
- `src/Host.Api/Logging/PostgreSqlSystemLogSink.cs`

## Artik hangi log nereye gidiyor?

### 1. Serilog loglari

Ornek:
- `Core bootstrap baslatiliyor`
- `Now listening on: http://localhost:5279`
- `Application started`
- `logger.LogInformation(...)` ile yazilan diger uygulama loglari

Kanallar:
- console
- file
- `logs.system_logs`

### 2. Request lifecycle loglari

`RequestLifecycleLoggingMiddleware` tarafindan yazilanlar:
- request body / response body
- status code
- duration
- user / correlation

Kanallar:
- `logs.http_request_logs`
- `logs.system_logs`
- `logs.performance_logs`
- GET basarili ise `logs.page_visit_logs`

### 3. EF / entity degisiklik loglari

Kanallar:
- `logs.database_query_logs`
- `logs.entity_change_logs`

## Neden hem Serilog hem middleware system_logs'a yaziyor?

Sebep kapsami tamamlamaktir:
- Serilog: framework + startup + application loglari
- middleware: request/response odakli ayrintili operasyon kayitlari

Bu ikisi birlikte "sisteme giristen son cevaba kadar" iz surmeyi saglar.

## Dikkat edilmesi gereken nokta

`system_logs` tablosunda bazi kayitlar fonksiyonel olarak benzer olabilir.
Bu bilincli bir tercihtir; cunku biri insan okunur application log, digeri request-odakli operasyon logudur.
