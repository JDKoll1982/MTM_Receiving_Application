<!-- 
[DOC-META-START]
- File Name: Proposed_Validation_Safeguards.md
- Description: Shared validation and error-surfacing rules for bulk multi-row editing: live asynchronous validation, severity gating, partial-failure reporting, and UI-thread safety.
- Last Updated: 2026-10-02
- Quick TOC:
  - Line 24-34: # Validation Safeguards for Bulk Editing
  - Line 35-49: # Scope
  - Line 50-62: # Principles
  - Line 63-131: # Live Validation Pipeline
  - Line 132-163: # Severity Model
  - Line 164-196: # What the Operator Sees, and When
  - Line 197-238: # Asynchronous Contracts
  - Line 239-284: # Partial Failure and Blocked Work
  - Line 285-320: # Error Escalation
  - Line 321-352: # Message Catalog
  - Line 353-372: # Logging
  - Line 373-400: # Test Expectations
  - Line 401-420: # Acceptance Criteria
- Critical Notes: A fuzzy part-number match must never silently apply across a batch. Fuzzy results are always Confirm severity.
[DOC-META-END]
-->

# Validation Safeguards for Bulk Editing

Last Updated: 2026-10-02

Applies to `Module_Receiving` Edit Mode and Batch Edit, and to `Module_Reprint` batch queueing.
Written for `Module_Shared` because the reusable pieces are the shared lookup control
(`Module_Shared/Views/Controls/Control_Shared_TypedLookupTextBox`) and the shared view-model base
(`ViewModel_Shared_Base`).

## Scope

The problem this document solves: a batch edit applies one typo, one fuzzy match, or one restricted
part to forty skids at once. Row-at-a-time editing fails safely because the operator sees each row.
Batch editing fails catastrophically. The safeguards below exist to make the multiplicative failure
mode impossible rather than merely unlikely.

## Principles

1. **Validate before the commit, not after.** `SaveAsync` already validates at save time. The batch
   overlay must surface the same findings while the operator is still deciding, so a blocker is
   never discovered after the dialog closes.
2. **Cheap checks synchronously, expensive checks asynchronously.** Format, blank, and range checks
   run inline. Anything that needs the database is debounced and cancellable.
3. **Every finding names its rows.** "Invalid part" is useless with a selection of 3. "`MMC00076`
   is not an exact part match (1 skid: V-F0-03)" is actionable.
4. **Ambiguity is never resolved silently.** A fuzzy match in a single-row edit is a convenience; in
   a batch it is a liability.
5. **The UI never freezes.** No synchronous database work on the UI thread, ever, for any validator.
6. **Failures are partial, and partial failures are reported per row.** No silent skipping.

## Live Validation Pipeline

```
Operator types into an overlay field
   -> 1. Debounce (300 ms) and cancel the previous request
   -> 2. Synchronous cheap checks (blank / format / sign / range)
   -> 3. Asynchronous checks (lookup, restricted part, ownership, queue conflict)
   -> 4. Reduce findings into a severity bucket
   -> 5. Publish to: inline field status, validation pane, Apply button gate, preview row status
```

### Field-by-field validation contract

| Field | Check | Source | Timing | Severity when it fails |
| --------------- | ---------------- | ------------------------------ | -------------- | ------------------------ |
| Part ID | Exact match exists | `TypedLookupTextBox` | Debounced | Block |
| Part ID | Fuzzy fallback used | `UsedFuzzyFallback` | Async | Confirm — never auto-apply |
| Part ID | Value was reformatted | `WasFormatted` | Async | Info — show the new value |
| Part ID | Restricted part | `RestrictedPartAsync` | Async, per batch | Confirm with acknowledgement |
| Part ID | Same as current value | Local comparison | Sync | Info — no change for N skids |
| Heat / Lot # | Blank | Local | Sync | Warn — saved as Nothing Entered |
| Location | Exact match in 002 | `TypedLookupTextBox` | Debounced | Block |
| Location | Overflow or locked | Location lookup | Async | Block |
| Weight / Qty | Negative result | Local arithmetic | Sync | Block |
| Weight / Qty | **Distribute** remainder | Local arithmetic | Sync | Block |
| Weight / Qty | Aggregate vs expected | Local | Sync | Warn or Confirm per tolerance |
| Pkgs / Load | Non-integer or negative | Local | Sync | Block |
| Ownership | Cannot manage a row | Ownership check, once per batch | Async, on open | Block |
| Reprint | Already queued | Reprint history | Async, pre-print | Warn with timestamp |

