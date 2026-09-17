# Design Cards - ADO 1857
**Result:** NEEDS REVIEW
**Score:** 57/100 (pass mark 100) after 1 attempt(s)
**Story:** US-ASK-001

## Cards
| Card ID | Type | Operation or change | Complexity | Status | Result |
|---|---|---|---|---|---|
| ASK-POST-CORE-ASKS | api | POST /core-asks — createCoreAsk | Complex | Ready for Review | FAIL |

## Needs Attention
The card failed automated validation with a score of 57/100. Nothing has been published. The draft remains at `design/drafts/ASK-POST-CORE-ASKS.draft.md`.

### Failed Checks (must be fixed before re-submission)
1. **ASK-POST-CORE-ASKS: Identification section heading lowercase** — Section heading is 'api identification' instead of 'API Identification'; all 7 identification fields (API ID, Operation, Module, Purpose, Complexity, Sources, Status) are unreadable by the checker. Fix: rename heading to title case and populate all fields. — owner Card Author
2. **ASK-POST-CORE-ASKS: Zero test scenarios** — Complex card requires 6–12 named test scenarios; 0 found. All 7 acceptance criteria (AC1–AC7) must have a corresponding named scenario plus an unauthorized-caller scenario. — owner Card Author
3. **ASK-POST-CORE-ASKS: RBAC permission code missing** — Authorization section names permission in plain English only; no permission code from US RBAC 001 matrix and no TBD marker with open item. — owner Security
4. **ASK-POST-CORE-ASKS: Transaction boundary not stated** — Contract Summary lacks Idempotency and Concurrency fields; multi-aggregate write (Ask, AskVersion, WorkflowInstance, WorkflowTask, WorkflowDecision, WorkflowHistory, AskComment, AskAttachmentLink) requires explicit statement or TBD with open item. — owner Solution Architecture
5. **ASK-POST-CORE-ASKS: 422 vs 400 conflict unresolved in Errors section** — Spec specifies 422 for date and FTE violations (AC5, AC7); Master LLD error catalogue (MLLD 12.2) only lists 400 validation_failed. Conflict must be called out in Errors section with TBD marker. — owner Solution Architecture

### Open Items (differences and missing facts to resolve)
- ASK-POST-CORE-ASKS: API path — spec /core-asks vs Master LLD /api/v1/asks — owner Solution Architecture
- ASK-POST-CORE-ASKS: HTTP 422 vs 400 for business rule violations — owner Solution Architecture
- ASK-POST-CORE-ASKS: Success HTTP status code ambiguous (200 or 201) — owner API Contract owner
- ASK-POST-CORE-ASKS: buttonValue=Exit no-op POST pattern not in approved catalogue — owner Solution Architecture
- ASK-POST-CORE-ASKS: Combined create-and-submit vs separate endpoints — owner Solution Architecture
- ASK-POST-CORE-ASKS: Numeric status IDs 123, 127, 145 not mapped to approved workflow state names — owner Business Analysis
- ASK-POST-CORE-ASKS: RBAC permission code for 'Create New ASK' not provided — owner Security
- ASK-POST-CORE-ASKS: 'Top-3 set' for levelNeedId (BR17) not defined — owner Business Analysis
- ASK-POST-CORE-ASKS: 'First option' for needReasonId (BR6) not defined — owner Business Analysis
- ASK-POST-CORE-ASKS: 'First option' for generalSpecialityNeedId (BR7) not defined — owner Business Analysis
- ASK-POST-CORE-ASKS: 'Retirement' value for needReasonId (BR11) not defined — owner Business Analysis
- ASK-POST-CORE-ASKS: Leadership group membership determination not specified (BR24, BR25, AC2, AC3) — owner Security
- ASK-POST-CORE-ASKS: 'New-version only' qualifier on pml (BR9), titlingCategory (BR18), transitionalCoach (BR19) not clarified — owner Business Analysis
- ASK-POST-CORE-ASKS: Transaction scope across all aggregates unconfirmed (spec Assumption 1) — owner Solution Architecture
- ASK-POST-CORE-ASKS: OpenAPI operation definition and approved request/response schemas not referenced — owner API Contract owner
- ASK-POST-CORE-ASKS: Data retention policy not specified — owner Business Analysis

## Files
- Draft: design/drafts/ASK-POST-CORE-ASKS.draft.md
- Evaluation record: design/runs/1857/design-card-evaluation.json
- This report: design/runs/1857/design-card-summary.md

## Next Step
The card author must fix the 5 failed checks listed above and re-submit for validation. No card has been published. Once all checks pass (score = 100/100), the card will be promoted to `design/api-cards/` and the run will proceed to the API contract stage.
