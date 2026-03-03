# Module Scaffold Standard (Çocuk Oyuncağı Sürüm)

Bu klasörün amacı, yeni modül açarken teknik karar yükünü sıfırlamaktır.

## 1) Ne Sağlar?

- Hazır klasör standardı
- Hazır endpoint iskeleti
- Hazır seed checklist
- Hazır frontend contract stub

## 2) Kullanım (5 Dakika)

1. `module-name` ve `tcode-prefix` belirle (örn: `Inventory`, `INV`).
2. `backend` ve `frontend` altındaki `.template` dosyalarını kopyala.
3. `{{PLACEHOLDER}}` alanlarını doldur.
4. Seed checklist'i adım adım uygula.
5. FE contract stub'u frontend ekibine ver.

## 3) Klasör Planı

- `backend/module-template/`
  - `Presentation/Controllers/{{ModuleName}}Controller.cs.template`
  - `Application/Services/I{{ModuleName}}AppService.cs.template`
  - `Application/Services/{{ModuleName}}AppService.cs.template`
  - `Application/Contracts/{{ModuleName}}ListItemDto.cs.template`
- `checklists/seed-checklist.md.template`
- `frontend/contract/{{module-name}}.contract.stub.json.template`

## 4) Zihinsel Kural

- Endpoint içinde teknik log yazma.
- Endpoint içinde global hata formatı üretmeye çalışma.
- Yetkiyi T-Code çözümünden al.
- İş akışını service'e delege et.

## 5) Örnek Akış

Ekran açılışı:
1. FE `GET /api/tcode/INV01` çağırır.
2. `isAllowed/actions/conditions` sonucuna göre UI açılır.

Listeleme:
1. FE `GET /api/inventory/items` çağırır.
2. API DTO döner, teknik log otomatik akar.
