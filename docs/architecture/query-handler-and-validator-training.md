# Query Handler ve Validator Egitim Rehberi

Bu dokuman command/query pipeline icindeki iki kritik rolü anlatir:

- query handler
- validator

Amac:

- bu iki katmanin neden ayri oldugunu gostermek
- hangi kuralin nerede yasanmasi gerektigini netlestirmek

## 1. Query Handler Nedir

Query handler, veri degistirmeyen okuma akislarinin sahibidir.

Tipik gorevleri:

- filtre uygulamak
- projection yapmak
- siralama yapmak
- sayfalama yapmak
- DTO olarak sonuc donmek

Query handler'in yapmamasi gerekenler:

- veri yazmak
- transaction yonetmek
- yan etki olusturmak

## 2. Neden Ayri Query Handler Kullaniyoruz

Sebep:

- okuma akislarini command akislarindan ayirmak
- performans dusuncesini daha temiz uygulamak
- `AsNoTracking`, projection ve filtre mantigini tek yerde tutmak

## 3. Ornek Query Handler'lar

### ListUsersQueryHandler

Ne yapar:

- silinmemis kullanicilari getirir
- `CreatedAt desc` siralar
- `UserListItemDto` projection'i yapar

### ListRolesQueryHandler

Ne yapar:

- silinmemis role'leri getirir
- role ismine gore siralar
- sade liste DTO'su doner

### ListUserActionPermissionsQueryHandler

Ne yapar:

- permission tablosu ile sayfa tablosunu join eder
- `userId` zorunlu filtre uygular
- ister page id ile ister transaction code ile daraltma yapar

## 4. Validator Nedir

Validator request'in "seklin dogrulugunu" kontrol eder.

Tipik sorular:

- zorunlu alan var mi?
- sayisal alan pozitif mi?
- iki alandan biri zorunlu mu?

Validator'in yapmamasi gerekenler:

- duplicate sorgusu
- transaction
- entity yazimi
- agir is kurali orkestrasyonu

## 5. Neden Handler Degil de Validator

Cunku handler'in asli isi business state degistirmektir.
Validator ise handler'a girecek request'in daha kapida temizlenmesini saglar.

Bu faydalari getirir:

- tekrar azalir
- test kolaylasir
- pipeline ile uyumlu ortak davranis elde edilir

## 6. Hangi Kural Nerede Yasar

### Validator'da:

- `UserCode` bos olamaz
- `CompanyId` pozitif olmali
- `SubModulePageId` veya `TransactionCode` verilmis olmali

### Handler'da:

- ayni kullanici zaten var mi
- ayni role code zaten var mi
- kullanici gercekten var mi
- hedef sayfa gercekten var mi

### Policy Service'de:

- password complexity
- password history
- minimum password age

## 7. Soru-Cevap

### Soru: Query handler neden DTO donuyor?

Cevap:
API'nin EF entity yapisina bagimli olmamasi icin.

### Soru: Validator neden DB sorgusu yapmiyor?

Cevap:
Yapabilir ama burada tercih edilmiyor. DB isteyen kurallar genelde handler veya domain/policy katmanina daha uygundur.

### Soru: Query handler neden `AsNoTracking` kullaniyor?

Cevap:
Cunku veri degistirmiyor. Change tracking gereksiz maliyet olur.

## 8. Sonuc

Query handler okuma akislarini temizler.
Validator request girisini temizler.

Bu ayrim oturdugunda handler'lar daha okunabilir ve daha guvenli hale gelir.
