# DRT Compact API Design Card

## API Identification

**Module/Functionality:** ASK - Retrieve Core ASK Comments
**Story ID:** US-ASK-005 (ADO Work Item 1856)
**Method:** GET
**Endpoint:** `/core-asks/{askId}/comments`
**Operation:** Returns all active, non-draft comments for a specified Core ASK, optionally filtered by version.

---

## Contract Summary

### Request

**Content-Type:** `application/json`

**Path Parameters:**
```json
{
  "askId": "integer (required, must be a valid, existing Core ASK identifier)"
}
```

**Query Parameters:**
```json
{
  "version": "integer (optional, if provided must be a valid version value)"
}
```

**Contract Notes:**
- `askId` is a required path parameter. Source: US-ASK-005-FR2.
- `version` is an optional query parameter. Source: US-ASK-005-FR3.
- The server always applies `isDraft=false` and `isActive=true` filters regardless of any client input. Source: US-ASK-005-BR1, US-ASK-005-BR2.
- No request body is accepted on a GET operation.
- Whether `version` filters by `AskVersion.version` or a separate comment-level version field is unresolved — see Open Item 9.

### Response

**Status Code:** `200 OK`
**Content-Type:** `application/json`

**Response Body:**
```json
{
  "comments": [
    {
      "commentId": "integer (unique comment identifier)",
      "requestId": "integer (TBD — source entity/field unconfirmed; see Open Item 7)",
      "moduleTypeId": "integer (TBD — source entity/field unconfirmed; see Open Item 8)",
      "comment": "string (comment text)",
      "isDraft": "boolean (always false in response; server-filtered)",
      "isActive": "boolean (always true in response; server-filtered)",
      "version": "integer (version associated with the comment)",
      "createdBy": "string (identity of the creator)",
      "createdOn": "string (ISO 8601 UTC datetime)",
      "modifiedBy": "string (identity of the last modifier)",
      "modifiedOn": "string (ISO 8601 UTC datetime)"
    }
  ]
}
```

**OpenAPI Reference:** Operation `getCoreAskComments` (proposed); request schema `GetCoreAskCommentsRequest`; response schema `GetCoreAskCommentsResponse`. Final approved operation ID and schema component names are unconfirmed — see Open Item 3.

---

## Authorization

**Required Permission:** TBD — exact permission code (e.g. `ASK.READ` or equivalent) is not defined in the story. See Open Item 1.

**Authorization Flow:**
1. Validate the Entra access token: issuer, tenant, audience, signature, and expiry per MLLD 8.1.
2. Resolve the current DRT actor using the immutable Entra Tenant ID and Object ID per MLLD 8.2.
3. Confirm the DRT user record exists and `IsActive = true` per MLLD 8.3 step 1.
4. Load current role and permission assignments from Azure SQL and combine permissions across roles per MLLD 8.3 step 2.
5. Confirm the actor holds the required read permission for the ASK module (permission code TBD — Open Item 1).
6. Apply business-unit scope rules: whether the caller must belong to the same BU as the ASK owner is unresolved — see Open Item 2.
7. Confirm the `askId` record is visible to the actor under current record-level authorization per MLLD 8.3 step 3.
8. If any check fails, return `401 authentication_required` (unauthenticated) or `403 access_denied` (unauthorized) per MLLD 12.2.

**Reference:** MLLD 8.1–8.3; US RBAC 001 matrix (implementation source for all allow/deny tests — approved roles that may call this endpoint are unconfirmed, see Open Item 10).

---

## Processing Flow

1. **Receive and bind request** Accept the HTTP GET request at `/core-asks/{askId}/comments`. Bind `askId` from the path and `version` from the query string.

2. **Validate token and resolve actor** Validate the Entra bearer token and resolve the current DRT actor per MLLD 8.1–8.2. Return `401 authentication_required` if the token is absent, expired, or invalid.

3. **Confirm actor is active** Verify the resolved DRT user exists and `IsActive = true`. Return `403 access_denied` if the user is deactivated.

4. **Evaluate permission and BU scope** Confirm the actor holds the required ASK read permission (TBD — Open Item 1) and satisfies BU scope rules (TBD — Open Item 2). Return `403 access_denied` on failure.

5. **Validate askId format** Confirm `askId` is a valid integer value per US-ASK-005-VR1. Return `400 validation_failed` if the value is malformed.

