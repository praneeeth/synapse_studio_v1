# DRT Compact API Design Card

## API Identification

**Module/Functionality:** ASK - Create Core ASK
**Story ID:** US-ASK-001 (ADO Work Item 1857)
**Method:** POST
**Endpoint:** `/core-asks`
**Operation:** Create a new Core ASK record with validated business data, supporting Save & Exit, SUBMIT, and Exit button behaviours with status routing based on submission intent and leadership-group membership.

---

## Contract Summary

### Request

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "coreAskDetails": {
    "coreAskName": "string (required, max 200 characters)",
    "fYear": "integer (required/optional TBD - see Open Item 9)",
    "regionId": "integer (required/optional TBD - see Open Item 9)",
    "dppGroupId": "integer (required, must reference an active DPP Group option)",
    "needReasonId": "integer (required, must reference an active Need Reason option)",
    "generalSpecialityNeedId": "integer (required, must reference an active General Specialty Need option)",
    "generalSpecialityNeedComment": "string (conditional - required unless generalSpecialityNeedId is the first option; see Open Item 3)",
    "levelNeedId": "integer (required, must reference an active Level Need option)",
    "pml": "string (optional, max 99 characters, new-version only - see Open Item 12)",
    "projectedStartDate": "date ISO 8601 (required)",
    "endDate": "date ISO 8601 (conditional - required unless needReasonId is the retirement value; see Open Item 4; must be after projectedStartDate and not earlier than today)",
    "employeeId": "integer (optional, source for derived outgoingResource field)",
    "headCountAmount": "integer (required)",
    "fteAmount": "decimal (required, must not exceed headCountAmount)",
    "rolePostingId": "integer (conditional - required when levelNeedId is in the top-3 set; see Open Item 2)",
    "titlingCategory": "string (required, new-version only - see Open Item 12)",
    "transitionalCoach": "string (optional, max 99 characters, new-version only - see Open Item 12)",
    "roleSummary": "string (required)",
    "roleResponsibility": "string (required)",
    "roleQualification": "string (required)"
  },
  "comment": "string (optional)",
  "documents": "array of file upload objects (optional - exact contract TBD; see Open Item 5)",
  "buttonValue": "string (required, one of: 'Save & Exit' | 'SUBMIT' | 'Exit')"
}
```

**Contract Notes:**
- `outgoingResource` is a derived, read-only field. The client must not supply it. The API derives it from the resolved `employeeId` value. [US-ASK-001-R24]
- `buttonValue` controls the entire persistence and routing behaviour of the request. [US-ASK-001-R31]
- The story schema contains a duplicate key `numberofresources` with values 1 and 2 in the same object. This is treated as a data defect in the source specification. The field is excluded from the contract until the intended cardinality and semantics are confirmed. See Open Item 8.
- `fYear` and `regionId` appear in the story's API Details schema but are not listed in the Functional Requirements field inventory. Their required/optional status and validation rules are TBD. See Open Item 9.
- `documents` is described as a "file upload array" in the source. The exact upload contract (MIME types, size limits, multipart vs base64, field name) is TBD. See Open Item 5.
- The `new-version only` qualifier applies to `pml`, `titlingCategory`, and `transitionalCoach`. The version discriminator mechanism is TBD. See Open Item 12.

### Response

**Status Code:** `TBD` — the story does not specify whether a successful Save & Exit returns `201 Created` or `200 OK`, nor the code for a successful SUBMIT. See Open Item 10.

**Content-Type:** `application/json`

**Response Body:**
```json
{
  "askId": "integer (identifier of the created ASK record)",
  "askDetailId": "integer (identifier of the created ASK detail record)",
  "coreAskDetails": "object (echo of persisted core ASK detail fields)",
  "version": "integer (version number of the created ASK)",
  "task": {
    "taskId": "integer (identifier of the created workflow task)",
    "statusId": "integer (workflow status assigned to the ASK)",
    "assigneeId": "integer (identifier of the assigned task owner)"
  },
  "comment": {
    "commentId": "integer (identifier of the persisted comment, when supplied)"
  },
  "auditHistory": {
    "auditId": "integer (identifier of the created audit history record)"
  }
}
```

**Note for Exit path:** When `buttonValue` is `Exit`, no data is persisted. The HTTP status code and response body for this no-op path are TBD. See Open Item 11.

**OpenAPI Reference:** Operation `createCoreAsk` on `POST /api/v1/core-asks`; request schema `CreateCoreAskRequest`; response schema `CreateCoreAskResponse`. Schemas are pending OpenAPI contract approval.

---

## Authorization

**Required Permission:** TBD — the story states the caller must have permission to create a new ASK [US-ASK-001-R09] but does not specify the permission code, token claim, or role name. See Open Item 7.

**Authorization Flow:**
1. **Token validation:** Validate the Entra access token issuer, tenant, audience, signature, and lifetime per MLLD 8.1.
2. **Actor resolution:** Resolve the current DRT actor using the immutable Entra tenant ID and object ID per MLLD 8.2.
3. **Active user check:** Confirm the DRT user record exists and is active per MLLD 8.3 enforcement order step 1.
4. **Permission load:** Load current role and permission assignments and combine permissions across roles per MLLD 8.3 step 2.
5. **Create ASK permission check:** Confirm the resolved actor holds the TBD Create New ASK permission. Return `403 access_denied` if the permission is absent. See Open Item 7.
6. **Business-unit scope check:** Apply business-unit and record visibility rules per MLLD 8.3 step 3.
7. **Failure:** Return `401 authentication_required` for token failure; `403 access_denied` for permission or scope failure.

**Reference:** MLLD 8.1 Authentication, MLLD 8.2 Authorization Context, MLLD 8.3 Enforcement Order, MLLD 8.5 Role Families.

---

## Processing Flow

1. **Receive and bind request:** Accept `POST /core-asks` with `Content-Type: application/json`. Generate or accept a correlation ID and attach it to the response per MLLD 6.1 step 1. [US-ASK-001-R32, US-ASK-001-R33]

2. **Authenticate and resolve actor:** Validate the Entra access token and resolve the current DRT actor per MLLD 6.1 steps 2–3. Return `401 authentication_required` on token failure.

3. **Authorise:** Evaluate the Create New ASK permission, business-unit scope, and record visibility per MLLD 6.1 steps 4–5 and the Authorization Flow above. Return `403 access_denied` on failure. [US-ASK-001-R09]

4. **Structural validation:** Validate the request body structure and data types. Return `400 validation_failed` for any structural violation. [US-ASK-001-R11, US-ASK-001-R45]

5. **Evaluate buttonValue — Exit short-circuit:** If `buttonValue` is `Exit`, perform no persistence operations and return the TBD no-op response immediately. No further steps execute. [US-ASK-001-R14, US-ASK-001-R52, AC4]

6. **Reference data validation:** For each reference ID in the request (`dppGroupId`, `needReasonId`, `generalSpecialityNeedId`, `levelNeedId`, `rolePostingId` when present), confirm the value maps to an active option in the corresponding reference data service. Return `400 validation_failed` for any inactive or unrecognised reference ID. [US-ASK-001-R10, US-ASK-001-R16, US-ASK-001-R39, US-ASK-001-R46]

7. **Required field validation:** Confirm all unconditionally required fields are present: `coreAskName`, `dppGroupId`, `needReasonId`, `generalSpecialityNeedId`, `levelNeedId`, `projectedStartDate`, `headCountAmount`, `fteAmount`, `roleSummary`, `roleResponsibility`, `roleQualification`, `titlingCategory` (new-version only, TBD). Return `400 validation_failed` listing each missing field. [US-ASK-001-R15, US-ASK-001-R17, US-ASK-001-R22, US-ASK-001-R25, US-ASK-001-R26, US-ASK-001-R28, US-ASK-001-R30, US-ASK-001-R37, US-ASK-001-R45, AC6]

8. **Conditional field validation:** Apply all conditional field rules:
   - If `needReasonId` is not the first option (TBD sentinel — see Open Item 3), confirm `projectedStartDate` and `endDate` are present.
   - If `needReasonId` is not the retirement value (TBD — see Open Item 4), confirm `endDate` is present.
   - If `generalSpecialityNeedId` is not the first option (TBD — see Open Item 3), confirm `generalSpecialityNeedComment` is present.
   - If `levelNeedId` is in the top-3 set (TBD — see Open Item 2), confirm `rolePostingId` is present.
   Return `400 validation_failed` for any unmet conditional requirement. [US-ASK-001-R18, US-ASK-001-R19, US-ASK-001-R20, US-ASK-001-R23, US-ASK-001-R27, US-ASK-001-R43, US-ASK-001-R44]

9. **Numeric and date business-rule validation:** Apply all numeric and date rules:
   - `endDate` must not be earlier than today. Return `422` on violation. [US-ASK-001-R40, US-ASK-001-R47, AC7]
   - `endDate` must be after `projectedStartDate`. Return `422` on violation. [US-ASK-001-R41, US-ASK-001-R48, AC7]
   - `fteAmount` must not exceed `headCountAmount`. Return `422` on violation. [US-ASK-001-R26, US-ASK-001-R42, US-ASK-001-R49, AC5]

10. **Max length validation:** Enforce character limits: `coreAskName` max 200, `pml` max 99, `transitionalCoach` max 99. Return `400 validation_failed` for any violation. [US-ASK-001-R15, US-ASK-001-R21, US-ASK-001-R29, US-ASK-001-R38]

11. **Derive outgoingResource:** If `employeeId` is supplied, resolve the corresponding employee record from the employee search/picker dependency and derive the `outgoingResource` value. The client-supplied value, if any, is ignored. [US-ASK-001-R24]

12. **Apply field clearing rules:** Before persisting:
    - If `needReasonId` equals the first option (TBD — see Open Item 3): clear `outgoingResource`, `employeeId`, `projectedStartDate`, and `endDate` from the payload to be persisted. [US-ASK-001-R18]
    - If `generalSpecialityNeedId` equals the first option (TBD — see Open Item 3): clear `generalSpecialityNeedComment` from the payload to be persisted. [US-ASK-001-R19]

13. **Determine target workflow status:** Based on `buttonValue`:
    - `Save & Exit`: target status = 123 In Progress. [US-ASK-001-R12, AC1]
    - `SUBMIT` with non-leadership submitter: target status = 127 PPL Review. [US-ASK-001-R13, AC2]
    - `SUBMIT` with leadership-group submitter: target status = 145 DPP Ops Review. [US-ASK-001-R13, AC3]
    The leadership-group determination mechanism is TBD. See Open Item 1.

14. **Persist atomically:** Within a single database transaction, create and commit:
    - `Ask` record with the resolved status.
    - `AskVersion` record linked to the `Ask`.
    - `AskComment` record when `comment` is supplied.
    - `AskAttachmentLink` records when `documents` are supplied (exact contract TBD — see Open Item 5).
    - `WorkflowInstance` linked to the new `Ask`.
    - `WorkflowTask` for the target status assignee.
    - `WorkflowDecision` recording the create action, actor, and idempotency key.
    - `WorkflowHistory` append-only record of the initial transition.
    - `OutboxMessage` for the post-commit notification event.
    Transaction behaviour across these write operations is explicitly unconfirmed in the story. See Open Item 6. [US-ASK-001-R51]

15. **Commit and return response:** Call `SaveChangesAsync` once and commit the transaction per Coding Standards §23. Map the committed identifiers to the response schema and return the TBD success status code. [US-ASK-001-R35, AC1, AC2, AC3]

16. **Post-commit notification:** After the transaction commits, the outbox worker claims the `OutboxMessage` and delivers the workflow notification via Microsoft Graph email and/or Azure SignalR per MLLD 11.2. Notification failure does not roll back the committed transaction.

---

## Business & Workflow Rules

### Required Field Rules
- `coreAskName` is required; max 200 characters. [US-ASK-001-R15]
- `dppGroupId` is required; must reference an active DPP Group option. [US-ASK-001-R16]
- `needReasonId` is required; must reference an active Need Reason option. [US-ASK-001-R17]
- `generalSpecialityNeedId` is required; must reference an active General Specialty Need option. [US-ASK-001-R17]
- `levelNeedId` is required; must reference an active Level Need option. [US-ASK-001-R17]
- `projectedStartDate` is required. [US-ASK-001-R22]
- `headCountAmount` is required. [US-ASK-001-R25]
- `fteAmount` is required. [US-ASK-001-R26]
- `roleSummary` is required. [US-ASK-001-R30]
- `roleResponsibility` is required. [US-ASK-001-R30]
- `roleQualification` is required. [US-ASK-001-R30]
- `titlingCategory` is required (new-version only — version discriminator TBD; see Open Item 12). [US-ASK-001-R28]
- `buttonValue` is required; must be one of `Save & Exit`, `SUBMIT`, or `Exit`. [US-ASK-001-R31]

### Conditional Field Rules
- `endDate` is required unless `needReasonId` equals the retirement value (TBD — see Open Item 4). [US-ASK-001-R23]
- `generalSpecialityNeedComment` is required unless `generalSpecialityNeedId` equals the first option (TBD — see Open Item 3). [US-ASK-001-R20]
- `rolePostingId` is required when `levelNeedId` is in the top-3 set (TBD — see Open Item 2). [US-ASK-001-R27]

### Field Clearing Rules
- If `needReasonId` equals the first option (TBD — see Open Item 3): clear `outgoingResource`, `employeeId`, `projectedStartDate`, and `endDate` before persisting. [US-ASK-001-R18]
- If `generalSpecialityNeedId` equals the first option (TBD — see Open Item 3): clear `generalSpecialityNeedComment` before persisting. [US-ASK-001-R19]

### Numeric and Date Validation Rules
- `endDate` must not be earlier than today (server date, UTC). [US-ASK-001-R40]
- `endDate` must be strictly after `projectedStartDate`. [US-ASK-001-R41]
- `fteAmount` must not exceed `headCountAmount`. [US-ASK-001-R42]

### Max Length Rules
- `coreAskName`: max 200 characters. [US-ASK-001-R15]
- `pml`: max 99 characters (new-version only). [US-ASK-001-R21]
- `transitionalCoach`: max 99 characters (new-version only). [US-ASK-001-R29]

### Button Behaviour Rules
- `Save & Exit`: validate all required and conditional fields, persist the ASK with status 123 In Progress, return created identifiers. [US-ASK-001-R12, AC1]
- `SUBMIT`: validate all required and conditional fields, persist the ASK, route to status 127 PPL Review (non-leadership) or 145 DPP Ops Review (leadership-group — TBD; see Open Item 1). [US-ASK-001-R13, AC2, AC3]
- `Exit`: perform no validation beyond structural and authentication checks, persist nothing, return TBD no-op response. [US-ASK-001-R14, US-ASK-001-R52, AC4]

### Status Routing Rules
- `Save & Exit`: status = 123 In Progress. [US-ASK-001-R12]
- `SUBMIT` (non-leadership submitter): status = 127 PPL Review. [US-ASK-001-R13]
- `SUBMIT` (leadership-group submitter): status = 145 DPP Ops Review. [US-ASK-001-R13]
- Status codes 123, 127, and 145 are used as specified in the story. Confirmation against the DRT Workflow State Transitions catalogue is required. See Validation Finding 3.

### Reference Data Validation
- `dppGroupId` must map to an active DPP Group record. [US-ASK-001-R16, US-ASK-001-R39]
- `needReasonId` must map to an active Need Reason record. [US-ASK-001-R17, US-ASK-001-R39]
- `generalSpecialityNeedId` must map to an active General Specialty Need record. [US-ASK-001-R17, US-ASK-001-R39]
- `levelNeedId` must map to an active Level Need record. [US-ASK-001-R17, US-ASK-001-R39]
- `rolePostingId` must map to an active Role Posting record when supplied. [US-ASK-001-R43, US-ASK-001-R39]

### Derived Field Rules
- `outgoingResource` is read-only and derived from the employee record resolved by `employeeId`. The client must not supply this field directly. [US-ASK-001-R24]

---

## Data Impact

| Operation | Entity/Table | Purpose |
|---|---|---|
| Create | Ask | Stores the root Core ASK record with resolved workflow status |
| Create | AskVersion | Stores the versioned snapshot of the ASK detail fields |
| Create | AskComment | Stores the optional comment supplied in the request |
| Create | AskAttachmentLink | Links uploaded document metadata to the ASK (exact contract TBD) |
| Create | WorkflowInstance | Links the new ASK to its initial workflow state and definition version |
| Create | WorkflowTask | Represents the first human work item for the target status assignee |
| Create | WorkflowDecision | Records the create action, actor identity, and idempotency key |
| Create | WorkflowHistory | Append-only record of the initial state transition |
| Create | OutboxMessage | Durable post-commit notification event for email and SignalR delivery |

**Transaction Boundary:** All nine entity writes above — Ask, AskVersion, AskComment, AskAttachmentLink, WorkflowInstance, WorkflowTask, WorkflowDecision, WorkflowHistory, and OutboxMessage — must be committed in a single `SaveChangesAsync` call per Coding Standards §23. Transaction behaviour across these operations is explicitly unconfirmed in the story [US-ASK-001-R51]; see Open Item 6.

**Consistency Boundary:** The ASK aggregate (Ask + AskVersion + AskComment + AskAttachmentLink) per MLLD 9.2. The WorkflowInstance and its associated task, decision, history, and outbox records are committed atomically with the ASK aggregate in the same transaction per MLLD 6.4 and Coding Standards §23.

---

## Events & Integrations

### Outbox Event

**Event Type:** `CoreAskCreated`
**Purpose:** Notifies downstream consumers (workflow task assignees, SignalR work-queue refresh) that a new Core ASK has been created and routed to its initial status.
**Outbox Record Fields:** message id (stable deduplication key), type and version `CoreAskCreated/v1`, payload `{ "askId": <integer>, "statusId": <integer>, "buttonValue": "<string>", "actorId": "<string>" }`, occurred UTC, correlation id, status, attempts, next-attempt UTC, lease owner, lease expiry, processed UTC, last error.
**Delivery Guarantee:** At least once per MLLD 11.1 and Coding Standards §24.
**Downstream Consumers:** Microsoft Graph email notification to the task assignee; Azure SignalR `WorkQueueChanged` event to refresh the work queue per MLLD 11.2 and Coding Standards §25.

### External Dependencies

**Reference Data Services:** The API must call the reference data services for DPP Group, ASK Reason, Level Need, General Specialty Need, and Role Posting to validate that supplied IDs map to active options. [US-ASK-001-R10, US-ASK-001-R39]

**Employee Search/Picker:** The API must resolve `employeeId` to the corresponding employee record to derive `outgoingResource`. [US-ASK-001-R24]

**Attachment Upload Handling:** The API must process the `documents` array and register attachment metadata. The exact upload contract is TBD. See Open Item 5. [US-ASK-001-R07, US-ASK-001-R53]

**Workflow Status Routing Service:** The API must determine the target workflow status based on `buttonValue` and the leadership-group membership of the submitter. The leadership-group determination mechanism is TBD. See Open Item 1. [US-ASK-001-R08, US-ASK-001-R13]

**Authorization Service:** The API must confirm the caller holds the Create New ASK permission. The permission code is TBD. See Open Item 7. [US-ASK-001-R09]

---

## Errors & Tests

### Error Scenarios

| Scenario | HTTP Status | Error Code | Behaviour |
|---|---|---|---|
| Missing or invalid Entra token | `401 Unauthorized` | `authentication_required` | Return 401; do not process the request |
| Caller lacks Create New ASK permission | `403 Forbidden` | `access_denied` | Return 403; do not process the request |
| Required field absent from request body | `400 Bad Request` | `validation_failed` | Return 400 with field-level detail listing each missing field [US-ASK-001-R45, AC6] |
| `dppGroupId` or other reference ID maps to inactive or unknown option | `400 Bad Request` | `validation_failed` | Return 400 identifying the invalid reference field [US-ASK-001-R46] |
| `endDate` is earlier than today | `422 Unprocessable Entity` | TBD — 422 is not in MLLD 12.2 catalogue; see Validation Finding 4 | Return 422 per story [US-ASK-001-R47, AC7] |
| `endDate` is not after `projectedStartDate` | `422 Unprocessable Entity` | TBD — see Validation Finding 4 | Return 422 per story [US-ASK-001-R48, AC7] |
| `fteAmount` exceeds `headCountAmount` | `422 Unprocessable Entity` | TBD — see Validation Finding 4 | Return 422 per story [US-ASK-001-R49, AC5] |
| Concurrent conflicting create with same idempotency key | `409 Conflict` | `concurrency_conflict` | Return 409 per MLLD 12.2 |
| Unexpected server failure | `500 Internal Server Error` | `unexpected_error` | Return 500 per MLLD 12.2 |
| Reference data service or employee service unavailable | `503 Service Unavailable` | `dependency_unavailable` | Return 503 per MLLD 12.2 |

**Error Response Standards:**
- All error responses use ASP.NET Core Problem Details format per Coding Standards §29, containing HTTP status, stable error code, problem title, safe problem detail, and trace/correlation identifier.
- Never expose SQL statements, internal exception detail, connection information, or secrets in error responses per MLLD 12.2 and Coding Standards §29.
- Emit structured telemetry without secrets or confidential payloads per MLLD 6.1 step 8.
- Map domain and concurrency exceptions to the standard API error response per Coding Standards §42.

### Test Scenarios

**Positive Scenarios:**

1. **Save Core ASK draft successfully**
   - Given: The caller is authenticated, holds the Create New ASK permission, and provides a structurally valid request with all required fields populated and `buttonValue` = `Save & Exit`.
   - When: The client calls `POST /core-asks`.
   - Then: The API persists the ASK with status 123 In Progress, returns the created `askId`, `askDetailId`, `version`, `task`, `comment`, and `auditHistory` identifiers, and commits the outbox event. [AC1]

2. **Submit Core ASK as non-leadership user**
   - Given: The caller is authenticated, holds the Create New ASK permission, is not in the leadership group (TBD), and provides a valid request with `buttonValue` = `SUBMIT`.
   - When: The client calls `POST /core-asks`.
   - Then: The API creates the ASK, routes it to status 127 PPL Review, returns the created identifiers, and commits the outbox event. [AC2]

3. **Submit Core ASK as leadership user**
   - Given: The caller is authenticated, holds the Create New ASK permission, is in the leadership group (TBD), and provides a valid request with `buttonValue` = `SUBMIT`.
   - When: The client calls `POST /core-asks`.
   - Then: The API creates the ASK, routes it to status 145 DPP Ops Review, returns the created identifiers, and commits the outbox event. [AC3]

4. **Exit without save**
   - Given: The caller is authenticated and sends a request with `buttonValue` = `Exit`.
   - When: The client calls `POST /core-asks`.
   - Then: No data is persisted, no outbox event is created, and the API returns the TBD no-op response. [AC4]

5. **Conditional field generalSpecialityNeedComment present when required**
   - Given: The caller provides a valid request where `generalSpecialityNeedId` is not the first option and `generalSpecialityNeedComment` is populated.
   - When: The client calls `POST /core-asks` with `buttonValue` = `Save & Exit`.
   - Then: The API accepts the request and persists the ASK successfully.

6. **rolePostingId present when levelNeedId is in top-3 set**
   - Given: The caller provides a valid request where `levelNeedId` is in the top-3 set (TBD) and `rolePostingId` is populated.
   - When: The client calls `POST /core-asks` with `buttonValue` = `SUBMIT`.
   - Then: The API accepts the request and persists the ASK successfully.

**Negative Scenarios:**

7. **Required field missing — returns 400**
   - Given: The caller provides a request body that omits a mandatory field (for example, `coreAskName`).
   - When: The client calls `POST /core-asks`.
   - Then: The API returns `400 validation_failed` with field-level detail identifying the missing field. [AC6]

8. **Invalid reference ID — returns 400**
   - Given: The caller provides a `dppGroupId` that does not map to an active DPP Group option.
   - When: The client calls `POST /core-asks`.
   - Then: The API returns `400 validation_failed` identifying `dppGroupId` as invalid.

9. **FTE exceeds headcount — returns 422**
   - Given: The caller provides a request where `fteAmount` is greater than `headCountAmount`.
   - When: The client calls `POST /core-asks`.
   - Then: The API returns `422` identifying the `fteAmount` violation. [AC5]

10. **endDate before today — returns 422**
    - Given: The caller provides a request where `endDate` is earlier than today's server date.
    - When: The client calls `POST /core-asks`.
    - Then: The API returns `422` identifying the `endDate` violation. [AC7]

11. **endDate not after projectedStartDate — returns 422**
    - Given: The caller provides a request where `endDate` equals or precedes `projectedStartDate`.
    - When: The client calls `POST /core-asks`.
    - Then: The API returns `422` identifying the `endDate` vs `projectedStartDate` violation. [AC7]

12. **Unauthenticated request — returns 401**
    - Given: The caller sends a request without a valid Entra access token.
    - When: The client calls `POST /core-asks`.
    - Then: The API returns `401 authentication_required` and does not process the request.

13. **Insufficient permission — returns 403**
    - Given: The caller is authenticated but does not hold the Create New ASK permission.
    - When: The client calls `POST /core-asks`.
    - Then: The API returns `403 access_denied` and does not process the request.

14. **generalSpecialityNeedComment absent when required — returns 400**
    - Given: The caller provides a request where `generalSpecialityNeedId` is not the first option and `generalSpecialityNeedComment` is absent.
    - When: The client calls `POST /core-asks`.
    - Then: The API returns `400 validation_failed` identifying `generalSpecialityNeedComment` as required.

15. **rolePostingId absent when levelNeedId is in top-3 set — returns 400**
    - Given: The caller provides a request where `levelNeedId` is in the top-3 set (TBD) and `rolePostingId` is absent.
    - When: The client calls `POST /core-asks`.
    - Then: The API returns `400 validation_failed` identifying `rolePostingId` as required.

---

## Assumptions & Open Items

### Assumptions

1. **Knowledge base availability:** The knowledge base returned HTTP 500 on both allowed attempts during the planning stage. The card has been written from ADO work item content and the DC Card Templates, DRT Master LLD, and Coding Standards documents retrieved in the design card writer stage.
2. **Exit is a no-op:** `buttonValue` = `Exit` is treated as a no-op that persists nothing, per the story assumption [US-ASK-001-R52]. No separate change-note card is raised for this path.
3. **numberofresources excluded:** The duplicate `numberofresources` key (values 1 and 2 in the same JSON object) in the story's API Details schema is treated as a data defect in the source specification. The field is excluded from the contract until the intended semantics are confirmed. See Open Item 8.
4. **Single API card only:** No Job, Integration, Notification, Data, or UI change items are described in the story. Only one API card is produced.
5. **UTC server date for endDate comparison:** The "not earlier than today" rule for `endDate` is evaluated against the server's current UTC date.
6. **Idempotency key required:** A workflow create operation is retry-sensitive. An idempotency key mechanism is assumed to be required per Coding Standards §22. The exact mechanism (header, body field, or database uniqueness constraint) is TBD and must be confirmed by Solution Architecture.

### Open Items

1. **Leadership-group determination:** The story routes `SUBMIT` to status 145 DPP Ops Review for leadership-group submitters [US-ASK-001-R13, AC3] but does not define which attribute, role, claim, or group membership identifies a submitter as being in the leadership group. This must be defined before the routing logic can be implemented. Owner: Product Owner. [LLD T03]
2. **Top-3 set for levelNeedId:** The story states `rolePostingId` is required when `levelNeedId` is in the "top-3 set" [US-ASK-001-R27] but does not list the exact IDs or values that constitute this set. Owner: Product Owner.
3. **First option for needReasonId and generalSpecialityNeedId:** The story references the "first option" of `needReasonId` [US-ASK-001-R18] and `generalSpecialityNeedId` [US-ASK-001-R19] as sentinel values that trigger field clearing and conditional requirement rules. Whether "first" means the lowest ID, a specific sentinel value, or UI ordering is not defined. Owner: Product Owner.
4. **Retirement value for needReasonId:** The story states `endDate` is not required when `needReasonId` is the retirement value [US-ASK-001-R23] but does not specify the exact ID or code for the retirement option. Owner: Product Owner.
5. **Document upload contract:** The story describes `documents` as a "file upload array" [US-ASK-001-R53] without specifying MIME types, size limits, multipart vs base64 encoding, or field name. The exact attachment contract must be confirmed before the documents payload can be implemented. Owner: Solution Architecture.
6. **Transaction behaviour across write operations:** The story explicitly marks transaction behaviour across write operations as unconfirmed [US-ASK-001-R51]. The atomicity guarantee across Ask, AskVersion, AskComment, AskAttachmentLink, WorkflowInstance, WorkflowTask, WorkflowDecision, WorkflowHistory, and OutboxMessage must be confirmed. Owner: Solution Architecture.
7. **Create New ASK permission code:** The story states the caller must have permission to create a new ASK [US-ASK-001-R09] but does not specify the permission code, token claim, or role name. Owner: Solution Architecture. [LLD T02]
8. **numberofresources field semantics:** The story's API Details schema contains a duplicate key `numberofresources` with values 1 and 2 in the same JSON object. The intended cardinality, field name, and semantics must be confirmed before the field can be included in the contract. Owner: Product Owner.
9. **fYear and regionId validation rules:** `fYear` and `regionId` appear in the story's API Details schema but are not listed in the Functional Requirements field inventory. Their required/optional status, data types, and validation rules must be confirmed. Owner: Product Owner.
10. **Success HTTP status code:** The story does not specify whether a successful `Save & Exit` returns `201 Created` or `200 OK`, nor the code for a successful `SUBMIT`. Owner: Solution Architecture.
11. **Exit no-op response:** The story states `Exit` persists nothing [US-ASK-001-R14] but does not specify the HTTP status code or response body for this path. Owner: Solution Architecture.
12. **New-version only qualifier:** The story applies a "new-version only" qualifier to `pml` [US-ASK-001-R21], `titlingCategory` [US-ASK-001-R28], and `transitionalCoach` [US-ASK-001-R29] without defining the version discriminator field, endpoint variant, or mechanism by which the API determines whether a request is "new-version". Owner: Product Owner.

---

## Traceability

### User Story Requirements Traced

| Requirement ID | Requirement Description | Card Section |
|---|---|---|
| US-ASK-001-R01 | As a permitted user I want to create and save | API Identification, Authorization |
| US-ASK-001-R02 | Enable authorized users to create new Core ASK records | API Identification, Authorization |
| US-ASK-001-R03 | This story covers the create Core ASK API including field validation, conditional rules, optional draft save, submission routing, comment handling, and attachment payload support | API Identification, Processing Flow |
| US-ASK-001-R04 | Create new Core ASK | API Identification, Processing Flow |
| US-ASK-001-R05 | Support Save & Exit, SUBMIT, and Exit button behaviors | Button Behaviour Rules, Processing Flow |
| US-ASK-001-R06 | Validate required and conditional fields | Business & Workflow Rules, Processing Flow |
| US-ASK-001-R07 | Support comment and documents payload | Contract Summary, Data Impact |
| US-ASK-001-R08 | Set resulting workflow status based on button value | Status Routing Rules, Processing Flow |
| US-ASK-001-R09 | Caller has permission to create a new ASK | Authorization |
| US-ASK-001-R10 | Reference data values used in the request are valid | Reference Data Validation, Processing Flow |
| US-ASK-001-R11 | Request body is structurally valid | Processing Flow step 4 |
| US-ASK-001-R12 | For Save & Exit draft data is saved with status 123 In Progress | Button Behaviour Rules, Status Routing Rules |
| US-ASK-001-R13 | For SUBMIT ASK is created and routed to status 127 PPL Review or 145 DPP Ops Review | Status Routing Rules, Processing Flow |
| US-ASK-001-R14 | For Exit no changes are persisted | Button Behaviour Rules, Processing Flow |
| US-ASK-001-R15 | coreAskName is required and max 200 characters | Required Field Rules, Max Length Rules |
| US-ASK-001-R16 | dppGroupId is required and must be an active option | Required Field Rules, Reference Data Validation |
| US-ASK-001-R17 | needReasonId generalSpecialityNeedId and levelNeedId are required | Required Field Rules |
| US-ASK-001-R18 | First needReasonId option clears outgoingResource employeeId projectedStartDate endDate | Field Clearing Rules, Open Item 3 |
| US-ASK-001-R19 | First generalSpecialityNeedId option clears generalSpecialityNeedComment | Field Clearing Rules, Open Item 3 |
| US-ASK-001-R20 | generalSpecialityNeedComment is required unless generalSpecialityNeedId is first | Conditional Field Rules |
| US-ASK-001-R21 | pml is optional max 99 characters new-version only | Contract Summary, Max Length Rules, Open Item 12 |
| US-ASK-001-R22 | projectedStartDate is required | Required Field Rules |
| US-ASK-001-R23 | endDate is required unless needReasonId is retirement must be greater | Conditional Field Rules, Numeric and Date Validation Rules, Open Item 4 |
| US-ASK-001-R24 | outgoingResource is derived and read-only from selected employeeId | Derived Field Rules, Contract Summary |
| US-ASK-001-R25 | headCountAmount is required | Required Field Rules |
| US-ASK-001-R26 | fteAmount is required and must not exceed headCountAmount | Required Field Rules, Numeric and Date Validation Rules |
| US-ASK-001-R27 | rolePostingId is required only when levelNeedId is in top-3 set | Conditional Field Rules, Open Item 2 |
| US-ASK-001-R28 | titlingCategory is required new-version only | Required Field Rules, Open Item 12 |
| US-ASK-001-R29 | transitionalCoach is optional max 99 characters new-version only | Contract Summary, Max Length Rules, Open Item 12 |
| US-ASK-001-R30 | roleSummary roleResponsibility and roleQualification are all required | Required Field Rules |
| US-ASK-001-R31 | buttonValue controls save submit exit behavior | Button Behaviour Rules, Processing Flow |
| US-ASK-001-R32 | Expose POST /core-asks | API Identification |
| US-ASK-001-R33 | Accept request body containing coreAskDetails comment documents buttonValue | Contract Summary |
| US-ASK-001-R34 | Validate all field and business rules | Business & Workflow Rules, Processing Flow |
| US-ASK-001-R35 | On valid create return new identifiers and created entities | Contract Summary (Response) |
| US-ASK-001-R36 | Support request fields coreAskName dppGroupId needReasonId generalSpecialityNeedId | Contract Summary |
| US-ASK-001-R37 | Required fields must be present when applicable | Required Field Rules |
| US-ASK-001-R38 | Max lengths must be enforced where specified | Max Length Rules |
| US-ASK-001-R39 | Reference IDs must map to valid active options | Reference Data Validation |
| US-ASK-001-R40 | endDate must not be earlier than today | Numeric and Date Validation Rules |
| US-ASK-001-R41 | endDate must be after projectedStartDate | Numeric and Date Validation Rules |
| US-ASK-001-R42 | fteAmount must not exceed headCountAmount | Numeric and Date Validation Rules |
| US-ASK-001-R43 | rolePostingId must be present when required | Conditional Field Rules |
| US-ASK-001-R44 | generalSpecialityNeedComment must be present when required | Conditional Field Rules |
| US-ASK-001-R45 | Return 400 when required fields are missing | Errors & Tests |
| US-ASK-001-R46 | Return 400 for invalid active option selections such as invalid dppGroupId | Errors & Tests |
| US-ASK-001-R47 | Return 422 when endDate is earlier than today | Errors & Tests, Validation Finding 4 |
| US-ASK-001-R48 | Return 422 when endDate is not after projectedStartDate | Errors & Tests, Validation Finding 4 |
| US-ASK-001-R49 | Return 422 when fteAmount exceeds headCountAmount | Errors & Tests, Validation Finding 4 |
| US-ASK-001-R50 | Endpoint POST /core-asks Method POST | API Identification |
| US-ASK-001-R51 | Transaction behavior across write operations is not confirmed | Data Impact, Open Item 6 |
| US-ASK-001-R52 | buttonValue Exit is treated as no-op and does not persist | Button Behaviour Rules, Processing Flow |
| US-ASK-001-R53 | Exact attachment file-upload contract is represented as file upload array | Contract Summary, Open Item 5 |

**Total Requirements Traced:** 53

### Acceptance Criteria Traced

| Acceptance Criteria | Card Section | Test Scenario |
|---|---|---|
| AC1: Save Core ASK draft successfully – buttonValue Save & Exit status 123 | Button Behaviour Rules, Status Routing Rules | Test Scenario 1 |
| AC2: Submit Core ASK as non-leadership user – status 127 PPL Review | Status Routing Rules, Processing Flow | Test Scenario 2 |
| AC3: Submit Core ASK as leadership user – status 145 DPP Ops Review | Status Routing Rules, Open Item 1 | Test Scenario 3 |
| AC4: Exit without save – no data is saved | Button Behaviour Rules, Processing Flow | Test Scenario 4 |
| AC5: FTE exceeds headcount – API returns 422 | Numeric and Date Validation Rules, Errors & Tests | Test Scenario 9 |
| AC6: Required field missing – API returns 400 | Required Field Rules, Errors & Tests | Test Scenario 7 |
| AC7: End date before projected start date – API returns 422 | Numeric and Date Validation Rules, Errors & Tests | Test Scenarios 10, 11 |

**Total Acceptance Criteria Traced:** 7

---

## Validation Findings

**Finding 1: Leadership-Group Routing Condition Undefined**
- **Type:** Missing Business Rule Detail
- **Description:** The story routes `SUBMIT` to status 145 DPP Ops Review for leadership-group submitters [US-ASK-001-R13, AC3] but does not define the attribute, role, claim, or group membership that identifies a submitter as being in the leadership group.
- **Impact:** The status routing fork in Processing Flow step 13 and the SUBMIT button behaviour rule cannot be implemented. Test Scenario 3 cannot be validated.
- **Recommendation:** Product Owner to define the exact leadership-group membership rule (role name, permission code, Entra group, or other attribute) before build. See also LLD T03.

**Finding 2: Sentinel Values for Field Clearing and Conditional Rules Undefined**
- **Type:** Missing Business Rule Detail
- **Description:** The story references the "first option" of `needReasonId` [US-ASK-001-R18] and `generalSpecialityNeedId` [US-ASK-001-R19] and the "retirement value" of `needReasonId` [US-ASK-001-R23] and the "top-3 set" of `levelNeedId` [US-ASK-001-R27] without providing the exact IDs, codes, or ordering rules.
- **Impact:** Field clearing rules, conditional field rules, and the `rolePostingId` requirement cannot be implemented. Test Scenarios 5, 6, 14, and 15 cannot be validated.
- **Recommendation:** Product Owner to provide the exact reference data IDs or sentinel values for each named set before build.

**Finding 3: Workflow Status Codes 123, 127, and 145 Not Confirmed Against DRT Catalogue**
- **Type:** Assumption Requiring Validation
- **Description:** The story uses status IDs 123 (In Progress), 127 (PPL Review), and 145 (DPP Ops Review) [US-ASK-001-R12, US-ASK-001-R13]. These have not been confirmed against the DRT Workflow State Transitions catalogue (knowledge base was unavailable during planning).
- **Impact:** Incorrect status IDs would cause incorrect workflow routing and task creation.
- **Recommendation:** Solution Architecture to confirm status IDs 123, 127, and 145 against the approved DRT Workflow State Transitions catalogue before build.

**Finding 4: HTTP 422 Not in MLLD 12.2 Error Code Catalogue**
- **Type:** Missing Error Handling Detail
- **Description:** The story specifies `422 Unprocessable Entity` for three business-rule violations [US-ASK-001-R47, US-ASK-001-R48, US-ASK-001-R49]. HTTP 422 does not appear in the MLLD 12.2 error code catalogue, which lists 400, 401, 403, 404, 409, 429, 500, and 503.
- **Impact:** The stable error code for 422 responses is undefined. The error response contract for these three scenarios cannot be finalised.
- **Recommendation:** Solution Architecture to either add 422 with a stable error code to the MLLD 12.2 catalogue or confirm that these violations should be mapped to 400 `validation_failed`. The card uses 422 as specified in the story pending this decision.

**Finding 5: Success HTTP Status Code Not Specified**
- **Type:** Missing Implementation Detail
- **Description:** The story does not specify whether a successful `Save & Exit` or `SUBMIT` returns `201 Created` or `200 OK`, nor the status code or body for the `Exit` no-op path.
- **Impact:** The response contract cannot be finalised. Client integration cannot be completed.
- **Recommendation:** Solution Architecture to confirm the success HTTP status codes for all three `buttonValue` paths.

**Finding 6: Document Upload Contract Undefined**
- **Type:** Missing Integration Detail
- **Description:** The story describes `documents` as a "file upload array" [US-ASK-001-R53] without specifying MIME types, size limits, multipart vs base64 encoding, or field name.
- **Impact:** The `AskAttachmentLink` creation step and the documents section of the contract cannot be implemented.
- **Recommendation:** Solution Architecture to confirm the platform file-upload standard and apply it to this endpoint before build.

**Finding 7: Create New ASK Permission Code Undefined**
- **Type:** Missing Implementation Detail
- **Description:** The story states the caller must have permission to create a new ASK [US-ASK-001-R09] but does not specify the permission code, token claim, or role name.
- **Impact:** The authorization policy for this endpoint cannot be implemented or tested.
- **Recommendation:** Solution Architecture and IAM to provide the exact permission code from the US RBAC 001 matrix before build. See also LLD T02.

**Finding 8: New-Version Only Qualifier Mechanism Undefined**
- **Type:** Missing Business Rule Detail
- **Description:** The story applies a "new-version only" qualifier to `pml` [US-ASK-001-R21], `titlingCategory` [US-ASK-001-R28], and `transitionalCoach` [US-ASK-001-R29] without defining the version discriminator field, endpoint variant, or mechanism.
- **Impact:** The conditional applicability of these three fields cannot be implemented.
- **Recommendation:** Product Owner to define the version discriminator mechanism before build.

**Finding 9: numberofresources Field Semantics Undefined**
- **Type:** Missing Implementation Detail
- **Description:** The story's API Details schema contains a duplicate key `numberofresources` with values 1 and 2 in the same JSON object. The intended cardinality and semantics are ambiguous.
- **Impact:** The field cannot be included in the contract until the defect is resolved.
- **Recommendation:** Product Owner and Solution Architecture to confirm the intended field name, cardinality, and semantics before build.

**Finding 10: fYear and regionId Validation Rules Undefined**
- **Type:** Missing Implementation Detail
- **Description:** `fYear` and `regionId` appear in the story's API Details schema but are absent from the Functional Requirements field inventory. Their required/optional status and validation rules are undefined.
- **Impact:** These fields cannot be included in the validated contract.
- **Recommendation:** Product Owner to confirm whether `fYear` and `regionId` are required or optional, and to provide their validation rules before build.

**Total Validation Findings:** 10

---

## Audit Summary

**Document Version:** 1.0
**Generated By:** DRT Design Card Writer
**Story ID:** US-ASK-001 (ADO 1857)
**Module/Functionality:** ASK - Create Core ASK
**Status:** Ready for Review
**Requirements Coverage:** 53 requirements traced (100%); 7 acceptance criteria traced (100%)
**Validation Findings:** 10 findings, 6 assumptions, 12 open items
**Knowledge Base Utilisation:** DC Card Templates (section contract, size budgets, writing details, entity names, error codes); DRT Master LLD v0.1 (entity names from MLLD 9.2, error codes from MLLD 12.2, authorization model from MLLD 8.1–8.3, workflow model from MLLD 7.1–7.4, transaction ownership from MLLD 6.4, outbox from MLLD 11.1–11.2); DRT-ProCode Coding Standards (transaction §23, outbox §24, notification §25, authorization §28, error §29, workflow §20).
**Architecture Compliance:**
- Dependency direction: Controller → Application Use Case → Domain → Repository/Integration Interface per MLLD 3.2 and Coding Standards §7.
- Authorization: Entra token validation, actor resolution, and DRT RBAC permission check per MLLD 8.1–8.3 and Coding Standards §28.
- Persistence: EF Core 10 with a single `SaveChangesAsync` commit per Coding Standards §16 and §23; entity names from MLLD 9.2.
- Workflow: Versioned WorkflowInstance, WorkflowTask, WorkflowDecision, WorkflowHistory per MLLD 7.1–7.2 and Coding Standards §20.
- Errors: ASP.NET Core Problem Details with MLLD 12.2 error codes per Coding Standards §29; 422 raised as Finding 4.
- Security: No secrets, SQL, or internal detail in responses; structured telemetry without confidential payloads per MLLD 12.1–12.3 and Coding Standards §41.
**Readiness for Code Generation:** Conditional. Ten validation findings must be resolved before build. Blocking items: leadership-group rule (Finding 1), sentinel values for field clearing (Finding 2), permission code (Finding 7), success HTTP status codes (Finding 5), and document upload contract (Finding 6).

---

## Document Control

**Document Owner:** DRT Solution Architecture
**Approval Required From:** Development Lead, Security Lead, Business Analyst
**Review Cycle:** Architecture Review, Development Review, Security Review
**Effective Date:** Pending Approval
