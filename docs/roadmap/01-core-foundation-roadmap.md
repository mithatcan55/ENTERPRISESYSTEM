# Core Foundation Roadmap (Auth + Business + Compliance First)

## Goal
Build a reusable enterprise core where new modules can be added without redesigning cross-cutting concerns.
The core must pass audit/compliance checks and include T-Code based authorization in user management.

## Program Scope
1. Auth service foundation (users, roles, permissions, sessions, auth lifecycle)
2. Business service foundation (module host, shared policy enforcement, integration-safe patterns)
3. Shared building blocks (error model, localization, logging, audit, correlation, policy abstractions)
4. Documentation and training system (every change explains what, why, and impact)
5. React admin/dashboard baseline (fast integration, graph-ready, module-ready)

## Target Topology
- src/Auth.Api
- src/Business.Api
- src/BuildingBlocks
  - Application
  - Infrastructure
  - SharedKernel
- src/Modules
  - Identity
  - (Future business modules)
- frontend/admin-dashboard

## Core Principles
1. Compliance by design: every critical action leaves an immutable trace.
2. Standard contracts: every API uses the same success/error envelope and localized messages.
3. One way to do things: module scaffolding removes architectural guesswork.
4. Explainability: implementation history can generate a training document.
5. Security first: T-Code + role + permission checks are first-class, not optional.

## Phase Plan

### Phase 0 - Baseline Alignment
- Freeze naming and folder conventions.
- Remove placeholder files and dead abstractions.
- Align docs with current code state.

Definition of Done:
- No `Class1.cs` placeholders.
- No conflicting namespace ownership across layers.
- Current architecture documented accurately.

### Phase 1 - Auth Core (Before Business Modules)
- User CRUD (create/list/update/deactivate/reactivate)
- Role and permission lifecycle
- User-role assignment and effective permission summary
- Session lifecycle (issue, list, revoke, expiry policy)
- T-Code user access integration

Definition of Done:
- User management endpoints are complete and policy protected.
- T-Code decisions are visible in API responses/logs.
- Security event logs include allow/deny context.

### Phase 2 - Compliance & Observability Backbone
- Request lifecycle logging (DB + file)
- Security logs, system logs, entity-change logs
- Immutable log DB policy and migration enforcement
- Correlation ID propagation end-to-end
- Audit actor and old/new value consistency rules

Definition of Done:
- Audit checklist passes for critical endpoint flows.
- Immutable policy validated in DB.
- Correlation trace works from API entry to persistence.

### Phase 3 - Localization Core (TR/DE/EN)
- Request culture resolution strategy
- Localized validation and problem details
- Localized domain/application error catalog
- Fallback chain and missing-resource monitoring

Definition of Done:
- Same error code returns localized text in TR/DE/EN.
- Default fallback behavior is deterministic and documented.

### Phase 4 - Module Scaffolding Maturity
- Extend scaffold to include backend + frontend contract + seed checklist
- Add policy and logging hook points as defaults
- Add module quality gates and test templates

Definition of Done:
- A new module can be opened with one script and passes basic checks.
- Teams do not decide cross-cutting implementation per module.

### Phase 5 - React Admin Dashboard Baseline
- Shell app with route groups, auth guard, permission guard, i18n, API client
- Graph-ready dashboard widgets and log/audit pages
- Module plugin-like registration pattern
- Generated docs for frontend integration steps

Definition of Done:
- Frontend can consume Auth + Ops endpoints with consistent contracts.
- New module page can be added with minimal boilerplate.

## Non-Negotiable Standards
1. Every endpoint has explicit authorization policy and expected error responses.
2. Every critical command writes auditable log entries.
3. No business module starts before Phase 1/2/3 completion.
4. Each implementation step updates training/report docs.

## Immediate Next Sprint (Recommended)
1. Namespace and ownership cleanup for Identity module boundaries.
2. Localization error catalog baseline (`tr-TR`, `de-DE`, `en-US`).
3. Auth user management completion (`update/deactivate/reactivate`).
4. Compliance gate checklist execution and evidence recording.