6. **Validate version format (conditional)** If `version` is provided, confirm it is a valid integer value per US-ASK-005-VR2. Return `400 validation_failed` if the value is malformed.

7. **Confirm Core ASK existence** Query the `Ask` entity to verify a record with the given `askId` exists per US-ASK-005-VR3 and US-ASK-005-DEP2. Return `404 resource_not_found` if no matching record is found per US-ASK-005-ER1.

8. **Apply record-level authorization** Confirm the resolved actor is permitted to view the identified Core ASK record per MLLD 8.3 step 3 and US-ASK-005-DEP3. Return `403 access_denied` if the actor lacks visibility.

9. **Query comments with mandatory server-side filters** Query the `AskComment` entity applying the mandatory server-side filters `isDraft = false` and `isActive = true` per US-ASK-005-BR1 and US-ASK-005-BR2. If `version` is provided, apply the version filter per US-ASK-005-FR3 (filter semantics TBD — Open Item 9).

10. **Return response** Map the result set to the `GetCoreAskCommentsResponse` schema. If no comments match the filters, return `200 OK` with `"comments": []` per US-ASK-005-BR3 and US-ASK-005-ER2. Sort order of the comments array is unspecified — see Open Item 6.

11. **Emit structured telemetry** Record route, method, status code, duration, trace ID, and actor ID without token or payload content per MLLD 12.3 and OBS 001.

---

## Business & Workflow Rules

### Required Field Rules
- `askId` is required as a path parameter. Source: US-ASK-005-FR2.

### Conditional Field Rules
- `version` is optional. If provided, it must be a valid integer and is used to filter comments. Source: US-ASK-005-FR3.

### Field Clearing Rules
- Not applicable. This is a read-only operation; no fields are cleared.

### Numeric and Date Validation Rules
- `askId` must be a valid integer. Source: US-ASK-005-VR1.
- `version`, if provided, must be a valid integer. Source: US-ASK-005-VR2.
- `createdOn` and `modifiedOn` are returned as ISO 8601 UTC datetime strings per MLLD 9.3.

### Max Length Rules
- No client-supplied string fields exist on this request. Max length rules apply to response fields as defined by the approved `AskComment` entity schema (not specified in the story).

### Button Behaviour Rules
- Not applicable. This is a server-side API operation.

### Status Routing Rules
- Not applicable. This is a read-only operation with no workflow state transition.

### Reference Data Validation
- `moduleTypeId` in the response is described as a reference data identifier. The source table and valid values are unconfirmed — see Open Item 8.

### Derived Field Rules
- `isDraft` is always `false` in the response; the server applies this filter and the value is not client-tunable. Source: US-ASK-005-BR1, US-ASK-005-BR2.
- `isActive` is always `true` in the response; the server applies this filter and the value is not client-tunable. Source: US-ASK-005-BR1, US-ASK-005-BR2.

---

## Data Impact

| Operation | Entity/Table | Purpose |
|---|---|---|
| Read | `Ask` | Verify the Core ASK identified by `askId` exists (US-ASK-005-VR3, US-ASK-005-DEP2) |
| Read | `AskComment` | Retrieve active, non-draft comments for the identified Core ASK (US-ASK-005-FR4) |

**Transaction Boundary:** No database transaction is required. Both reads are non-mutating, read-only projections using `AsNoTracking` per Coding Standards EF Core Query Standard.

**Consistency Boundary:** The ASK aggregate (`Ask`, `AskComment`) per MLLD 9.2. No data is written; consistency is read-time only.

---

## Events & Integrations

Not applicable. This is a read-only operation. No outbox event is emitted, no workflow transition occurs, and no external integration is triggered. Source: US-ASK-005-AS2.

---

## Errors & Tests

### Error Scenarios

