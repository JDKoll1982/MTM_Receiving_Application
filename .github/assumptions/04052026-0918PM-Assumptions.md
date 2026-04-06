# Assumptions For Multi-Transfer Location Reconciliation

Date: 2026-04-05 09:18 PM

This file is required before implementation because the requested behavior introduces several major assumptions about how saved MTM receiving rows should be matched to multiple downstream transfer locations.

## 1. Reconciliation should allocate locations per saved MTM load row, not per part/PO as a whole

Why this assumption is needed:
The current workflow reconciles one saved MTM row to one proposed location using aggregated evidence for a part, PO, optional line, and date. Your request describes one receipt being split across multiple transferred destinations and expects each individual saved coil/load row to land in the best matching destination.

Potential impact if wrong:
If reconciliation should stay aggregated at the part/PO level, rewriting it to allocate per load row would overcomplicate the feature, change the review experience, and potentially assign different locations than operators expect.

Alternative interpretations considered:
1. Keep the current one-row-to-one-location behavior and only improve the evidence explanation.
2. Allocate one final location per part/PO using the largest or latest destination.
3. Allocate each saved MTM row independently by quantity and evidence.

Proposed assumption:
Implement option 3. Each saved MTM load row should be treated as its own candidate for reconciliation and should be matched to a destination location independently.

## 2. Matching should use a greedy quantity-allocation strategy with exact and closest-fit preference

Why this assumption is needed:
"Place each one in the proper location as close as possible" does not define a precise allocation algorithm when multiple transfer rows and multiple saved MTM rows exist.

Potential impact if wrong:
Different matching strategies can produce different destination assignments for the same saved rows, especially when quantities do not match perfectly. A wrong strategy could silently place rows in the wrong location or create operator distrust in the reconciliation review.

Alternative interpretations considered:
1. Match rows strictly by chronology only.
2. Match rows strictly by exact quantity only and leave all others unresolved.
3. Match rows greedily by strongest evidence using this order: exact quantity match first, otherwise closest remaining quantity that does not exceed available evidence when possible, otherwise nearest quantity overall, and leave ties unresolved.
4. Use a full optimization algorithm across all rows and locations.

Proposed assumption:
Implement option 3. Use a deterministic greedy allocator that prefers exact quantity matches first, then the closest remaining quantity, and leaves ambiguous ties unresolved for manual review.

## 3. The evidence query should return multiple transfer rows and multiple current-location candidates instead of only the latest transfer summary row

Why this assumption is needed:
The current `16_GetReceivingLocationEvidence.sql` query returns a single latest transaction location plus a flat list of current inventory locations. That is not enough to allocate individual saved rows across multiple transfer destinations.

Potential impact if wrong:
If the SQL contract stays single-row for transfer evidence, the app cannot reliably distinguish one split transfer from another and will continue collapsing the evidence into one location proposal.

Alternative interpretations considered:
1. Keep the current query shape and infer allocations from current inventory only.
2. Extend the query to return all relevant transfer-location evidence rows, including quantities and dates, so the app can allocate rows correctly.

Proposed assumption:
Implement option 2. Update the query and consuming model/service so reconciliation receives multiple transfer-location evidence rows, not just the latest one.

## 4. Mock-data generation should create one destination transfer record per saved MTM load when more than one load exists

Why this assumption is needed:
Your request explicitly asks for the earlier mock-data creation work to support multiple locations when more than one saved load exists.

Potential impact if wrong:
If mock data still generates only one destination per part/PO, the new reconciliation behavior cannot be tested meaningfully in mock mode and the UI will appear to support a feature that the seeded data cannot exercise.

Alternative interpretations considered:
1. Keep one mock destination per part/PO and only improve live reconciliation.
2. Generate multiple mock destination transfers whenever multiple saved loads exist, with quantities split across distinct locations.

Proposed assumption:
Implement option 2. When more than one saved MTM load exists for the same receipt group, generate multiple mock transfer rows and distinct mock destination locations so reconciliation can allocate each saved load row separately.

## 5. The review UI should remain a per-row review flow but display quantity-allocation evidence more explicitly

Why this assumption is needed:
The current review dialog is already row-based with Save and Ignore actions. Your request says "also update UI accordingly" but does not specify whether the review should become bulk, grid-based, or stay one-row-at-a-time.

Potential impact if wrong:
Changing to a radically different review surface would increase scope and regression risk. Keeping the current row-based flow but not surfacing the new evidence clearly enough could make the new matching logic hard to trust.

Alternative interpretations considered:
1. Replace the modal with a multi-row grid review.
2. Keep the modal but add per-row quantity-allocation details such as matched transfer quantity, destination location, and why that location was selected.

Proposed assumption:
Implement option 2. Keep the existing review flow and extend it to show the quantity-based destination evidence for each proposed row.

## Request For Confirmation

Please confirm or correct the following before implementation continues:

1. Reconciliation should allocate locations per saved MTM load row, not just one location per part/PO.
2. The matching algorithm should use a deterministic greedy strategy: exact quantity match first, then closest-fit, with ambiguous ties left unresolved.
3. `16_GetReceivingLocationEvidence.sql` should be expanded to return multiple transfer evidence rows instead of only the latest transfer summary row.
4. Mock-data generation should create multiple destination locations and transfer rows when more than one saved load exists.
5. The reconciliation review UI should stay row-based but show stronger quantity-allocation evidence.

If you want a different matching rule, reply with the exact rule you want. Examples:

- "Match strictly by chronology only"
- "Only allow exact quantity matches"
- "Never split one saved row across multiple locations"
- "Use the largest remaining destination quantity first"