# DRT Compact API Design Card

## API Identification

**Module/Functionality:** ASK - Cancel Core ASK
**Story ID:** US-ASK-015 (ADO Work Item 1858)
**Method:** POST
**Endpoint:** `/core-asks/{askId}/cancel`
**Operation:** Cancels a Core ASK in a cancellable status, updates the ASK status to Cancelled (577), persists a mandatory comment, and writes an audit entry.

---

## Contract Summary

### Request

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "askId": "integer (required, must match the route {askId}; see Contract Notes)",
  "comment": "string (required, non-empty cancellation reason)",
  "cancelledBy": "string (present in story request body; SEC 003 prohibits accepting actor identity from the browser — see Open Item OI-02 and Validation Finding F-02)"
}
```

**Contract Notes:**
- `askId` in the request body must equal the `{askId}` route parameter. If they differ the request is rejected with 400 `validation_failed`. Source: US-ASK-015-FR2, US-ASK-015-VR1.
- `comment` is mandatory and must be non-empty. Source: US-ASK-015-BR1, US-ASK-015-VR2.
- `cancelledBy` is present in the story-supplied request schema. SEC 003 (MLLD 8, Coding Standards §27) prohibits accepting actor identity from the browser; the server must resolve the cancelling actor from the validated Entra token. This field must be ignored on inbound requests. The resolved actor is returned in the response. See Open Item OI-02 and Finding F-02.
- `statusId` is a derived, read-only response field set by the server to 577 on success. It is not accepted in the request body.

### Response

**Status Code:** `200 OK`

**Content-Type:** `application/json`

**Response Body:**
```json
{
  "askId": "integer (the cancelled ASK identifier)",
  "statusId": "integer (577 — Cancelled status constant)",
  "cancelledAt": "string (ISO 8601 UTC timestamp of cancellation)",
  "cancelledBy": "string (server-resolved actor identity, not the browser-supplied value)",
  "auditEntry": {
    "action": "string (fixed value: Core Ask Cancelled)",
    "description": "string (cancellation comment text)",
    "createdBy": "string (server-resolved actor identity)",
    "createdOn": "string (ISO 8601 UTC timestamp)"
  }
}
```

**OpenAPI Reference:** Operation `cancelCoreAsk` on `POST /core-asks/{askId}/cancel`; request schema `CancelCoreAskRequest`; response schema `CancelCoreAskResponse`. Pending OpenAPI contract approval.

---

## Authorization

**Required Permission:** TBD — story names DPP Ops group / DPP Operation Editors membership as the access condition but does not supply a discrete permission code. See Open Item OI-01.

**Authorization Flow:**
1. Validate the Entra access token: issuer, tenant, audience, signature, and expiry per MLLD 8.1 and Coding Standards §27.
2. Resolve the current DRT actor using the immutable Entra tenant ID and object ID; confirm the DRT user record is active per MLLD 8.3 step 1.
3. Load current role and permission assignments; combine permissions across roles per MLLD 8.3 steps 2–3.
4. Confirm the resolved actor holds the DPP Ops permission (code TBD — OI-01) and that BU scope permits access to the target ASK (see OI-08).
5. If the actor is not active or does not hold the required permission, return `403 access_denied`. Source: US-ASK-015-ER4, US-ASK-015-VR5.

**Reference:** MLLD 8 (Identity, Authorization and RBAC); Coding Standards §27 (Authentication Standard), §28 (DRT Authorization / RBAC Standard).

---

## Processing Flow

1. **Receive and bind request** Accept `POST /core-asks/{askId}/cancel` with `Content-Type: application/json`. Generate or accept a correlation ID and attach it to the response per MLLD 6.1 step 1.

2. **Authenticate the caller** Validate the Entra bearer token (issuer, tenant, audience, signature, expiry). Resolve the DRT actor from the validated token. Return `401 authentication_required` if the token is absent, invalid, or expired. Source: MLLD 6.1 steps 2–3.

3. **Authorise the request** Confirm the actor is an active DRT user holding the DPP Ops permission (TBD — OI-01) with BU scope covering the target ASK (OI-08). Return `403 access_denied` if any check fails. Source: US-ASK-015-BR3, US-ASK-015-VR5, US-ASK-015-ER4.

4. **Validate the request body** Confirm `comment` is present and non-empty. Confirm the body `askId` matches the route `{askId}`. Return `400 validation_failed` for any violation. Source: US-ASK-015-BR1, US-ASK-015-VR1, US-ASK-015-VR2, US-ASK-015-ER2.

5. **Validate idempotency** Check whether an idempotency key (scope and header name TBD — OI-05) has already been processed for this cancel command. If a matching committed result exists, return the prior `200` response without re-executing. Source: MLLD 7.5; Coding Standards §22.

6. **Load the ASK record** Retrieve the Ask entity by `askId` including its current `statusId` and `rowVersion`. Return `404 resource_not_found` if no record exists. Source: US-ASK-015-VR3, US-ASK-015-ER1.

7. **Validate the ASK status** Confirm the current `statusId` is not 577 (Cancelled) and not 135 (Completed). Return `409 concurrency_conflict` if either blocked status is detected. The full list of cancellable source statuses beyond these two blocked values is TBD — OI-04. Source: US-ASK-015-BR2, US-ASK-015-VR4, US-ASK-015-ER3.

8. **Validate the concurrency token** Confirm the `rowVersion` loaded in step 6 matches the expected version. Return `409 concurrency_conflict` if another actor has already modified the record. Source: MLLD 7.5; Coding Standards §21.

9. **Apply the domain transition** Transition the Ask entity status to 577 (Cancelled). Record `cancelledAt` as the current UTC timestamp. Record `cancelledBy` as the server-resolved actor identity from step 2. Source: US-ASK-015-BR4, US-ASK-015-BR5.

10. **Persist the comment record** Create an AskComment record with `isDraft = false`, `isActive = true`, and the supplied `comment` text linked to the ASK. The approved entity name for ASK comments is AskComment (MLLD 9.2). Source: US-ASK-015-BR4.

11. **Persist the audit record** Create an audit history entry with `action = "Core Ask Cancelled"`, `description` set to the comment text, `createdBy` set to the server-resolved actor, and `createdOn` set to the current UTC timestamp. The approved audit entity name is TBD — OI-07; the story references an audit record but no approved entity name for ASK audit history appears in MLLD 9.2. Source: US-ASK-015-BR4.

12. **Write the outbox record** Insert an OutboxMessage record (MLLD 9.2) with event type TBD (OI-03), aggregate type `Ask`, aggregate ID `askId`, minimal payload, occurred UTC, and correlation ID. Whether an outbox event is required for cancellation is TBD — OI-03. Source: MLLD 7.2 step 8; Coding Standards §24.

13. **Commit atomically** Call `SaveChangesAsync` once to commit the Ask status update, AskComment, audit record, and OutboxMessage in a single database transaction. Source: MLLD 6.4; Coding Standards §23.

14. **Trigger post-commit notification** After the transaction commits, the App Service BackgroundService claims the OutboxMessage and dispatches any required email (Microsoft Graph) or UI refresh (Azure SignalR) notifications. Notification failure does not roll back the committed transition. Source: MLLD 11; Coding Standards §24, §25.

15. **Return the response** Map the committed state to `CancelCoreAskResponse` and return `200 OK` with `askId`, `statusId` (577), `cancelledAt`, `cancelledBy`, and `auditEntry`. Source: US-ASK-015-FR4, AC1.

---

## Business & Workflow Rules

### Required Field Rules
- `comment` is required; must be non-empty. Source: US-ASK-015-BR1, US-ASK-015-VR2.
- `askId` route parameter is required; must be a valid integer. Source: US-ASK-015-FR2, US-ASK-015-VR1.

### Conditional Field Rules
- No conditional field rules are defined by the story beyond the required fields above.

### Field Clearing Rules
- No field clearing rules are defined by this operation.

### Numeric and Date Validation Rules
- `askId` must be a positive integer matching an existing Ask record. Source: US-ASK-015-VR1, US-ASK-015-VR3.
- `cancelledAt` and `auditEntry.createdOn` are set to the current UTC `DateTimeOffset` by the server; they are not accepted from the client. Source: MLLD 9.3 (Timestamps).
- Status constant for Cancelled is 577. Status constant for Completed is 135. Source: US-ASK-015-BR5, US-ASK-015-BR6.

### Max Length Rules
- `comment`: maximum length is TBD — OI-06. The story does not specify a character limit beyond requiring presence.

### Button Behaviour Rules
- Not applicable — this is a server-side API operation with no button state.

### Status Routing Rules
- Successful cancellation: ASK `statusId` transitions to 577 (Cancelled). This is a terminal state; reopen is prohibited per MLLD 7.3. Source: US-ASK-015-BR5, AC1.
- Cancellation is blocked when current `statusId` is 577 (Cancelled) or 135 (Completed): return `409 concurrency_conflict`. Source: US-ASK-015-BR2, US-ASK-015-VR4.
- The full enumeration of cancellable source statuses is TBD — OI-04.

### Reference Data Validation
- Status ID 577 (Cancelled) and 135 (Completed) are used as blocking guards. Their mapping to approved workflow state names in the DRT state-transition document is TBD — OI-04. Source: US-ASK-015-BR5, US-ASK-015-BR6.

### Derived Field Rules
- `statusId` in the response is derived from the domain transition result; it is always 577 on a successful cancellation.
- `cancelledAt` is derived from the server UTC clock at the moment of commit.
- `cancelledBy` is derived from the server-resolved Entra actor identity; it is never accepted from the request body. Source: SEC 003 (MLLD 8; Coding Standards §27).
- `auditEntry.createdBy` and `auditEntry.createdOn` are derived from the server-resolved actor and server UTC clock respectively.
- `auditEntry.action` is the fixed string `"Core Ask Cancelled"`. Source: US-ASK-015-BR4.

---

## Data Impact

| Operation | Entity/Table | Purpose |
|---|---|---|
| Update | Ask | Set `statusId` to 577 (Cancelled), record `cancelledAt` and `cancelledBy`, increment `rowVersion` |
| Create | AskComment | Persist the mandatory cancellation comment with `isDraft = false` and `isActive = true` |
| Create | TBD (Audit entity — OI-07) | Persist audit entry with `action = "Core Ask Cancelled"`, actor, and timestamp |
| Create | OutboxMessage | Durable post-commit event record for downstream notification (event type TBD — OI-03) |

**Transaction Boundary:** The Ask status update, AskComment creation, audit record creation, and OutboxMessage insertion are committed in a single `SaveChangesAsync` call. Source: MLLD 6.4; Coding Standards §23.

**Consistency Boundary:** The Ask aggregate (Ask + AskComment + AskAttachmentLink per MLLD 9.2) is the consistency boundary. The audit record and OutboxMessage are co-committed within the same transaction to guarantee that no cancellation is recorded without a corresponding audit trail and notification trigger.

---

## Events & Integrations

### Outbox Event

**Event Type:** TBD — the story does not name an outbox event for cancellation. See Open Item OI-03.

**Purpose:** Notify downstream consumers (email recipients, SignalR work-queue refresh) that a Core ASK has been cancelled.

**Outbox Record Fields:** message id (stable GUID), type and version `<TBD-EventType>/v1`, payload `{ "askId": <integer>, "statusId": 577, "cancelledBy": "<actor>", "cancelledAt": "<UTC>" }`, occurred UTC, correlation id, status, attempts.

**Delivery Guarantee:** At least once. Source: MLLD 11.2; Coding Standards §24.

**Downstream Consumers:** TBD — recipient rules, email template, and SignalR channel for ASK cancellation are not specified in the story. See Open Item OI-03.

### External Dependencies

**Authorization Service:** The DRT authorization model in Azure SQL is queried to confirm DPP Ops group membership and BU scope. Source: US-ASK-015-DEP4.

**Comment Creation:** AskComment is persisted within the same transaction as the ASK status update; no separate external service is required. Source: US-ASK-015-DEP2.

**Audit History Creation:** The audit record is persisted within the same transaction; no separate external service is required. Source: US-ASK-015-DEP3.

**Microsoft Graph / Azure SignalR:** Post-commit notification delivery via the App Service BackgroundService outbox worker. Source: MLLD 11; US-ASK-015-DEP1.

---

## Errors & Tests

### Error Scenarios

| Scenario | HTTP Status | Error Message / Behaviour |
|---|---|---|
| `comment` is absent or empty in the request body | `400 validation_failed` | `"comment is required and must not be empty"` |
| Body `askId` does not match route `{askId}` | `400 validation_failed` | `"askId in request body must match the route parameter"` |
| Bearer token is absent, invalid, or expired | `401 authentication_required` | Standard 401 problem detail |
| Caller does not hold DPP Ops permission | `403 access_denied` | `"Caller does not have permission to cancel a Core ASK"` |
| Caller BU scope does not cover the target ASK (TBD — OI-08) | `403 access_denied` | `"Caller does not have scope to cancel this Core ASK"` |
| `askId` does not match any existing Ask record | `404 resource_not_found` | `"Core ASK not found"` |
| ASK current `statusId` is 577 (Cancelled) | `409 concurrency_conflict` | `"Core ASK is already Cancelled and cannot be cancelled again"` |
| ASK current `statusId` is 135 (Completed) | `409 concurrency_conflict` | `"Core ASK is Completed and cannot be cancelled"` |
| Optimistic concurrency conflict on the Ask `rowVersion` | `409 concurrency_conflict` | `"The Core ASK was modified by another user; reload and retry"` |
| Idempotency key already processed (TBD — OI-05) | `200 OK` | Return prior committed response without re-executing |
| Unexpected server failure | `500 unexpected_error` | Standard 500 problem detail; no internal detail exposed |
| Required dependency unavailable (e.g. Azure SQL) | `503 dependency_unavailable` | Standard 503 problem detail |

**Error Response Standards:**
- Never expose SQL statements, internal exception detail, connection information, or secrets in error responses.
- Emit structured telemetry including correlation ID and actor ID without token or payload content per MLLD 12.3.
- Map domain and concurrency exceptions to the standard API error response (Problem Details with `code`, `title`, `detail`, `traceId`) per MLLD 12.2; Coding Standards §29.

### Test Scenarios

**Positive Scenarios:**

1. **Successful cancellation of a Core ASK**
   - Given: A Core ASK exists with a cancellable status (not 577 or 135), and the caller is an active DRT user holding the DPP Ops permission with BU scope covering the ASK.
   - When: `POST /core-asks/{askId}/cancel` is called with a valid non-empty `comment`.
   - Then: The API returns `200 OK`; the Ask `statusId` is 577; an AskComment record exists with `isDraft = false` and `isActive = true`; an audit record exists with `action = "Core Ask Cancelled"`; the response contains `askId`, `statusId = 577`, `cancelledAt`, `cancelledBy` (server-resolved), and `auditEntry`.

2. **Idempotent re-submission of a cancel command (TBD pending OI-05)**
   - Given: A cancel command with a specific idempotency key has already been committed successfully.
   - When: The same request is resubmitted with the same idempotency key.
   - Then: The API returns `200 OK` with the prior committed response; no duplicate Ask update, AskComment, audit record, or OutboxMessage is created.

3. **Response payload completeness**
   - Given: A successful cancellation as in Scenario 1.
   - When: The response body is inspected.
   - Then: All six fields are present: `askId`, `statusId`, `cancelledAt`, `cancelledBy`, `auditEntry.action`, `auditEntry.description`, `auditEntry.createdBy`, `auditEntry.createdOn`.

4. **OutboxMessage committed atomically with ASK update**
   - Given: A valid cancel request is processed.
   - When: The transaction commits.
   - Then: An OutboxMessage record exists in the database with the correct aggregate type `Ask`, aggregate ID, and occurred UTC; it is not yet marked Processed.

**Negative Scenarios:**

5. **Missing cancellation comment**
   - Given: A valid cancellable Core ASK and an authorised caller.
   - When: `POST /core-asks/{askId}/cancel` is called with `comment` absent or empty.
   - Then: The API returns `400 validation_failed`; no Ask record is modified; no AskComment or audit record is created.

6. **ASK already Cancelled (577)**
   - Given: The Core ASK `statusId` is 577.
   - When: `POST /core-asks/{askId}/cancel` is called with a valid comment.
   - Then: The API returns `409 concurrency_conflict`; no state change occurs.

7. **ASK already Completed (135)**
   - Given: The Core ASK `statusId` is 135.
   - When: `POST /core-asks/{askId}/cancel` is called with a valid comment.
   - Then: The API returns `409 concurrency_conflict`; no state change occurs.

8. **Caller lacks DPP Ops permission**
   - Given: The caller is an active DRT user without the DPP Ops permission.
   - When: `POST /core-asks/{askId}/cancel` is called.
   - Then: The API returns `403 access_denied`; no state change occurs.

9. **ASK not found**
   - Given: The provided `askId` does not correspond to any existing Ask record.
   - When: `POST /core-asks/{askId}/cancel` is called.
   - Then: The API returns `404 resource_not_found`.

10. **Unauthenticated request**
    - Given: No bearer token is provided.
    - When: `POST /core-asks/{askId}/cancel` is called.
    - Then: The API returns `401 authentication_required`.

11. **Optimistic concurrency conflict**
    - Given: Two concurrent callers load the same Ask record simultaneously; the first commits a state change.
    - When: The second caller submits the cancel command with a stale `rowVersion`.
    - Then: The API returns `409 concurrency_conflict`; the second caller's change is not applied.

12. **Body askId does not match route askId**
    - Given: A valid cancellable Core ASK and an authorised caller.
    - When: `POST /core-asks/100/cancel` is called with `"askId": 999` in the request body.
    - Then: The API returns `400 validation_failed`.

---

## Assumptions & Open Items

### Assumptions

1. **Task status update exclusion:** Associated workflow task status updates on cancellation are excluded from this endpoint's scope per US-ASK-015-AS1. The conflict with MLLD 7.3 (Cancel must complete open tasks as Cancelled) is recorded as Validation Finding F-04 and requires resolution by Solution Architecture before implementation.

2. **cancelledBy server resolution:** The story includes `cancelledBy` in the request body schema. This card assumes the correct behaviour is server-side actor resolution per SEC 003 (MLLD 8; Coding Standards §27). The browser-supplied value is ignored. This is recorded as Validation Finding F-02 and requires confirmation by Solution Architecture.

3. **Status constants as stated:** Status constants 577 (Cancelled) and 135 (Completed) are taken as stated in the story. Their mapping to approved workflow state names in the DRT state-transition document is an open item (OI-04).

4. **Comment content validation:** Validation beyond presence (e.g. maximum length, profanity filter, format) is not specified. This card enforces presence only per US-ASK-015-AS2.

5. **HTTP 200 on success:** The story specifies `200 OK` for a successful cancellation (AC1). This card implements 200. The absence of a Master LLD catalogue entry for this status on terminal cancel operations is recorded as Validation Finding F-03.

### Open Items

1. **OI-01 — DPP Ops permission code:** The story names DPP Ops group / DPP Operation Editors membership as the access condition but does not supply a discrete permission code. The exact permission code required for the cancel action must be confirmed. Owner: Security.

2. **OI-02 — cancelledBy field in request body:** The story includes `cancelledBy` in the request body, which conflicts with SEC 003. Solution Architecture must confirm whether the field should be removed from the request schema entirely or retained for a specific purpose. Owner: Solution Architecture.

3. **OI-03 — Outbox event type, payload, and recipients:** The story does not name an outbox event for ASK cancellation. The event type, version, minimal payload, recipient rule, email template, and SignalR channel must be defined. Owner: Business Analysis.

4. **OI-04 — Full list of cancellable ASK statuses:** The story states the ASK must be in a cancellable status and blocks 577 and 135, but does not enumerate all valid source statuses. The complete approved state-transition table must be provided, including the mapping of numeric status IDs 577 and 135 to approved workflow state names. Owner: Business Analysis.

5. **OI-05 — Idempotency key scope and header:** The cancel command is a retry-sensitive state-changing operation per REL 001 and MLLD 7.5. The idempotency key header name, uniqueness scope, and storage mechanism must be defined. Owner: Solution Architecture.

6. **OI-06 — Maximum length for comment field:** The story requires comment presence but does not specify a character limit. A maximum length must be defined to support database column sizing and API validation. Owner: Business Analysis.

7. **OI-07 — Audit entity name:** The story references an audit record with `action = "Core Ask Cancelled"` but no approved audit entity name for ASK audit history appears in MLLD 9.2. The approved entity name must be confirmed before the Data Impact table and Processing Flow can be finalised. Owner: Solution Architecture.

8. **OI-08 — BU scope rule for cancellation:** Whether cancellation is restricted to ASKs within the caller's BU or is global for any DPP Ops Editor is not specified. The BU scope rule must be confirmed. Owner: Security.

9. **OI-09 — API path prefix and resource name:** The story exposes `POST /core-asks/{askId}/cancel` (no `/api/v1` prefix; uses `core-asks`). The Master LLD catalogue (MLLD 15) lists `POST /api/v1/asks/{id}/cancel` (with `/api/v1` prefix; uses `asks`). The approved base path and resource name must be confirmed. Owner: Solution Architecture.

---

## Traceability

### User Story Requirements Traced

| Requirement ID | Requirement Description | LLD Section |
|---|---|---|
| US-ASK-015-FR1 | Expose POST /core-asks/{askId}/cancel | API Identification, Contract Summary |
| US-ASK-015-FR2 | Accept required askId | Contract Summary, Processing Flow step 4, Business & Workflow Rules |
| US-ASK-015-FR3 | Accept request body with required comment | Contract Summary, Business & Workflow Rules |
| US-ASK-015-FR4 | Return confirmation payload including askId, statusId, cancelledAt, cancelledBy, auditEntry fields | Contract Summary (Response), Processing Flow step 15 |
| US-ASK-015-BR1 | comment is mandatory | Business & Workflow Rules (Required Field Rules), Processing Flow step 4 |
| US-ASK-015-BR2 | Cancellation not allowed if ASK is already Cancelled (577) or Completed (135) | Business & Workflow Rules (Status Routing Rules), Processing Flow step 7 |
| US-ASK-015-BR3 | Caller must be in DPP Ops group | Authorization, Processing Flow step 3 |
| US-ASK-015-BR4 | Cancellation writes: ASK status update, Comment record (isDraft=false, isActive=true), Audit record (action "Core Ask Cancelled") | Processing Flow steps 9–13, Data Impact |
| US-ASK-015-BR5 | Status constant for Cancelled is 577 | Business & Workflow Rules (Status Routing Rules, Numeric and Date Validation Rules) |
| US-ASK-015-BR6 | Completed status constant is 135 | Business & Workflow Rules (Status Routing Rules, Numeric and Date Validation Rules) |
| US-ASK-015-VR1 | askId must be valid | Contract Summary (Contract Notes), Processing Flow step 4 |
| US-ASK-015-VR2 | comment must be present | Business & Workflow Rules (Required Field Rules), Processing Flow step 4 |
| US-ASK-015-VR3 | ASK must exist | Processing Flow step 6, Errors & Tests |
| US-ASK-015-VR4 | ASK must not already be Cancelled or Completed | Processing Flow step 7, Business & Workflow Rules |
| US-ASK-015-VR5 | Caller must have DPP Ops permission | Authorization, Processing Flow step 3 |
| US-ASK-015-ER1 | Return 404 if askId is not found | Errors & Tests (Error Scenarios), Processing Flow step 6 |
| US-ASK-015-ER2 | Return 400 if comment is missing | Errors & Tests (Error Scenarios), Processing Flow step 4 |
| US-ASK-015-ER3 | Return 409 if ASK is already Cancelled or Completed | Errors & Tests (Error Scenarios), Processing Flow step 7 |
| US-ASK-015-ER4 | Return 403 if caller is not in DPP Ops group | Authorization, Processing Flow step 3, Errors & Tests |
| US-ASK-015-AS1 | Task status updates excluded | Assumptions & Open Items (Assumption 1), Validation Findings (F-04) |
| US-ASK-015-AS2 | Comment content validation beyond presence not specified | Assumptions & Open Items (Assumption 4) |

**Total Requirements Traced:** 21

### Acceptance Criteria Traced

| Acceptance Criteria | LLD Section | Test Scenario |
|---|---|---|
| AC1 — Cancel Core ASK successfully: 200, status 577, comment saved, audit created | Contract Summary, Processing Flow steps 9–15, Data Impact | Test Scenario 1 |
| AC2 — Missing cancellation comment returns 400 | Business & Workflow Rules (Required Field Rules), Errors & Tests | Test Scenario 5 |
| AC3 — ASK already Completed (135) returns 409 | Business & Workflow Rules (Status Routing Rules), Errors & Tests | Test Scenario 7 |
| AC4 — ASK already Cancelled (577) returns 409 | Business & Workflow Rules (Status Routing Rules), Errors & Tests | Test Scenario 6 |
| AC5 — User lacks permission returns 403 | Authorization, Errors & Tests | Test Scenario 8 |
| AC6 — ASK not found returns 404 | Processing Flow step 6, Errors & Tests | Test Scenario 9 |

**Total Acceptance Criteria Traced:** 6

---

## Validation Findings

**Finding F-01: API Path Prefix and Resource Name Mismatch**
- **Type:** Missing Implementation Detail
- **Description:** The story exposes `POST /core-asks/{askId}/cancel` with no `/api/v1` prefix and uses the resource name `core-asks`. The Master LLD catalogue (MLLD 15) lists `POST /api/v1/asks/{id}/cancel` with the `/api/v1` prefix and the resource name `asks`. These are different paths.
- **Impact:** The implementation cannot determine the correct base path and resource name without a confirmed decision. Generating code against the wrong path will require a breaking contract change.
- **Recommendation:** Solution Architecture must confirm the approved base path (`/api/v1` or none) and resource name (`asks` or `core-asks`) before implementation begins.

**Finding F-02: cancelledBy Field Conflicts with SEC 003**
- **Type:** Missing Business Rule Detail
- **Description:** The story includes `cancelledBy` as a caller-supplied field in the request body. SEC 003 (MLLD 8; Coding Standards §27) prohibits accepting actor identity, role, or recipient as authoritative from the browser. The server must resolve the cancelling actor from the validated Entra token.
- **Impact:** If the browser-supplied `cancelledBy` is accepted as authoritative, the authorization model is bypassed. If the field is silently ignored, the API contract diverges from the story schema without a confirmed decision.
- **Recommendation:** Solution Architecture must confirm whether `cancelledBy` should be removed from the request schema or retained as an informational field that is always overridden by the server-resolved actor.

**Finding F-03: HTTP 200 vs 204 for Terminal Cancel Operation**
- **Type:** Missing Implementation Detail
- **Description:** The story specifies `200 OK` on successful cancellation (AC1). The Master LLD does not include a catalogue entry specifying 200 vs 204 for terminal cancel operations.
- **Impact:** If the OpenAPI contract is later defined as 204, the response body schema becomes invalid and clients must be updated.
- **Recommendation:** The API Contract Owner must confirm the approved success status code (200 with body, or 204 without body) for this operation and update the OpenAPI contract accordingly.

**Finding F-04: Workflow Task Closure Excluded from Scope**
- **Type:** Assumption Requiring Validation
- **Description:** The story explicitly excludes task status updates from scope (US-ASK-015-AS1). MLLD 7.3 states that a Cancel action must complete open tasks as Cancelled and that reopen is prohibited. These two statements are in direct conflict.
- **Impact:** If task closure is excluded, the workflow instance may be left in an inconsistent state with open tasks pointing to a terminal ASK. This violates the atomic transition requirement in MLLD 7.2 step 8 and WF 003.
- **Recommendation:** Solution Architecture must resolve whether task closure is in scope for this endpoint before implementation. If excluded, the workflow consistency model must be documented as an approved exception.

**Finding F-05: Status Constants 577 and 135 Not Mapped to Approved Workflow State Names**
- **Type:** Missing Business Rule Detail
- **Description:** The story uses numeric status IDs 577 (Cancelled) and 135 (Completed) as blocking guards. These numeric values are not mapped to approved workflow state names in any document available in the knowledge base.
- **Impact:** The implementation cannot validate status transitions against the approved state-transition table without confirmed state name mappings. Test scenarios cannot be verified against the approved workflow diagram.
- **Recommendation:** Business Analysis must provide the approved DRT state-transition document mapping numeric status IDs to workflow state names, and confirm the full list of cancellable source statuses.

**Finding F-06: Audit Entity Name Not in MLLD 9.2**
- **Type:** Missing Implementation Detail
- **Description:** The story requires an audit record with `action = "Core Ask Cancelled"` to be created on cancellation. No approved audit entity name for ASK audit history appears in the MLLD 9.2 entity list.
- **Impact:** The Data Impact table and Processing Flow cannot reference an approved entity name. The EF Core entity configuration and migration cannot be written without a confirmed table name.
- **Recommendation:** Solution Architecture must confirm the approved entity name for ASK audit history and add it to the MLLD 9.2 entity list, or confirm that an existing entity (e.g. WorkflowHistory) covers this requirement.

**Finding F-07: Comment Entity Maximum Length Not Specified**
- **Type:** Missing Implementation Detail
- **Description:** The story requires `comment` to be present but does not specify a maximum character length. MLLD 9.3 requires explicit string lengths in entity configurations.
- **Impact:** The EF Core entity configuration for AskComment cannot be finalised without a confirmed maximum length. An unbounded column may cause database truncation or performance issues.
- **Recommendation:** Business Analysis must specify the maximum length for the cancellation comment field.

**Finding F-08: Outbox Event Type and Recipients Not Defined**
- **Type:** Missing Integration Detail
- **Description:** The story does not name an outbox event type, version, recipient rule, email template, or SignalR channel for ASK cancellation. MLLD 11 requires the outbox record to carry a versioned event type.
- **Impact:** The OutboxMessage cannot be written with a confirmed event type. The BackgroundService worker cannot resolve recipients or select a notification template without this information.
- **Recommendation:** Business Analysis must define the outbox event type, version, minimal payload, recipient rule, and notification template for ASK cancellation.

**Total Validation Findings:** 8

---

## Audit Summary

**Document Version:** 1.0
**Generated By:** DRT Design Card Writer
**Story ID:** US-ASK-015 (ADO Work Item 1858)
**Module/Functionality:** ASK - Cancel Core ASK
**Status:** Ready for Review

**Requirements Coverage:** 21 requirements traced (100%); 6 acceptance criteria traced (100%).

**Validation Findings:** 8 findings, 5 assumptions, 9 open items.

**Knowledge Base Utilisation:**
- *DC Card Templates (DC_Card_Templates_1.md)*: Section contract, size budgets, writing rules, entity name list, error code catalogue, and example card format.
- *DRT Master Low Level Design v0.1 (DRT_Master_Low_Level_Design_v0.1.docx)*: MLLD 6.1 (request pipeline), MLLD 6.2 (REST conventions), MLLD 7.2–7.5 (workflow execution, concurrency, idempotency), MLLD 8 (authentication, authorization, RBAC), MLLD 9.2 (entity names), MLLD 9.3 (data conventions), MLLD 11 (outbox and notifications), MLLD 12.2 (error codes), MLLD 12.3 (telemetry), MLLD 15 (proposed API catalogue).
- *DRT-ProCode Coding Standards (Coding Standards.docx)*: §21 (optimistic concurrency), §22 (idempotency), §23 (transaction), §24 (transactional outbox), §25 (notification), §27 (authentication), §28 (authorization/RBAC), §29 (API error standard).

**Architecture Compliance:**
- *Dependency direction:* Processing Flow follows Controller → Application Use Case → Domain → Repository → EF Core → Azure SQL per MLLD 3.2 and Coding Standards §7.
- *Authorization:* Actor resolved from validated Entra token; current DRT role, permission, BU scope, and record scope evaluated per MLLD 8.3 and Coding Standards §28.
- *Persistence:* Ask, AskComment, audit entity (TBD), and OutboxMessage committed atomically in a single `SaveChangesAsync` call per MLLD 6.4 and Coding Standards §23.
- *Workflow:* Terminal Cancelled transition applied via domain entity method; reopen prohibited per MLLD 7.3; task closure conflict recorded as Finding F-04.
- *Errors:* Problem Details with stable error codes from MLLD 12.2 catalogue; no internal detail exposed per Coding Standards §29.
- *Security:* `cancelledBy` resolved server-side; no actor identity accepted from request body per SEC 003 (MLLD 8; Coding Standards §27).

**Readiness for Code Generation:** Conditional. Eight validation findings must be resolved before implementation. Blocking items: F-01 (API path), F-02 (cancelledBy field), F-04 (task closure scope), F-06 (audit entity name). Non-blocking but required before finalisation: F-03 (HTTP status), F-05 (status constants mapping), F-07 (comment max length), F-08 (outbox event type).

---

## Document Control

**Document Owner:** DRT Solution Architecture
**Approval Required From:** Development Lead, Security Lead, Business Analyst
**Review Cycle:** Architecture Review, Development Review, Security Review
**Effective Date:** Pending Approval
