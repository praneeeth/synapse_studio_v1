# ASK-POST-CORE-ASKS - POST /core-asks

| Field | Value |
|---|---|
| API ID | ASK-POST-CORE-ASKS |
| Operation | POST /core-asks - operationId createCoreAsk (proposed) |
| Module | ASK |
| Purpose | Create a new Core ASK record with validated business data, supporting draft save, workflow submission, and no-op exit behaviours. Routes the submitted ASK to PPL Review or DPP Ops Review based on submitter leadership-group membership. |
| Complexity | Complex - creates multiple aggregates atomically (Ask, AskVersion, AskResource, WorkflowTask, AskComment, AskAttachmentLink), changes workflow state conditionally on buttonValue and submitter role, and writes an outbox event on SUBMIT. |
| Sources | US-ASK-001; ADO 1857; FR1–FR8; BR1–BR18; AC1–AC7 |
| Standards | ARCH 001; ARCH 002; ARCH 003; ARCH 004; DATA 001; DATA 003; DATA 004; API 001; API 002; SEC 001; SEC 002; SEC 003; WF 001; WF 003; REL 001; REL 002; NTF 001; NTF 002; OBS 001; TEST 001 |
| Status | Blocked |

## Contract summary

| Field | Value |
|---|---|
| Path and query | POST /core-asks — no path or query parameters |
| Request schema | CreateCoreAskRequest — body contains: coreAskDetails (coreAskName, dppGroupId, needReasonId, generalSpecialityNeedId, generalSpecialityNeedComment, levelNeedId, pml, projectedStartDate, endDate, outgoingResource, employeeId, headCountAmount, fteAmount, rolePostingId, titlingCategory, transitionalCoach, roleSummary, roleResponsibility, roleQualification); comment (string); documents (file upload array — exact MIME types, size limits and encoding TBD OI-6); buttonValue (Save & Exit \| SUBMIT \| Exit). Note: duplicate field numberofresources appears twice in the specification schema — treated as a specification defect (OI-5). |
| Response schema | CreateCoreAskResponse — returns askId, askDetailId, coreAskDetails (echo of accepted fields), version, task (taskId, statusId, assigneeId), comment (commentId), auditHistory (auditId). Returned on Save & Exit and SUBMIT only; Exit returns no body (TBD OI-4). |
| Success status | 201 Created for Save & Exit and SUBMIT; TBD OI-4 for Exit (no persistence — 204 or client-side no-op) |
| Idempotency | TBD OI-7 — whether a client-supplied idempotency key is required to prevent duplicate ASK creation on retry is unresolved (REL 001). |
| Concurrency | Not applicable on creation; rowversion returned in response for subsequent updates (DATA 004). |

## Authorization

| Control | Value |
|---|---|
| Authentication | SEC 001 — validated Entra JWT required on every call. |
| Permission | TBD OI-1 — exact application permission code (e.g. ASK.CREATE) not defined in specification; owner: Security. |
| Role condition | Leadership-group membership determines SUBMIT routing (AC2 vs AC3); exact role(s) or group(s) that qualify as leadership submitters TBD OI-2. |
| Record scope | Caller must belong to an authorized BusinessUnit for Core ASK creation; exact BU scope rule TBD OI-10 (SEC 002). |
| Workflow scope | No prior task required — this is a creation operation; workflow instance is created by this call. |
| Audit | Actor, action (Create), resulting statusId, correlation ID, and CreatedAt recorded on Ask and WorkflowHistory (OBS 001). |

## Processing flow

1. Validate Entra JWT and resolve current DRT actor (SEC 001; OBS 001).
2. If buttonValue is Exit, return immediately with no persistence — no further steps executed (AC4; BR — buttonValue controls behaviour).
3. Validate all request fields: required presence, max lengths, active reference IDs for dppGroupId, needReasonId, generalSpecialityNeedId, levelNeedId; return 400 validation_failed on failure (FR3; BR1–BR9; AC6).
4. Apply conditional clearing rules: if needReasonId is the first option, clear outgoingResource, employeeId, projectedStartDate, endDate; if generalSpecialityNeedId is the first option, clear generalSpecialityNeedComment (BR4; BR5).
5. Apply cross-field business validation: endDate must be after projectedStartDate and not earlier than today unless needReasonId is retirement; fteAmount must not exceed headCountAmount; rolePostingId required when levelNeedId is in the top-3 set; return 400 validation_failed on failure (BR10–BR15; AC5; AC7 — OI-3 on 422 vs 400 conflict).
6. Authorize caller: confirm active DRT user, required permission, and BU scope (SEC 002; SEC 003; OI-1; OI-10).
7. Determine target workflow status: buttonValue Save & Exit → statusId 123 In Progress; buttonValue SUBMIT and non-leadership submitter → statusId 127 PPL Review; buttonValue SUBMIT and leadership-group submitter → statusId 145 DPP Ops Review (AC1; AC2; AC3; WF 001; OI-2).
8. Within one database transaction: insert Ask, AskVersion, AskResource; insert AskComment if comment is present; insert AskAttachmentLink records for each document; insert WorkflowTask for the target status; insert WorkflowHistory; insert OutboxMessage when buttonValue is SUBMIT (DATA 003; WF 003; REL 002; OI-8).
9. Commit transaction with one SaveChangesAsync; on concurrency conflict return 409 concurrency_conflict (DATA 003; DATA 004).
10. Return 201 Created with CreateCoreAskResponse containing new identifiers, version, task, comment, and auditHistory (FR4; API 001; API 002).

