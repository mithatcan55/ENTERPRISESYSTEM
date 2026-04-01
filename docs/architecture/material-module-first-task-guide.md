# Material Module First Task Guide

Bu not, `Material` modülunu senin yazmaya baslayacagin ilk faz icin pratik rehberdir.

## Faz 1 hedefi

Ilk fazda sadece su cekirdekleri yaz:

- `MaterialUnit`
- `MaterialGroup`
- `Material`

Bu fazda:
- stok olmayacak
- dosya yukleme olmayacak
- dinamik attribute degeri olmayacak
- approval baglantisi olmayacak

Amac:
- modül ekleme akisini ogrermek
- entity -> config -> context -> migration -> contract -> handler -> controller siralamasini oturtmak

## Entity kurallari

### MaterialUnit

Alanlar:
- `Id`
- `Code`
- `Name`
- `Symbol`
- `DecimalPrecision`
- `IsActive`
- `CreatedAt`

Kurallar:
- `Code` unique
- `Name` zorunlu
- pasif birim yeni material create icin secilemez

### MaterialGroup

Alanlar:
- `Id`
- `Code`
- `Name`
- `Description`
- `IsActive`
- `CreatedAt`

Kurallar:
- `Code` unique
- `Name` zorunlu

### Material

Alanlar:
- `Id`
- `Code`
- `Name`
- `Description`
- `BaseUnitId`
- `SecondaryUnitId`
- `MaterialGroupId`
- `DefaultShelfLocation`
- `IsBatchTracked`
- `IsSerialTracked`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`

Kurallar:
- `Code` unique
- `Name` zorunlu
- `BaseUnitId` zorunlu
- `SecondaryUnitId` opsiyonel
- `MaterialGroupId` opsiyonel olabilir ama ben tavsiye olarak nullable baslat derim
- `IsBatchTracked` ve `IsSerialTracked` ayni anda acik olabilir

## Klasor onerisi

Persistence entity dosyalari:
- `src/BuildingBlocks/Infrastructure/Persistence/Entities/Materials/Material.cs`
- `src/BuildingBlocks/Infrastructure/Persistence/Entities/Materials/MaterialUnit.cs`
- `src/BuildingBlocks/Infrastructure/Persistence/Entities/Materials/MaterialGroup.cs`

DbContext:
- `src/BuildingBlocks/Infrastructure/Persistence/MaterialsDbContext.cs`

Module projects:
- `src/Modules/Materials/Materials.Application`
- `src/Modules/Materials/Materials.Infrastructure`
- `src/Modules/Materials/Materials.Presentation`

## Yazim sirasi

1. entity siniflari
2. `MaterialsDbContext`
3. `OnModelCreating` config
4. migration
5. contract DTO'lari
6. create/list/detail handler'lari
7. controller
8. test

## Ilk endpoint seti

- `GET /api/material-units`
- `POST /api/material-units`
- `GET /api/material-groups`
- `POST /api/material-groups`
- `GET /api/materials`
- `GET /api/materials/{id}`
- `POST /api/materials`
- `PUT /api/materials/{id}`

## Validation notlari

- `Code` bos olamaz
- `Code` tekrar edemez
- `BaseUnitId` var olmali
- `BaseUnitId` pasif birime isaret edemez
- `SecondaryUnitId` verilirse var olmali
- `SecondaryUnitId == BaseUnitId` olmasina ilk fazda izin verebilirsin ama sonradan kisitlayabiliriz

## Permission notlari

Ilk fazda action-level yeterli:
- `Material.View`
- `Material.Create`
- `Material.Update`
- `MaterialUnit.View`
- `MaterialUnit.Manage`
- `MaterialGroup.View`
- `MaterialGroup.Manage`

Field-level dynamic policy bu fazda senin tarafindan yazilmayacak.
O cekirdek daha sonra `Authorization Policy` ile entegre olacak.

## Review beklentim

Sen ilk olarak yalnizca su dosyalari yaz:
- `MaterialUnit`
- `MaterialGroup`
- `Material`
- `MaterialsDbContext`

Bunlari yazdiktan sonra ben:
- property isimlerini
- nullable kararlarini
- iliski yonlerini
- index ihtiyaclarini
- naming tutarliligini
review edecegim.
