# React Admin Dashboard Blueprint (Fast and Module-Ready)

## Goal
Create a reusable React admin shell that connects quickly to Auth/Business APIs and scales by module.

## Tech Baseline
- React + TypeScript + Vite
- Router: route groups by domain (`auth`, `ops`, `modules`)
- Data: TanStack Query
- HTTP: typed client wrappers
- UI: component system with chart-ready widgets
- i18n: TR/DE/EN same culture strategy as backend
- Auth state: session-key based adapter

## Folder Convention
- frontend/admin-dashboard/src/app
  - bootstrap
  - routes
  - providers
- frontend/admin-dashboard/src/features
  - auth
  - users
  - roles
  - permissions
  - sessions
  - audit
  - logs
- frontend/admin-dashboard/src/shared
  - api
  - i18n
  - guards
  - ui
  - charts

## Mandatory Frontend Cross-Cutting
1. API client automatically sends correlation id.
2. Error handler maps standard ProblemDetails to UI alerts.
3. Permission guard supports role + T-Code + action permission.
4. Localization switch is global and persisted.
5. Audit views support filtering by correlation id and user.

## Dashboard Core Pages (Before Business Modules)
1. Auth Overview (active sessions, login success/fail trends)
2. User Management
3. Role & Permission Matrix
4. T-Code Access Viewer
5. Security Events
6. Request/System Logs

## Graph Pack (Initial)
- Login success/failure over time
- Top denied T-Codes
- Session activity trend
- Request latency percentile chart
- Error code distribution

## Module Plugin Pattern
Each module exposes:
- route registration
- menu item metadata
- permission/tcode requirements
- optional dashboard widgets

## Frontend Delivery Steps
1. Build shell with auth + i18n + api client.
2. Connect user/role/session pages to backend.
3. Add graph widgets for auth/security/ops.
4. Add module registration contract.
5. Add module generator for page/service stubs.

## Quality Gates
- Route guard tests for unauthorized/forbidden access
- i18n regression checks (TR/DE/EN)
- API contract mismatch detection in CI
- Dashboard performance budget for initial load