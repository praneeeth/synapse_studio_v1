# ASK-POST-CORE-ASKS - POST /core-asks

| Field | Value |
|---|---|
| API ID | ASK-POST-CORE-ASKS |
| Operation | POST /core-asks - operationId createCoreAsk (proposed) |
| Module | ASK |
| Purpose | Create a new Core ASK record in draft or submitted state. Supports Save & Exit (draft), SUBMIT (workflow routing), and Exit (no-op) button modes. |
| Complexity | Complex - SUBMIT mode changes workflow state, creates a WorkflowTask, writes an OutboxMessage, and commits multiple aggregates (Ask, AskVersion, WorkflowInstance, WorkflowTask, WorkflowDecision, WorkflowHistory, OutboxMessage, AskComment, AskAttachmentLink, audit) atomically in one transaction. |
| Sources | US-ASK-001; ADO 1857; FR1, FR2, FR3, FR4, FR5, FR6, FR7, FR8, FR9, FR10, FR11, FR12; BR1–BR26; AC1–AC7 |
| Standards | ARCH 001, ARCH 002, ARCH 003, ARCH 004, DATA 001, DATA 003, DATA 004, API 001, API 002, SEC 001, SEC 002, SEC 003, WF 001, WF 003, REL 001, REL 002, OBS 001, TEST 001 |
| Status | Ready for Review |

## Contract summary

| Row | Detail |
|---|---|
| Path and query | POST /core-asks — no path or query parameters |
| Request schema | CreateCoreAskRequest (proposed): `coreAskDetails` object (coreAskName, dppGroupId, needReasonId, generalSpecialityNeedId, generalSpecialityNeedComment, levelNeedId, pml, projectedStartDate, endDate, outgoingResource, employeeId, headCountAmount, fteAmount, rolePostingId, numberofresources [TBD OI-9], titlingCategory, transitionalCoach, roleSummary, roleResponsibility, roleQualification); `comment` string (optional); `documents` file-upload array (optional); `buttonValue` enum {"Save & Exit", "SUBMIT", "Exit"} |
| Response schema | CreateCoreAskResponse (proposed): askId, askDetailId, taskId, statusId, assigneeId, commentId (when comment provided), auditId, version (1), coreAskDetails echo |
| Success status | TBD (OI-6) — spec states 200 or 201; not confirmed |
| Idempotency | Not idempotent; duplicate submissions create separate records. REL 001 applies for retry handling. |
| Concurrency | Not applicable on create; DATA 004 applies on subsequent updates. |

## Authorization

| Row | Detail |
|---|---|
| Authentication | Entra bearer token required — SEC 001 |
| Permission | TBD (OI-1) — caller must hold the "Create New ASK" permission; exact permission code not in RBAC matrix |
| Role condition | Leadership group membership determines SUBMIT routing path (FR5, BR24, BR25); membership is derived server-side from authenticated caller context — SEC 003. Definition of leadership group is TBD (OI-2). |
| Record scope | Caller creates a new record; no existing record scope check required at creation — SEC 002 |
| Workflow scope | Not applicable at creation |
| Audit | Creation event written to AuditId record — OBS 001 |

## Processing flow