## Business and workflow rules

| Rule ID | Condition | Result |
|---|---|---|
| BR1 | coreAskName is absent or exceeds 200 characters | 400 validation_failed |
| BR2 | dppGroupId is absent or not an active reference option | 400 validation_failed |
| BR3 | needReasonId, generalSpecialityNeedId, or levelNeedId is absent | 400 validation_failed |
| BR4 | needReasonId equals the first option (exact ID TBD OI-5b) | Clear outgoingResource, employeeId, projectedStartDate, endDate from persisted data |
| BR5 | generalSpecialityNeedId equals the first option (exact ID TBD OI-5b) | Clear generalSpecialityNeedComment from persisted data |
| BR6 | generalSpecialityNeedComment is absent and generalSpecialityNeedId is not the first option | 400 validation_failed |
| BR7 | projectedStartDate is absent | 400 validation_failed |
| BR8 | endDate is absent and needReasonId is not retirement | 400 validation_failed |
| BR9 | headCountAmount is absent or fteAmount is absent | 400 validation_failed |
| BR10 | endDate is earlier than today (and needReasonId is not retirement) | 400 validation_failed (OI-3: spec states 422; conflict with MLLD 12.2) |
| BR11 | endDate is not after projectedStartDate (and needReasonId is not retirement) | 400 validation_failed (OI-3: spec states 422; conflict with MLLD 12.2) |
| BR12 | fteAmount exceeds headCountAmount | 400 validation_failed (OI-3: spec states 422; conflict with MLLD 12.2) |
| BR13 | levelNeedId is in the top-3 set (exact IDs TBD OI-4b) and rolePostingId is absent | 400 validation_failed |
| BR14 | needReasonId is retirement | endDate validation (BR8, BR10, BR11) is exempt; exact scope of exemption TBD OI-11 |
| BR15 | buttonValue is Exit | No data persisted; return immediately |
| BR16 | buttonValue is Save & Exit | Persist with statusId 123 In Progress; no WorkflowTask or OutboxMessage |
| BR17 | buttonValue is SUBMIT and submitter is not in leadership group | Route to statusId 127 PPL Review; create WorkflowTask and OutboxMessage |
| BR18 | buttonValue is SUBMIT and submitter is in leadership group | Route to statusId 145 DPP Ops Review; create WorkflowTask and OutboxMessage |

## Data impact

| Operation | Entity or table | Purpose |
|---|---|---|
| Insert | Ask | Root aggregate record for the new Core ASK |
| Insert | AskVersion | First version of the ASK detail data |
| Insert | AskResource | Resource demand data linked to the version |
| Insert | AskComment | Caller-supplied comment, when present |
| Insert | AskAttachmentLink | One row per uploaded document, when present |
| Insert | WorkflowTask | Current human task for the target status (SUBMIT only) |
| Insert | WorkflowHistory | Append-only transition record from creation to target status |
| Insert | OutboxMessage | Post-commit notification event (SUBMIT only; REL 002) |
| Read | Reference data (app schema) | Validate dppGroupId, needReasonId, generalSpecialityNeedId, levelNeedId, rolePostingId against active options |
| Read | User / UserBusinessUnit (security schema) | Resolve actor, confirm active status and BU scope (SEC 002) |

## Events and integrations

| Item | Specification |
|---|---|
| Outbox event | Emitted on SUBMIT only; event type, version and minimal payload TBD OI-9; aggregate type Ask, aggregateId = new askId; written in the same transaction as the business change (REL 002; WF 003). |
| Email | Triggered by outbox worker after commit; recipient rule and template key TBD OI-9 (NTF 001; SEC 003). |
| SignalR | WorkQueueChanged emitted by outbox worker after commit to refresh assignee work queue (NTF 002). |

