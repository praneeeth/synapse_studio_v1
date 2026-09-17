# Design Cards - ADO 1857

**Result:** BLOCKED
**Score:** 100/100 (pass mark 100) after 1 attempt(s)
**Branch:** main

## Cards

| Card ID | Type | Operation or change | Complexity | Status | Result |
|---|---|---|---|---|---|
| ASK-POST-CORE-ASKS | API | POST /core-asks | Complex | Blocked | PASS |

## Needs attention

**Blockers (must be resolved before API contract stage)**

- ASK-POST-CORE-ASKS: HTTP status code conflict — specification uses 422 for business validation failures (endDate, fteAmount) but MLLD 12.2 defines only 400 validation_failed. Decision needed: which status code is approved? Owner: Solution Architecture
- ASK-POST-CORE-ASKS: Endpoint path conflict — specification defines POST /core-asks but MLLD 15 proposed catalogue defines POST /api/v1/asks with a type discriminator. Decision needed: confirm path and whether a type discriminator is required. Owner: Solution Architecture
- ASK-POST-CORE-ASKS: Duplicate field numberofresources appears twice in the request and response schemas. Decision needed: remove the duplicate and confirm the correct value. Owner: API Contract owner

**Spec-vs-MLLD differences requiring a decision**

- ASK-POST-CORE-ASKS: Workflow routing on SUBMIT — leadership-group submitters route to status 145 DPP Ops Review, bypassing PPL Review. Exact bypass path not fully specified in MLLD 7.4. Owner: Solution Architecture
- ASK-POST-CORE-ASKS: buttonValue=Exit as a no-op API call — no equivalent pattern in the Master LLD API catalogue. Owner: Solution Architecture

**Missing facts (open items)**

- ASK-POST-CORE-ASKS: Exact permission code required to call POST /core-asks — owner: Security
- ASK-POST-CORE-ASKS: Definition of leadership group membership rule for routing to status 145 vs 127 — owner: Business Analysis
- ASK-POST-CORE-ASKS: Status ID to name mapping (123 = In Progress, 127 = PPL Review, 145 = DPP Ops Review) — confirmation required — owner: Business Analysis
- ASK-POST-CORE-ASKS: Exact reference IDs for the top-3 set of levelNeedId that triggers mandatory rolePostingId — owner: Business Analysis
- ASK-POST-CORE-ASKS: Exact reference IDs for first option of needReasonId and generalSpecialityNeedId used in clearing rules — owner: Business Analysis
- ASK-POST-CORE-ASKS: File-upload contract for documents field (MIME types, size limits, number of files, multipart vs base64) — owner: API Contract owner
- ASK-POST-CORE-ASKS: Idempotency key requirement and scope for this POST operation — owner: Solution Architecture
- ASK-POST-CORE-ASKS: Transaction scope confirmation for all aggregates and OutboxMessage in single SaveChangesAsync — owner: Solution Architecture
- ASK-POST-CORE-ASKS: Outbox event type, version, minimal payload, recipient rule and template key for SUBMIT event — owner: Business Analysis
- ASK-POST-CORE-ASKS: BU scope rule for Core ASK creation — owner: Security
- ASK-POST-CORE-ASKS: Whether needReasonId = retirement exempts endDate entirely or only from the after-projectedStartDate rule — owner: Business Analysis
- ASK-POST-CORE-ASKS: Approved OpenAPI operation ID and schema component names — owner: API Contract owner

## Files

- API Design Cards: design/api-cards/
- Change Design Notes: design/change-notes/
- Evaluation record: design/runs/1857/design-card-evaluation.json

## Next step

BLOCKED: The card ASK-POST-CORE-ASKS has been published to design/api-cards/ and can proceed to the API contract stage once the three blockers above are resolved. Resolve the blockers listed under Needs attention before the API contract can be finalised.
