# PROJECT STATE — AYGÜN ERP FRONTEND

## Last Updated
2026-04-05 16:30 UTC

## Professional Standards (NON-NEGOTIABLE)

### RULE 1 — NEVER USE RAW SQL FOR SCHEMA CHANGES
Always use EF Core migrations. Never bypass with ALTER TABLE.

### RULE 2 — NO UNILATERAL DECISIONS
Do exactly what was asked. State what you will do before doing it. Ask if unclear.

### RULE 3 — UPDATE THIS FILE AFTER EVERY SESSION

### RULE 4 — GIT COMMIT & PUSH AFTER EVERY TASK
One commit per logical task. Conventional Commits format. Always push.

---

## Tech Stack
- Backend:  .NET 9, Modular Monolith, PostgreSQL, EF Core
- Frontend: Vite 8, React 19, TypeScript, Tailwind CSS v4, shadcn/ui
- State:    Zustand, TanStack Query v5
- Forms:    React Hook Form + Zod v4
- Tables:   TanStack Table
- Charts:   Recharts
- DnD:      @dnd-kit/core
- Router:   React Router v7
- Icons:    Lucide React
- Font:     Plus Jakarta Sans (UI) + JetBrains Mono (code/data)
- Backend URL: http://localhost:5279

## Color System
- Page bg:    #F0F4F8
- Surface:    #FFFFFF
- Sidebar:    #1B3A5C (navy)
- Primary:    #2E6DA4
- Accent:     #5B9BD5
- Text main:  #1B3A5C
- Text body:  #2C4A6B
- Text muted: #7A96B0
- Danger:     #E05252
- Success:    #1E8A6E
- Warning:    #D4891A

## COMPLETED ✅

### Phase 1 — Foundation
- [x] Vite + React + TS project scaffolding
- [x] Tailwind CSS configured
- [x] shadcn/ui installed (New York style, zinc base)
- [x] Plus Jakarta Sans + JetBrains Mono fonts (Google Fonts CDN)
- [x] Axios client with JWT interceptor + refresh token logic
- [x] Zustand auth store (accessToken, refreshToken, user, roles, hasRole)
- [x] React Router with ProtectedRoute + AdminRoute guards
- [x] Layout.tsx: collapsible sidebar, full navigation, notifications, user dropdown, search overlay (Ctrl+K), fullscreen toggle, breadcrumb header

### Phase 2 — Navigation & Routing
- [x] All 19 routes registered in AppRouter.tsx
- [x] Sidebar: 5 groups, 19 items, role-based visibility
- [x] Placeholder pages created for all modules
- [x] Role detection fixed (JWT role claim → roles array)

### Phase 3 — Design System Components
- [x] DataGrid — sortable, filterable, paginated, mobile card view
- [x] PageHeader — title, subtitle, actions slot
- [x] FilterBar — search, pills, boolean, daterange, reset
- [x] CrudModal — create/edit/delete modes, responsive
- [x] StatusBadge — auto-detect variant, Turkish labels
- [x] ActionMenu — dropdown with danger variant
- [x] KpiCard — top accent border, delta indicators
- [x] PasswordField — generator, strength bar, eye toggle, copy, temporary toggle
- [x] TopBar — window pills, refresh, live indicator
- [x] useList hook — pagination + sort + filter + debounce
- [x] useCrud hook — mutations + modal state + toast

### Phase 4 — Login Page
- [x] Split-screen design (navy left + white right)
- [x] Geometric SVG background
- [x] Feature list + stats row on left
- [x] Tag pill + JWT footer on right
- [x] Input styling with icon prefixes
- [x] JWT decode + role extraction on login

### Phase 5 — Users Module (Partial)
- [x] api.ts — list, getById, create, update, deactivate, reactivate, delete
- [x] columns.tsx — Kod, Ad Soyad (avatar), E-posta, Rol, Durum, Oluşturulma, İşlem
- [x] schema.ts — createUserSchema, updateUserSchema (Zod)
- [x] ListPage.tsx — KPI cards, FilterBar, DataGrid, confirm modals
- [x] DetailPage.tsx — user detail cards
- [x] UserFormPage.tsx — 3-tab form (Bilgiler / Rol / Yetki)
- [x] UserCode auto-generator (Ad+Soyad algorithm, Turkish char normalize)
- [x] PasswordField component integrated in create form
- [x] ProfileImageEditor (crop + upload + URL) — commit: 7b6bc5d9
- [x] ProfileImageDisplay (read-only avatar) in table, detail, sidebar — commit: 7b6bc5d9
- [x] DetailPage Düzenle button navigates to /users/:id/edit — commit: 7b6bc5d9