| Scenario | HTTP Status | Error Message / Behaviour |
|---|---|---|
| Bearer token absent, expired, or invalid | `401 authentication_required` | Authentication required; token validation failed |
| DRT user is deactivated | `403 access_denied` | Access denied; user account is inactive |
| Actor lacks required ASK read permission | `403 access_denied` | Access denied; insufficient permission |
| Actor fails BU scope check | `403 access_denied` | Access denied; record not within authorized scope |
| `askId` is not a valid integer | `400 validation_failed` | Validation failed; askId must be a valid integer |
| `version` is provided but not a valid integer | `400 validation_failed` | Validation failed; version must be a valid integer |
| `askId` does not match any Core ASK record | `404 resource_not_found` | Resource not found; the specified Core ASK does not exist |
| Unexpected server failure | `500 unexpected_error` | Unexpected error; contact support with the trace ID |
| Comment store unavailable | `503 dependency_unavailable` | Dependency unavailable; comment store could not be reached |

**Error Response Standards:**
- Never expose SQL statements, internal exception detail, connection information, or secrets per MLLD 12.1 and Coding Standards Section 29.
- Emit structured telemetry without secrets or confidential payloads per MLLD 12.3.
- Map domain and concurrency exceptions to the standard API error response using Problem Details format per Coding Standards Section 29.
- Include HTTP status, stable error code, problem title, safe detail, and trace/correlation ID in every error response.

### Test Scenarios

**Positive Scenarios:**

1. **Retrieve comments successfully**
   - Given: A valid Core ASK exists with multiple active, non-draft comments
   - When: GET `/core-asks/{askId}/comments` is called with a valid authenticated actor holding the required permission
   - Then: The API returns `200 OK` with a `comments` array containing all matching comments, each with `isDraft=false` and `isActive=true`. Source: AC1.

2. **Draft and inactive comments excluded**
   - Given: A Core ASK exists with a mix of active non-draft, draft, and inactive comments in storage
   - When: GET `/core-asks/{askId}/comments` is called with a valid authenticated actor
   - Then: The API returns `200 OK` with only comments where `isDraft=false` and `isActive=true`; draft and inactive comments are absent from the response. Source: AC2.

3. **No active non-draft comments returns empty array**
   - Given: A valid Core ASK exists but has no comments satisfying `isDraft=false` and `isActive=true`
   - When: GET `/core-asks/{askId}/comments` is called with a valid authenticated actor
   - Then: The API returns `200 OK` with `"comments": []`. Source: AC3, US-ASK-005-BR3, US-ASK-005-ER2.

4. **Optional version parameter filters results**
   - Given: A valid Core ASK exists with active non-draft comments across multiple versions
   - When: GET `/core-asks/{askId}/comments?version=2` is called with a valid authenticated actor
   - Then: The API returns `200 OK` with only comments matching the specified version (filter semantics TBD — Open Item 9). Source: US-ASK-005-FR3.

**Negative Scenarios:**

5. **Core ASK not found**
   - Given: No Core ASK exists for the given `askId`
   - When: GET `/core-asks/{askId}/comments` is called with a valid authenticated actor
   - Then: The API returns `404 resource_not_found`. Source: AC4, US-ASK-005-ER1.

6. **Unauthenticated request**
   - Given: No bearer token is provided
   - When: GET `/core-asks/{askId}/comments` is called
   - Then: The API returns `401 authentication_required`.

7. **Deactivated user**
   - Given: The authenticated Entra identity maps to a DRT user with `IsActive = false`
   - When: GET `/core-asks/{askId}/comments` is called
   - Then: The API returns `403 access_denied`.

8. **Insufficient permission**
   - Given: The authenticated actor does not hold the required ASK read permission
   - When: GET `/core-asks/{askId}/comments` is called
   - Then: The API returns `403 access_denied`.

9. **Invalid askId format**
   - Given: `askId` is supplied as a non-integer value (e.g. a string)
   - When: GET `/core-asks/{askId}/comments` is called
   - Then: The API returns `400 validation_failed` with a validation error identifying `askId`.

10. **Invalid version format**
    - Given: `version` is supplied as a non-integer value
    - When: GET `/core-asks/{askId}/comments?version=abc` is called
    - Then: The API returns `400 validation_failed` with a validation error identifying `version`.

---

## Assumptions & Open Items

### Assumptions

1. **askId type:** `askId` is treated as an integer path parameter as stated in the story API Details (US-ASK-005-FR2). Alignment with the Master LLD GUID convention (MLLD 9.3) is recorded as a difference for Solution Architecture — see Open Item 5.
2. **Read-only operation:** The endpoint is read-only and returns stored metadata only; no state change, outbox event, or external integration is triggered. Source: US-ASK-005-AS2.
3. **AskComment entity:** Comments are stored in and retrieved from the `AskComment` entity per MLLD 9.2. No additional comment store entity is assumed.
4. **Empty result is success:** An empty `comments` array is a valid successful response, not an error condition. Source: US-ASK-005-BR3.