Ownership and quality-hold checks are deliberately **per-batch, not per-row**. Calling
`IsRestrictedPartAsync` forty times for forty rows that are all being set to the same part is a
needless forty round trips; call it once with the new value.

### Debounce, cancellation, and stale responses

The shared lookup control already exposes `IsValidationInProgress`. Build on it rather than adding a
second busy concept.

```
Field input changes
  -> cancel the CancellationTokenSource for this field
  -> start a new CTS, await Task.Delay(300, token)
  -> run the async check with the token
  -> if the token is cancelled, discard the result and return without touching the UI
  -> if the field's value changed while the check was in flight, discard the result
```

The second guard matters more than the first. A slow exact-match response for `MMC00065` must not
overwrite a fast response for `MMC000767`.

### Mixed-value and batch sizing rules

- A field left unticked is never validated for value correctness, only for consistency with the
  batch (for example, all unticked rows must still be non-negative).
- Validation runs against the **new value**, not per row, for single-value overrides.
- Validation runs per row only for operations that produce row-specific results: `Add`, `Subtract`,
  `Scale by`, and `Distribute`.

## Severity Model

| Severity | Meaning | Effect on the Apply button | Presentation |
| -------- | ------- | -------------------------- | ------------ |
| `Block` | The batch cannot be applied as configured | Disabled | Red inline field message plus a red validation pane entry |
| `Confirm` | Applicable, but the operator must acknowledge | Enabled, gated behind an acknowledgement checkbox in the validation pane | Amber inline message; acknowledgement required once per batch, not per row |
| `Warn` | Worth noticing, does not change the outcome | Enabled | Amber inline message; dismissible |
| `Info` | Neutral information about the batch | Enabled | Grey inline message |

Exactly one `Block` finding disables Apply. The pane always lists every finding, not just the first,
so the operator fixes the batch in one pass instead of discovering problems serially.

## What the Operator Sees, and When

| Moment | Surface |
| ------ | ------- |
| Typing in a field | Inline status under that field: spinner while `IsValidationInProgress`, then a glyph plus one line of text |
| Any finding present | Validation pane `InfoBar` above the preview, summarizing counts by severity |
| Both counts non-zero | Pane reads "2 issues need attention, 1 must be fixed" rather than showing only the blocker |
| Apply pressed with blockers | Apply is already disabled; the button tooltip names the blocker count |
| Apply pressed with confirms pending | Apply stays enabled, but the click is intercepted, the acknowledgement checkbox is focused, and the button does nothing until it is ticked |
| Apply pressed successfully | Dialog closes; findings are preserved on the view model so the Selection Bar can show "3 skids pending change, reconciliation does not balance" |
| After save | Reconciled rows clear their pending marker; unresolved warnings are logged |

## Asynchronous Contracts

### Threading rules

| Rule | Reason |
| ---- | ------ |
| All view-model async methods resume on the UI thread (`ConfigureAwait(true)`, the default) | Property notifications and `ObservableCollection` mutations must occur on the UI thread |
| Never use `.Result` or `.Wait()` on any validation or persistence call | Both deadlock against the UI `SynchronizationContext` |
| No validator performs synchronous database work | A frozen receiving station during a 40-row batch is a production stoppage |
| Use `DispatcherQueue.TryEnqueue` only from genuine background continuations | The lookup control's own implementation is the reference pattern |
| Bound the work: one in-flight request per field, one in-flight batch operation total | Prevents request pile-up when the operator types quickly |

### Cancellation surfaces

| Operation | Cancellable | Behavior on cancel |
| --------- | ----------- | ------------------ |
| Field validation | Yes | Discard result, clear inline status, leave Apply state unchanged |
| Batch apply to in-memory rows | No | Sub-millisecond operation on held objects; cancellation adds no value |
| Save / persist | No | `SaveAsync` semantics are unchanged; the operator sees the existing busy state |
| Reprint queueing of N rows | No, but progress-reported | Report `Queuing 12 of 40`; a failed row never aborts the remaining rows |
| LabelView launch | No | Single shell invocation |

