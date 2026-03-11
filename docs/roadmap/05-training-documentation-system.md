# Training and Documentation System

## Objective
Every implementation step should be understandable later as a training artifact.
A new AI session should reconstruct project intent and reasoning from docs alone.

## Required Records Per Work Session
1. Session summary (scope and result)
2. Change record (what changed)
3. Why record (why this way)
4. Risk and rollback notes
5. Compliance evidence links

## Canonical Files
- project-book.md (long-form architecture history)
- project-map.md (current state snapshot)
- docs/roadmap/* (forward plan and standards)
- learning-log.md (session-level practical log)
- session-handoff-YYYY-MM-DD.md (handoff continuity)

## Report Quality Rules
1. Name exact files and endpoint names.
2. Explain cause and effect, not only actions.
3. Record rejected alternatives briefly.
4. Include validation/test evidence.
5. Include next-step with one clear target.

## AI-Ready Structure
When writing entries, include these headings in order:
1. Context
2. Problem
3. Decision
4. Implementation
5. Verification
6. Risks
7. Next Step

## Change Record Minimal Contract
- Date
- Scope
- Impacted Layers
- Security Impact
- Audit Impact
- Localization Impact
- API Contract Impact
- Migration Impact
- Test Evidence

## Documentation Workflow
1. Before coding: define scope in one paragraph.
2. During coding: append small notes to learning log.
3. After coding: produce session report from template.
4. Update project map and project book if architecture changed.
5. Commit docs with code in same logical unit.