# Phase 1 Data Model: Receiving Validator

**Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md) | **Research**: [research.md](research.md)
**Created**: 2026-11-12

Every type here belongs to `Module_Reconciliation` and to no other module. The two physical tables the module reads live in the Receiving schema, but the module reaches them only through its own data access and its own procedures (`R-MOD-07`, `R-MOD-08`).

Quantities are `decimal` throughout (`R-SCOPE-05`). Identifiers are nullable-free unless a null carries a meaning that is stated.

## Entity Overview

```text
Model_ReconciliationDay
  1 --- * Model_ReconciliationGroup        grouped by key
          1 --- * Model_ReconciliationRow
          1 --- * Model_ReconciliationMovement      selected evidence
          1 --- * Model_ReconciliationDestinationBucket
          1 --- 1 Model_ReconciliationSourceResidual
          1 --- * Model_ReconciliationFinding      one per row, always
Model_ReconciliationRun
  1 --- 1 Model_ReconciliationDaySummary
Enum_ReconciliationOutcome                 carried by every finding
Model_ReconciliationLocationBalance        read-only input to the residual check
```

## Enum_ReconciliationOutcome

The eight outcomes FR-032 permits. No other value exists, and `Ambiguous` and `IgnoredBySettings` are gone.

| Member | Display text | Written to a row | Surfaced in the review cycle | Counted in the day summary |
| --- | --- | --- | --- | --- |
| `Unchanged` | `Unchanged` | no | no | unchanged |
| `Updated` | `Updated` | yes, on accept | yes | written |
| `ExtraSurplus` | `Extra Surplus` | no | no, Needs Attention | unresolved |
| `NotEnough` | `Not Enough` | no | no, Needs Attention | unresolved |
| `NotFound` | `NotFound` | no | no | not-found |
| `Skipped` | `Skipped` | no | no | skipped |
| `Error` | `Error` | no | no, Needs Attention | error |
| `ManuallySet` | `Manually Set` | yes, on accept | yes | written |

Display text is the exact string from the Verbatim Constraints table. It is produced by one converter and nowhere else.

**Rule (FR-033)**: `ExtraSurplus` and `NotEnough` both carry the raw difference as a `decimal` in the finding's reason fields. A finding in either state without a difference is a defect.

**Rule (FR-034)**: `NotFound` is logged with part, day and group, and counted. It is not a Needs Attention item.

## Model_ReconciliationRow

One saved receiving line. This is the unit the operator corrects.

| Field | Type | Source | Notes |
| --- | --- | --- | --- |
| `RowSource` | `Enum_ReconciliationRowSource` | which table | `CurrentLabel` or `History` |
| `LabelDataRecordId` | `int?` | `receiving_label_data.id` | set for current labels |
| `HistoryRecordId` | `int?` | `receiving_history.id` | set for history |
| `LoadId` | `Guid?` | `load_id` | grouping and identity |
| `LoadNumber` | `int?` | `load_number` | allocation tie-break |
| `PartId` | `string` | `part_id` | required |
| `PartDescription` | `string` | `part_description` | detail pane |
| `PurchaseOrderNumber` | `string` | `po_number` | empty for non-purchase-order rows |
| `PurchaseOrderLineNumber` | `string` | `po_line_number` | evidence matching |
| `TransactionDate` | `DateOnly` | `transaction_date` | day scope, from R-1 |
| `ReceivedAt` | `DateTime?` | `received_date` | display only |
| `Quantity` | `int` | `quantity` | **the proof's quantity** (FR-027) |
| `WeightQuantity` | `decimal?` | `weight_quantity` | display only, never compared |
| `UnitOfMeasure` | `string` | `unit_of_measure` | display and warning only (FR-028) |
| `SavedLocation` | `string` | `initial_location` | the value being corrected |
| `VendorName` | `string` | `vendor_name` or `po_vendor` | non-purchase-order grouping |
| `IsNonPurchaseOrder` | `bool` | `is_non_po_item` | selects the resolver |
| `IsReprint` | `bool` | `is_reprint` | excluded from candidates (FR-016) |

**Validation rules**

- A row is a candidate unless `IsReprint` is true. Excluded reprints are counted and logged (FR-016).
- A purchase-order row with a blank `PartId`, a blank `PurchaseOrderNumber`, or a purchase order number that cannot be normalised to `PO-######[A-Z]` resolves to `Skipped` before grouping.
- A non-purchase-order row is never skipped for lacking a purchase order (FR-020). It is skipped only for a blank `PartId`.
- Every loaded row produces exactly one finding. Counts must reconcile (FR-021).

## Model_ReconciliationGroupKey

A value object, so two keys compare equal without the caller reasoning about strings.

| Member | Purchase-order group | Non-purchase-order group |
| --- | --- | --- |
| `PurchaseOrderNumber` | canonical `PO-######` | empty |
| `PartId` | normalised part | normalised part |
| `TransactionDate` | the receipt day | the receipt day |
| `VendorKey` | empty | normalised vendor |

**Rule (FR-015, LG-01)**: two receipts of one purchase order and part on one day are one group. The same pair on two days is two groups. Two different purchase orders of one part on one day are two groups, which today's `(part, date)` key gets wrong (decision D-06).

**Rule (FR-020)**: a non-purchase-order row with no vendor is grouped under an empty vendor key and resolves to `Not Enough` with a stated reason. It is never dropped from the day's counts.

## Model_ReconciliationGroup

The set of rows competing for one pool of movement evidence.

| Field | Type | Notes |
| --- | --- | --- |
| `Key` | `Model_ReconciliationGroupKey` | identity |
| `Rows` | `IReadOnlyList<Model_ReconciliationRow>` | every candidate in the group |
| `SourceLocations` | `IReadOnlySet<string>` | the saved locations to seed chained-move reachability |
| `ReceivingLocation` | `string` | the group's original receiving location, the residual anchor |
| `Movements` | `IReadOnlyList<Model_ReconciliationMovement>` | selected evidence |
| `Buckets` | `IReadOnlyList<Model_ReconciliationDestinationBucket>` | after chained resolution |
| `Residual` | `Model_ReconciliationSourceResidual` | Part 2 |
| `Findings` | `IReadOnlyList<Model_ReconciliationFinding>` | one per row |

**Validation rules**

- `ReceivingLocation` is the saved location shared by the group's rows. Where the rows disagree, the modal saved location is used and the disagreement is logged at Warning. Two rows of one group saved to two different receiving locations is physical activity, not a data error.
- The group is the failure-isolation boundary (FR-068). A Visual failure marks only this group's findings as `Error`.

## Model_ReconciliationMovement

One resolved Infor Visual transfer. Distinct from the receiving module's model of the same idea, because no type is shared.

| Field | Type | Source column |
| --- | --- | --- |
| `PartId` | `string` | `PART_ID` |
| `PurchaseOrderNumber` | `string` | `PURC_ORDER_ID` |
| `PurchaseOrderLineNumber` | `string` | `PURC_ORDER_LINE_NO` |
| `SourceWarehouseId` | `string` | source leg `WAREHOUSE_ID` |
| `SourceLocationId` | `string` | source leg `LOCATION_ID` |
| `DestinationWarehouseId` | `string` | destination leg `WAREHOUSE_ID` |
| `DestinationLocationId` | `string` | destination leg `LOCATION_ID` |
| `Quantity` | `decimal` | destination leg `QTY`, never negative |
| `TransactionDate` | `DateTime` | destination leg `TRANSACTION_DATE` |
| `TransactionUserId` | `string` | destination leg `USER_ID` |
| `SourceTransactionId` | `int` | source leg `TRANSACTION_ID` |
| `DestinationTransactionId` | `int` | destination leg `TRANSACTION_ID` |