1. Authenticate caller via Entra bearer token (SEC 001); return 401 `authentication_required` if token is missing or invalid.
2. Authorise caller: verify "Create New ASK" permission TBD (OI-1); return 403 `access_denied` if denied (SEC 002).
3. If buttonValue is "Exit", return success response immediately without persisting any data (FR6, BR23, AC4).
4. Validate all required and conditional fields against BR1–BR22; return 400 `validation_failed` with field-level detail for any missing required field or invalid active reference ID (FR3, AC6).
5. Apply business-rule constraint checks: fteAmount ≤ headCountAmount (BR16, AC5); endDate > projectedStartDate (BR12, AC7); endDate ≥ today (BR13); return TBD (OI-10) error status for violations — spec states 422, Master LLD defines 400 only; difference recorded.
6. Derive outgoingResource server-side from employeeId (FR11, BR14); do not accept outgoingResource from the request body (SEC 003).
7. Determine target statusId: buttonValue "Save & Exit" → 123 In Progress (FR4, BR26, AC1); buttonValue "SUBMIT" and caller is not in leadership group → 127 PPL Review (FR5, BR24, AC2); buttonValue "SUBMIT" and caller is in leadership group → 145 DPP Ops Review (FR5, BR25, AC3).
8. Persist Ask, AskVersion, WorkflowInstance, WorkflowTask, WorkflowDecision, WorkflowHistory, OutboxMessage, AskComment (when provided), AskAttachmentLink records (when documents provided), and audit record in a single atomic transaction (DATA 003, WF 003); transaction scope is TBD (OI-7).
9. Publish outbox message for downstream notification or integration after commit (REL 002, WF 003).
10. Return success response with askId, askDetailId, taskId, statusId, assigneeId, commentId, auditId, version 1, and coreAskDetails echo (FR7, FR12).

## Business and workflow rules

| Rule ID | Condition | Result |
|---|---|---|
| BR1 | coreAskName is absent or exceeds 200 characters | 400 `validation_failed` |
| BR2 | dppGroupId is absent or not an active reference option | 400 `validation_failed` |
| BR3 | needReasonId is absent | 400 `validation_failed` |
| BR4 | generalSpecialityNeedId is absent | 400 `validation_failed` |
| BR5 | levelNeedId is absent | 400 `validation_failed` |
| BR6 | needReasonId equals the first option TBD (OI-3) | Server clears outgoingResource, employeeId, projectedStartDate, endDate |
| BR7 | generalSpecialityNeedId equals the first option TBD (OI-3) | Server clears generalSpecialityNeedComment |
| BR8 | generalSpecialityNeedId is not the first option and generalSpecialityNeedComment is absent | 400 `validation_failed` |
| BR9 | pml provided and exceeds 99 characters; new-version-only applicability TBD (OI-8) | 400 `validation_failed` |
| BR10 | projectedStartDate is absent | 400 `validation_failed` |
| BR11 | endDate is absent and needReasonId is not the retirement value TBD (OI-4) | 400 `validation_failed` |
| BR12 | endDate is present and not after projectedStartDate | TBD (OI-10) error — spec 422, Master LLD 400 |
| BR13 | endDate is present and earlier than today | TBD (OI-10) error — spec 422, Master LLD 400 |
| BR14 | outgoingResource supplied in request body | Ignored; value derived server-side from employeeId (SEC 003) |
| BR15 | headCountAmount is absent | 400 `validation_failed` |
| BR16 | fteAmount is absent or exceeds headCountAmount | TBD (OI-10) error — spec 422, Master LLD 400 |
| BR17 | levelNeedId is in the top-3 set TBD (OI-5) and rolePostingId is absent | 400 `validation_failed` |
| BR18 | titlingCategory is absent; new-version-only applicability TBD (OI-8) | 400 `validation_failed` |
| BR19 | transitionalCoach provided and exceeds 99 characters; new-version-only applicability TBD (OI-8) | 400 `validation_failed` |
| BR20 | roleSummary is absent | 400 `validation_failed` |
| BR21 | roleResponsibility is absent | 400 `validation_failed` |
| BR22 | roleQualification is absent | 400 `validation_failed` |
| BR24 | buttonValue is SUBMIT and caller is not in leadership group | Route to statusId 127 PPL Review |
| BR25 | buttonValue is SUBMIT and caller is in leadership group TBD (OI-2) | Route to statusId 145 DPP Ops Review |
| BR26 | buttonValue is Save & Exit | Route to statusId 123 In Progress |

## Data impact

