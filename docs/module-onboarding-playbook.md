# Module Onboarding Playbook (Core-First)

Amaç: Yeni bir modül/sayfa eklerken "burada log var mı, yetki nerede, hata yönetimi nerede" sorularını sıfırlamak.

## 1) Sabit Kurallar (Her modül için aynı)

- Request loglama: Otomatik (RequestLifecycleLoggingMiddleware). Endpoint içinde ayrıca log yazma zorunlu değil.
- Hata yönetimi: Otomatik (GlobalExceptionHandler). Endpoint içinde try/catch zorunlu değil.
- Audit actor: Otomatik (IAuditActorAccessor + CurrentUserContext).
- Yetki: T-Code resolver üzerinden merkezi (ITCodeAuthorizationService).
- DbContext yaşam döngüsü: ASP.NET scope başına tek context. Ayrı UnitOfWork sınıfı kullanılmaz.

## 2) Backend Event Map (WinForms benzeri düşün)

1. "Page Open / FormLoad" eşleniği:
   - FE: `GET /api/tcode/{transactionCode}`
   - API: Erişim + action + condition policy döner.

2. "Button Click" eşleniği:
   - FE: ilgili command endpoint çağrılır.
   - API: işlem yürütülür, unhandled hata olursa global handler devreye girer.

3. "Grid Load" eşleniği:
   - FE: liste endpoint çağrılır.
   - API: standart pagination/filter kuralları uygulanır.

## 3) Yeni Modül Ekleme Checklist

### A. Authorization Seed
- Module
- SubModule
- SubModulePage (TransactionCode zorunlu)

### B. Permission Matrix
- UserModulePermission
- UserSubModulePermission
- UserPagePermission
- UserCompanyPermission
- (ihtiyaç varsa) UserPageActionPermission
- (ihtiyaç varsa) UserPageConditionPermission

### C. API Contract
- Controller route adı
- Request/response DTO
- Validation kuralları

### D. Frontend Contract
- Ekran açılışında T-Code resolve çağrısı
- Dönen `actions` ile buton görünürlüğü
- Dönen `conditions` ile filtre/maskeleme

## 4) “Ne Zaman Ne Yazacağım?” Kısa Cevap

- Controller yazarken:
  - Sadece iş akışı kodu.
  - Tekrarlı log/hata/audit kodu yazılmaz.

- Service yazarken:
  - Sadece domain/application davranışı.
  - Cross-cutting (log/error/audit/auth) mevcut altyapıya bırakılır.

## 5) Definition of Done (Modül Tamam Kriteri)

- T-Code seed hazır
- Permission kayıtları hazır
- Endpoint cevapları stabil
- FE T-Code resolve ile buton/sayfa davranışı doğru
- Build başarılı
