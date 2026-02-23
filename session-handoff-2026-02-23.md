# Session Handoff - 2026-02-23

## User Preferences (Critical)
- Eğitim modu: özet istemiyor, detaylı öğretici anlatım istiyor.
- Kodu kullanıcı yazacak, asistan otomatik değişiklik yapmayacak.
- DDD kesinlikle istenmiyor (aggregate, aggregate root, domain event jargon yok).
- Mimari: .NET 9, Clean + Vertical Slice + Modular Monolith.
- AuthZ: role + permission + condition-based yetki.
- Logging çok güçlü: request başından sonuna, DB + txt.
- Audit: oldValue/newValue zorunlu.
- Her tabloda audit kolonları: IsDeleted, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt, DeletedBy, DeletedAt.
- Log DB ayrı ve immutable (uygulama ile update/delete yapılamaz).

## What Was Done
- `Bootstrapper` -> `Host.Api` dönüşümü yapıldı.
- MVC template kalıntıları kaldırıldı (API host yaklaşımı).
- `Program.cs` API pipeline’a çevrildi.
- `Directory.Packages.props` kökte mevcut.
- `global.json` kökte mevcut.
- `.gitignore` eklendi.
- `EnterpriseSystem.sln` başlangıçta boştu; kullanıcı tekrar projeleri solution’a ekledi.
- Son build sonucu: başarılı.

## Current Project State (User Report)
- `dotnet build EnterpriseSystem.sln` successful.
- Solution’da görünen projeler:
  - `src\BuildingBlocks\Infrastructure\Infrastructure.csproj`
  - `src\BuildingBlocks\SharedKernel\SharedKernel.csproj`
  - `src\Host.Api\Host.Api.csproj`
  - `src\Modules\Identity\Identity.Application\Identity.Application.csproj`
  - `src\Modules\Identity\Identity.Domain\Identity.Domain.csproj`
  - `src\Modules\Identity\Identity.Infrastructure\Identity.Infrastructure.csproj`
  - `src\Modules\Identity\Identity.Presentation\Identity.Presentation.csproj`
- Not: `src\BuildingBlocks\Application\Application.csproj` listede görünmüyorsa solution’a eklenmeli.

## Pending Immediate Check
- Run:
  - `dotnet sln EnterpriseSystem.sln list`
- If missing, add:
  - `dotnet sln EnterpriseSystem.sln add src\BuildingBlocks\Application\Application.csproj`

## Next Lesson Plan (Non-DDD)
1. BuildingBlocks klasör iskeletini netleştir.
2. Dosya-kontratları oluştur:
   - `BaseEntity`
   - `IAuditableEntity`
   - `ISoftDeletable`
   - `Result/Error`
   - `IRepository<T>`
   - `IUnitOfWork`
   - `IApplicationEvent` (sadece outbox amaçlı, DDD değil)
3. EF Core altyapısında:
   - audit alanları otomatik doldurma
   - soft delete global filter
   - old/new value audit modeli
4. Log DB ayrı + immutable DB policy tasarımı.

## Communication Rule to Continue
- Assistant should act as instructor.
- User writes all code.
- Provide step-by-step, file-by-file guidance.
- No autonomous bulk edits.
