# EnterpriseSystem Gorsel Mimari Seti

Bu klasor, projenin gorsel ve kavramsal mimari haritasini icerir.

## Icerik

- `solution-tree.md` -> Solution bazli agac iskeleti
- `solution-dependency.mmd` -> Proje bagimlilik grafigi
- `request-log-flow.mmd` -> Request to log runtime akisi
- `authorization-6-level.mmd` -> 6 seviye yetki katmani
- `class-diagram-authz.puml` -> Yetki domain class diyagrami
- `sequence-tcode-access.puml` -> T-Code erisim sequence diyagrami
- `current-state-target-state-guide.md` -> Mevcut durum, hedef mimari, servis + CQRS, loglama, localization ve T-Code enforcement rehberi
- `identity-module-training.md` -> Identity modulu icin egitim odakli detayli teknik rehber
- `authorization-module-training.md` -> Authorization modulu icin egitim odakli detayli teknik rehber
- `integrations-module-training.md` -> Integrations modulu icin egitim odakli detayli teknik rehber
- `operations-module-training.md` -> Operations modulu icin egitim odakli detayli teknik rehber
- `localization-training.md` -> Localization sistemi icin egitim odakli detayli teknik rehber
- `error-policy-and-notification-training.md` -> Error policy, event routing ve notification omurgasi rehberi
- `manual-api-learning-guide.md` -> Host.Api.http dosyasini egitim sirasiyla kullanma rehberi
- `system-learning-path.md` -> Tum rehberleri ve kod okumayi siraya koyan ogrenme yolu
- `query-handler-and-validator-training.md` -> Query handler ve validator katmanlarinin egitim rehberi
- `api-response-catalog.md` -> Basari ve hata response sozlesmeleri katalogu
- `observability-deep-dive.md` -> Middleware, interceptor, event publisher ve log writer derin rehberi
- `approvals-module-training.md` -> Approval workflow ve delegation cekirdegi icin egitim odakli teknik rehber

## Kullanim

1. Mermaid dosyalarini markdown icinde `mermaid` bloklarinda goruntule.
2. PlantUML dosyalarini uygun editor eklentisi ile render et.
3. `current-state-target-state-guide.md` dosyasini kodla birlikte paralel oku; bu dosya current-state ve target-state ayrimini anlatir.
4. Modul bazli derin egitim icin `identity-module-training.md` ve `authorization-module-training.md` dosyalarini birlikte oku.
5. Asenkron entegrasyon ve operasyon gorunurlugu icin `integrations-module-training.md` ve `operations-module-training.md` dosyalarini oku.
6. Dil yonetimi ve hata/notification omurgasi icin `localization-training.md` ve `error-policy-and-notification-training.md` dosyalarini oku.
7. Canli endpoint ogrenimi icin `manual-api-learning-guide.md` ve `src/Host.Api/Host.Api.http` dosyalarini birlikte kullan.
8. Uzun vadeli, baglam kaybetmeyen tam okuma sirasi icin `system-learning-path.md` dosyasini takip et.
9. Query/validator ayrimi ve response sozlesmeleri icin `query-handler-and-validator-training.md` ile `api-response-catalog.md` dosyalarini oku.
10. Log/denetim omurgasini derin anlamak icin `observability-deep-dive.md` dosyasini oku.
11. Approval ve vekalet tasarimini anlamak icin `approvals-module-training.md` dosyasini oku.

## Not

Bu klasordeki diyagramlar ve rehberler canlidir. Yeni modul, katman veya cross-cutting altyapi eklendiginde guncellenmelidir.

## Uygulama Notu

- T-Code yetki endpoint'i: `GET /api/tcode/{transactionCode}`
- Query parametreleri: `amount` opsiyoneldir
- `userId` ve `companyId` query ile verilebilir
- Query ile verilmezse claim fallback devreye girer
