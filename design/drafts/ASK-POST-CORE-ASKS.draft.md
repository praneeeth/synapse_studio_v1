# API Design Card — ASK-POST-CORE-ASKS

## 5.1 API Identification

| Field | Value |
|---|---|
| API ID | ASK-POST-CORE-ASKS |
| Operation | POST /core-asks — operationId: createCoreAsk |
| Module | ASK |
| Purpose | Create a new Core ASK record in draft or submitted state based on the caller's buttonValue and leadership group membership. On SUBMIT, the operation starts the Core ASK review workflow and creates the first WorkflowTask. |
| Complexity | Complex — SUBMIT mode changes workflow state, creates a WorkflowTask, writes WorkflowHistory and OutboxMessage, and mutates multiple aggregates atomically (Ask, AskVersion, WorkflowInstance, WorkflowTask, WorkflowDecision, WorkflowHistory, AskComment, AskAttachmentLink); external reference-data and employee services are called for validation. |
| Sources | US-ASK-001, ADO 1857, FR1–FR12, BR1–BR26, AC1–AC7, WF 001, WF 003, REL 002, SEC 002, SEC 003 |
| Status | Ready for Review |

---

## 5.2 Contract Summary

| Field | Value |
|---|---|
| Path | POST /core-asks (OI-1: path differs from Master LLD proposed /api/v1/asks — see Open Items) |
| Request schema | CreateCoreAskRequest — contains coreAskDetails, comment (optional), documents (optional array), buttonValue enum {"Save & Exit", "SUBMIT", "Exit"} (FR2, FR10) |
| Response schema | CreateCoreAskResponse — returns askId, askDetailId, taskId, statusId, assigneeId, commentId (when comment provided), auditId, version, coreAskDetails (FR7, FR12) |
| Success status | TBD (OI-2): spec states 200 or 201 — ambiguous; 201 is conventional for POST resource creation under API 001 |
| Idempotency | Not applicable — no idempotency key defined in spec; OI-3 if retry risk is confirmed by Solution Architecture |
| Concurrency | Not applicable on creation — no expectedVersion or ETag on initial POST |

---

## 5.3 Authorization

| Control | Value |
|---|---|
| Authentication | SEC 001 |
| Permission | "Create New ASK" — TBD (OI-4): exact RBAC permission code not provided in spec; owner: Security |
| Role condition | Not required beyond the permission check per approved policy |
| Record scope | No existing record scope on creation; caller's BU scope must cover the dppGroupId selected (SEC 002) |
| Workflow scope | Not applicable on creation — no prior task assignment required |
| Audit | Actor (DRT user ID), action CREATE or SUBMIT, resulting statusId, correlation ID; written to WorkflowHistory and AuditId returned in response (FR7) |

---

## 5.4 Processing Flow

1. **Validate request** — reject missing or malformed fields per BR1–BR22 and BR23; return 400 `validation_failed` (API 002). If buttonValue is "Exit", return success immediately without any persistence (FR6, AC4, BR23).
2. **Resolve current DRT actor** — load active DRT user, roles, permissions, and BU scope from server context (SEC 002, SEC 003).
3. **Authorize** — confirm caller holds the "Create New ASK" permission and that dppGroupId is within the caller's BU scope; return 403 `access_denied` on failure (SEC 002).
4. **Validate reference data** — call reference-data service to confirm dppGroupId, needReasonId, generalSpecialityNeedId, levelNeedId, and (when required by BR17) rolePostingId are active; call employee service to validate employeeId and derive outgoingResource (FR11); return 400 `validation_failed` on any inactive or unknown ID.
5. **Apply business rules** — evaluate conditional field rules BR6–BR19 and date/numeric constraints BR10–BR16; return 400 `validation_failed` for missing required fields (AC6) and 400 `validation_failed` for business constraint violations (OI-5: spec specifies 422 for BR12, BR13, BR16 — see Open Items).
6. **Determine routing** — if buttonValue is "SUBMIT", resolve caller's leadership group membership from server-side DRT actor context (TBD OI-6); non-leadership routes to status 127 (BR24, AC2); leadership routes to status 145 (BR25, AC3). If buttonValue is "Save & Exit", target status is 123 (BR26, AC1).
7. **Persist atomically** (DATA 003, WF 003) — within one SaveChangesAsync: insert Ask, AskVersion; insert WorkflowInstance pinned to current workflow definition version (WF 001); if SUBMIT, insert WorkflowTask and WorkflowDecision; append WorkflowHistory; insert AskComment when comment provided (FR8); insert AskAttachmentLink records when documents provided (FR9); insert OutboxMessage for post-commit notification (REL 002); write audit record.
8. **Commit** — on concurrency or dependency failure, return 409 `concurrency_conflict` or 503 `dependency_unavailable` respectively (API 002).
9. **Return response** — map persisted identifiers to CreateCoreAskResponse including askId, askDetailId, taskId, statusId, assigneeId, commentId, auditId, version 1, and coreAskDetails (FR7, FR12).

