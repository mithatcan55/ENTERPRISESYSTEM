# Identity Modulu Egitim Rehberi

Bu dokumanin amaci `Identity` modülünü sadece dosya listesi olarak degil, calisma mantigi olarak anlatmaktir.

Bu rehberin hedefi sudur:

- projeye yeni giren biri `Identity` modülünü tek basina anlayabilsin
- `Users`, `Roles`, `Permissions` akislarinin nasil calistigini gorebilsin
- `Controller -> Pipeline -> Validator -> PreCheck -> Handler -> DbContext -> Event/Log` zincirini baglantili sekilde ogrenebilsin

## 1. Identity Modulu Ne Is Yapar

`Identity` modulu su alanlardan sorumludur:

- login / refresh / change password
- kullanici yonetimi
- rol yonetimi
- kullaniciya rol atama
- action permission yonetimi
- sifre politikasi

Bu modulu anlamanin en dogru yolu "katmanlar ne tasiyor?" sorusuyla baslamaktir.

## 2. Katmanlar

### 2.1 Identity.Application

Bu katmanin amaci sozlesme tasimaktir.

Burada tipik olarak sunlari bulursun:

- DTO'lar
- request modelleri
- handler interface'leri
- option/config tipleri

Ornekler:

- `Identity.Application/Users/Commands/CreateUserCommand.cs`
- `Identity.Application/Users/Queries/ListUsersQuery.cs`
- `Identity.Application/Roles/Commands/CreateRoleCommand.cs`
- `Identity.Application/Permissions/Commands/UpsertUserActionPermissionCommand.cs`

### 2.2 Identity.Infrastructure

Bu katman gercek isi yapar.

Burada sunlari bulursun:

- query handler implementasyonlari
- command handler implementasyonlari
- validator'lar
- pre-check'ler
- auth/password gibi teknik servisler

Ornekler:

- `Identity.Infrastructure/Users/Commands/CreateUserCommandHandler.cs`
- `Identity.Infrastructure/Roles/Commands/CreateRoleCommandHandler.cs`
- `Identity.Infrastructure/Permissions/Commands/UpsertUserActionPermissionCommandHandler.cs`
- `Identity.Infrastructure/Users/PreChecks/TCodeProtectedRequestPreCheck.cs`
- `Identity.Infrastructure/Permissions/PreChecks/PermissionProtectedRequestPreCheck.cs`

### 2.3 Identity.Presentation

Bu katman HTTP ile konusur.

Buradaki controller'larin gorevi:

- route almak
- auth attribute uygulamak
- request almak
- pipeline cagirip response donmek

Ornek dosyalar:

- `Identity.Presentation/Controllers/UsersController.cs`
- `Identity.Presentation/Controllers/RolesController.cs`
- `Identity.Presentation/Controllers/AuthController.cs`

## 3. Neden Service + CQRS

Eski durumda tek bir servis birden fazla sorumluluk tasiyordu:

- list
- create
- update
- deactivate
- reactivate
- delete

Bu durum uzun vadede su sorunlari uretir:

- servisler siser
- test etmek zorlasir
- bir akisin degisikligi baska akisi kirabilir

Bu nedenle akislar command/query bazinda ayrildi.

Ornek:

- `ListUsersQueryHandler`
- `CreateUserCommandHandler`
- `UpdateUserCommandHandler`
- `DeleteUserCommandHandler`

Bu `Vertical Slice` degildir.

Cunku:

- moduller korunuyor
- feature feature daginik mini projeler yok
- sadece modül icinde is sorumluluklari ayriliyor

## 4. Uctan Uca Users.Create Akisi

`POST /api/users`

Akis:

1. `UsersController.Create` HTTP istegini alir
2. body'den `CreateUserRequest` cozulur
3. controller bir `CreateUserCommand` request modeli olusturur
4. `IRequestExecutionPipeline.ExecuteCommandAsync` cagrilir
5. validator'lar calisir
6. T-Code pre-check calisir
7. `CreateUserCommandHandler` gercek veri yazma isini yapar
8. pipeline basari veya hata eventi uretir
9. observability sistemi bu eventi log/notification'a route eder