### Open Items

1. **Required permission code:** The exact permission code required to call this endpoint (e.g. `ASK.READ` or equivalent) is not defined in the story. Owner: Security.
2. **BU scope rule:** Whether the caller must belong to the same business unit as the ASK owner to retrieve comments is not defined. Owner: Security.
3. **Approved OpenAPI operation ID and schema names:** The final approved `operationId`, request schema name, and response schema name for this endpoint are unconfirmed. Owner: API Contract Owner.
4. **Confirmed route and path prefix:** Whether the final path is `/api/v1/asks/{id}/comments` (per MLLD 15 proposed catalogue) or `/core-asks/{askId}/comments` (per story US-ASK-005-FR1) is unresolved. Owner: Solution Architecture.
5. **Confirmed primary key type for askId:** The story states `askId` is an integer; MLLD 9.3 specifies GUID or approved database-generated key as the primary key convention. The correct type is unconfirmed. Owner: Solution Architecture.
6. **Comment sort order:** The story explicitly states comment ordering is not defined (US-ASK-005-AS1). The sort order for the response `comments` array must be confirmed before implementation. Owner: Business Analysis.
7. **requestId field definition:** The `requestId` field appears in the story response schema (US-ASK-005-FR4) but is not listed in any approved ASK aggregate entity in MLLD 9.2. Its source table, entity, and semantics are unconfirmed. Owner: Solution Architecture.
8. **moduleTypeId field definition:** The `moduleTypeId` field appears in the story response schema (US-ASK-005-FR4) but is not listed in any approved ASK aggregate entity in MLLD 9.2. Its source table, entity, valid values, and reference data are unconfirmed. Owner: Solution Architecture.
9. **version filter semantics:** Whether the `version` query parameter filters by `AskVersion.version` or a separate comment-level version field is not defined in the story. Owner: Business Analysis.
10. **Approved RBAC matrix row:** The US RBAC 001 matrix row confirming which roles (e.g. DPP Operations Editor, DPP Operations Viewer, PPL, DPP Leadership) may call this endpoint has not been confirmed. Owner: Security.

---

## Traceability

### User Story Requirements Traced

| Requirement ID | Requirement Description | LLD Section |
|---|---|---|
| US-ASK-005-FR1 | Expose GET /core-asks/{askId}/comments | API Identification, Contract Summary |
| US-ASK-005-FR2 | Accept required askId | Contract Summary, Processing Flow step 5, Business & Workflow Rules |
| US-ASK-005-FR3 | Accept optional version | Contract Summary, Processing Flow step 6, Business & Workflow Rules |
| US-ASK-005-FR4 | Return comments array with all specified fields | Contract Summary (Response Body) |
| US-ASK-005-BR1 | Server always filters comments using isDraft=false and isActive=true | Contract Summary (Contract Notes), Processing Flow step 9, Business & Workflow Rules (Derived Field Rules) |
| US-ASK-005-BR2 | Filtering is not client-tunable | Contract Summary (Contract Notes), Business & Workflow Rules (Derived Field Rules) |
| US-ASK-005-BR3 | Empty comments list must return 200 | Processing Flow step 10, Business & Workflow Rules, Errors & Tests |
| US-ASK-005-VR1 | askId must be valid | Processing Flow step 5, Business & Workflow Rules |
| US-ASK-005-VR2 | version, if provided, must be valid | Processing Flow step 6, Business & Workflow Rules |
| US-ASK-005-VR3 | askId must exist | Processing Flow step 7, Data Impact |
| US-ASK-005-ER1 | Return 404 if askId is not found | Processing Flow step 7, Errors & Tests |
| US-ASK-005-ER2 | Return 200 with empty comments array when no comments exist | Processing Flow step 10, Errors & Tests |
| US-ASK-005-DEP1 | Comment store | Data Impact, Events & Integrations |
| US-ASK-005-DEP2 | Core ASK existence validation | Processing Flow step 7, Data Impact |
| US-ASK-005-DEP3 | Record access authorization | Authorization, Processing Flow step 8 |
| US-ASK-005-AS1 | Comment ordering is not explicitly defined | Assumptions & Open Items (Open Item 6) |
| US-ASK-005-AS2 | Endpoint is read-only and returns stored metadata only | Events & Integrations, Assumptions & Open Items |