---

## 5.5 Business and Workflow Rules

| Rule ID | Condition | Result |
|---|---|---|
| BR1 | coreAskName absent or exceeds 200 characters | 400 validation_failed |
| BR2 | dppGroupId absent or not an active reference value | 400 validation_failed |
| BR3 | needReasonId absent | 400 validation_failed |
| BR4 | generalSpecialityNeedId absent | 400 validation_failed |
| BR5 | levelNeedId absent | 400 validation_failed |
| BR6 | needReasonId equals the "first option" (TBD OI-7) | Clear outgoingResource, employeeId, projectedStartDate, endDate from persisted record |
| BR7 | generalSpecialityNeedId equals the "first option" (TBD OI-8) | Clear generalSpecialityNeedComment from persisted record |
| BR8 | generalSpecialityNeedId is not the first option and generalSpecialityNeedComment is absent | 400 validation_failed |
| BR9 | pml provided and exceeds 99 characters | 400 validation_failed; new-version-only applicability TBD (OI-9) |
| BR10 | projectedStartDate absent | 400 validation_failed |
| BR11 | endDate absent and needReasonId is not the retirement value (TBD OI-10) | 400 validation_failed |
| BR12 | endDate not after projectedStartDate | 400 validation_failed (OI-5: spec specifies 422) |
| BR13 | endDate earlier than today | 400 validation_failed (OI-5: spec specifies 422) |
| BR14 | outgoingResource supplied by caller | Reject; derive from employeeId server-side only (SEC 003) |
| BR15 | headCountAmount absent | 400 validation_failed |
| BR16 | fteAmount absent or exceeds headCountAmount | 400 validation_failed (OI-5: spec specifies 422 for exceeds case) |
| BR17 | levelNeedId is in the top-3 set (TBD OI-11) and rolePostingId absent | 400 validation_failed |
| BR18 | titlingCategory absent; new-version-only applicability TBD (OI-9) | 400 validation_failed |
| BR19 | transitionalCoach provided and exceeds 99 characters; new-version-only applicability TBD (OI-9) | 400 validation_failed |
| BR20 | roleSummary absent | 400 validation_failed |
| BR21 | roleResponsibility absent | 400 validation_failed |
| BR22 | roleQualification absent | 400 validation_failed |
| BR23 | buttonValue = "Exit" | Return success; no persistence |
| BR24 | buttonValue = "SUBMIT" and caller is not in leadership group | Route WorkflowInstance to status 127; create WorkflowTask for PPL Review |
| BR25 | buttonValue = "SUBMIT" and caller is in leadership group | Route WorkflowInstance to status 145; create WorkflowTask for DPP Ops Review |
| BR26 | buttonValue = "Save & Exit" | Persist Ask and AskVersion at status 123; no WorkflowTask created |

---

