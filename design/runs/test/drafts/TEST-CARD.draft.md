# ASK-POST-CORE-ASKS - POST /core-asks
 
| Field | Value |
|---|---|
| API ID | ASK-POST-CORE-ASKS |
| Operation | POST /core-asks - operationId createCoreAsk (proposed) |
| Module | ASK |
| Purpose | Create a new Core ASK and either save it as a draft or submit it for review, based on buttonValue. Exit makes no change. |
| Complexity | Complex - sets the workflow status, creates a task and writes several entities in one request |
| Sources | US-ASK-001; ADO 1857; FR1-FR12; BR1-BR26; AC1-AC7; Master LLD 7.4 Core ASK routing |
| Standards | ARCH 002, ARCH 004, API 002, SEC 001, SEC 002, SEC 003, DATA 001, DATA 003, WF 001, WF 003, OBS 001, TEST 001 |
| Status | Ready for Review |
 
## Contract summary
| Field | Value |
|---|---|
| Path and query | None |
| Request schema | CreateCoreAskRequest (proposed): coreAskDetails, comment, documents, buttonValue (Save & Exit, SUBMIT, Exit) |
| Response schema | CreateCoreAskResponse (proposed): askId, askDetailId, coreAskDetails, version, task (taskId, statusId, assigneeId), comment.commentId, auditHistory.auditId |
| Success status | TBD - 200 or 201 (OI-4). Exit returns success with no changes |
| Idempotency | TBD (OI-5) |
| Concurrency | Not applicable - new record created at version 1 |
 
## Authorization
| Control | Value |
|---|---|
| Authentication | SEC 001 |
| Permission | Create New ASK - exact permission code TBD (OI-3) |
| Role condition | Leadership-group membership decides SUBMIT routing (BR24, BR25); taken from the caller profile, never from the request (SEC 003) |
| Record scope | TBD - business-unit scope is not stated in the story (OI-3) |
| Workflow scope | New record; no existing task |
| Audit | Creator, create action, submitted values and resulting status (FR7) |
 
## Processing flow
1. If buttonValue is Exit, return success and save nothing (BR23, AC4).
2. Resolve the current DRT actor and check the Create New ASK permission (SEC 002); otherwise return 403.
3. Validate required fields, lengths and formats (BR1-BR5, BR9, BR10, BR15, BR18-BR22); otherwise return 400.
4. Check that reference IDs are active (BR2) and apply conditional rules BR6-BR8, BR11 and BR17; otherwise return 400.
5. Apply rules BR12, BR13 and BR16; otherwise return 422 as the story states (OI-2).
6. Derive outgoingResource from employeeId (BR14) and ignore any value sent by the caller.
7. Set the status: Save & Exit gives 123 In Progress; SUBMIT gives 127 PPL Review, or 145 DPP Ops Review for a leadership-group submitter (BR24-BR26).
8. In one transaction, save the ASK, version 1, comment, attachment links, workflow instance, task and audit record (DATA 003, WF 003, OI-7).
9. Return the created identifiers, version 1 and task status (FR7, FR12).
 
## Business and workflow rules
| Rule ID | Condition | Result |
|---|---|---|
| BR6 | needReasonId is the first option | Clear outgoingResource, employeeId, projectedStartDate and endDate |
| BR7, BR8 | generalSpecialityNeedId is the first option | Clear generalSpecialityNeedComment; for any other option the comment is required |
| BR11 | needReasonId is retirement | endDate is not required |
| BR12, BR13 | endDate is provided | Must be after projectedStartDate and not before today; otherwise 422 |
| BR16 | fteAmount is greater than headCountAmount | Reject with 422 |
| BR17 | levelNeedId is in the top-3 set | rolePostingId is required |
| BR9, BR18, BR19 | New-version fields | pml and transitionalCoach optional, max 99 characters; titlingCategory required |
| BR24-BR26 | buttonValue and submitter group | Save & Exit to 123; SUBMIT to 127, or to 145 for the leadership group |
 
## Data impact
| Operation | Entity or table | Purpose |
|---|---|---|
| Insert | Ask | New Core ASK record |
| Insert | AskVersion | Version 1 holding coreAskDetails |
| Insert | AskComment | Only when a comment is provided |
| Insert | AskAttachmentLink | Only when documents are provided |
| Insert | WorkflowInstance | Links the ASK to status 123, 127 or 145 |
| Insert | WorkflowTask | Task and assignee returned in the response |
| Insert | Audit history (name TBD, OI-7) | Creation event returned as auditId |
| Read | Reference data and employee | Validate option IDs and derive outgoingResource |
 
## Events and integrations
| Item | Specification |
|---|---|
| Outbox event | Not applicable - notifications are out of scope in the story |
| Email | Not applicable - out of scope |
| External integration | Attachment upload: contract is only "file upload array" (OI-6) |
 
## Errors and tests
| Area | Content |
|---|---|
| Errors | 400 validation_failed - missing required field or inactive reference ID; 403 access_denied - no create permission; 422 - date or FTE rule broken (story value, OI-2); 503 dependency_unavailable - reference, employee or attachment service down |
| Tests | Save & Exit gives 123; SUBMIT by non-leadership gives 127; SUBMIT by leadership gives 145; Exit saves nothing; missing coreAskName gives 400; inactive dppGroupId gives 400; fteAmount above headCountAmount gives 422; endDate not after projectedStartDate gives 422; endDate before today gives 422; missing rolePostingId for a top-3 level gives 400; caller without permission gives 403; failure during save leaves no records |
 
## Assumptions and open items
| ID | Item | Owner | Decision required |
|---|---|---|---|
| OI-1 | Route /core-asks differs from Master LLD POST /api/v1/asks; save and submit are one call here but separate in the Master LLD | Solution Architecture | Confirm route, version prefix and submit design |
| OI-2 | 422 is not in the Master LLD error table, which uses 400 validation_failed | Solution Architecture | Confirm status code for rule violations |
| OI-3 | Permission code, business-unit scope and leadership-group source are not defined | Security | Provide the RBAC rows |
| OI-4 | Success status 200 or 201 is not stated | API Contract owner | Confirm the status code |
| OI-5 | No idempotency key for a create command (REL 001) | Solution Architecture | Confirm a key or accept none |
| OI-6 | Attachment upload contract is only "file upload array" | API Contract owner | Define the upload contract |
| OI-7 | Single transaction across writes and the audit table name are not confirmed in the story | Solution Architecture | Confirm the transaction and table name |
| OI-8 | Values for "first option", "retirement", "top-3 set" and the status ID to name mapping are not given | Business Analysis | Provide the reference values |
