# Design Cards - ADO 1857

**Result:** APPROVED
**Score:** 100/100 (pass mark 100) after 1 attempt(s)
**Branch:** main

## Cards

| Card ID | Type | Operation or change | Complexity | Status | Result |
|---|---|---|---|---|---|
| ASK-POST-CORE-ASKS | API | POST /core-asks | Complex | Ready for Review | PASS |

## Needs attention

- ASK-POST-CORE-ASKS: Status ID to name mapping (123, 127, 145) not confirmed in any approved source - owner Business Analysis
- ASK-POST-CORE-ASKS: 'Top-3 set' for levelNeedId (BR17) — specific reference data IDs not defined - owner Business Analysis
- ASK-POST-CORE-ASKS: 'First option' ID for needReasonId (BR6) and generalSpecialityNeedId (BR7) — specific reference data IDs not defined - owner Business Analysis
- ASK-POST-CORE-ASKS: 'Retirement' value for needReasonId (BR11) — specific reference data ID not defined - owner Business Analysis
- ASK-POST-CORE-ASKS: 'New-version only' field semantics for pml, titlingCategory, transitionalCoach (BR9, BR18, BR19) — not clarified - owner Business Analysis
- ASK-POST-CORE-ASKS: Leadership group membership rule and attribute source for routing (BR24, BR25, AC2, AC3) — not present in RBAC matrix reference - owner Security
- ASK-POST-CORE-ASKS: Exact permission code for 'Create New ASK' — permission name not specified - owner Security
- ASK-POST-CORE-ASKS: Success HTTP status code (200 or 201) not confirmed - owner API Contract owner
- ASK-POST-CORE-ASKS: Idempotency key requirement and scope for the create command — not specified - owner Solution Architecture
- ASK-POST-CORE-ASKS: Transaction scope across Ask, Task, Comment, Audit, and Attachment entities — explicitly unconfirmed - owner Solution Architecture

**Differences from Master LLD recorded for awareness (not blockers):**
- API path: spec uses POST /core-asks; Master LLD catalogue (MLLD §15) proposes POST /api/v1/asks — owner Solution Architecture
- HTTP 422 used for business rule violations; Master LLD error catalogue (MLLD §12.2) defines only 400 — owner Solution Architecture
- Leadership submitter routing shortcut detail unconfirmed in Master LLD (open item LLD T03) — owner Solution Architecture
- buttonValue=Exit no-op pattern has no equivalent in Master LLD — owner Solution Architecture
- Workflow status numeric IDs (123, 127, 145) not mapped to named states in Master LLD (MLLD §7.4) — owner Business Analysis

## Files

- API Design Cards: design/api-cards/
- Change Design Notes: design/change-notes/
- Evaluation record: design/runs/1857/design-card-evaluation.json

## Next step

APPROVED: API contract stage for the API cards.