**Total Requirements Traced:** 17

### Acceptance Criteria Traced

| Acceptance Criteria | LLD Section | Test Scenario |
|---|---|---|
| AC1: Given a valid Core ASK exists with active comments, returns 200 with comments list | Processing Flow step 10, Errors & Tests | Test Scenario 1 |
| AC2: Given draft or inactive comments in storage, only isDraft=false and isActive=true are returned | Processing Flow step 9, Business & Workflow Rules | Test Scenario 2 |
| AC3: Given no active non-draft comments, returns 200 with empty array | Processing Flow step 10, Business & Workflow Rules | Test Scenario 3 |
| AC4: Given no Core ASK exists for askId, returns 404 | Processing Flow step 7, Errors & Tests | Test Scenario 5 |

**Total Acceptance Criteria Traced:** 4

---

## Validation Findings

**Finding 1: Exact Permission Code Undefined**
- **Type:** Missing Implementation Detail
- **Description:** The story does not define the permission code required to call GET `/core-asks/{askId}/comments`. No value such as `ASK.READ` or equivalent is specified in the story or the US RBAC 001 matrix reference.
- **Impact:** Authorization policy cannot be implemented; the endpoint cannot be secured correctly.
- **Recommendation:** Security to confirm the exact permission code and update the US RBAC 001 matrix before implementation begins.

**Finding 2: BU Scope Rule Undefined**
- **Type:** Missing Business Rule Detail
- **Description:** The story does not define whether the caller must belong to the same business unit as the ASK owner to retrieve comments (US-ASK-005-DEP3 references record access authorization without specifying BU scope).
- **Impact:** Record-level authorization cannot be fully implemented.
- **Recommendation:** Security and Business Analysis to confirm BU scope requirements for this read operation.

**Finding 3: requestId Field Not in Approved Entity List**
- **Type:** Missing Implementation Detail
- **Description:** The story response schema (US-ASK-005-FR4) includes `requestId` (integer, e.g. 12345), but no `requestId` field is listed in any approved ASK aggregate entity in MLLD 9.2 (`Ask`, `AskVersion`, `AskResource`, `AskComment`, `AskAttachmentLink`).
- **Impact:** The source table and entity for `requestId` cannot be determined; the field cannot be mapped or queried.
- **Recommendation:** Solution Architecture to confirm the source entity and column for `requestId`, or remove the field from the contract if it is not applicable.

**Finding 4: moduleTypeId Field Not in Approved Entity List**
- **Type:** Missing Implementation Detail
- **Description:** The story response schema (US-ASK-005-FR4) includes `moduleTypeId`, but no `moduleTypeId` field is listed in any approved ASK aggregate entity in MLLD 9.2. Its reference data source and valid values are also undefined.
- **Impact:** The source table, entity, and valid values for `moduleTypeId` cannot be determined; the field cannot be mapped, queried, or validated.
- **Recommendation:** Solution Architecture to confirm the source entity and reference data for `moduleTypeId`, or remove the field from the contract if it is not applicable.

**Finding 5: Path Prefix and Route Conflict**
- **Type:** Missing Implementation Detail
- **Description:** The story specifies the endpoint as `/core-asks/{askId}/comments` (US-ASK-005-FR1), while the MLLD 15 proposed catalogue uses `/api/v1/asks/{id}` as the base path with no `/core-asks` sub-path listed. The final approved route is unconfirmed.
- **Impact:** The implemented route may not match the approved API catalogue, causing integration failures.
- **Recommendation:** Solution Architecture to confirm the final approved path including prefix before implementation.

**Finding 6: askId Type Conflict (Integer vs GUID)**
- **Type:** Missing Implementation Detail
- **Description:** The story API Details specify `askId` as an integer, while MLLD 9.3 specifies GUID or approved database-generated key as the primary key convention. The correct type is unconfirmed.
- **Impact:** The path parameter type, entity mapping, and EF Core query cannot be finalized.
- **Recommendation:** Solution Architecture to confirm the approved primary key type for the ASK entity before implementation.