## 5.6 Data Impact

| Operation | Entity | Purpose |
|---|---|---|
| Insert | Ask | Root aggregate record for the new Core ASK (FR1, FR4, FR5) |
| Insert | AskVersion | Version 1 of the Core ASK detail fields (FR10, FR12) |
| Insert | WorkflowInstance | Links Ask to workflow definition version and initial state (WF 001, FR4, FR5) |
| Insert | WorkflowTask | First human task created on SUBMIT only; not created for Save & Exit (BR24, BR25, BR26) |
| Insert | WorkflowDecision | Records the SUBMIT decision actor, action, and idempotency key on SUBMIT (WF 003) |
| Insert | WorkflowHistory | Append-only transition record: from state, to state, actor, timestamp, correlation ID (WF 003) |
| Insert | AskComment | Created when comment payload is provided (FR8) |
| Insert | AskAttachmentLink | One row per document in the documents array when provided (FR9) |
| Insert | OutboxMessage | Post-commit notification event written atomically with business state (REL 002) |
| Read | Reference data tables | Validate dppGroupId, needReasonId, generalSpecialityNeedId, levelNeedId, rolePostingId (BR2–BR5, BR17) |
| Read | Employee service | Validate employeeId and derive outgoingResource (FR11, BR14) |

---

## 5.7 Events and Integrations

| Item | Specification |
|---|---|
| Outbox event | Type: CoreAskCreated (version TBD OI-12); aggregate type: Ask; aggregate ID: new askId; minimal payload: statusId, assigneeId, correlation ID; written atomically in step 7 (REL 002) |
| Email | Triggered by outbox worker after commit; recipient rule and template key TBD (OI-12); worker behaviour follows shared design — cite REL 002, NTF 001, do not re-plan |
| SignalR | WorkQueueChanged sent by outbox worker to the assignee's group after commit (NTF 002); worker behaviour follows shared design |
| External integration — reference data service | Called in step 4 to validate active option IDs; timeout and retry per shared dependency policy (OBS 001); 503 `dependency_unavailable` on failure |
| External integration — employee service | Called in step 4 to validate employeeId and derive outgoingResource; timeout and retry per shared dependency policy (OBS 001); 503 `dependency_unavailable` on failure |

---

## 5.8 Errors and Tests

### Errors

| Condition | Status | Code |
|---|---|---|
| Missing required field (BR1–BR5, BR10, BR15, BR16, BR20–BR22) | 400 | validation_failed |
| Invalid or inactive reference data ID (BR2–BR5, BR17) | 400 | validation_failed |
| Conditional field violation (BR8, BR11, BR17, BR18) | 400 | validation_failed |
| Date or numeric constraint violation (BR12, BR13, BR16) | 400 | validation_failed (OI-5: spec specifies 422) |
| Caller lacks create permission | 403 | access_denied |
| Reference data or employee service unavailable | 503 | dependency_unavailable |
| Unexpected server failure | 500 | unexpected_error |

### Tests (TEST 001)

Save & Exit persists Ask at status 123 and returns version 1 (AC1); SUBMIT non-leadership routes to status 127 and creates WorkflowTask (AC2); SUBMIT leadership routes to status 145 and creates WorkflowTask (AC3); Exit returns success with no database writes (AC4); fteAmount exceeds headCountAmount returns 400 (AC5, OI-5); mandatory field omitted returns 400 (AC6); endDate not after projectedStartDate returns 400 (AC7, OI-5); caller without create permission returns 403; reference data service unavailable returns 503 with no partial persistence; employee service unavailable returns 503 with no partial persistence; generalSpecialityNeedComment absent when generalSpecialityNeedId is not first option returns 400 (BR8); rolePostingId absent when levelNeedId in top-3 set returns 400 (BR17)

---

## 5.9 Assumptions and Open Items

