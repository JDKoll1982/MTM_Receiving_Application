Reconcile Saved Locations --- Module Specification
Purpose: Convert the handwritten planning notes into an
implementation-oriented specification for a VS Code AI coding agent.

Source: SProd Offic26111208330.pdf (2 pages, handwritten notes).

Status: Working specification. The workflow below preserves the
source notes; items marked \(VERIFY\) are handwriting ambiguities
or decisions not fully specified in the source.

1. Goal
Create a standalone module named Module_Recon by moving "Reconcile
Saved Locations from InfoInVisual" out of Module_Receiving and into
its own module.

The new module will reconcile saved receiving locations against
subsequent Visual inventory transactions for a selected date range.
2. Repository cleanup
Remove reconcile-related artifacts from the existing repository before
or as part of the module separation:
- Reconcile-related documents
- Reconcile-related code
- Reconcile-related memories
- Reconcile-related tests
- Reconcile-related SQL for both Visual and MySQL
Important: Limit removal to artifacts that are specifically related
to the reconcile feature. Do not remove unrelated Receiving
functionality or shared dependencies.
3. User workflow
1. The user opens the new Module_Recon module.
2. The user supplies the desired reconciliation date range.
3. The module identifies receiving records and groups/matches them by:
   - Purchase Order (PO)
   - Part Number
   - Total quantity received for each matching line / combination
4. The module evaluates subsequent inventory transactions in Visual for
   the matching material.
5. The module determines whether the quantity received has been fully
   accounted for at the material's current and/or subsequent locations.
6. If the reconciliation conditions are met, the module updates the
   corresponding receiving application's saved location(s) to the
   location(s) where the material was used.
4. Example scenario from the notes
The handwritten notes provide this example:
1. A user receives material in Visual:
   - PO: PO-073026
   - Line: 9
   - Part Number: MMC0000654 \(VERIFY exact part number\)
   - Quantity: 25,240#
   - Receiving location: RECV
2. The user creates four labels for the material. The handwritten
   quantities appear to be:
   - 8,200
   - 8,200
   - 4,420
   - 4,420 \(VERIFY: the final handwritten quantity is
     unclear\)
3. The user later transfers material in Visual:
   - 16,400 to X-CO-07
   - The remaining quantity to X-CO-05
4. The user clicks Update Locations in the new reconciliation
   module.
5. The module retrieves the receiver transaction associated with the
   PO. The handwritten transaction number appears to be 594488
   \(VERIFY\).
6. The module checks the material's current locations in Visual.
7. It checks whether the Receiving App and Visual locations match.
8. If they match, no update is needed; continue to the next receiver
   within the selected date range.
9. If they do not match, inspect the transaction history for the
   selected date range and calculate how much of the part number was
   taken from the original location recorded on the receiver.
5. Reconciliation algorithm
Implement the following workflow for each receiver / PO / part-number
combination in the selected date range.
5.1 Identify the receiver
- Retrieve the receiver record / transaction for the PO and part
  number.
- Capture the receiver's original location.
- Capture the quantity received.
- Retain the PO and part-number combination so that later Receiving
  App lines can be matched to the correct receipt.
5.2 Check current locations
- Query Visual for the current location(s) of the material.
- Compare the location information in the Receiving App with the
  location information in Visual.
- If the locations already match:
  - Do not change the Receiving App.
  - Continue to the next receiver in the selected date range.
5.3 Review transaction history when locations do not match
- Query transaction history for the selected date range only.
- Review transactions for the relevant part number and the receiver's
  original location.
- Build an array/list containing the relevant transactions.
- Tally the total quantity represented by those transactions. The
  handwritten wording appears to say "total Qty of assy" \(VERIFY
  intended meaning; possibly "assembly" or another domain term\).
- Compare the calculated total with the quantity received on the
  receiver.
5.4 Update Receiving App locations
If the calculated total equals the quantity received on the receiver:
1. Find each Receiving App line matching the same Part Number + PO
   combination within the selected date range.
2. Determine the location where the material was used from the
   corresponding Visual transaction history.
3. Update each matching Receiving App line's saved location to the
   location where that material was used.
If the calculated total does not equal the receiver quantity, the source
notes do not define what action to take. Do not silently update
locations in this case. Treat it as an unresolved reconciliation and
report or flag it for review until a business rule is supplied.
6. Pseudocode
function updateLocations(dateRange):
    receivers = getReceivers(dateRange)

    for each receiver in receivers:
        po = receiver.purchaseOrder
        partNumber = receiver.partNumber
        receivedQty = receiver.quantityReceived
        originalLocation = receiver.originalLocation

        visualLocations = getCurrentVisualLocations(partNumber)
        receivingLocations = getReceivingAppLocations(po, partNumber, dateRange)

        if locationsMatch(receivingLocations, visualLocations):
            continue

        transactions = getVisualTransactionHistory(
            partNumber,
            originalLocation,
            dateRange
        )

        transactionQty = sumRelevantTransactionQuantities(transactions)

        if transactionQty == receivedQty:
            for each receivingLine in getReceivingLines(po, partNumber, dateRange):
                usedLocation = determineLocationWhereMaterialWasUsed(
                    receivingLine,
                    transactions
                )
                updateReceivingLineLocation(receivingLine, usedLocation)
        else:
            flagForReview(
                receiver,
                receivedQty,
                transactionQty
            )
