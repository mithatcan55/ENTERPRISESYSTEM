# Manual API Learning Guide

Bu rehber `Host.Api.http` dosyasini egitim amacli kullanmak isteyen gelistiriciler icin yazildi.

Amac:

- endpoint'leri rastgele degil, baglantili bir sira ile denemek
- her cagrinin neyi dogruladigini anlamak
- auth, authorization, operations ve outbox akislarini tek oturumda ogrenebilmek

Temel dosya:

- `src/Host.Api/Host.Api.http`

## 1. Onerilen Ogrenme Sirasi

1. `GET /health`
2. `POST /api/auth/login`
3. `GET /api/sessions`
4. `GET /api/users`
5. `POST /api/permissions/actions`
6. `GET /api/permissions/actions`
7. `GET /api/tcode/{transactionCode}`
8. `GET /api/ops/audit/dashboard/summary`
9. `POST /api/ops/outbox/mail`
10. `GET /api/ops/outbox/messages`
11. negatif senaryolar

## 2. Neden Bu Sirayla

### Health

Ilk adimda uygulamanin ayakta oldugunu gorursun.

### Login

Bu cagridan iki sey kazanirsin:

- token/session bilgisi
- auth lifecycle davranisinin canli testi

### Sessions

Kimlik aldiktan sonra aktif session gorunumu test edilir.

### Users

Bu adim T-Code korumali endpoint'e ulasilabildigini gosterir.

### Permissions Upsert + List

Permission tarafinda create ve list akislarini bir arada test edersin.

### TCode Resolve

Bu endpoint ozellikle debug ve denetim icindir.

Burada:

- `actionCode`
- `amount`
- `userId`
- `companyId`

gibi alanlari degistirerek yetki motorunun davranisini gorebilirsin.

### Audit Dashboard

Toplanan loglardan turetilmis dashboard verisini tek cagrida gorursun.

### Outbox

Sistemin asenkron yan etkileri nasil kuyruklayip isledigini anlarsin.

## 3. Ozel Ogrenme Senaryolari

### Senaryo A: Auth + Session

1. login ol
2. session token'i degiskene yaz
3. `GET /api/sessions`

Bu sirada ogrenilenler:

- auth response yapisi
- session listeleme
- claim fallback mantigi

### Senaryo B: T-Code ve Permission

1. `GET /api/users`
2. `POST /api/permissions/actions`
3. `GET /api/permissions/actions`
4. `GET /api/tcode/SYS03?...`

Bu sirada ogrenilenler:

- ekran bazli yetki
- action permission
- deny / allow sonuc yapisi

### Senaryo C: Operations + Outbox

1. `GET /api/ops/audit/dashboard/summary`
2. `POST /api/ops/outbox/mail`
3. `GET /api/ops/outbox/messages`

Bu sirada ogrenilenler:

- operasyonel KPI
- asenkron is kuyrugu
- pending/succeeded/failed durumlari

## 4. Negatif Senaryolari Neden Calistirmaliyim

Pozitif senaryo tek basina yeterli degildir.

Negatif testler su sorulari cevaplar:

- auth yoksa ne olur?
- token var ama yetki yoksa ne olur?
- kotu request geldiginde sistem ne dondurur?
- T-Code resolve gerekli baglam olmadan ne yapar?

Bu sorular denetim ve frontend entegrasyonu icin kritiktir.

## 5. Beklenen Ogrenim Ciktilari

Bu rehberi ve `.http` dosyasini uygulayarak su kazanimi elde etmelisin:

- login sonrasi hangi endpoint'ler denenir biliyor olmak
- permission ile T-Code farkini canli goruyor olmak
- outbox deseninin neden kullanildigini anlamak
- dashboard ve log endpoint'lerinin ne ise yaradigini bilmek

## 6. Sonuc

`Host.Api.http` bu projede sadece istek koleksiyonu degildir.
Dogru kullanildiginda sistemin canli ogrenme laboratuvari gibi davranir.
