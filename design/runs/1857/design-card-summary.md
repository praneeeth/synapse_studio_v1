# Design Cards - ADO 1857

**Result:** NEEDS REVIEW
**Score:** 93/100 (pass mark 100) after 1 attempt(s)
**Branch:** main

## Cards

| Card ID | Type | Operation or change | Complexity | Status | Result |
|---|---|---|---|---|---|
| ASK-POST-CORE-ASKS | API | POST /core-asks | Complex | Ready for Review | FAIL |

## Needs attention

- **ASK-POST-CORE-ASKS – DC-R1 item 9 FAIL:** Missing sequence/flow diagram. Complex cards require at least 1 diagram; the Design Card Checker reports 0. Fix: add a Mermaid sequence diagram covering the main SUBMIT flow (caller → API → auth → reference-data validation → persistence → workflow routing → response), the Save & Exit branch, and the Exit no-op branch.
- ASK-POST-CORE-ASKS: Exact permission code for 'Create New ASK' and RBAC matrix row not defined — owner Security
- ASK-POST-CORE-ASKS: Definition of 'leadership group' membership (role/group/permission flag) not specified — owner Security
- ASK-POST-CORE-ASKS: Reference data IDs for 'first option' of needReasonId (BR6) and generalSpecialityNeedId (BR7) not defined — owner Business Analysis
- ASK-POST-CORE-ASKS: Reference data ID for 'retirement' needReasonId (BR11) not defined — owner Business Analysis
- ASK-POST-CORE-ASKS: Enumerated set of levelNeedId values for 'top-3 set' (BR17) not defined — owner Business Analysis
- ASK-POST-CORE-ASKS: Confirmed success HTTP status code (200 or 201) not specified — owner API Contract owner
- ASK-POST-CORE-ASKS: Transaction scope across all created entities unconfirmed — owner Solution Architecture
- ASK-POST-CORE-ASKS: 'New-version only' qualifier on pml (BR9), titlingCategory (BR18), transitionalCoach (BR19) applicability to create unconfirmed — owner Business Analysis
- ASK-POST-CORE-ASKS: Duplicate 'numberofresources' field in request schema unresolved — owner API Contract owner
- ASK-POST-CORE-ASKS: Approved OpenAPI components (CreateCoreAskRequest, CreateCoreAskResponse) not yet defined — owner API Contract owner
- ASK-POST-CORE-ASKS: API path difference — spec uses POST /core-asks; Master LLD catalogue uses POST /api/v1/asks — owner Solution Architecture
- ASK-POST-CORE-ASKS: HTTP status code difference — spec uses 422 for business rule violations; Master LLD maps all validation failures to 400 validation_failed — owner Solution Architecture
- ASK-POST-CORE-ASKS: Leadership submitter routing shortcut — exact permission/group definition not confirmed in Master LLD — owner Solution Architecture
- ASK-POST-CORE-ASKS: Workflow status numeric IDs (123, 127, 145) not mapped to named states in Master LLD or state-transition document — owner Business Analysis

## Files

- API Design Cards: design/api-cards/
- Change Design Notes: design/change-notes/
- Evaluation record: design/runs/1857/design-card-evaluation.json

## Next step

NEEDS REVIEW: A person must review the draft in design/drafts/ASK-POST-CORE-ASKS.draft.md. The card failed DC-R1 item 9 (missing sequence/flow diagram for a Complex card). Once the diagram is added and the card re-passes the checker, re-run the validator. Nothing has been published to design/api-cards/.
