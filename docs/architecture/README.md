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

## Kullanim

1. Mermaid dosyalarini markdown icinde `mermaid` bloklarinda goruntule.
2. PlantUML dosyalarini uygun editor eklentisi ile render et.
3. `current-state-target-state-guide.md` dosyasini kodla birlikte paralel oku; bu dosya current-state ve target-state ayrimini anlatir.

## Not

Bu klasordeki diyagramlar ve rehberler canlidir. Yeni modul, katman veya cross-cutting altyapi eklendiginde guncellenmelidir.

## Uygulama Notu

- T-Code yetki endpoint'i: `GET /api/tcode/{transactionCode}`
- Query parametreleri: `amount` opsiyoneldir
- `userId` ve `companyId` query ile verilebilir
- Query ile verilmezse claim fallback devreye girer
