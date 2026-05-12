# Copilot Prompt — Reconciliation Location Fix

**Primary target files:**
- `Module_Receiving/Services/Service_ReceivingLocationReconciliation.cs`
- `Database/InforVisualScripts/Queries/18a_GetTransferCandidatePairs.sql`
- `Database/InforVisualScripts/Queries/18b_GetTransferPairDetails.sql`

---

## Context

The Receiving Location Reconciliation feature
(`Service_ReceivingLocationReconciliation`) compares saved MTM receiving rows
against Infor Visual transfer movements to propose correct warehouse
locations. Several related defects and one design decision need to be encoded
in the implementation prompt below. Do **not** change any other behavior,
tests, or files unless they directly implement the fixes described here.

---

## Fix 1 — PO Normalization Must Accept Blanket-Order Suffix

### Problem

Inside `ReconcileRowsAsync`, a row is skipped with the reason
`"PO number could not be normalized to Infor Visual format."` when
`NormalizeInforVisualPoNumber` returns an empty string.

The current `CanonicalPoPattern` regex is:

```csharp
@"^(?:PO-)?(?<digits>\d{1,6})(?<suffix>[A-Za-z]?)$"
```

This regex already captures one optional letter suffix, but the normalized
output string built from it is:

```csharp
return $"PO-{digits}{suffix}";
```

The suffix capture group is already present in the pattern, so a PO like
`PO-061555B` *should* normalize to `PO-061555B`.  Verify that the full
round-trip — pattern match → `NormalizeInforVisualPoNumber` output → skip
guard — works correctly for all of the following inputs:

| Raw input     | Expected normalized output |
|---------------|---------------------------|
| `61555`       | `PO-061555`               |
| `PO-61555`    | `PO-061555`               |
| `061555B`     | `PO-061555B`              |
| `PO-061555B`  | `PO-061555B`              |
| `61555b`      | `PO-061555B`              |

If the normalization or the skip guard is silently dropping rows with a
blanket-order suffix (any single A–Z/a–z letter), fix it so those rows are
**not** skipped.  The suffix must be uppercased in the output.

The `CanonicalPoPattern` regex field and `NormalizeInforVisualPoNumber` method
are the only members that may need to change for this fix.

---

## Fix 2 — Strict Transfer Validation Must Use a Date-Scoped Reference Total

### Problem

`BuildTransferBuckets` computes a net quantity for each destination location:

```
NetQuantity = total inbound to location − total outbound from location
```

`ResolveTransferBasedGroup` then allocates rows to buckets using a greedy
exact-fit / best-fit pass. The current prompt describes a strict validation
that compares the allocated row total to `bucket.NetQuantity`. That is wrong
when transfer discovery intentionally uses a ±2 day window.

Example:

- Day 0 receipt for part `MMC0000500`: 3 000 EA total.
- Day 0 transfer to `V-A0-01`: 3 000 EA.
- Day 1 receipt for the same part: 1 000 EA total.
- Day 1 transfer to `V-A0-01`: 1 000 EA.

When reconciling the Day 0 group, the movement search correctly returns both
Day 0 and Day 1 transfers because Day 1 is inside the ±2 day discovery window.
If the strict check compares against `bucket.NetQuantity`, the reference total
for `V-A0-01` becomes 4 000 EA instead of the Day 0 receipt's intended total of
3 000 EA, and the Day 0 rows are falsely rejected.

### Required behaviour

After the greedy allocation loop finishes for a group, add a **strict totals
validation pass** that uses a **date-scoped reference total** rather than the
full ±2 day bucket total:

1. For every bucket that had at least one row allocated to it, compute the
  reference transfer quantity by filtering `relevantMovements` down to only
  movements whose `TransactionDate.Date` falls on:

  - `receivedDate.Date`
  - or `receivedDate.Date.AddDays(1)`

  Then sum only those filtered movements that contribute to that bucket.

2. Assert:

   ```
  sum(SavedRowQuantity of all rows assigned to this bucket) == dateScopedReferenceQuantity
   ```