Bu akisin buyuk faydasi:

- controller sade
- guvenlik ikinci savunma hatli
- loglama standart
- handler tek ise odakli

## 5. Pipeline Tam Olarak Ne Yapiyor

Ana dosya:

- `src/BuildingBlocks/Infrastructure/Pipeline/RequestExecutionPipeline.cs`

Siralama su sekildedir:

1. operation name cozulur
2. validator'lar calisir
3. pre-check'ler calisir
4. handler calisir
5. event publish edilir

Pipeline olmasaydi:

- her controller veya handler kendi validation mantigini farkli yazardi
- event uretimi daginik olurdu
- admin veya permission gibi ortak kurallar tekrar tekrar kodlanirdi

## 6. Validator ile PreCheck Arasindaki Fark

Bu ayrim cok onemlidir.

### Validator

Soru:

- "Gelen request bicim olarak gecerli mi?"

Ornek:

- bos alan var mi
- string uzunlugu gecerli mi
- sayisal alan pozitif mi

### PreCheck

Soru:

- "Bu request su an bu kullanici/kapsam/ortam icin calistirilabilir mi?"

Ornek:

- kullanici admin mi
- permission var mi
- T-Code erisimi var mi
- company context cozuldu mu

## 7. T-Code Neden Iki Yerde Kontrol Ediliyor

Bu soru ozellikle onemli.

Birinci kontrol:

- controller ustundeki `[TCodeAuthorize(...)]`

Ikinci kontrol:

- pipeline icindeki `TCodeProtectedRequestPreCheck`

### Neden?

Cunku controller attribute sadece HTTP giris noktasi icin gecerlidir.

Ama ayni command ileride:

- baska API katmani
- message consumer
- background worker
- test harness

uzerinden de calisabilir.

Bu durumda ikinci savunma hattina ihtiyac vardir.

## 8. Permission PreCheck Neden Ayrica Var

T-Code ile permission ayni kavram degildir.

- T-Code daha cok ekran / islem kodu mantigidir
- Permission daha serbest capability mantigidir

Bu nedenle iki ayri marker tercih edildi:

- `ITCodeProtectedRequest`
- `IPermissionProtectedRequest`

Bu ayrim ilerde sistemi buyuturken cok fayda saglar.

## 9. CreateUserCommandHandler Detayli Okuma

Dosya:

- `src/Modules/Identity/Identity.Infrastructure/Users/Commands/CreateUserCommandHandler.cs`

Bu handler su adimlari izler:

1. is kurali seviyesinde alanlari kontrol eder
2. `UserCode`, `Username`, `Email` degerlerini normalize eder
3. password policy calistirir
4. duplicate kayit var mi bakar
5. `SYS` modulunu bulur
6. transaction acilir
7. `Users` tablosuna kayit atilir
8. password history kaydedilir
9. module permission seed edilir
10. company permission seed edilir
11. transaction commit edilir
12. gerekiyorsa admin mail kuyruga alinir

### Soru

Neden mail islemi transaction disinda?

### Cevap

Cunku mail ana is kurali degil, yan etkidir.
Mail sorunu yuzunden kullanici kaydi rollback olmamali.

## 10. CreateRoleCommandHandler Detayli Okuma

Dosya:

- `src/Modules/Identity/Identity.Infrastructure/Roles/Commands/CreateRoleCommandHandler.cs`

Bu handler'in mantigi yalindir:

1. `Code` ve `Name` kontrol edilir
2. degerler normalize edilir
3. ayni kod veya isim var mi bakilir
4. yeni role olusturulur
5. DTO olarak geri donulur

### Soru

Neden entity donulmuyor?

### Cevap

Cunku API'nin EF entity yapisina bagimli olmasi istenmiyor.
DTO donerek persistence detayi dis katmandan saklaniyor.

## 11. UpsertUserActionPermissionCommandHandler Detayli Okuma

Dosya:

- `src/Modules/Identity/Identity.Infrastructure/Permissions/Commands/UpsertUserActionPermissionCommandHandler.cs`

Bu handler'in iki kritik ozelligi vardir:

- hem create hem update yapar
- hedef sayfayi hem id hem transaction code ile cozer

Akis:

1. `UserId` gecerli mi?
2. `ActionCode` dolu mu?
3. kullanici var mi?
4. hedef sayfa bulunur
5. mevcut permission aranir
6. varsa guncellenir
7. yoksa yeni kayit acilir
8. soft-delete edilmis kayit varsa tekrar canlandirilir

### Soru

Neden sadece `SubModulePageId` ile calismadik?

### Cevap

Cunku API istemcilerinin hepsi numeric id bilmek zorunda degil.
Bazi istemciler T-Code ile daha rahat calisir.

## 12. UsersController Neden Boyle Yazildi

Ornek dosya:

- `src/Modules/Identity/Identity.Presentation/Controllers/UsersController.cs`

Bu controller'da dikkat edilmesi gereken sey:

- handler dogrudan cagrilmiyor
- once request modeli uretiliyor
- sonra pipeline uzerinden handler cagriliyor

Bu su standardi saglar:

- validation davranisi tek yerde
- pre-check davranisi tek yerde
- event/log davranisi tek yerde

Controller sadece HTTP yuzudur.

## 13. Siniflar Arasi Iliski Haritasi

`Users.Create` icin:

```text
UsersController
  -> CreateUserCommand
  -> RequestExecutionPipeline
    -> CreateUserCommandValidator
    -> TCodeProtectedRequestPreCheck<CreateUserCommand>
    -> CreateUserCommandHandler
      -> BusinessDbContext
      -> PasswordPolicyService
      -> IdentityNotificationService
    -> OperationalEventPublisher
      -> LogEventWriter
      -> Notification Channels
```

`Permissions.Upsert` icin:

```text
PermissionsController
  -> UpsertUserActionPermissionCommand
  -> RequestExecutionPipeline
    -> UpsertUserActionPermissionCommandValidator
    -> PermissionProtectedRequestPreCheck<UpsertUserActionPermissionCommand>
    -> UpsertUserActionPermissionCommandHandler
      -> BusinessDbContext
    -> OperationalEventPublisher
```

## 14. Yeni Bir CRUD Akisi Eklemek Istersem

Ornek: `Departments` akisi ekleyeceksin.

Izlenecek yol:

1. `Identity.Application` veya ilgili modülde request tipini olustur
2. command/query handler interface'lerini ekle
3. infrastructure implementasyonlarini yaz
4. gerekiyorsa validator ekle
5. gerekiyorsa pre-check ekle
6. controller'da pipeline uzerinden cagir
7. operation name'i acik ver
8. build al
9. egitim dokumanina akis diyagramini ekle

## 15. Soru-Cevap

### Soru: Neden her sey service icinde kalmadi?

Cevap:
Tek servis buyudukce okunabilirlik ve degisiklik guvenligi duser.
CQRS ayrimi buyumeyi daha kontrollu hale getirir.

### Soru: Bu kadar dosya fazla degil mi?

Cevap:
Kisa projede fazla gelebilir.
Ama kurumsal projede net sorumluluk ayrimi bakim maliyetini dusurur.

### Soru: Handler icindeki butun validation'lari disari tasiyacak miyiz?

Cevap:
Hayir. Saf request dogrulamalari validator'a tasinabilir.
Ama is kurali olan kisimlar handler veya policy servislerinde kalabilir.

### Soru: T-Code ve permission neden ayni interface degil?

Cevap:
Cunku kavramsal olarak farklilar.
Tek interface altina zorlamak uzun vadede sistemin dilini bozar.

## 16. Sonuc

`Identity` modulu bugun su olgunlukta:

- modül siniri net
- controller'lar ayri presentation katmaninda
- service sisligi kirildi
- CQRS ayrimi aktif
- pipeline standardi var
- T-Code ve permission icin ikinci savunma hatti var
- log/event/notification altyapisina bagli

Bu rehberin hedefi sadece bugunku kodu anlatmak degil, yarin yeni bir geliştiricinin ayni standardi dogru sekilde surdurebilmesini saglamaktir.