### Busy state

`ViewModel_Shared_Base.IsBusy` is the single busy flag. Existing controls depend on it
(`ProgressRing` on the Reprint page, footer state in Edit Mode). Batch work must set it and must
always clear it in a `finally` block, exactly as `SaveAsync` and the load commands already do. Long
queueing loops must report progress rather than leaving a static spinner for thirty seconds.

## Partial Failure and Blocked Work

### The rule

A batch operation never partially applies silently, and a partial failure never loses the operator's
work.

### Persistence failures

`IService_MySQL_Receiving.UpdateReceivingLoadsAsync` and `UpdateCurrentLabelDataAsync` return counts,
not per-row results. Two safeguards are therefore required at the UI layer:

1. **Pre-flight revalidation.** Immediately before the write, re-verify the batch against the same
   rules the overlay used, so a row that became invalid while the dialog was open is caught.
2. **Count reconciliation.** Compare the returned updated count against the requested count. A
   mismatch is reported to the operator as a partial write, with the difference quantified, not
   silently accepted. The existing `SaveAsync` path already computes `requestedLabelUpdates` versus
   `labelUpdated` and takes the max; that heuristic smooths over a real discrepancy and should be
   replaced with an explicit mismatch report once batch sizes grow.

### Reprint partial failures

`Model_ReprintBatchResult` already buckets `Queued`, `AlreadyQueued`, and `Failed`. Surface all three
as counts plus a details list, and offer `Retry failed (N)` so the operator repairs the run without
re-selecting anything. Failed rows keep their checked state.

### Preserving work

| Situation | Guarantee |
| --------- | --------- |
| Validation blocks apply | Selection and typed values remain in the dialog |
| Apply succeeds, save fails | In-memory rows still hold the change; the operator can retry save |
| Save partially succeeds | The operator is told which rows did not persist |
| App closes with pending changes | No database write occurred; this is existing behavior and must stay true |
| Reprint partially fails | Failed rows remain selected and the batch handoff remains active |

## Error Escalation

`IService_ErrorHandler` is the established escalation path. Batch operations must reuse it rather
than inventing dialogs.

| Situation | Call | Severity |
| ------------------------ | ---------------------------------- | ------------------- |
| Field validation finds a blocker | Inline pane only | Not escalated |
| Apply blocked by ownership rules | `ShowErrorDialogAsync` | `Warning` |
| Batch apply succeeded | `ShowErrorDialogAsync` summary | `Info` |
| Partial write persisted | `ShowErrorDialogAsync` with the count difference | `Warning` |
| Validation service unreachable | `HandleErrorAsync` with a "could not verify" message and a retry path | `Error` |
| Save threw | `HandleErrorAsync` | `Critical`, with the exception, matching `SaveAsync` |
| Label launch failed | `ShowErrorDialogAsync` with the launcher message | `Warning` |

A validator that cannot reach its data source must never default to "valid". Unverifiable is a
distinct outcome from verified, and it surfaces as `Error` with a retry path.

## Message Catalog

Exact operator-facing copy. Keep it plain, specific, and free of error codes.

| Key | Text |
| ------------------------------------------ | -------------------------------- |
| `Batch.SelectionHeadline` | "3 skids selected" |
| `Batch.ReconciliationBalanced` | "Balances to 154,540 lb" |
| `Batch.ReconciliationShort` | "Short by 18,040 lb — selected 136,500 lb, expected 154,540 lb" |
| `Batch.ReconciliationOver` | "Over by 82,094 lb — selected 236,634 lb, expected 154,540 lb" |
| `Batch.ReconciliationOutOfTolerance` | "This does not balance. Confirm you want to apply it anyway." |
| `Batch.BlockerPartNotFound` | "MMC00076 is not an exact part number. Check the value or cancel." |
| `Batch.ConfirmFuzzyPart` | "The part was matched approximately. Confirm MMC000767 is correct." |
| `Batch.WarnHeatLotBlank` | "Heat / Lot is blank. It will be saved as Nothing Entered." |
| `Batch.InfoNoChange` | "No change for 2 of 3 skids." |
| `Batch.BlockNegativeQuantity` | "This would make 1 skid negative (V-F0-03: -12,000 lb)." |
| `Batch.BlockOwnership` | "1 selected skid is owned by another user and cannot be changed here." |
| `Batch.PartialWrite` | "14 of 15 skids were saved. 1 skid did not update. It is still marked as changed." |
| `Batch.Unverifiable` | "Could not verify the part number. Nothing was applied. Try again." |
| `Reprint.QueuedOnly` | "3 labels queued. Label opened in LabelView." |
| `Reprint.AlreadyQueuedWithOwner` | "Queued on 10/1/2026 4:12 PM by mlaurin." |
| `Reprint.PartialQueue` | "2 of 3 labels queued. 1 failed. Retry failed rows?" |