3. If any bucket fails this check, **reassign all rows that were allocated to
   that bucket back to `Ambiguous`** with a `Details` message in this form
   (use `CultureInfo.InvariantCulture` for all numeric formatting):

   ```
   Allocation rejected: rows assigned to {bucket.DisplayLocation} sum to
   {allocatedSum:0.##} {unitOfMeasure} but the confirmed transfer quantity
  to that location for the receipt date window is
  {dateScopedReferenceQuantity:0.##} {unitOfMeasure}.
   The full transferred quantity must be consumed by the allocated rows.
   ```

4. Do **not** re-attempt allocation for those rows. Leave them unresolved
   so the user can handle them manually during the review step.

5. Rows that were allocated to buckets that *do* pass the strict check keep
   their `Updated` or `Unchanged` resolution — do not disturb them.

### Important constraint

This is a **pure C# fix** inside `ResolveTransferBasedGroup`. Do **not** create
an extra SQL file to calculate exact-day totals. The existing movement set is
already available in memory; the strict reference total must be derived from
that in-memory movement list after the ±2 day discovery step has completed.

### Why this matters

Partial consumption of a bucket would mean the app is claiming a skid went to
a location when Infor Visual actually moved more material there than the app
can account for.  Only a full, exact match gives the warehouse staff confidence
that the proposed location is correct. The reference quantity must also be
scoped to the specific receipt date window so that consecutive-day receipts of
the same part do not poison each other's validation.

---

## Fix 3 — Reconciliation Must Be Cross-Pass Aware For Partial / Incremental Batches

### Decision

Use **Option A — Cross-pass awareness**.

### Problem

`PreviewLocationsAsync` currently reconciles Current Labels and History in
separate passes. That breaks incremental reconciliation.

Example:

- Five skids of the same part are received on Day 0, 1 000 EA each.
- Infor Visual transfers 5 000 EA to `V-A0-01`.
- Session A already reconciled three skids, so those rows now live in History.
- Two skids still remain in Current Labels.

If History rows and Current Label rows are reconciled separately:

- History pass sees only 3 000 EA and fails the strict check against 5 000 EA.
- Current Labels pass sees only 2 000 EA and also fails the strict check.

Both subsets become `Ambiguous` even though the combined batch is correct.

### Required behaviour

Before the strict reconciliation pass is performed, merge Current Labels and
History rows that belong to the same logical reconciliation group into a single
allocation group keyed by the same part and received-date grouping rules used
by the reconciliation engine.

1. `PreviewLocationsAsync` must become cross-pass aware.
2. Current Labels and History rows for the same `(partID, receivedDate)` group
  must be reconciled together, not in isolated passes.
3. The strict date-scoped validation described in Fix 2 must run against the
  combined allocation group.
4. The final preview output can still preserve the correct source-row identity
  and origin, but the allocation decision must be made from the merged group.

### Why this matters

Without cross-pass awareness, already-correct rows can flip to `Ambiguous`
solely because the batch was reconciled incrementally across sessions.

---

## Fix 4 — Split Transfer Lookup Into 18a And 18b

### Required behaviour

Split the Infor Visual transfer retrieval workflow into two SQL files:

1. `18a_GetTransferCandidatePairs.sql`
  - Finds candidate source/destination transaction ID pairs for a part and the
    existing ±2 day discovery window.
  - Returns integer `(SourceTransactionId, DestinationTransactionId)` pairs.

2. `18b_GetTransferPairDetails.sql`
  - Receives those candidate source transaction IDs from C#.
  - Hydrates them into the full transfer movement rows used by the existing
    reconciliation logic.

### Parameter passing requirement

Pass the IDs from C# to 18b using a dynamically constructed `IN (...)` clause.

- Do **not** use a table-valued parameter.
- Do **not** use a `#temp` table.
- An `IN` clause is acceptable here because the candidate set size is expected
  to remain small for this workflow.

The generated SQL for 18b should follow the shape:

```sql
WHERE src.TRANSACTION_ID IN (1,2,3,...)
```

### Why this matters

The split makes the candidate-pair discovery step and the detail hydration step
independently testable in SSMS while keeping the runtime design straightforward.

---

## Fix 5 — Preserve The Existing Transfer-Pair Assumption But Keep It Testable

### Existing assumption

The transfer pairing logic assumes the lower `TRANSACTION_ID` is the source and
the higher `TRANSACTION_ID` is the destination.

### Required behaviour

Do not redesign this assumption as part of this change. Keep the current logic
intact, but ensure the 18a / 18b split leaves the pairing behavior observable
and easy to test independently.

### What must not change

- The sort order used during the greedy pass
  (`quantity desc → load_number → load_id`) must stay the same.
- The `SelectRelevantMovements` chained-walk logic must stay the same.
- The `BuildTransferBuckets` net-quantity calculation must stay the same.
- The ±2 day discovery window used to find candidate transfers must stay the
  same.
- The `NotFound`, `IgnoredBySettings`, and `Skipped` resolution paths must
  stay the same.
- The `ApplyLocationUpdateAsync` write path must stay the same.

---

## Acceptance Criteria

### Fix 1 — PO normalization

- A row with `po_number = "PO-061555B"` (or `"061555b"`, `"61555B"`) is **not**
  skipped; it is included as a valid row for reconciliation.
- The normalized key used internally is `"PO-061555B"` (digits zero-padded to
  six, suffix uppercased).

### Fix 2 — Strict bucket totals

Given:
- Part `MMC0000500`, one reconciliation group, four rows: 1 000, 1 000,
  1 000, 1 000.
- Relevant movements found through the ±2 day search include only a Day 0
  transfer of 4 000 EA into `V-A0-00` for the receipt being reconciled.

Expected result after the strict check:
- All four rows are `Updated` (or `Unchanged`) pointing to `V-A0-00`, because
  `1 000 + 1 000 + 1 000 + 1 000 = 4 000 = dateScopedReferenceQuantity`.
  The bucket passes the strict check.

Given:
- Same part, same bucket, same receipt date window, reference quantity = 4 000.
- Three rows: 1 000, 1 000, 1 000 (total = 3 000 ≠ 4 000).

Expected result:
- All three rows are reassigned to `Ambiguous` because the allocated sum
  (3 000) does not equal the receipt-date-scoped transfer total (4 000).

Given:
- Day 0 receipt rows total 3 000 EA for part `MMC0000500`.
- Day 0 transfer to `V-A0-01` = 3 000 EA.
- Day 1 transfer to `V-A0-01` = 1 000 EA.
- The ±2 day movement search returns both transfers.

Expected result:
- The Day 0 reconciliation group validates against 3 000 EA, not 4 000 EA.
- The Day 1 movement does not contaminate the Day 0 strict check.

### Fix 3 — Cross-pass awareness

Given:
- Five Day 0 rows for the same part, each 1 000 EA.
- Three rows currently live in History because they were reconciled earlier.
- Two rows still live in Current Labels.
- Infor Visual confirms one 5 000 EA transfer to `V-A0-01`.

Expected result:
- The service reconciles all five rows as one logical group before strict
  validation.
- The combined allocated sum is 5 000 EA.
- Already-correct rows do not flip to `Ambiguous` just because the batch spans
  Current Labels and History.

### Fix 4 — SQL split

- `18a_GetTransferCandidatePairs.sql` is created and returns source/destination
  transaction ID pairs.
- `18b_GetTransferPairDetails.sql` is created and hydrates those IDs into full
  movement rows.
- No `18c_GetExactDayTransferTotals.sql` file is created.

---

## Files to Change

| File | Change |
|------|--------|
| `Module_Receiving/Services/Service_ReceivingLocationReconciliation.cs` | Apply Fix 1, Fix 2, and the Option A cross-pass grouping change |
| `Database/InforVisualScripts/Queries/18a_GetTransferCandidatePairs.sql` | Add candidate transfer-pair lookup query |
| `Database/InforVisualScripts/Queries/18b_GetTransferPairDetails.sql` | Add detail hydration query driven by C# `IN (...)` IDs |

Do **not** create `18c_GetExactDayTransferTotals.sql`. The exact-day strict
reference total belongs in C# inside `ResolveTransferBasedGroup`.

No other files should need to change unless a unit test directly exercises the
scenarios above and needs updating to match the new behaviour.