This pseudocode expresses the intended flow, not confirmed repository
method names, database schema, or transaction-sign conventions.
Inspect the existing codebase and Visual / MySQL integration before
implementing it.

7. Implementation requirements for the coding agent
1. Inspect before changing: Examine the repository structure and
   identify the existing Module_Receiving reconcile implementation,
   related UI, data access, SQL, tests, and documentation.
2. Separate the module: Move the feature into Module_Recon; keep
   Receiving-specific functionality in Module_Receiving.
3. Trace dependencies: Identify which Visual and MySQL queries,
   models, services, and helpers are reconcile-specific versus shared.
   Do not delete shared code.
4. Preserve existing behavior where applicable: Use the existing
   application conventions for UI, database access, error handling,
   logging, and naming.
5. Date-range scope: All receiver and transaction-history matching
   must respect the user-selected date range as described above.
6. Match on both identifiers: Use the PO + Part Number combination
   when matching Receiving App lines.
7. Quantity validation: Do not update locations unless the
   transaction quantity satisfies the receiver quantity condition.
8. Safe failure behavior: If quantities do not reconcile, records
   are ambiguous, or a destination location cannot be determined, leave
   the affected Receiving App records unchanged and make the issue
   visible to the user.
9. Avoid unrelated cleanup: Remove only reconcile-specific
   documents, code, memories, tests, and SQL from the old location.
10. Tests: Add or update tests for matching locations, mismatched
    locations, quantity equality, quantity mismatch, multiple
    destination locations, date-range boundaries, and multiple receipts
    for the same part number.
8. Acceptance criteria
- [ ] A dedicated Module_Recon exists.
- [ ] "Reconcile Saved Locations from InfoInVisual" is no longer
  housed in Module_Receiving.
- [ ] Reconcile-specific documents, code, memories, tests, and Visual
  / MySQL SQL are removed from the old module/repository locations
  where appropriate.
- [ ] The user can specify a reconciliation date range.
- [ ] Receivers are evaluated using PO, part number, received
  quantity, and original location.
- [ ] The module checks current Visual locations against Receiving App
  saved locations.
- [ ] Matching locations are left unchanged.
- [ ] Mismatched locations trigger a date-range-limited
  transaction-history review.
- [ ] Transaction quantities are tallied and compared with the
  receiver quantity.
- [ ] When quantities reconcile, matching Receiving App lines are
  updated to the locations where the material was used.
- [ ] Unreconciled or ambiguous cases are not automatically changed
  and are surfaced for review.
- [ ] Automated tests cover the key reconciliation paths.
9. Open questions / items to verify
Resolve these against the source author or existing application behavior
before finalizing implementation:
1. Receiver source: What exact table, API, or application data
   source supplies the receiver transaction?
2. Receiving App source: What table or data source stores the saved
   locations that must be updated?
3. Transaction quantity semantics: Are transfer quantities stored
   as positive values, negative values, or separate issue/receipt
   transaction types? Which transaction types count toward the tally?
4. Quantity comparison: Should equality be exact, or is a tolerance
   required for weight / rounding?
5. Transaction tally meaning: The notes say "total Qty of assy."
   What exactly is "assy" in this context?
6. Multiple destinations: If a receipt is split across multiple
   used locations, should each matching Receiving App line be assigned
   based on label / quantity-level traceability, or is another
   allocation rule used?
7. Partial reconciliation: What should happen when the total
   transaction quantity is less than or greater than the received
   quantity?
8. Date fields: Which date controls inclusion in the selected date
   range: receiver date, transaction date, or both?
9. No transaction history: What should happen when locations differ
   but no qualifying transactions are found?
10. Example verification: Confirm the handwritten part number, final
    label quantity, and receiver transaction number in the example.
10. Source fidelity notes
- The source notes explicitly call for a new Module_Recon, a
  user-selected date range, PO / Part Number / received-quantity
  matching, a current-location comparison, a transaction-history
  tally, and updating Receiving App locations when the quantities
  reconcile.
- The source notes do not specify exact database schemas, method
  names, transaction sign rules, or the behavior for quantity
  mismatches. Those are intentionally left as implementation questions
  rather than invented requirements.
- Page 1 contains the goal, repository cleanup, workflow steps, and
  example. Page 2 continues the transaction-array / quantity-tally
  logic and the Receiving App location update step.