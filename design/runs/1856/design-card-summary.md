# Design Card Review Summary

**Run:** design/runs/1856  
**Workflow:** LLD  
**Stage:** design_card_reviewer_publisher  
**Ticket:** ADO 1856 – US-ASK-005 – Core ASK API – Retrieve Core ASK Comments  
**Iterations:** 1  
**Score:** 93 / 100 (threshold: 100)  
**Status:** NEEDS_REVIEW  
**Result:** BLOCKED  

---

## Cards Reviewed

| Card ID | Type | Method | Path | Complexity | Checker | Card Result | Promotion |
|---|---|---|---|---|---|---|---|
| ASK-GET-CORE-ASKS-ID-COMMENTS | api | GET | /core-asks/{askId}/comments | Simple | FAIL | FAIL | NOT_PROMOTED |

---

## Checker Results

### ASK-GET-CORE-ASKS-ID-COMMENTS
- **Result:** FAIL  
- **Word count:** 3386 (budget: 1500 for Simple)  
- **Tables:** 4 | **Sections:** 13 | **Processing steps:** 11 | **Test scenarios:** 10 | **Error rows:** 8 | **Traceability rows:** 21  
- **Issues:** Output budget: 3386 words exceeds 1500 for Simple.  
- **Warnings:** None  

---

## DC-R1 Checklist

| Item | Result | Reason |
|---|---|---|
| 1. Card ID and metadata match plan | Pass | cardId, method, path, module, storyId, ADO ID all match |
| 2. Checker result PASS | Fail | Checker returned FAIL: 3386 words exceeds 1500-word Simple budget |
| 3. Method and path match story | Pass | GET /core-asks/{askId}/comments matches FR1 |
| 4. Request parameters match story | Pass | askId (required integer path) and version (optional integer query) match story API Details |
| 5. Response schema matches story | Pass | All 11 fields present and match FR4 |
| 6. Business rules covered | Pass | BR1, BR2, BR3 all addressed |
| 7. Validation rules covered | Pass | VR1, VR2, VR3 all covered |
| 8. Error handling covered | Pass | ER1 (404), ER2 (200 empty) and additional error scenarios present |
| 9. Processing steps present and logical | Pass | 11 steps covering full request lifecycle |
| 10. Test scenarios cover ACs | Pass | AC1–AC4 all mapped to test scenarios |
| 11. Traceability table complete | Pass | 17 requirements and 4 ACs traced |
| 12. Open items and differences documented | Pass | 10 open items and 4 differences with owners |
| 13. Audit Summary present | Pass | Present with Conditional readiness and numbered findings |
| 14. No prohibited content | Pass | No secrets, tokens, or prohibited content |

**Applicable items:** 14 | **Pass:** 13 | **Fail:** 1 | **Score:** 93

---

## Feedback

### ASK-GET-CORE-ASKS-ID-COMMENTS

**Section:** API Identification / Card-level  
**Problem:** The draft is 3386 words, which exceeds the 1500-word budget for Simple complexity by 1886 words. The Design Card Checker returned FAIL on this basis.  
**Fix:** Condense all sections to fit within the 1500-word Simple budget. Reduce verbose prose in Processing Flow, Business & Workflow Rules, Validation Findings, and Test Scenarios to concise bullet points or table rows. Remove duplicated content between sections.

---

## Open Points

1. ASK-GET-CORE-ASKS-ID-COMMENTS: Exact permission code required to call this endpoint (e.g. ASK.READ) not defined – owner: Security
2. ASK-GET-CORE-ASKS-ID-COMMENTS: BU scope rule for comment retrieval not defined – owner: Security
3. ASK-GET-CORE-ASKS-ID-COMMENTS: Approved OpenAPI operation ID and schema component names unconfirmed – owner: API Contract Owner
4. ASK-GET-CORE-ASKS-ID-COMMENTS: Confirmed route and path prefix (/api/v1 vs /core-asks) unresolved – owner: Solution Architecture
5. ASK-GET-CORE-ASKS-ID-COMMENTS: Confirmed primary key type for askId (integer vs GUID) unresolved – owner: Solution Architecture
6. ASK-GET-CORE-ASKS-ID-COMMENTS: Comment sort order undefined – owner: Business Analysis
7. ASK-GET-CORE-ASKS-ID-COMMENTS: requestId field source entity/table unconfirmed – owner: Solution Architecture
8. ASK-GET-CORE-ASKS-ID-COMMENTS: moduleTypeId field source entity/table and reference data unconfirmed – owner: Solution Architecture
9. ASK-GET-CORE-ASKS-ID-COMMENTS: version query parameter filter semantics (AskVersion.version vs comment-level version) undefined – owner: Business Analysis
10. ASK-GET-CORE-ASKS-ID-COMMENTS: Approved RBAC matrix row for this endpoint not confirmed – owner: Security

---

## Next Stage

The card must be revised to reduce word count to within the 1500-word Simple budget before it can be promoted. Resubmit for review after revision.

---

*Evaluation record:* design/runs/1856/design-card-evaluation.json  
*Report:* design/runs/1856/design-card-summary.md  
