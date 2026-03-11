# Authorization Modulu Egitim Rehberi

Bu dokuman `Authorization` modülünü teknik ve egitsel dille anlatir.

Temel hedefler:

- T-Code modelinin seviyelerini aciklamak
- permission modeli ile T-Code modelinin farkini gostermek
- HTTP authorization handler ile servis katmani arasindaki iliskiyi anlatmak
- deny kararinin nasil uretildigini adim adim gostermek

## 1. Authorization Modulu Ne Icin Var

Bu modül kullanicinin bir kaynaga, ekrana veya aksiyona erisip erisemeyecegini belirler.

Projede iki temel yetki yaklasimi vardir:

- T-Code bazli yetki
- Permission bazli yetki

Bu ikisi ayni sey degildir.

### T-Code

Daha cok ekran, menu, sayfa ve islem kodu mantigiyla calisir.

Ornek:

- `SYS01`
- `SYS03`

### Permission

Daha serbest operasyon kodlari ile calisir.

Ornek:

- `PERMISSIONS_READ`
- `PERMISSIONS_WRITE`
- `PERMISSIONS_DELETE`

## 2. Proje Yapisi

`Authorization` modulu su projelere ayrilir:

- `Authorization.Application`
- `Authorization.Infrastructure`
- `Authorization.Presentation`

### Authorization.Application

Bu katmanda sunlar bulunur:

- contract'lar
- security modelleri
- servis arayuzleri

Ornekler:

- `ITCodeAuthorizationService`
- `IPermissionAuthorizationService`
- `TCodeAccessResult`

### Authorization.Infrastructure

Bu katman gercek yetki cozumlemesini yapar.

Ornekler:

- `TCodeAuthorizationService`
- `PermissionAuthorizationService`
- `TCodeAuthorizationHandler`
- `PermissionAuthorizationHandler`

### Authorization.Presentation

Burada operasyonel endpoint'ler bulunur.

Ornek:

- `TCodeController`

## 3. T-Code 6 Seviye Modeli

Bu projede T-Code yetki modeli asagidaki seviyelerle ilerler:

1. modul
2. submodule
3. page
4. company scope
5. action
6. condition

Bu seviye mantigi neden onemli?

Cunku "izin var / yok" yerine "hangi seviyede neden yok?" sorusuna cevap verir.

Bu denetim ve debug icin cok degerlidir.

## 4. TCodeAuthorizationService Nasil Calisir

Ana dosya:

- `src/Modules/Authorization/Authorization.Infrastructure/Services/TCodeAuthorizationService.cs`

Servis su adimlarla calisir:

1. gelen `transactionCode` normalize edilir
2. `SubModulePage` bulunur
3. bagli `SubModule` bulunur
4. bagli `Module` bulunur
5. level 1 module yetkisi kontrol edilir
6. level 2 submodule yetkisi kontrol edilir
7. level 3 page yetkisi kontrol edilir
8. level 4 company scope kontrol edilir
9. level 5 action permission kontrol edilir
10. level 6 condition kontrol edilir
11. sonuc `TCodeAccessResult` olarak doner

### Onemli Nokta

Bu servis sadece boolean donmez.

Donen bilgi icerir:

- allowed / denied
- denied level
- denied reason
- action listesi
- condition sonuclari
- eksik context alanlari

Bu zengin sonuc sayesinde UI, log, audit ve debug akislarinin hepsi ayni sozlesmeyi kullanabilir.

## 5. Level 5 Action Mantigi

Soru:

- Kullanici bir sayfaya girebiliyor ama her aksiyonu yapabilir mi?

Cevap:

- Hayir.

Level 5 tam burada devreye girer.

Mantik:

- eger action permission hic tanimlanmadiysa geriye donuk uyumluluk bozulmasin diye deny vermeyiz
- ama action permission kayitlari varsa artik istenen action acikca izinli olmalidir

Ornek:

- kullanici `SYS01` ekranina girebilir
- ama `DELETE` aksiyonu icin kaydi yoksa deny alir

## 6. Level 6 Condition Mantigi

Condition seviyesi su soruya cevap verir:

- "Bu kullanici bu aksiyonu hangi veri kosullarinda yapabilir?"

Ornek condition'lar:

- `amount <= 1000`
- `companyId = 2`
- `documentType in A,B,C`

Servis su isi yapar:

- request context icinden alan degerlerini toplar
- veritabanindaki condition kurallariyla karsilastirir
- tatmin olmayanlari listeler

`denyOnUnsatisfiedConditions = true` ise deny verir.

Bu alan operasyonel olarak onemlidir.

Cunku bazen UI:

- condition'lari sadece bilgi icin gormek ister

bazen de:

- dogrudan deny mekanizmasi ister

## 7. TCodeAuthorizationHandler Ne Is Yapar

Dosya:

