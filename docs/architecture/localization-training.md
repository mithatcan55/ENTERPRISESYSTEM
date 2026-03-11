# Localization Egitim Rehberi

Bu dokuman proje icindeki localization mantigini anlatir.

Amaç:

- dil secimi nasil calisiyor gostermek
- JSON resource tabanli yapiyi aciklamak
- hata mesajlari ile localization arasindaki bagi netlestirmek

## 1. Neden Localization Var

Kurumsal sistemlerde farkli dil ihtiyaci sadece UI tarafinda olmaz.
API hata mesajlari, operasyonel mesajlar ve denetim ciktisi da tutarli dil davranisi ister.

Bu projede hedef diller:

- `tr-TR`
- `en-US`
- `de-DE`

## 2. Ana Dosyalar

- `src/Host.Api/Localization/ApiTextLocalizer.cs`
- `src/Host.Api/Localization/Resources/tr-TR.json`
- `src/Host.Api/Localization/Resources/en-US.json`
- `src/Host.Api/Localization/Resources/de-DE.json`

## 3. ApiTextLocalizer Nasil Calisir

Bu sinifin mantigi basittir ama dogru kurulmustur:

1. uygulama basladiginda resource dosyalari tembel olarak yuklenir
2. once tam culture aranir
3. tam culture yoksa neutral culture fallback uygulanir
4. o da yoksa `en-US`
5. hala yoksa key'in kendisi doner

Bu strateji neden iyidir?

- yeni dil eklemek kolaydir
- tum culture'ler eksiksiz olmak zorunda degildir
- hicbir durumda sistem null donmez

## 4. Resource Dosyalari Ne Tasir

Her JSON dosyasi bir anahtar-deger sozlugudur.

Ornek:

```json
{
  "validation_title": "Dogrulama Hatasi",
  "internal_error_title": "Sunucu Hatasi"
}
```

Bu anahtarlarin sabit tutulmasi onemlidir.

Sebep:

- kod anahtara baglanir
- ceviri dosyasi mesaja baglanir
- boylece metin degisimi kodu bozmaz

## 5. Fallback Mantigi

Ornek senaryo:

- `CultureInfo.CurrentUICulture.Name = en-GB`

Sistem sirayla sunu dener:

1. `en-GB`
2. `en-US`
3. key'in kendisi

Bu, kulturlere karsi dayanikli davranis saglar.

## 6. Localization ve Hata Yonetimi Iliskisi

`GlobalExceptionHandler` hata title ve detail alanlarini localization ile uretebilir.

Mantik:

- `AppException` varsa status code'a gore title secilir
- varsa `errorCode` localization anahtari gibi kullanilir
- gelistirme modunda exception mesaji daha dogrudan gosterilebilir

Bu su faydayi saglar:

- uygulama teknik hata kodu ile konusur
- kullanici ise kendi dilinde aciklama gorur

## 7. ErrorCode Neden Onemli

`errorCode` sadece frontend icin degil, localization icin de omurgadir.

Ornek:

- `internal_error`
- `rate_limited`
- `tcode_request_precheck_failed`

Bu kodlar:

- log filtrelemede
- istemci davranisinda
- ceviri anahtari cozumlemede

ortak sabit gibi davranir.

## 8. Yeni Bir Metin Ekleyeceksem Ne Yapmaliyim

1. once yeni anahtari belirle
2. `tr-TR.json`, `en-US.json`, `de-DE.json` dosyalarina ekle
3. kod icinde sabit Turkce metin yerine bu anahtari kullan
4. build al

## 9. Soru-Cevap

### Soru: Neden .resx degil JSON?

Cevap:
Bu projede okunabilirlik ve API/ops odakli duzenleme kolayligi icin JSON secildi.

### Soru: Neden key bulunamayinca null donmuyor?

Cevap:
Null sessiz hata uretir. Key'in kendisini donmek sorunu fark etmeyi kolaylastirir.

### Soru: Localization sadece UI icin mi?

Cevap:
Hayir. API hata dili ve operasyonel uyum icin de onemlidir.

## 10. Sonuc

Localization bu projede "sonradan eklenecek ceviri katmani" degil, hata dili ve standart mesaj yonetiminin parcasidir.
