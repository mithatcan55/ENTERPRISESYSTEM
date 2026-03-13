# Reports Backend Egitim Notu

## Bu modulu neden actik?

Frontend tarafinda `pdfme` designer calismaya baslasa bile, rapor taslaklarinin ve versiyonlarinin
kalici olarak saklanacagi bir backend omurgasi olmadan sistem eksik kalirdi.

Bu modulun amaci su sorulari cevaplamaktir:

1. Rapor sablonlari veritabaninda nasil tutulacak?
2. Taslak ile yayinlanmis versiyon nasil ayrilacak?
3. Ayni raporun gecmis versiyonlari nasil korunacak?
4. Frontend, local draft yerine ne zaman gercek registry'ye yazacak?

## Kullanilan veri modeli

Iki ana tablo var:

1. `ReportTemplates`
2. `ReportTemplateVersions`

### `ReportTemplates`

Bu tablo raporun kimligini ve ana metadata bilgisini tutar:

- kod
- ad
- aciklama
- modül anahtari
- tip
- durum
- aktif versiyon numarasi
- yayinlanmis versiyon numarasi

### `ReportTemplateVersions`

Bu tablo rapor govdesini tutar:

- `TemplateJson`
- `SampleInputJson`
- versiyon numarasi
- yayin bilgisi
- notlar

Bu ayrim bilincli bir tercihtir. Raporun metadata bilgisini ve govdesini tek tabloda
karistirmak yerine, versiyon tarihcesini dogal olarak saklayabiliyoruz.

## CQRS akisi

Bu modulde ilk asamada su handler'lar acildi:

- listeleme
- detay alma
- olusturma
- guncelleme
- yayinlama
- arsivleme

Guncelleme mantigi "kayit uzerine yaz" degil, yeni versiyon olustur mantigiyla calisir.

## Migration konusu

Bu modulun en onemli farki su:

- yalnizca `EnsureCreated` mantigina birakilmadi
- `ReportsDbContext` icin migration zinciri acildi

Bu, kullanicinin hakli su itirazini kapatmak icin gereklidir:

> Entity var ama tablo yoksa veya tablo olusumu migration disi ilerliyorsa,
> altyapi eksik demektir.

## Su anki gecis durumu

Backend artik report registry persistence'i destekliyor.
Ancak frontend su anda hala local draft mantigiyla calisiyor.

Bu bir bug degil, asamali gecistir:

1. once designer
2. sonra backend registry
3. sonra frontend'i API'ye baglama

Bir sonraki dogru teknik adim:

- frontend `ReportsWorkspacePage` ekranini bu yeni API'ye baglamak
- local draft ile backend draft ayrimini yonetmek
- publish / archive butonlarini acmak
