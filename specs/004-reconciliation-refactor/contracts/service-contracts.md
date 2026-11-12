# Service Contracts

**Module**: `Module_Reconciliation`
**Namespace**: `MTM_Receiving_Application.Module_Reconciliation.Contracts`

Every method returns `Model_Dao_Result<T>` rather than throwing for an expected failure (`R-DATA-02`). Every asynchronous method name ends in `Async` (`R-NAM-04`). No contract here mentions a type from `Module_Receiving` (`FR-002`).

## IService_ReconciliationDayReader

Loads one day's rows and answers where the neighbouring active days are.

| Method | Returns | Behaviour |
| --- | --- | --- |
| `GetDayAsync(DateOnly date)` | `Model_Dao_Result<Model_ReconciliationDay>` | loads the day's candidate rows, sets `HasActivity` from the row count, and returns a day in state `Loaded` |
| `GetActivityDatesAsync(DateOnly from, DateOnly to)` | `Model_Dao_Result<IReadOnlyList<DateOnly>>` | the days in the range that hold at least one receiving row, from the cheap activity query |
| `FindNearestActiveDayAsync(DateOnly from, int direction)` | `Model_Dao_Result<DateOnly?>` | the nearest day in `direction` (`-1` or `1`) that has activity; null when the range is exhausted |

**Rules**

- "No activity" means no receiving rows for the day, never no Infor Visual transfers (FR-010).
- `FindNearestActiveDayAsync` must not run a preview. A day with rows but no movements is still an active day (FR-009).
- Reprint rows are excluded from the candidate set here, and the excluded count travels on the day so it can reach the summary and the log (FR-016).

## IService_ReconciliationPreview

Composes one day. This is the only path a ViewModel calls to produce findings.

| Method | Returns | Behaviour |
| --- | --- | --- |
| `PreviewDayAsync(DateOnly date)` | `Model_Dao_Result<Model_ReconciliationDay>` | loads, groups, selects movements, builds buckets, allocates, proves, and returns the day in state `Previewed` with one finding per row |

**Rules**

- Preview writes nothing (FR-038).
- Every row loaded produces exactly one finding, and the summary totals reconcile against the rows loaded (FR-021).
- One bounded retry per Infor Visual call. A failure is isolated to its group's findings, which become `Error`, and the day still returns rather than failing whole (FR-068).
- A day-level failure, meaning the application database or the Visual connection itself is unavailable, returns a failed result with a stated reason and produces no findings (FR-048).

## IService_ReconciliationGroupResolver

| Method | Returns | Behaviour |
| --- | --- | --- |
| `Resolve(IReadOnlyList<Model_ReconciliationRow> rows)` | `Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>>` | partitions candidates into purchase-order groups keyed by purchase order, part and transaction date, and hands the non-purchase-order rows to the non-purchase-order resolver |
| `ResolvePurchaseOrderOnly(IReadOnlyList<Model_ReconciliationRow> rows)` | `Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>>` | the purchase-order key alone, exposed so a test can assert the key without standing up a whole day |

**Rules**

- Rows that fail candidacy never reach a group. They come back as findings through the day, already `Skipped` (FR-032).
- Two purchase orders of one part on one day are two groups. One purchase order and part on two days is two groups. One purchase order and part on one day is one group (FR-015).
- Current-label and history rows for the same part and receipt date reconcile together in one pass (FR-019).
- The methods are synchronous and pure. They take data and return data, so they are unit-testable without a database.

## IService_ReconciliationNonPurchaseOrderResolver

| Method | Returns | Behaviour |
| --- | --- | --- |
| `Resolve(IReadOnlyList<Model_ReconciliationRow> rows)` | `Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>>` | groups non-purchase-order rows by part, transaction date and vendor, under the same quantity proof and the same outcomes |

**Rules**

- A non-purchase-order row is never skipped for lacking a purchase order (FR-020).
- A row with no vendor is grouped under an empty vendor key and resolves to `Not Enough` with a reason naming the missing vendor. It stays in the day's counts (FR-020, user story 5 scenario 4).
- This resolver produces groups of the same shape as the purchase-order resolver, so the movement selector, bucket builder, allocator and proof need no special case.

## IService_ReconciliationMovementSelector

| Method | Returns | Behaviour |
| --- | --- | --- |
| `Select(Model_ReconciliationGroup group, IReadOnlyList<Model_ReconciliationMovement> movements)` | `IReadOnlyList<Model_ReconciliationMovement>` | returns only the movements reachable from the group's seeds |

**Rules**

- Seeds are the group's saved source locations plus the canonical purchase order keys taken from the group's rows. A movement is reachable when its source location is already reached or its purchase order key is one of the seeds (FR-017).
- Each movement is considered at most once, and the pass repeats until no further movement is admitted.
- The result is ordered by transaction date, then destination transaction id, then source transaction id, so the caller sees a stable sequence.
- A movement whose source and destination are the same location is discarded.

## IService_ReconciliationBucketBuilder

| Method | Returns | Behaviour |
| --- | --- | --- |
| `Build(IReadOnlyList<Model_ReconciliationMovement> movements)` | `IReadOnlyList<Model_ReconciliationDestinationBucket>` | resolves chained moves into net destination buckets |

**Rules**

- Net quantity per location is the sum inbound minus the sum outbound across the selected movements (FR-017).
- A location whose net is zero drops out. A location that retains quantity stays a bucket for that remainder (FR-017).
- A location whose net is negative is not a bucket, and any row that would have used it resolves to `Not Enough` with the negative-destination reason (FR-025).
- `RanNegative` is set when any leg at that location took its running balance below zero inside the window.
- Buckets are ordered by net quantity descending, then latest movement descending, then location, so allocation is deterministic without a second sort.
- Evidence fields carry the latest inbound leg's user and time, the count of inbound legs, and the distinct source locations that fed the bucket.

## IService_ReconciliationAllocator

| Method | Returns | Behaviour |
| --- | --- | --- |
| `Allocate(IReadOnlyList<Model_ReconciliationRow> rows, IReadOnlyList<Model_ReconciliationDestinationBucket> buckets)` | `Model_Dao_Result<IReadOnlyList<Model_ReconciliationAllocation>>` | binds as many rows as it can, and returns one allocation per bound row |

**Rules, in order.**

1. **Saved-location pass first** (FR-029, decision D-03). Rows are visited in the documented order. A row whose saved location is a bucket with remaining quantity greater than or equal to the row's quantity is bound to that bucket, flagged `BoundBySavedLocation`, and the bucket's remaining quantity falls by the row's quantity.
2. **General pass** over the rows left. For each row, prefer the bucket whose remaining quantity is exactly the row quantity. Failing that, take the smallest bucket that still covers the row. Break ties on the latest movement time, then on location id (FR-018).
3. **Ordering** inside both passes is quantity descending, then load number, then load id (FR-018).
4. **No unit gate.** Unit of measure never excludes a bucket or a row. A group holding more than one unit logs a warning and the unit appears in the detail pane (FR-028).
5. **Exact arithmetic.** Quantities are `decimal`, comparison is exact, and no tolerance, epsilon or percentage allowance exists anywhere (FR-026, `R-SCOPE-05`).
6. A row that finds no bucket gets no allocation. The caller turns that into `Not Enough`.

**Postcondition.** The sum of allocated quantities plus the residual quantity equals the group's total row quantity, and no bucket's remaining quantity is below zero.

## IService_ReconciliationQuantityProof

| Method | Returns | Behaviour |
| --- | --- | --- |
| `VerifyAsync(Model_ReconciliationGroup group, IReadOnlyList<Model_ReconciliationAllocation> allocations)` | `Model_Dao_Result<Model_ReconciliationProofResult>` | runs both parts and returns a verdict per bucket plus the residual |

**Part 1, destination buckets (FR-022).** Every bucket's allocated quantity must equal its net quantity exactly.

- Allocated less than net means the movement showed more material than the rows account for, so the rows in that bucket become `Extra Surplus`.
- Allocated more than net means the rows demand more than the movement delivered, so they become `Not Enough`.

**Part 2, source residual (FR-023, FR-024).** The residual is the group's total row quantity minus the quantity allocated to destination buckets.