These belong under the settings keys pattern used across the app, not inline in XAML, so wording can
be corrected without a code change.

## Logging

| Event | Level | Content |
| ----- | ----- | ------- |
| Batch opened | Info | Row count, source mode, source part and target part |
| Validation finding | Info or Warning by severity | Field, severity, row count, **not** the operator's typed value if it is sensitive |
| Batch applied to memory | Info | Row count, changed field count |
| Batch persisted | Info | Requested versus returned update count |
| Partial write | Warning | The exact difference and affected identifiers |
| Reconciliation out of tolerance | Warning | Selected total, expected total, variance |
| Reprint queue result | Info | Queued, already queued, failed counts plus batch id |

Log the batch id on every line in the sequence so a support call can reconstruct one operator's run
from the log alone. That is the entire point of the `BatchId` on the handoff model.

## Test Expectations

Following the repository testing rules (FluentAssertions, `MethodName_ShouldResult_WhenCondition`,
`IAsyncLifetime` for integration, `TEST-` data prefix).

This list replaces the table that was here. A proposed test name can run past 65 characters, and a
`code` span that long cannot be broken across lines, so it overflows any table column narrower than
the full page. As a list each entry owns the whole text width, and the names below stay short enough
to fit it comfortably while still following the repository naming convention.

- `ApplyBatch_ShouldUpdateEverySelectedRow_WhenTicked` — unit. Field application semantics.
- `ApplyBatch_ShouldLeaveUntickedFieldsUnchanged` — unit. Override contract.
- `Reconciliation_ShouldReportShort_WhenTotalIsLow` — unit. Variance math and severity.
- `Reconciliation_ShouldAllowApply_WhenNoExpectedTotal` — unit. Neutral default.
- `ValidateBatch_ShouldBlock_WhenPartHasNoExactMatch` — unit. Block severity gate.
- `ValidateBatch_ShouldRequireAcknowledgment_WhenFuzzy` — unit. The fuzzy guard.
- `ValidateBatch_ShouldCallRestrictedPartOncePerBatch` — unit. No N+1 validation calls.
- `ValidateBatch_ShouldDiscardStaleResult_WhenFieldChanges` — unit. Cancellation and stale guard.
- `SaveAsync_ShouldReportPartialWrite_WhenCountsDiffer` — unit. Count reconciliation.
- `BatchEdit_ShouldPersistOnlyOnSave_WhenDialogApplies` — integration. No write before save.
- `BatchEdit_ShouldSurvivePagination` — integration. Selection durability across pages.
- `Reprint_ShouldOfferRetryForFailedOnly` — unit. Partial-failure recovery.

## Acceptance Criteria

- [ ] Every field in the batch overlay shows inline status within 300ms of typing stopping.
- [ ] No `Block` finding allows Apply; every `Confirm` finding requires one acknowledgement per batch.
- [ ] A fuzzy part match never applies without acknowledgement.
- [ ] Blockers list every affected row identifier, not just a count.
- [ ] The UI thread is never blocked: no `.Result`, no `.Wait()`, no synchronous data access in any validator.
- [ ] A validation service outage surfaces as `Error` with a retry path and applies nothing.
- [ ] Partial persistence writes are reported with the exact count difference and leave unpersisted rows marked as changed.
- [ ] Partial reprint failures offer `Retry failed (N)` and preserve failed rows' checked state.
- [ ] All operator-facing strings are settings-driven.