### Phase 5b — Backend: User Entity Updates
- [x] Added FirstName + LastName to User entity
- [x] Updated UserListItemDto, UserDetailDto, CreatedUserDto with FirstName/LastName/DisplayName
- [x] Updated CreateUserRequest, UpdateUserRequest with FirstName/LastName
- [x] Updated CreateUserCommandHandler, UpdateUserCommandHandler
- [x] Updated ListUsersQueryHandler, GetUserByIdQueryHandler Select projections
- [x] EF Core migration: 20260405140000_AddUserFirstLastName + snapshot update
- [x] Added GET /api/users/:id/permissions/summary endpoint
- [x] Added POST /api/users/:id/permissions/grant endpoint
- [x] DI registration for new handlers
- [x] Username made optional in UpdateUserRequest — commit: fd2d9663
- [x] PasswordExpiresAt removed from UpdateUserRequest (system-managed) — commit: fd2d9663
- [x] PasswordPolicyOptions: PasswordExpiryDays + ExpiryWarningDays — commit: fd2d9663
- [x] LoginResponseDto: IsPasswordExpiringSoon + DaysUntilPasswordExpiry — commit: fd2d9663
- [x] CreateUserCommandHandler uses policy ExpiryDays — commit: fd2d9663

## IN PROGRESS 🔄

### Users Module — Remaining Work
- [ ] Tab 2 (Rol Atama) — @dnd-kit drag-and-drop needs full rebuild
- [ ] Tab 3 (Yetki Atama) — accordion tree UI for Level 1-5 toggles
- [ ] Frontend: test full create/edit flow end-to-end

## PENDING 📋

### Remaining Modules
- [ ] Rol Yönetimi (/roles)
- [ ] Oturum Yönetimi (/sessions)
- [ ] Yetki Yönetimi (/permissions)
- [ ] TCode Test Aracı (/tcode-test)
- [ ] Audit Dashboard (/dashboard)
- [ ] Sistem Logları (/logs/system)
- [ ] Güvenlik Olayları (/logs/security)
- [ ] İstek Logları (/logs/requests)
- [ ] Varlık Değişiklikleri (/logs/entity-changes)
- [ ] Outbox Yönetimi (/outbox)
- [ ] Şifre Politikası (/password-policy)
- [ ] Onay Workflow (/approvals/workflows)
- [ ] Bekleyen Onaylar (/approvals/pending)
- [ ] Delegasyon Yönetimi (/approvals/delegations)
- [ ] Doküman Yönetimi (/documents)
- [ ] Rapor Şablonları (/reports/templates)
- [ ] ERP Servis Kataloğu (/erp/services)
- [ ] ERP Sorgu Çalıştırıcı (/erp/runner)

## KEY DECISIONS & CONVENTIONS
- Username field: NEVER shown in UI. Auto-generated = UserCode.toLowerCase()
- UserCode label: always shown as "Kod" in tables/forms
- Ad + Soyad replaces Username in all forms
- UserCode algorithm: firstLetter(firstName) + firstWord(lastName) → uppercase, no Turkish chars
- All new users: mustChangePassword = true by default
- Role system: Roles handle Level 1-4 access. /api/permissions/actions handles Level 5 only.
- Drag-and-drop: @dnd-kit/core with PointerSensor (distance: 8)
- All API calls: optimistic UI update first, revert on error
- Toast library: sonner
- Date formatting: date-fns with tr locale

## KNOWN ISSUES
- Tab 2 (Rol) DnD was broken — needs full rebuild with correct @dnd-kit setup
- Tab 3 (Yetki) not properly implemented — awaiting proper accordion tree UI

## API REFERENCE
Base URL: http://localhost:5279

Auth:
  POST /api/auth/login
  POST /api/auth/refresh
  POST /api/auth/change-password

Users:
  GET    /api/users?page&pageSize&search&sortBy&sortDirection&isActive&includeDeleted
  GET    /api/users/:id
  POST   /api/users
  PUT    /api/users/:id
  POST   /api/users/:id/deactivate
  POST   /api/users/:id/reactivate
  DELETE /api/users/:id
  GET    /api/users/:id/permissions/summary
  POST   /api/users/:id/permissions/grant

Roles:
  GET    /api/roles
  POST   /api/roles
  GET    /api/roles/users/:userId
  POST   /api/roles/:roleId/assign/:userId
  DELETE /api/roles/:roleId/assign/:userId
  DELETE /api/roles/:roleId

Permissions (Level 5 actions):
  GET    /api/permissions/actions?userId&transactionCode
  POST   /api/permissions/actions
  DELETE /api/permissions/actions/:id

Sessions:
  GET    /api/sessions?userId&onlyActive
  POST   /api/sessions/:id/revoke

Operations:
  GET    /api/ops/audit/dashboard/summary?windowHours
  GET    /api/ops/logs/system
  GET    /api/ops/logs/security-events
  GET    /api/ops/logs/http-requests
  GET    /api/ops/logs/entity-changes
  GET    /api/ops/outbox/messages?page&pageSize&status
  POST   /api/ops/outbox/mail
  GET    /api/ops/security/password-policy
  PUT    /api/ops/security/password-policy

ERP:
  GET    /api/erp/services
  POST   /api/erp/run
  POST   /api/erp/export-excel
  GET    /api/erp/params/:endpoint

Approvals:
  GET    /api/approvals/workflows
  POST   /api/approvals/workflows
  PUT    /api/approvals/workflows/:id
  GET    /api/approvals/delegations
  POST   /api/approvals/delegations

Documents:
  GET    /api/documents
  POST   /api/documents
  POST   /api/documents/:id/versions

Reports:
  GET    /api/reports/templates
  POST   /api/reports/templates
  PUT    /api/reports/templates/:id
  POST   /api/reports/templates/:id/publish
  POST   /api/reports/templates/:id/archive