## Errors and tests

| Area | Content |
|---|---|
| Errors | validation_failed 400 — required field absent; validation_failed 400 — dppGroupId not active; validation_failed 400 — endDate not after projectedStartDate (OI-3); validation_failed 400 — fteAmount exceeds headCountAmount (OI-3); validation_failed 400 — rolePostingId absent when levelNeedId in top-3 set; authentication_required 401 — missing or invalid JWT; access_denied 403 — permission or BU scope denied; concurrency_conflict 409 — duplicate commit; unexpected_error 500 — unhandled failure |
| Tests | Save draft with valid data and buttonValue Save & Exit returns 201 with statusId 123; Submit as non-leadership user returns 201 with statusId 127 PPL Review; Submit as leadership user returns 201 with statusId 145 DPP Ops Review; Exit buttonValue returns no persisted data; fteAmount exceeds headCountAmount returns 400; required field absent returns 400; endDate before today returns 400; endDate not after projectedStartDate returns 400; rolePostingId absent when levelNeedId in top-3 set returns 400; invalid dppGroupId returns 400; unauthenticated caller returns 401; caller without required permission returns 403 |

## Assumptions and open items

| ID | Item | Owner | Decision required |
|---|---|---|---|
| OI-1 | Exact application permission code required to call POST /core-asks (e.g. ASK.CREATE) is not defined in the specification. | Security | Confirm permission code before authorization policy can be finalised. |
| OI-2 | Definition of leadership-group membership used to route SUBMIT to statusId 145 vs 127 — which role(s) or group(s) qualify — is not specified. | Business Analysis | Confirm role or group criteria before workflow routing logic can be implemented. |
| OI-3 | Specification returns 422 for endDate and fteAmount business validation failures; MLLD 12.2 defines only 400 validation_failed for invalid requests. These two sources conflict. Card uses 400 pending resolution. | Solution Architecture | Confirm approved HTTP status code for business validation failures before error contract is finalised. |
| OI-4 | buttonValue=Exit as a POST /core-asks call with no persistence has no precedent in the Master LLD API catalogue. Success status code (204 or other) and whether the endpoint should accept this call are unresolved. | Solution Architecture | Confirm whether Exit is an API concern or client-side navigation only. |
| OI-5 | The specification request and response schemas contain a duplicate field numberofresources with two different values (1 and 2) in the same object. This is a specification defect. | API Contract owner | Resolve duplicate field before schema can be modelled. |
| OI-5b | Exact reference IDs for the 'first option' of needReasonId and generalSpecialityNeedId that trigger clearing rules (BR4, BR5) are not defined. | Business Analysis | Provide reference data IDs for clearing rule conditions. |
| OI-4b | Exact reference IDs constituting the 'top-3 set' for levelNeedId that make rolePostingId mandatory (BR13) are not defined. | Business Analysis | Provide reference data IDs for the top-3 set. |
| OI-6 | File-upload contract for the documents field (MIME types, size limits, number of files, multipart vs base64 encoding) is represented only as 'file upload array' in the specification. | API Contract owner | Define upload contract before endpoint schema can be approved. |
| OI-7 | Whether a client-supplied idempotency key is required to prevent duplicate ASK creation on retry is not addressed in the specification. | Solution Architecture | Confirm idempotency key requirement and scope (REL 001). |
| OI-8 | Transaction scope — whether Ask, AskVersion, WorkflowTask, AskComment, AskAttachmentLink and OutboxMessage must all commit in a single SaveChangesAsync, or whether attachment handling is deferred — is explicitly noted as unconfirmed in the specification. | Solution Architecture | Confirm transaction boundary before DATA 003 compliance can be verified. |
| OI-9 | Outbox event type, version, minimal payload, recipient rule and template key for the WorkflowTaskCreated event emitted on SUBMIT are not defined. | Business Analysis | Provide event specification before REL 002 and NTF 001 can be implemented. |
| OI-10 | BU scope rule — which BusinessUnit(s) the caller must belong to in order to create a Core ASK — is not specified. | Security | Confirm BU scope rule before SEC 002 authorization can be implemented. |
| OI-11 | Whether needReasonId = retirement exempts endDate entirely or only from the 'must be after projectedStartDate' rule is ambiguous in the specification. | Business Analysis | Clarify retirement exemption scope for BR14. |
| OI-12 | Specification path is POST /core-asks; MLLD 15 proposed catalogue defines POST /api/v1/asks for Core or Rotational ASK creation. Path and type-discriminator approach must be confirmed. | Solution Architecture | Confirm approved endpoint path before route contract is finalised. |
