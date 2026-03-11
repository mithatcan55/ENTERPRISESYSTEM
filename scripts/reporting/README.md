# Reporting Scripts

## Purpose
Generate standardized documentation files after each development session.

## Commands
```powershell
./scripts/reporting/new-session-report.ps1 -Title "auth-localization-baseline"
./scripts/reporting/new-change-record.ps1 -Title "identity-namespace-cleanup"
```

## Output
- `docs/sessions/*-session-report.md`
- `docs/change-records/*-change-record.md`

## Notes
- Use `-Force` only when intentionally overwriting an existing file.
- Fill the generated files before commit.