- The residual must equal the as-of balance at the receiving location: live on-hand minus the signed net movement at that location after the end of the strict window (decision D-02).
- A mismatch leaves the rows that keep the receiving location not writable and carries `Not Enough` with both figures stated. It never corrects anything silently.
- A positive residual is legitimate stock, not a failure (FR-031).

**Rules**

- The strict window is the receipt date and the receipt date plus one day, configurable, and never widened by a sweep (FR-014).
- A bucket that ran negative fails regardless of the arithmetic (FR-025).
- Every figure in every reason string formats with `CultureInfo.InvariantCulture` (FR-058).
- The proof writes nothing and depends on no ViewModel state.

## IService_ReconciliationWriteService

| Method | Returns | Behaviour |
| --- | --- | --- |
| `AcceptAsync(Model_ReconciliationFinding finding, string destinationLocation, string operatorUserId)` | `Model_Dao_Result<Model_ReconciliationFinding>` | re-runs the proof for the finding's group, then performs exactly one write and returns the finding in state `Written` |

**Rules**

- The finding must be in state `Writable`, or in `Unresolved` when the operator typed a destination in Needs Attention. Any other state is refused with a reason and no write happens (FR-041).
- The proof is re-run at accept time rather than trusted from preview. A day is a snapshot and the balance moves.
- Exactly one call to `sp_Reconciliation_ReceivingLocation_Update`. There is no bulk path, no end-of-run apply, and no second procedure (FR-041, `DS-06`).
- A typed destination sets `DestinationSource = Operator` and the finding resolves to `Manually Set`. An accepted proposal resolves to `Updated` (FR-047).
- A failed write leaves the finding in `Unresolved` with the failure reason, and the review cycle does not advance as if it succeeded (edge case: a rejected write).
- The operator id is recorded on the write. No supervisor gate is applied (decision D-09, FR-054).

## IService_ReconciliationSettings

| Method | Returns | Behaviour |
| --- | --- | --- |
| `GetDiscoveryWindowDaysAsync()` | `Task<int>` | the discovery window, default 2 |
| `GetStrictWindowDaysAsync()` | `Task<int>` | the strict validation window, default 1 |

**Rules**

- Both values are read through the settings core facade under the module's own `Reconciliation.` key prefix (FR-059).
- A missing value returns the default rather than failing the day.
- The module never reads a `Receiving.` settings key, and the two old Receiving keys are not migrated (FR-060).

## ViewModel Surface

`ViewModel_ReconciliationMain` depends only on `IService_ReconciliationPreview`, `IService_ReconciliationDayReader` and `IService_ReconciliationWriteService`, never on a DAO and never on a collaborator (`R-MVVM-01`, `R-MVVM-09`).

| Member | Type | Purpose |
| --- | --- | --- |
| `SelectedDate` | `DateOnly` | the working day, opening on today (FR-008) |
| `GoToPreviousDayCommand` | `IRelayCommand` | moves to the nearest earlier active day (FR-009) |
| `GoToNextDayCommand` | `IRelayCommand` | moves to the nearest later active day (FR-009) |
| `GoToTodayCommand` | `IRelayCommand` | returns from any position in one action (FR-009) |
| `AcceptCommand` | `IRelayCommand` | writes the selected line and advances (FR-045) |
| `SkipCommand` | `IRelayCommand` | skips the selected line for this run (FR-045) |
| `OpenNeedsAttentionCommand` | `IRelayCommand` | disabled while the count is zero (FR-046) |
| `StartSweepCommand` | `IRelayCommand` | runs the backlog sweep day by day (FR-011) |
| `StopSweepCommand` | `IRelayCommand` | stops the sweep between days (FR-011) |
| `OutcomeFilter` | `Enum_ReconciliationOutcome?` | filter by outcome (FR-043) |
| `TextFilter` | `string` | filter by part or purchase order text (FR-043) |
| `CurrentLineIndex` | `int` | progress as line N of M (FR-044) |
| `TotalLineCount` | `int` | progress as line N of M (FR-044) |
| `AcceptedCount` | `int` | preserved for the session (FR-044) |
| `SkippedCount` | `int` | preserved for the session (FR-044) |
