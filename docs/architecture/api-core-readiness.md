# API Core Readiness

Bu not, frontend yeniden yazdirilmadan once backend omurgasinin hangi seviyeye geldigini netlestirmek icin tutulur.

## Tamamlanan cekirdek alanlar

- Authentication:
  - login
  - refresh
  - session validation
  - bootstrap admin seed
- Authorization:
  - role tabani
  - permission / t-code authorize
  - SYS_ADMIN bypass mantigi
- Operations:
  - audit dashboard summary
  - outbox monitoring
  - log/observability omurgasi
- Reports:
  - template registry
  - version history
  - persistence migration
- Approvals:
  - workflow tanimi
  - workflow resolve
  - instance start
  - business module trigger contract
  - approve / reject / return
  - pending inbox
  - delegation create/list
  - delegation revoke/reactivate
  - deadline bazli system decision

## Veritabaninda dogrulanan approvals alanlari

- `ApprovalWorkflowDefinitions`
- `ApprovalWorkflowSteps`
- `ApprovalWorkflowConditions`
- `ApprovalInstances`
- `ApprovalInstanceSteps`
- `ApprovalDecisions`
- `DelegationAssignments`

Ek deadline/delegation lifecycle kolonlari:

- `ApprovalWorkflowSteps.DecisionDeadlineHours`
- `ApprovalWorkflowSteps.TimeoutDecision`
- `ApprovalDecisions.IsSystemDecision`
- `DelegationAssignments.RevokedByUserId`
- `DelegationAssignments.RevokedAt`
- `DelegationAssignments.RevokedReason`

## Test kapsami

Unit test ile korunan kritik approval davranislari:

- en spesifik workflow secimi
- delegation ile approver resolve
- approval step advance
- pending inbox filtreleme
- delegation revoke/reactivate
- expired deadline icin system reject
- business trigger icin not_required / already_pending davranisi

Integration test ile korunan HTTP davranislari:

- outbox monitoring auth/validation
- approvals/delegations auth ve temel response contract
- audit dashboard auth ve summary contract

## Frontend isteme oncesi kalan teknik isler

- business modullerin approval trigger contract'larini ilgili request handler'lara baglamak
- dashboard ve identity ekranlarinda gercek veri odakli smoke senaryolarini artirmak
- reports tarafinda pdfme designer entegrasyonunu yeni frontend tasarimina gore yeniden almak

## Sonuc

Platform cekirdegi artik "frontend talep etmeye elverecek" seviyededir.
Ancak yeni frontend gelirken mevcut API contract'lari bozulmadan kullanilmali; ekranlarin veri akisi bu omurga uzerinden kurulmalidir.
