# Auth-First Module Onboarding (End-to-End)

## Rule
No business module starts until auth foundation and compliance gates are green.

## Prerequisites
1. User, role, permission, session endpoints are stable.
2. T-Code authorization service is active and logged.
3. Localization works for error/validation responses.
4. Request/security/audit logs are verifiable.

## Backend Module Checklist
1. Generate module scaffold using script.
2. Define T-Code set for module pages/actions.
3. Add endpoint policies and expected error responses.
4. Map audit expectations (which actions require change logs).
5. Add module smoke tests.

## Frontend Module Checklist
1. Register module route and menu metadata.
2. Add permission/T-Code guards.
3. Bind list/detail/create flows with typed API client.
4. Add localized labels and validation texts.
5. Add module dashboard widgets if required.

## Documentation Checklist
1. Add module section to project-book and project-map.
2. Add endpoint matrix entries.
3. Add compliance evidence links.
4. Add one session training report.

## Fast Failure List
- Policy missing on endpoint
- Non-localized validation message
- No security log for deny events
- No audit trail for critical update commands
- Frontend route without guard