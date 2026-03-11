# Compliance and Audit Gates (Release Blocking)

## Purpose
This checklist defines mandatory controls before a phase can be marked complete.
If any gate fails, the phase remains open.

## Gate A - Security and Authorization
- Every protected endpoint declares explicit policy/role/T-Code requirements.
- Unauthorized and forbidden responses follow the standard problem details contract.
- Deny events are recorded in security logs with actor and correlation id.

Evidence:
- Endpoint matrix snapshot
- Security log sample records

## Gate B - Session Governance
- Session issue, validation, and revoke flows are traceable.
- Session revocation rules enforce owner/admin boundaries.
- Session expiry policy is deterministic and tested.

Evidence:
- API test results for session endpoints
- Security event logs for revoke attempts

## Gate C - Audit Integrity
- Created/modified/deleted actor and timestamps are populated.
- Old/new values are captured for change tracking where required.
- Soft delete conventions are respected.

Evidence:
- Entity change logs for representative commands
- Database sample rows with audit fields

## Gate D - Immutable Logging
- Log tables reject update/delete operations.
- Migration scripts include immutable policy setup.
- Runtime writes only append new records.

Evidence:
- Migration review note
- DB verification script output

## Gate E - Localization and Error Standards
- ProblemDetails titles/details are localizable.
- Validation messages support TR/DE/EN.
- Fallback language behavior is documented.

Evidence:
- Multi-language API response captures
- Resource key inventory

## Gate F - Documentation and Training Traceability
- Change record exists for every significant implementation.
- Architecture delta and security delta are updated.
- Session training report explains what changed and why.

Evidence:
- Session report markdown
- Updated roadmap/docs references

## Sign-off Template
- Phase:
- Date:
- Checked by:
- Result: PASS / FAIL
- Open risks:
- Required follow-up: