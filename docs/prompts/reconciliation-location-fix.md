# Copilot Prompt — Reconciliation Location Fix

**Target file:** `Module_Receiving/Services/Service_ReceivingLocationReconciliation.cs`

---

## Context

The Receiving Location Reconciliation feature
(`Service_ReceivingLocationReconciliation`) compares saved MTM receiving rows
against same-day Infor Visual transfer movements to propose correct warehouse
locations.  Two specific defects need to be corrected.  Do **not** change any
other behavior, tests, or files unless they directly implement one of the two
fixes below.

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

## Fix 2 — Transfer Bucket Total Must Exactly Match Rows Allocated to It

### Problem

`BuildTransferBuckets` computes a net quantity for each destination location:

```
NetQuantity = total inbound to location − total outbound from location
```

`ResolveTransferBasedGroup` then allocates rows to buckets using a greedy
exact-fit / best-fit pass.  After allocation, `RemainingQuantity` on a bucket
can be **greater than zero**, meaning some transferred quantity was never
consumed by any row.  This is incorrect: if Infor Visual transferred 4 000 EA
of part `MMC0000500` to `V-A0-00`, then the rows assigned to that bucket must
sum to exactly 4 000 — no more, no less.

### Required behaviour

After the greedy allocation loop finishes for a group, add a **strict totals
validation pass**:

1. For every bucket that had at least one row allocated to it, assert:

   ```
   sum(SavedRowQuantity of all rows assigned to this bucket) == bucket.NetQuantity
   ```

2. If any bucket fails this check, **reassign all rows that were allocated to
   that bucket back to `Ambiguous`** with a `Details` message in this form
   (use `CultureInfo.InvariantCulture` for all numeric formatting):

   ```
   Allocation rejected: rows assigned to {bucket.DisplayLocation} sum to
   {allocatedSum:0.##} {unitOfMeasure} but the confirmed transfer quantity
   to that location is {bucket.NetQuantity:0.##} {unitOfMeasure}.
   The full transferred quantity must be consumed by the allocated rows.
   ```

3. Do **not** re-attempt allocation for those rows.  Leave them unresolved
   so the user can handle them manually during the review step.

4. Rows that were allocated to buckets that *do* pass the strict check keep
   their `Updated` or `Unchanged` resolution — do not disturb them.

### Why this matters

Partial consumption of a bucket would mean the app is claiming a skid went to
a location when Infor Visual actually moved more material there than the app
can account for.  Only a full, exact match gives the warehouse staff confidence
that the proposed location is correct.

### What must not change

- The sort order used during the greedy pass
  (`quantity desc → load_number → load_id`) must stay the same.
- The `SelectRelevantMovements` chained-walk logic must stay the same.
- The `BuildTransferBuckets` net-quantity calculation must stay the same.
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
- One transfer bucket: `V-A0-00`, `NetQuantity = 4 000`.

Expected result after the strict check:
- All four rows are `Updated` (or `Unchanged`) pointing to `V-A0-00`, because
  `1 000 + 1 000 + 1 000 + 1 000 = 4 000 = NetQuantity`.  The bucket passes
  the strict check.

Given:
- Same part, same bucket (`V-A0-00`, `NetQuantity = 4 000`).
- Three rows: 1 000, 1 000, 1 000 (total = 3 000 ≠ 4 000).

Expected result:
- All three rows are reassigned to `Ambiguous` because the allocated sum
  (3 000) does not equal the bucket total (4 000).

---

## Files to Change

| File | Change |
|------|--------|
| `Module_Receiving/Services/Service_ReceivingLocationReconciliation.cs` | Apply both fixes as described above |

No other files should need to change unless a unit test directly exercises the
two scenarios above and needs updating to match the new behaviour.
