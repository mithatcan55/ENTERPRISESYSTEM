# EnterpriseSystem Görsel Mimari Seti

Bu klasör, projenin görsel haritasını içerir.

## İçerik

- `solution-tree.md` → Solution bazlı ağaç iskeleti
- `solution-dependency.mmd` → Proje bağımlılık grafiği (Mermaid)
- `request-log-flow.mmd` → Request->Log runtime akışı (Mermaid)
- `authorization-6-level.mmd` → 6 seviye yetki katmanı (Mermaid)
- `class-diagram-authz.puml` → Yetki domain class diyagramı (PlantUML)
- `sequence-tcode-access.puml` → T-Code erişim sequence diyagramı (PlantUML)

## Kullanım

1. Mermaid dosyalarını markdown içinde ````mermaid```` bloğuna kopyalayarak görüntüleyebilirsin.
2. PlantUML için VS Code PlantUML eklentisi ile doğrudan render alabilirsin.

## Not

Bu klasördeki diyagramlar canlıdır; yeni modül/katman eklendiğinde güncellenir.

## Uygulama Notu

- T-Code yetki endpoint'i: `GET /api/tcode/{transactionCode}`
- Query parametreleri: `amount` (zorunlu değil)
- Opsiyonel: `userId`, `companyId`
- `userId/companyId` yoksa claim fallback devreye girer.
