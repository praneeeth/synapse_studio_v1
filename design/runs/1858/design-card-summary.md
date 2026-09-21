# Design Cards - ADO 1858

**Result:** NEEDS REVIEW
**Score:** 86/100 (pass mark 100) after 1 attempt(s)
**Branch:** main

## Cards

| Card ID | Type | Operation or change | Complexity | Status | Result |
|---|---|---|---|---|---|
| ASK-POST-ASKS-ID-CANCEL | API | POST /core-asks/{askId}/cancel | Complex | Ready for Review | FAIL |

## Needs attention

- **ASK-POST-ASKS-ID-CANCEL — Prohibited SQL content (items 2 and 13 FAIL):** The card contains a SQL statement ('Update Ask Set') in the Data Impact section. DC-R1 prohibits SQL statements in design cards. Remove the SQL and replace with a plain English description of the operation (e.g. 'Set statusId to 577, record cancelledAt and cancelledBy, increment rowVersion'). Re-run the checker after fixing.
- **OI-01:** DPP Ops permission code not defined — owner Security
- **OI-02:** cancelledBy field in request body conflicts with SEC 003 — owner Solution Architecture
- **OI-03:** Outbox event type, payload and recipients not defined — owner Business Analysis
- **OI-04:** Full list of cancellable ASK statuses and status ID mapping not confirmed — owner Business Analysis
- **OI-05:** Idempotency key scope and header not defined — owner Solution Architecture
- **OI-06:** Maximum length for comment field not specified — owner Business Analysis
- **OI-07:** Audit entity name not confirmed in MLLD 9.2 — owner Solution Architecture
- **OI-08:** BU scope rule for cancellation not defined — owner Security
- **OI-09:** API path prefix and resource name differ from Master LLD catalogue — owner Solution Architecture
- Draft kept at: design/drafts/ASK-POST-ASKS-ID-CANCEL.draft.md

## Files

- API Design Cards: design/api-cards/
- Change Design Notes: design/change-notes/
- Evaluation record: design/runs/1858/design-card-evaluation.json

## Next step

NEEDS REVIEW: a person reviews the drafts in design/drafts. Fix the prohibited SQL content in the Data Impact section of ASK-POST-ASKS-ID-CANCEL, then re-run the workflow.
