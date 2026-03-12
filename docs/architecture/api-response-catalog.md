# API Response Catalog

Bu dokuman projedeki tipik basari ve hata response kaliplarini ozetler.

Amac:

- frontend ve entegrasyon ekiplerine hizli referans vermek
- response sozlesmelerini tek yerde toplamak

## 1. Basari Response Kaliplari

### 200 OK

Tipik kullanim:

- listeleme
- sorgu sonucu
- preview endpoint'leri

Ornek:

```json
[
  {
    "id": 1,
    "userCode": "SYSADMIN",
    "username": "sysadmin",
    "email": "sysadmin@enterprise.local",
    "isActive": true
  }
]
```

### 201 Created

Tipik kullanim:

- yeni kaynak olusturma

Ornek:

```json
{
  "id": 15,
  "userCode": "USR015",
  "username": "operator15",
  "email": "operator15@enterprise.local",
  "mustChangePassword": true,
  "passwordExpiresAt": "2026-06-12T10:15:00Z"
}
```

### 202 Accepted

Tipik kullanim:

- outbox gibi asenkron akislar

Ornek:

```json
{
  "id": 42,
  "eventType": "MailNotification",
  "status": "Pending",
  "createdAt": "2026-03-12T10:15:00Z"
}
```

### 204 No Content

Tipik kullanim:

- revoke
- deactivate
- reactivate
- delete
- change password

Bu response body tasimaz.

## 2. ProblemDetails Hata Kalibi

Sistem standart hata sozlesmesi olarak `ProblemDetails` kullanir.

Tipik alanlar:

```json
{
  "type": "https://httpstatuses.com/403",
  "title": "Yasak",
  "status": 403,
  "detail": "Gecersiz kullanici adi veya sifre.",
  "instance": "/api/auth/login",
  "errorCode": "auth_invalid_credentials",
  "correlationId": "abc-123"
}
```

## 3. Validation Hatalari

Validation hatalarinda ek olarak `errors` alani gelir.

Ornek:

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Dogrulama Hatasi",
  "status": 400,
  "detail": "CreateUserCommand dogrulamasi basarisiz.",
  "instance": "/api/users",
  "errorCode": "validation_error",
  "correlationId": "abc-123",
  "errors": {
    "userCode": ["UserCode zorunludur."],
    "email": ["Email zorunludur."]
  }
}
```

## 4. T-Code Resolve Ozel Durumu

`GET /api/tcode/{transactionCode}` deny durumunda da detayli bir model doner.

Ornek:

```json
{
  "transactionCode": "SYS03",
  "isAllowed": false,
  "deniedAtLevel": 5,
  "deniedReason": "Kullanicinin T-Code 'SYS03' icin 'DELETE' aksiyon yetkisi yok.",
  "requiredActionCode": "DELETE",
  "conditions": [],
  "missingContextFields": []
}
```

Bu response klasik `ProblemDetails` degildir.
Bilerek boyledir, cunku bu endpoint debug ve denetim amacli bir cozumleme endpoint'idir.

## 5. Password Policy Preview Ozel Durumu

Preview endpoint'i teknik olarak 200 dondurur ama sonuc icinde konfigurasyon gecerli mi bilgisini tasir.

Ornek:

```json
{
  "isValidConfiguration": false,
  "validationErrors": [
    "MinLength 8 ile 128 arasinda olmalidir."
  ],
  "warnings": [
    "Preview sonucu runtime konfigurasyonunu degistirmez."
  ],
  "sampleEvaluations": []
}
```

## 6. Sık Karsilasilan ErrorCode Ornekleri

- `auth_invalid_credentials`
- `auth_user_inactive`
- `auth_password_expired`
- `refresh_token_invalid`
- `refresh_token_reused`
- `session_not_found`
- `tcode_request_precheck_failed`
- `permission_request_precheck_failed`
- `internal_error`

## 7. Frontend Icin Pratik Notlar

1. `errorCode` alanini her zaman dikkate al
2. `correlationId` alanini hata ekraninda veya log ekraninda gostermek faydalidir
3. validation hatalarinda `errors` alanini alan-bazli goster
4. `202 Accepted` response'larini "islem suruyor" mantigiyla ele al
5. `TCode resolve` endpoint'ini genel hata endpoint'i gibi degil, yetki debug endpoint'i gibi dusun

## 8. Sonuc

Bu katalog response sozlesmelerinin dogru okunmasi icin hizli referanstir.
Detayli akis icin ilgili modul egitim rehberlerine inilmelidir.
