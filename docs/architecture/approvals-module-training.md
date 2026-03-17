# Approvals Module Training

## Neden ayrÄ± bir approval modÃ¼lÃ¼?

Onay mekanizmasÄ±nÄ± sabit `3 level` veya `4 level` koduna gÃ¶mmek kÄ±sa vadede hÄ±zlÄ± gÃ¶rÃ¼nÃ¼r ama uzun vadede sistemi kÄ±rar.
Bu projede hedef:

- belge tipine gÃ¶re farklÄ± akÄ±ÅŸ tanÄ±mlayabilmek
- modÃ¼le gÃ¶re farklÄ± seviyeler kurabilmek
- organizasyon deÄŸiÅŸse bile kodu kÄ±rmadan veriyle yeni akÄ±ÅŸ tanÄ±mlayabilmek
- vekalet mekanizmasÄ±nÄ± aynÄ± Ã§ekirdeÄŸe oturtmak

Bu nedenle approval yapÄ±sÄ± ayrÄ± modÃ¼l olarak aÃ§Ä±ldÄ±.

## Temel entity mantÄ±ÄŸÄ±

### ApprovalWorkflowDefinition

Bir onay akÄ±ÅŸÄ±nÄ±n ana kaydÄ±dÄ±r.

Ã–rnek:

- `FAZLA_MESAI_STANDARD`
- `PURCHASE_CFO_FLOW`
- `ANNUAL_LEAVE_FLOW`

Bu tabloda:

- hangi modÃ¼le ait olduÄŸu
- hangi belge tipi iÃ§in geÃ§erli olduÄŸu
- aktif olup olmadÄ±ÄŸÄ±

saklanÄ±r.

### ApprovalWorkflowStep

AkÄ±ÅŸÄ±n kaÃ§Ä±ncÄ± seviyede kim tarafÄ±ndan onaylanacaÄŸÄ±nÄ± tanÄ±mlar.

Ã–rnek approver tipleri:

- `specific_user`
- `role`
- `manager_of_requester`
- `department_head`
- `title`

Yani "ÅŸef", "mÃ¼dÃ¼r", "CFO" gibi kavramlar sabit if/else deÄŸil,
`ApproverType + ApproverValue` mantÄ±ÄŸÄ±yla veri olarak tutulur.

### ApprovalWorkflowCondition

Hangi akÄ±ÅŸÄ±n hangi durumda seÃ§ileceÄŸini belirler.

Ã–rnek:

- `companyId = 2`
- `amount >= 100000`
- `documentType = overtime`

Bu sayede aynÄ± modÃ¼l iÃ§in birden fazla workflow tanÄ±mlanabilir.

### ApprovalInstance

GerÃ§ek iÅŸ kaydÄ± iÃ§in baÅŸlayan runtime approval sÃ¼recidir.

Ã–rnek:

- fazla mesai talebi `OT-2026-001`
- satÄ±n alma talebi `PO-2026-145`

Burada artÄ±k tanÄ±m deÄŸil, gerÃ§ek Ã§alÄ±ÅŸan sÃ¼reÃ§ vardÄ±r.

### ApprovalInstanceStep

Runtime instance iÃ§indeki her step'in kime dÃ¼ÅŸtÃ¼ÄŸÃ¼nÃ¼ ve hangi durumda olduÄŸunu tutar.

### ApprovalDecision

Approve, reject, return gibi kararlarÄ± audit zinciri bozulmadan saklar.

Bu tablo gelecekte:

- web UI
- mail linki
- WhatsApp aksiyonu

gibi farklÄ± kanallarÄ±n hepsini ortak iÅŸ mantÄ±ÄŸÄ±nda toplamak iÃ§in gereklidir.

### DelegationAssignment

Vekalet mekanizmasÄ±nÄ±n temel kaydÄ±dÄ±r.

Burada:

- kim yetki devrediyor
- kime devrediyor
- hangi tarih aralÄ±ÄŸÄ±nda geÃ§erli
- hangi scope'lar dahil
- hangi scope'lar hariÃ§

saklanÄ±r.

## Bu ilk fazda neler tamam?

- workflow tanÄ±mlarÄ± veritabanÄ±na kaydedilebilir
- workflow detayÄ± step ve condition ile okunabilir
- delegation assignment kaydÄ± tutulabilir
- runtime instance/decision tablolarÄ± Ã§ekirdekte aÃ§Ä±ldÄ±
- workflow resolver ilk fazda calisir
- approval instance baslatma komutu vardir
- approve / reject / return karar akisi vardir
- pending approval inbox sorgusu vardir

## Bu ilk fazda neler henÃ¼z yok?

- organizasyon bazli approver resolver (`manager_of_requester`, `department_head`, `title`)
- tam delegation scope motoru
- frontend approval workspace
- notification ve inbox ekran entegrasyonu
- WhatsApp / mail adapter seviyesinde karar kanallari

Yani bu tur approval engine sadece veri omurgasi olarak kalmadi; temel runtime kararlari da uretebilir hale geldi.

## Neden instance tablolarÄ± ÅŸimdiden eklendi?

Ã‡Ã¼nkÃ¼ approval modÃ¼lÃ¼ne sonradan runtime tablolar eklemek,
workflow tanÄ±mÄ±nÄ± baÅŸtan bozma riskini arttÄ±rÄ±r.

Bu projede hedef:

- Ã¶nce Ã§ekirdeÄŸi saÄŸlam kurmak
- sonra karar motorunu bunun Ã¼zerine oturtmak

Bu nedenle daha ilk fazda instance ve decision tablolarÄ± da ÅŸemaya alÄ±ndÄ±.

## Resolver mantÄ±ÄŸÄ± ilk fazda nasÄ±l Ã§alÄ±ÅŸÄ±r?

1. `moduleKey + documentType` ile aktif workflow'lar cekilir.
2. PayloadJson icindeki alanlar kosullarla eslestirilir.
3. Tum kosullari saglayan workflow'lar arasindan en fazla condition eslesen secilir.
4. Step listesi runtime'a acilir.
5. `specific_user` veya `role` tipi approver satirlari somut user listesine cevrilir.
6. Aktif delegation varsa atama vekile kaydirilir.

## Karar mantigi ilk fazda nasil calisir?

- `approve` -> step satiri approved olur
- minimum approver count saglandiysa ayni step'teki kalan pending satirlar skipped olur
- sonraki step varsa `Waiting -> Pending` gecer
- step kalmadiysa instance `Approved` olur
- `reject` -> instance `Rejected` olur
- `return` -> instance `Returned` olur

## Sonraki teknik adÄ±mlar

1. organizasyon bazli approver resolver
2. workflow secimi icin daha zengin condition operator seti
3. delegation scope motorunu permission/workflow/module seviyesine indirmek
4. frontend approval workspace
5. notification ve inbox entegrasyonu
6. WhatsApp / mail approval adapter katmani