| Operation | Entity or table | Purpose |
|---|---|---|
| INSERT | Ask | New Core ASK aggregate root (FR1) |
| INSERT | AskVersion | Version 1 of the ASK detail fields (FR12) |
| INSERT | WorkflowInstance | Workflow instance for the new ASK (WF 001) |
| INSERT | WorkflowTask | Initial task assigned to routing target (FR7) |
| INSERT | WorkflowDecision | Decision record for the create action (WF 003) |
| INSERT | WorkflowHistory | History entry for the initial state transition (WF 003) |
| INSERT | OutboxMessage | Outbox entry for downstream notification or integration (REL 002) |
| INSERT | AskComment | Comment record when comment payload is provided (FR8) |
| INSERT | AskAttachmentLink | Attachment link records when documents payload is provided (FR9) |
| INSERT | Audit record | Creation audit event (OBS 001) |

## Errors and tests

| Area | Content |
|---|---|
| Errors | `authentication_required` - missing or invalid bearer token; `access_denied` - caller lacks create permission; `validation_failed` - required field absent or reference ID inactive; TBD (OI-10) - fteAmount exceeds headCountAmount (BR16); TBD (OI-10) - endDate not after projectedStartDate (BR12); TBD (OI-10) - endDate earlier than today (BR13); `dependency_unavailable` - upstream reference or employee service unreachable |
| Tests | AC1 Save & Exit persists draft at status 123 and returns all identifiers; AC2 SUBMIT non-leadership routes to status 127 PPL Review; AC3 SUBMIT leadership routes to status 145 DPP Ops Review; AC4 Exit returns success with no persisted data; AC5 fteAmount exceeds headCountAmount returns TBD error status; AC6 missing required field returns 400 validation_failed; AC7 endDate not after projectedStartDate returns TBD error status; endDate earlier than today returns TBD error status; invalid or inactive dppGroupId returns 400; unauthenticated caller returns 401; unauthorised caller returns 403; upstream service unavailable returns 503 |

## Assumptions and open items

| ID | Item | Owner | Decision required |
|---|---|---|---|
| OI-1 | Exact permission code granting "Create New ASK" access is not in the RBAC matrix | Security | Confirm permission code and add to RBAC matrix |
| OI-2 | Definition of "leadership group": which role(s), group(s) or permission flag the API checks to determine SUBMIT routing path | Security | Confirm group or permission identifier |
| OI-3 | Reference data IDs for the "first option" of needReasonId (BR6) and generalSpecialityNeedId (BR7) that trigger field-clearing behaviour | Business Analysis | Confirm option IDs |
| OI-4 | Reference data ID or value for "retirement" needReasonId that makes endDate optional (BR11) | Business Analysis | Confirm retirement option ID |
| OI-5 | Enumerated set of levelNeedId values constituting the "top-3 set" that makes rolePostingId required (BR17) | Business Analysis | Confirm level IDs in the set |
| OI-6 | Confirmed success HTTP status code: spec states 200 or 201 for a successful create | API Contract owner | Confirm 200 or 201 |
| OI-7 | Transaction scope: whether Ask, AskVersion, WorkflowInstance, WorkflowTask, WorkflowDecision, WorkflowHistory, OutboxMessage, AskComment, AskAttachmentLink, and audit must all commit atomically or any are deferred (spec Assumption 1) | Solution Architecture | Confirm atomic boundary |
| OI-8 | "New-version only" qualifier on pml (BR9), titlingCategory (BR18), transitionalCoach (BR19): whether these fields are in scope for the initial create or reserved for amend | Business Analysis | Confirm applicability to create operation |
| OI-9 | numberofresources appears twice in the request schema in the ADO description; assumed documentation error (spec Assumption 12) | API Contract owner | Confirm single field definition and validation rules |
| OI-10 | HTTP status for business-rule constraint violations: spec uses 422 for BR12, BR13, BR16; Master LLD error table defines only 400 `validation_failed` for invalid requests; 422 is not in the approved error table | Solution Architecture | Confirm approved status code for constraint violations |
| OI-11 | API path difference: spec states POST /core-asks; Master LLD catalogue proposes POST /api/v1/asks | Solution Architecture | Confirm canonical path |
| OI-12 | Workflow status numeric IDs (123, 127, 145) used in spec; Master LLD uses named states only; numeric-to-name mapping not confirmed in Master LLD or state-transition document | Business Analysis | Confirm numeric ID to named state mapping |