- `src/Modules/Authorization/Authorization.Infrastructure/Security/TCodeAuthorizationHandler.cs`

Bu sinifin gorevi karar vermek degildir.

Gorevi:

- HTTP context'ten user/company bilgisini okumak
- route/query degerlerini toplamak
- gerekirse action code infer etmek
- bu veriyi `TCodeAuthorizationService`'e tasimak

Yani handler daha cok adaptor gibidir.

## 8. Action Code Infer Neden Var

Eger endpoint attribute icinde action code acik verilmediyse HTTP method'tan cikarim yapilir.

Ornek:

- `GET` -> `READ`
- `PUT` -> `UPDATE`
- `DELETE` -> `DELETE`
- `POST /deactivate` -> `DEACTIVATE`

Bu sayede her endpoint'te tekrar tekrar action code yazmak zorunlu olmaz.

Ama kritik yerlerde acik yazmak hala daha guvenlidir.

## 9. PermissionAuthorizationService Nasil Calisir

Dosya:

- `src/Modules/Authorization/Authorization.Infrastructure/Services/PermissionAuthorizationService.cs`

Bu servis iki asamali calisir:

1. once claim icinde permission var mi diye bakar
2. yoksa veritabanina duser

Bu neden iyi?

- token icinde permission varsa hizli karar verilir
- claim eksik veya stale ise DB ikinci kaynaktir

Bu hibrit yaklasim performans ve dogruluk arasinda iyi denge kurar.

## 10. TCodeController Neden Var

Dosya:

- `src/Modules/Authorization/Authorization.Presentation/Controllers/TCodeController.cs`

Bu endpoint sadece son kullanici ekrani icin degil, ayni zamanda operasyonel debug icindir.

Sunlari test etmeyi kolaylastirir:

- bu kullanici bu T-Code'a girebilir mi?
- neden deny aldi?
- action kodu etkiliyor mu?
- amount gibi context alanlari sonucu degistiriyor mu?

### Ornek Senaryo

Istek:

```text
GET /api/tcode/SYS01?actionCode=DELETE&amount=1500
```

Servis sunlari dondurebilir:

- `DeniedAtLevel = 5`
- veya `DeniedAtLevel = 6`

Bu operasyonel ariza analizinde cok degerlidir.

## 11. Uctan Uca T-Code Akisi

```text
UsersController
  -> [TCodeAuthorize("SYS01", "CREATE")]
  -> ASP.NET Authorization Pipeline
    -> TCodeAuthorizationHandler
      -> TCodeAuthorizationService
        -> AuthorizationDbContext
        -> OperationalEventPublisher
          -> Log / Notification
  -> RequestExecutionPipeline
    -> TCodeProtectedRequestPreCheck<CreateUserCommand>
      -> TCodeAuthorizationService
```

Bu diyagramdaki kritik detay:

- controller attribute birinci savunma hattidir
- request pre-check ikinci savunma hattidir

## 12. Permission Akisinin Uctan Uca Ozeti

```text
PermissionsController
  -> Permission attribute veya pipeline request modeli
  -> PermissionAuthorizationHandler / PermissionProtectedRequestPreCheck
    -> PermissionAuthorizationService
      -> claim kontrolu
      -> gerekirse AuthorizationDbContext
```

## 13. Soru-Cevap

### Soru: Neden T-Code ve Permission ayri servislerde?

Cevap:
Kavramsal olarak farklilar. Tek servis altina zorlamak tasarimi bulaniklastirir.

### Soru: Neden deny level tutuyoruz?

Cevap:
Yetkisizligi sadece "403" diye bilmek yetmez. Kurumsal denetimde neden reddedildigi de kritik bilgidir.

### Soru: Condition sonucu neden detayli donuyor?

Cevap:
UI, debug ve audit akislarinin her biri ayni sonucu farkli nedenle kullanabilir.

### Soru: Claim varsa neden yine DB kontrolu dusunuluyor?

Cevap:
Claim her zaman en guncel veri olmayabilir. Bu nedenle hibrit model daha sagliklidir.

## 14. Yeni Yetki Kuralini Nereye Koymaliyim

Eger yeni kural:

- T-Code ekran/aksiyon modeliyle ilgiliyse `TCodeAuthorizationService`
- serbest permission kodu ile ilgiliyse `PermissionAuthorizationService`
- HTTP'ye ozel veri topluyorsa handler katmani
- request seviyesinde ikinci savunma hatti gerektiriyorsa pre-check katmani

uygun yerdedir.

## 15. Sonuc

`Authorization` modulu bugun su kazanimi sagliyor:

- host katmanindan ayrik
- tek merkezli yetki karari
- 6 seviyeli T-Code modeli
- action ve condition enforcement
- deny reason ve level raporlama
- event/log/notification altyapisina entegre calisma

Bu modulu dogru anlamak, projenin geri kalanindaki guvenlik modelini dogru anlamak demektir.
