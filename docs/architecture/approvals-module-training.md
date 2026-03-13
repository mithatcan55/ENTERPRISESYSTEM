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

## Bu ilk fazda neler henÃ¼z yok?

- workflow resolver motoru
- approval instance baÅŸlatma komutu
- approve/reject/return command akÄ±ÅŸÄ±
- delegation resolve motoru
- approval inbox ekranlarÄ±

Yani bu tur "approval engine'in veri omurgasÄ±" kuruldu.

## Neden instance tablolarÄ± ÅŸimdiden eklendi?

Ã‡Ã¼nkÃ¼ approval modÃ¼lÃ¼ne sonradan runtime tablolar eklemek,
workflow tanÄ±mÄ±nÄ± baÅŸtan bozma riskini arttÄ±rÄ±r.

Bu projede hedef:

- Ã¶nce Ã§ekirdeÄŸi saÄŸlam kurmak
- sonra karar motorunu bunun Ã¼zerine oturtmak

Bu nedenle daha ilk fazda instance ve decision tablolarÄ± da ÅŸemaya alÄ±ndÄ±.

## Sonraki teknik adÄ±mlar

1. workflow resolver
2. approval instance create/start
3. approve / reject / return komutlarÄ±
4. delegation scope resolve
5. frontend approval workspace
6. notification ve inbox entegrasyonu
