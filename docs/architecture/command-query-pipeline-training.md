# Command/Query Pipeline Egitim Rehberi

Bu dokumanin amaci iki seydir:

1. Projede kurulan ortak `command/query pipeline` katmaninin ne yaptigini ogretmek
2. Yeni modul yazarken ayni standardi nasil kullanacagini gostermek

## 1. Problem Neydi

Handler'lari ayirdik ama su riskler hala vardi:

- validation her handler icine dagiliyordu
- admin gibi ortak yetki kontrolleri tekrar yazilabiliyordu
- command/query seviyesinde sure, hata ve event uretimi standart degildi

Yani sadece handler ayirmak yetmez.
Onlari calistiran ortak bir katman gerekir.

## 2. Cozumun Ozeti

Yeni yapi:

```text
Controller
  -> Request modeli olusturur
  -> IRequestExecutionPipeline cagirir
      -> Validator'lari calistirir
      -> Pre-check'leri calistirir
      -> Handler'i cagirir
      -> Basari/basarisizlik eventi uretir
  -> Sonucu doner
```

## 3. Temel Dosyalar

- `src/BuildingBlocks/Application/Pipeline/IRequestExecutionPipeline.cs`
- `src/BuildingBlocks/Application/Pipeline/IRequestValidator.cs`
- `src/BuildingBlocks/Application/Pipeline/IRequestPreCheck.cs`
- `src/BuildingBlocks/Application/Pipeline/IAdminOnlyRequest.cs`
- `src/BuildingBlocks/Infrastructure/Pipeline/RequestExecutionPipeline.cs`

## 4. Her Dosya Ne Ise Yarar

### 4.1 `IRequestExecutionPipeline`

Bu arayuz pipeline'in dis yuzudur.
Controller bunu gorur.

Uclu gorevi vardir:

- query calistirmak
- response donen command calistirmak
- response donmeyen command calistirmak

### 4.2 `IRequestValidator<TRequest>`

Belirli bir request tipi icin dogrulama kuralini tasir.

Ornek:

- `CreateUserCommandValidator`
- `CreateRoleCommandValidator`
- `UpsertUserActionPermissionCommandValidator`

Buradaki kural:

- handler is kurali yazar
- validator giris dogrulamasini yapar

### 4.3 `IRequestPreCheck<TRequest>`

Validation'dan farkli olarak yetki, kapsam, ortam veya islem on-kosulu gibi kontrolleri tasir.

Ornek kullanimlar:

- "bu request yalnizca SYS_ADMIN tarafindan calisabilir"
- "company context olmadan bu command calisamaz"
- "T-Code kontrolu command seviyesinde de yapilsin"

Bu turda sonuncusunun ilk gercek ornegi eklendi:

- `TCodeProtectedRequestPreCheck<TRequest>`

Bu sinif `Users` command/query request'lerini pipeline icinde tekrar T-Code kontrolunden gecirir.
Yani sadece controller attribute'una guvenilmez.

### 4.4 `IAdminOnlyRequest`

Bu bir marker interface'tir.
Ici bostur.

Ama anlami sunudur:

- bu request admin yetkisi ister

Pipeline bunu gorur ve ortak kontrol uygular.

## 5. RequestExecutionPipeline NasIl Calisiyor

### Adim 1: Operation name belirlenir

Controller isterse acik operation adi verir:

```csharp
"Users.Create"
```

Vermezse request tipi adi kullanilir.

### Adim 2: Validator'lar calisir

Pipeline:

- `IRequestValidator<TRequest>` kayitlarini bulur
- hepsini sirayla calistirir

Bir validator hata firlatirsa handler hic calismaz.

### Adim 3: Pre-check'ler calisir

Burada:

- `IAdminOnlyRequest` marker'i kontrol edilir
- varsa `SYS_ADMIN` zorunlu tutulur
- sonra varsa `IRequestPreCheck<TRequest>` implementasyonlari calisir

### Adim 4: Handler calisir

Burada asil is mantigi calisir.

### Adim 5: Event uretimi

Basarili ise:

- `CommandExecuted`
- `QueryExecuted`

Basarisiz ise:

- `CommandFailed`
- `QueryFailed`

eventleri uretilir.

Bu eventler sonra:

- system log
- performance log
- notification

gibi sink'lere route edilir.

## 6. Controller Tarafinda Nasil Kullaniliyor

### Ornek: Users Create

Controller artik dogrudan sadece handler cagirmiyor.
Araya pipeline giriyor.

Mantik:

1. `CreateUserCommand` request modeli olusturulur
2. Pipeline cagrilir
3. Pipeline validator ve pre-check calistirir
4. Handler gercek isi yapar

Bu sayede controller sade kalir ama standart davranis kaybolmaz.

## 7. Neden Handler'a Validation Koymuyoruz

Soru:
Handler zaten validation yapabilir, neden ayri validator lazim?

Cevap:

- tekrar azaltir
- test etmeyi kolaylastirir
- ortak davranis olusturur
- handler'i giris kirinden temizler

Handler'in asil isi:

- state degistirmek
- transaction yonetmek
- repository/dbcontext ile calismak
- business rule uygulamak

## 8. Neden Bu Vertical Slice Degil

Cunku burada amac:

- her endpoint icin bagimsiz feature klasoru acmak degil
- service + CQRS standardi kurmak

Dosya yapisi hala modul odakli.
Sadece modul icinde command/query ayrimi var.

## 9. Simdiye Kadar Nerelerde Kullanildi

Bu turda pipeline su akislara baglandi:

- Users
- Roles
- Permissions

Yani yeni CRUD standardi bu uc akista gorulebilir.

## 10. Sonraki Genisleme Adimlari

Bu katman daha da buyutulebilir:

1. `IRequestPreCheck<TRequest>` ile T-Code command seviyesi kontrolu
2. `IRequestPreCheck<TRequest>` ile company scope kontrolu
3. FluentValidation benzeri validator entegrasyonu
4. retry / idempotency davranisi
5. transaction behavior
6. cache invalidation behavior

## 11. Kisa Soru-Cevap

### Soru: Controller neden direkt handler cagirmiyor?

Cevap:
Direkt cagirirsa ortak davranis dagilir.
Pipeline bu daginikligi engeller.

### Soru: Her request icin validator yazmak zorunlu mu?

Cevap:
Hayir.
Validator yoksa pipeline sessizce devam eder.

### Soru: Admin kontrolu neden marker interface ile yapildi?

Cevap:
Cunku tekrar eden yetki kurali tek yerde calissin istedik.

### Soru: T-Code zaten controller attribute ile kontrol ediliyor, neden bir de pre-check eklendi?

Cevap:
Controller attribute birinci savunma hattidir.
Pipeline pre-check ikinci savunma hattidir.
Bu sayede ileride ayni command farkli bir giris noktasindan da calisirsa yetki mantigi korunur.

### Soru: Handler icindeki tum validation'lar kaldirilacak mi?

Cevap:
Hayir, bir anda degil.
Kademeli olarak tasinacak.
Bu daha guvenli.

## 12. Ozet

Bu katmanin ana fikri:

- request once standart bir borudan gecer
- sonra handler'e ulasir

Yani:

- validation standart
- pre-check standart
- log/event standart
- handler daha temiz

Bu kurgu oturdugunda yeni CRUD modulu cikarirken ayni altyapiyi tekrar kurmak gerekmez.