**Rule (FR-030, `R-DATA-09`)**: quantity is always positive and direction comes from `TYPE`, never from the value's sign and never from transaction id ordering (decision D-04). The source leg is `TYPE='O'` and the destination leg is `TYPE='I'`.

**Rule**: a movement whose source and destination locations are equal is discarded.

## Model_ReconciliationDestinationBucket

A location that received net quantity from the group's receipt, after chained movements resolve.

| Field | Type | Notes |
| --- | --- | --- |
| `WarehouseId` | `string` | display |
| `LocationId` | `string` | the proposed destination |
| `NetQuantity` | `decimal` | inbound minus outbound, immutable |
| `RemainingQuantity` | `decimal` | mutable during allocation |
| `AllocatedQuantity` | `decimal` | sum of row quantities bound here |
| `LatestMovementAt` | `DateTime` | tie-break, and the detail pane's movement time |
| `TransactionUserId` | `string` | the Visual user attributed to the move |
| `MovementCount` | `int` | the detail pane's movement count |
| `EvidenceLocations` | `string` | the human-readable chain that reached this location |
| `RanNegative` | `bool` | true when any leg at this location went below zero in the window |

**Rule (FR-017)**: an intermediate location that fully forwards drops out of the buckets because its net is zero. One that retains quantity stays a bucket for that remainder.

**Rule (FR-022)**: after allocation, `AllocatedQuantity` must equal `NetQuantity` exactly, or the rows assigned to the bucket become `Extra Surplus` (allocated less than net) or `Not Enough` (allocated more than net).

**Rule (FR-025)**: when `RanNegative` is true the bucket's rows are not writable and carry `Not Enough` with the negative-destination reason. They are never corrected automatically.

## Model_ReconciliationSourceResidual

The quantity the group's rows are expected to leave in the receiving location.

| Field | Type | Notes |
| --- | --- | --- |
| `ReceivingLocation` | `string` | where the residual must be |
| `ExpectedResidual` | `decimal` | group row total minus the quantity allocated to destination buckets |
| `BalanceAsOf` | `decimal?` | the reconstructed on-hand, null when Visual returned nothing for the location |
| `Difference` | `decimal?` | `ExpectedResidual - BalanceAsOf` |
| `Agrees` | `bool` | `Difference` is exactly zero |
| `BalanceSource` | `string` | which query produced the balance, for the evidence pane |

**Rule (FR-023, D-02)**: `BalanceAsOf` is live on-hand minus the signed net movement at the receiving location after the end of the strict window. For a same-day run the post-window delta is empty and the value is the live on-hand figure.

**Rule (FR-024)**: when `Agrees` is false, the rows that were to keep the receiving location are not written, and the disagreement appears on the page and in the day summary.

**Rule (FR-031)**: a positive `ExpectedResidual` is legitimate stock. It is not a failure and it does not block the destination writes.

## Model_ReconciliationLocationBalance

Read-only input, one row per location the residual check cares about.

| Field | Type |
| --- | --- |
| `PartId` | `string` |
| `WarehouseId` | `string` |
| `LocationId` | `string` |
| `OnHand` | `decimal` |
| `MovementAfterWindow` | `decimal` |

`BalanceAsOf` for a location is `OnHand - MovementAfterWindow`.

## Model_ReconciliationFinding

One row's verdict. This is what the grid shows, what the detail pane explains, and what the write path consumes.

| Field | Type | Notes |
| --- | --- | --- |
| `Row` | `Model_ReconciliationRow` | the line being judged |
| `Outcome` | `Enum_ReconciliationOutcome` | exactly one (FR-032) |
| `SavedLocation` | `string` | what is there now |
| `ProposedLocation` | `string` | empty when nothing is proposed |
| `DestinationSource` | `Enum_ReconciliationDestinationSource` | `Derived` or `Operator` |
| `Reason` | `string` | plain sentence for the pane and the attention window |
| `Difference` | `decimal?` | raw divergence, required for the surplus and shortfall outcomes |
| `Bucket` | `Model_ReconciliationDestinationBucket?` | evidence link |
| `MovementAt` | `DateTime?` | pane evidence |
| `MovementUserId` | `string` | pane evidence, the Visual user |
| `State` | `Enum_ReconciliationFindingState` | see the transitions below |
| `IsWritable` | `bool` | true only in `Writable` |

**Outcome derivation**, evaluated in this order. First match wins.

| Order | Condition | Outcome |
| --- | --- | --- |
| 1 | the row failed candidacy (reprint excluded, blank part, unnormalisable purchase order) | `Skipped` |
| 2 | a Visual call failed for the group | `Error` |
| 3 | the group found no qualifying movement at all | `NotFound` |
| 4 | the row's saved location is a bucket with enough remaining quantity (D-03) | `Unchanged` |
| 5 | allocation bound the row to a bucket whose location equals the saved location | `Unchanged` |
| 6 | allocation bound the row to a bucket with a different location, and the proof passed | `Updated` |
| 7 | no remaining bucket could cover the row | `Not Enough` |
| 8 | the row's bucket retained quantity no row could consume | `Extra Surplus` |
| 9 | the row's bucket ran negative during the window | `Not Enough` |
| 10 | the residual disagrees with the as-of balance for a row keeping the receiving location | `Not Enough` |
| 11 | the operator supplied the destination and the write succeeded | `Manually Set` |
| 12 | the write procedure reported failure | `Error` |

A non-purchase-order row with no vendor reaches order 7 by way of an explicit check, so the reason names the missing vendor rather than the arithmetic.

**Finding state transitions**

```text
Proposed  --proof passes-->  Proven  --location differs-->  Writable  --accept-->  Written
    |                          |                              |
    |                          +--location equal--> Unchanged |
    +--proof fails--> Unresolved                              +--skip-->  SkippedForRun
    +--no bucket-----> Unresolved
    +--visual error--> Unresolved
Written --write failure--> Unresolved
```

`SkippedForRun` is session-scoped and clears when the day reloads (FR-040). `Unresolved` findings reach the operator only through Needs Attention (FR-046).

## Model_ReconciliationDay

One calendar day of receiving rows, its activity state, and whether it completed.

| Field | Type | Notes |
| --- | --- | --- |
| `Date` | `DateOnly` | the unit of work (FR-007) |
| `HasActivity` | `bool` | at least one receiving row exists (FR-010) |
| `Rows` | `IReadOnlyList<Model_ReconciliationRow>` | loaded candidates |
| `Groups` | `IReadOnlyList<Model_ReconciliationGroup>` | resolved groups |
| `Findings` | `IReadOnlyList<Model_ReconciliationFinding>` | exactly one per row |
| `Summary` | `Model_ReconciliationDaySummary` | counts and identity |
| `State` | `Enum_ReconciliationDayState` | see the transitions |
| `FailureReason` | `string` | empty unless failed |

**Day state transitions**

```text
NotLoaded --load--> Loaded --preview--> Previewing --ok--> Previewed --review--> Reviewing --done--> Complete
                                                 |
                                                 +--visual or database failure--> Failed
Reviewing --stop or failure--> Failed
```

**Rule (FR-009)**: `HasActivity` comes from a cheap range query over the receipt date column, not from running a preview. Stepping forward and back moves only to a day whose activity index entry exists (FR-009, SC-005).

**Rule (FR-012)**: `Complete` is remembered for the session so a visited day carries its completed mark on return.

## Model_ReconciliationRun

One pass over one day.

