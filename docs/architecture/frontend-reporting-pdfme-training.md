# Frontend Reporting ve pdfme Egitim Rehberi

## Neden `pdfme`?

Bu projede rapor tasarim katmani icin ilk tercih `pdfme` oldu. Bunun ana nedenleri:

1. React icine dogrudan gomulebilen bir tasarimci mantigi sunmasi
2. Ucretsiz ve acik kaynak olmasi
3. JSON template mantigi ile starter-kit mimarisine uyumlu olmasi
4. Label, image, dinamik alan, tablo benzeri tekrarli alanlar ve cok sayfali PDF ihtiyacina uygun olmasi

Bu secim su anlama gelir:

- `Reports` modulu siradan bir export sayfasi olmayacak
- rapor tanimlari veri modeli olarak da ele alinacak
- tasarim ekraninda cizilen yapi backend payload'u ile baglanacak

## Bu modulu nasil kuruyoruz?

Ilk fazda uc katman dusunuyoruz:

1. `Report Registry`
2. `Report Designer`
3. `Report Preview / Render`

### 1. Report Registry

Bu katman raporun kendisini tanimlar:

- rapor kodu
- rapor adi
- tipi
- versiyonu
- durumu
- hangi modulle iliskili oldugu

Yani registry, raporun katalogudur.

Onemli not:

- Bu asamada registry backend veritabanina yazilmiyor.
- `Reports` ekranindaki template listesi ve designer taslaklari frontend tarafinda katalog + localStorage mantigi ile calisiyor.
- Bunun nedeni backend tarafinda report template entity / migration / tablo setinin henuz acilmamis olmasidir.

Bu karar bilinclidir. Kullaniciya veritabanina kayit atiliyormus gibi davranmak yerine
taslak davranisini acikca gostermek daha dogrudur.

### 2. Report Designer

Bu katmanda `pdfme` editoru calisir.

Burada beklenen yetenekler:

- label/text
- image/logo
- dinamik field binding
- loop/table alanlari
- sayfa numarasi ve footer
- cok sayfali rapor

### 3. Report Preview / Render

Bu katmanda artik tasarim tamamlanmis olur ve veri ile birlestirilir:

- frontend preview
- backend payload binding
- PDF export

## Neden once registry tiplerini ekledik?

Direkt designer acmak cazip gorunur. Ancak dogru sira bu degildir.

Eger once su sorulari cevaplamazsak designer sonradan dagilir:

- hangi rapor hangi module ait?
- taslak mi, yayinlanmis mi?
- versiyon mantigi olacak mi?
- rapor tipi ne?

Bu nedenle ilk olarak su dosyalar eklendi:

- `frontend/src/modules/reports/reporting.types.ts`
- `frontend/src/modules/reports/reporting.catalog.ts`

Bu dosyalar rapor modulu icin tip guvencesi ve ilk katalog mantigini kurar.

## `pdfme` ile ne tasarlayacagiz?

Ilk ornek raporlar su tipte olmali:

1. Kullanici yetki ozeti
2. Outbox operasyon ozeti
3. Audit olay ozeti

Sebep:

- veri modelleri belli
- tablo + dinamik alan ihtiyaci var
- markalama ihtiyaci var

## Soru-Cevap

### Soru: Neden hemen designer acmiyoruz?

Cevap:
Designer UI tek basina sistem degildir. Registry, version, template kaydi ve payload standardi yoksa sonradan raporlar yonetilemez hale gelir.

### Soru: Bu yapi sadece PDF mi uretecek?

Cevap:
Ilk fazda evet, odak PDF olacak. Sonra gerekirse ek export katmanlari eklenebilir.

### Soru: Her modulde farkli rapor mantigi mi olacak?

Cevap:
Hayir. Her modulu ayni `Reports` modulu besleyecek. Moduller sadece veri payload'u saglayacak.

### Soru: Tasarimi kim yapacak?

Cevap:
Urun icindeki designer ile analist veya yetkili yonetici yapabilecek. Ama yayina alma kurali ayrica tanimlanabilir.

## Sonraki Teknik Adim

Bir sonraki teknik turda:

1. `pdfme` designer component'i `Reports` workspace icine gomulecek
2. secili template ile preview akisi eklenecek
3. marka renkleri ve logo binding'i designer'a tasinacak

## Su Anki Gercek Durum

Bu egitim dosyasini okuyan kisi icin durum net olsun:

1. `pdfme` designer artik frontend icinde calisiyor
2. preview uretiliyor
3. template JSON disari alinabiliyor
4. taslaklar localStorage'da tutulabiliyor
5. backend report registry tablosu ve API omurgasi artik acildi
6. frontend tarafinda local draft ile backend registry birlikte kullanilabiliyor

Bu nedenle sonraki backend adimi su olacak:

1. frontend ekraninda local draft gecis mantigini sadeleştirmek
2. registry senkronizasyonunu daha otomatik hale getirmek
3. publish / archive akisina role ve approval kurallari eklemek
4. rapor payload kaynaklarini is modullerine baglamak