| ID | Item | Owner | Decision Required |
|---|---|---|---|
| OI-1 | API path: spec uses /core-asks; Master LLD proposes /api/v1/asks with versioned prefix and unqualified plural resource name (MLLD 15, API 001) | Solution Architecture | Confirm approved path and version prefix before OpenAPI contract is finalised |
| OI-2 | Success HTTP status: spec states 200 or 201 — ambiguous; 201 is conventional for POST resource creation | API Contract owner | Confirm single success status code |
| OI-3 | Idempotency key: no key defined in spec; SUBMIT creates a WorkflowDecision and OutboxMessage which are retry-sensitive (REL 001) | Solution Architecture | Confirm whether an idempotency key is required and its scope |
| OI-4 | RBAC permission code: spec names "Create New ASK" in plain English; no permission code from US RBAC 001 matrix provided | Security | Supply exact permission code for authorization policy |
| OI-5 | HTTP 422 vs 400: spec specifies 422 for BR12, BR13, BR16 violations; Master LLD approved error catalogue contains only 400 validation_failed — 422 is not present (MLLD 12.2) | Solution Architecture | Confirm whether 422 is added to the catalogue or all business constraint violations use 400 |
| OI-6 | Leadership group membership: spec requires server-side determination of caller's leadership group for routing (BR24, BR25, AC2, AC3); derivation mechanism not specified — DRT role, permission, BU scope attribute, or separate group flag | Security | Define how leadership group membership is resolved from the DRT actor context |
| OI-7 | "First option" for needReasonId (BR6): specific reference data ID or value not provided | Business Analysis | Supply the reference data ID that represents the first option |
| OI-8 | "First option" for generalSpecialityNeedId (BR7): specific reference data ID or value not provided | Business Analysis | Supply the reference data ID that represents the first option |
| OI-9 | "New-version only" qualifier on pml (BR9), titlingCategory (BR18), transitionalCoach (BR19): unclear whether these fields apply to initial creation in this story or only on amendment | Business Analysis | Confirm applicability to initial creation |
| OI-10 | "Retirement" value for needReasonId (BR11): specific reference data ID or value not provided | Business Analysis | Supply the reference data ID that represents retirement |
| OI-11 | "Top-3 set" for levelNeedId (BR17): specific reference data IDs not provided | Business Analysis | Supply the reference data IDs that constitute the top-3 set |
| OI-12 | OutboxMessage event type, version, notification template key, and recipient rule for CoreAskCreated: not specified in ticket | Business Analysis | Define event type version, template key, and recipient derivation rule |
| OI-13 | Combined create-and-submit in a single endpoint: spec uses one POST /core-asks for Save & Exit, SUBMIT, and Exit; Master LLD proposes separate POST /api/v1/asks (create draft) and POST /api/v1/asks/{id}/submit (start workflow) (MLLD 15) | Solution Architecture | Confirm whether the single combined endpoint is approved or the catalogue split applies |
| OI-14 | Workflow state numeric IDs 123, 127, 145: not mapped to approved state names in Master LLD (Draft/In Progress, Pending PPL Approval, DPP Ops Review equivalent) (MLLD 7.4) | Business Analysis | Supply mapping of numeric IDs to approved workflow state names |
| OI-15 | Transaction atomicity: spec Assumption 1 flags as unconfirmed whether Ask, AskVersion, WorkflowInstance, WorkflowTask, WorkflowDecision, WorkflowHistory, AskComment, AskAttachmentLink, and OutboxMessage are committed in a single SaveChangesAsync (DATA 003) | Solution Architecture | Confirm transaction scope |
| OI-16 | OpenAPI contract: no approved OpenAPI operation or schemas (CreateCoreAskRequest, CreateCoreAskResponse) referenced in the specification | API Contract owner | Provide approved OpenAPI operation definition and component schemas before build |
| OI-17 | Data retention policy for Core ASK records: not specified | Business Analysis | Confirm retention period aligned to LLD A03 (seven-year baseline assumption) |