| Field | Type | Notes |
| --- | --- | --- |
| `RunId` | `Guid` | copyable from the summary (FR-049) |
| `Day` | `DateOnly` | which day this pass covered |
| `StartedAt` | `DateTime` | duration source |
| `FinishedAt` | `DateTime?` | null while running |
| `VisualUserId` | `string` | the attribution the summary shows |
| `IsSweepChild` | `bool` | true when the run belongs to a sweep |

**Rule (FR-069)**: every log line written during a run carries `RunId`. A failure names the part, the day and the group in addition.

## Model_ReconciliationDaySummary

The counts the page shows. Each count maps to exactly one outcome, so the totals reconcile against the rows loaded (FR-021, SC-003).

| Field | Type | Derivation |
| --- | --- | --- |
| `Scanned` | `int` | rows loaded for the day |
| `Written` | `int` | `Updated` plus `Manually Set` accepted and written |
| `Unchanged` | `int` | `Unchanged` |
| `Unresolved` | `int` | `Extra Surplus` plus `Not Enough` |
| `Skipped` | `int` | `Skipped` plus session skips |
| `Error` | `int` | `Error` |
| `NotFound` | `int` | `NotFound` |
| `ExcludedReprintCount` | `int` | reprints kept out of the candidate set |
| `ResidualDisagreement` | `bool` | true when any residual check failed |

**Rule (FR-021)**: `Scanned` equals `Written + Unchanged + Unresolved + Skipped + Error + NotFound` minus session skips, which are already inside `Skipped`. The summary exposes one immutable `TotalAccounted` projection that a test can assert.

**Rule (FR-049)**: the summary offers no export action (FR-049, SC-012). There is no export method on the model.

## Model_ReconciliationAllocation

One row bound to one bucket. Allocation produces a list of these, and the proof consumes them.

| Field | Type | Notes |
| --- | --- | --- |
| `Row` | `Model_ReconciliationRow` | the row being placed |
| `Bucket` | `Model_ReconciliationDestinationBucket` | where it was placed |
| `Quantity` | `decimal` | the row's `Quantity` |
| `MatchedExactly` | `bool` | true when the bucket's remaining quantity equalled the row quantity |
| `BoundBySavedLocation` | `bool` | true when the D-03 pass bound it, not the general pass |

A row with no allocation is the shortfall case and resolves to `Not Enough`.

## Model_ReconciliationProofResult

The verdict the proof hands back for one group.

| Field | Type | Notes |
| --- | --- | --- |
| `BucketVerdicts` | `IReadOnlyList<Model_ReconciliationBucketVerdict>` | one per bucket |
| `Residual` | `Model_ReconciliationSourceResidual` | Part 2 |
| `Passed` | `bool` | true only when every verdict and the residual agree |

## Model_ReconciliationBucketVerdict

| Field | Type | Notes |
| --- | --- | --- |
| `Bucket` | `Model_ReconciliationDestinationBucket` | the bucket judged |
| `NetQuantity` | `decimal` | what the movement delivered |
| `AllocatedQuantity` | `decimal` | what the rows consumed |
| `Remainder` | `decimal` | `NetQuantity - AllocatedQuantity`, signed |
| `ConsumedToZero` | `bool` | `Remainder` is exactly zero |
| `RanNegative` | `bool` | copied from the bucket |
| `OutcomeWhenFailed` | `Enum_ReconciliationOutcome` | `Extra Surplus` when the remainder is positive, `Not Enough` when negative or when the bucket ran negative |

## Reused Types From Module_Core

The module depends only on `Module_Core` and `Module_Shared`. From `Module_Core` it uses `Model_Dao_Result` and `Model_Dao_Result<T>` for every data outcome, `IService_LoggingUtility`, the error handler, and the settings core facade. It shares no model, enum, constant, DAO, procedure name, or settings key with `Module_Receiving` (FR-002).

The module's own enums are `Enum_ReconciliationOutcome`, `Enum_ReconciliationRowSource`, `Enum_ReconciliationDestinationSource`, `Enum_ReconciliationFindingState` and `Enum_ReconciliationDayState`.