**Finding 7: Comment Sort Order Undefined**
- **Type:** Missing Business Rule Detail
- **Description:** The story explicitly states that comment ordering is not defined (US-ASK-005-AS1). No sort order for the `comments` response array is specified.
- **Impact:** The implementation will produce an indeterminate order, which may cause inconsistent UI behaviour.
- **Recommendation:** Business Analysis to define the required sort order (e.g. `createdOn` ascending or descending) before implementation.

**Finding 8: version Filter Semantics Undefined**
- **Type:** Missing Business Rule Detail
- **Description:** The story accepts an optional `version` query parameter (US-ASK-005-FR3) but does not define whether it filters by `AskVersion.version` or a comment-level version field within `AskComment`.
- **Impact:** The query predicate for version filtering cannot be implemented correctly.
- **Recommendation:** Business Analysis to confirm the version filter semantics and the entity field it maps to.

**Finding 9: Approved RBAC Matrix Row Not Confirmed**
- **Type:** Missing Implementation Detail
- **Description:** The US RBAC 001 matrix row confirming which roles may call this endpoint has not been confirmed. The story references record access authorization (US-ASK-005-DEP3) without specifying role names.
- **Impact:** Authorization tests (allow and deny paths) cannot be written without confirmed role assignments.
- **Recommendation:** Security to confirm the RBAC matrix row for this endpoint before implementation and testing.

**Finding 10: OpenAPI Operation ID and Schema Names Unconfirmed**
- **Type:** Missing Implementation Detail
- **Description:** The proposed `operationId` (`getCoreAskComments`), request schema (`GetCoreAskCommentsRequest`), and response schema (`GetCoreAskCommentsResponse`) are proposed values from the plan and have not been approved in a published OpenAPI contract.
- **Impact:** Code generation and contract testing cannot be finalized against an unconfirmed OpenAPI specification.
- **Recommendation:** API Contract Owner to publish and approve the OpenAPI operation and schema names before implementation.

**Total Validation Findings:** 10

---

## Audit Summary

**Document Version:** 1.0
**Generated By:** DRT Design Card Writer
**Story ID:** US-ASK-005 (ADO 1856)
**Module/Functionality:** ASK - Retrieve Core ASK Comments
**Status:** Ready for Review
**Requirements Coverage:** 17 requirements traced (100%); 4 acceptance criteria traced (100%)
**Validation Findings:** 10 findings; 4 assumptions; 10 open items
**Knowledge Base Utilisation:** DC Card Templates (section contract, size budgets, writing rules, entity names, error codes); DRT Master LLD v0.1 (MLLD 8.1–8.3, 8.5, 9.2, 9.3, 12.2, 12.3, 15 — authorization model, entity list, error catalogue, telemetry, proposed API catalogue); DRT-ProCode Coding Standards (Sections 17, 28, 29, 34, 35, 40, 41 — EF Core query, authorization, error response, async, cancellation, performance, security)
**Architecture Compliance:**
- Dependency direction: Read-only query follows Controller → Application Query → Repository → EF Core → Azure SQL per Coding Standards Section 50.
- Authorization: Entra token validation, DRT actor resolution, permission and BU scope checks per MLLD 8.1–8.3.
- Persistence: `AsNoTracking` projection on `Ask` and `AskComment` entities; no SaveChangesAsync; no transaction required per Coding Standards Section 17.
- Workflow: Not applicable; no state transition, task, or outbox event per US-ASK-005-AS2.
- Errors: Problem Details format with stable error codes from MLLD 12.2; no internal detail exposed per Coding Standards Section 29.
- Security: Input validation, authentication, authorization, parameterized data access, and structured telemetry without secrets per Coding Standards Section 41.
**Readiness for Code Generation:** Conditional. Ten validation findings must be resolved before implementation can be completed without invention. Blocking items: permission code (Finding 1), BU scope rule (Finding 2), requestId source (Finding 3), moduleTypeId source (Finding 4), confirmed route (Finding 5), askId type (Finding 6), version filter semantics (Finding 8).

---

## Document Control

**Document Owner:** DRT Solution Architecture
**Approval Required From:** Development Lead, Security Lead, Business Analyst
**Review Cycle:** Architecture Review, Development Review, Security Review
**Effective Date:** Pending Approval
