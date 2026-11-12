# Phase 0 Research: Receiving Validator

**Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)
**Created**: 2026-11-12

Every investigation the clarification register deferred to this phase is closed here, except R-5, which needs a live day and is recorded as a post-implementation measurement with the decision it governs.

Each entry states the evidence found, the decision taken, and what the decision changes. Where an investigation came back inconclusive, the register's stated default was applied rather than stalling.

## Result Summary

| Id | Investigation | Outcome | Settles |
| --- | --- | --- | --- |
| R-1 | Application database date columns | Closed: `transaction_date` is the day column, `received_date` is the display time | `DS-01`, FR-013 |
| R-2 | Infor Visual `RECEIVER` / `RECEIVER_LINE` | Closed: no row-level join exists. Default applied | `C-01`, `LG-02` = A, `DS-03` = No |
| R-3 | Visual `TRANSACTION_DATE` meaning | Closed: posting time, direction in `TYPE` | `DS-01`, FR-013, FR-014 |
| R-4 | `CR_PART_LOCATION` coverage and freshness | Closed with a correction: live snapshot, so the residual needs an as-of reconstruction | FR-023, FR-024 |
| R-5 | Non-writable rate on one live day | Open by design: post-implementation measurement | C-04, `LG-05` |
| R-6 | 18a/18b split shape | Closed: shape fits, direction heuristic must go | FR-067, `DS-02` |
| R-7 | Query 17 callers | Closed: zero feature callers | FR-067 |
| R-8 | Numeric persistence of the workflow step | Closed: none | FR-063, `TS-06` |
| R-9 | Receiving proposed UI documents | Closed: not reconcile specific, stay in place | `MG-01` |
| R-10 | Project memories naming the reconciliation path | Closed: none | `MG-03` |

## R-1: Application Database Date Columns

**Evidence.** `Database/Database_Deployment/Sql_Files/Schemas/11_Table_receiving_label_data.sql` and `10_Table_receiving_history.sql` both declare:

- `received_date DATETIME NULL`, described as "full timestamp when the record was received".
- `transaction_date DATE NOT NULL`, described as "date portion of the receiving transaction", carrying the index `idx_transaction_date` in both tables.
- `created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP`.

Only `transaction_date` is non-nullable and indexed for range queries in both tables. `received_date` is nullable in both. `created_at` records when the row was written, so imported history rows carry the import time rather than the receipt time, which the comment on `receiving_history.load_guid` confirms ("NULL for imported historical records").

The two columns are also collapsed in code today: `Module_Receiving/Data/Dao_ReceivingLoad.cs:646` and `Dao_ReceivingLabelData.cs:597` both read `WeightQuantity = ReadDecimal(row, "weight_quantity", "quantity")`, and `Model_ReceivingLoad.ReceivedDate` carries the `received_date` timestamp.

**Decision D-06, R-1.10**: the day boundary and every day-scoped query use `transaction_date`. `received_date` supplies the time of day for display and ordering only. `created_at` is never used for scope.

**Why.** The operator means "the day I received this", and `transaction_date` is the only column that is non-nullable, indexed, and identical in meaning across the label queue and the history archive. A day boundary built on a nullable timestamp would silently drop rows.

**Alternatives considered.** `received_date` alone: rejected, nullable and unindexed. `created_at`: rejected, wrong for imported history rows. Splitting the pair per table: rejected, the two tables share the column definitions and the module reads them as one day.

**Consequence for FR-013.** The date column pair the spec asks to be recorded is `(transaction_date for scope, received_date for display)`, and this is that record.

## R-2: Infor Visual Receiver Tables

**Evidence.** Against `docs/development/InforVisual/DatabaseCSVFiles/ColumnDetails/`:

- `RECEIVER` carries `ID`, `PURC_ORDER_ID`, `RECEIVED_DATE` (nullable), `CREATE_DATE`, `USER_ID`, `SITE_ID`. It carries no part and no quantity.
- `RECEIVER_LINE` carries `RECEIVER_ID`, `LINE_NO`, `PURC_ORDER_ID`, `PURC_ORDER_LINE_NO`, `USER_RECEIVED_QTY`, `RECEIVED_QTY`, `INSPECT_QTY`, `REJECTED_QTY`, `WAREHOUSE_ID`, `LOCATION_ID`, `TRANSACTION_ID`. It carries **no `PART_ID`**, and no load, skid, package, or label identity of any kind.
- The only path to a part runs `RECEIVER_LINE` to `PURC_ORDER_LINE` on `(PURC_ORDER_ID, PURC_ORDER_LINE_NO)`.

The repository already performs exactly that join in `Database/InforVisualScripts/Queries/16_GetReceivingLocationEvidence.sql`. That query uses the join to return receipt count and the latest receipt warehouse and location. It returns **no quantity**, which is the strongest available signal that a quantity join was not considered dependable by whoever wrote it.

**Decision (C-01 default applied)**: `DS-03` = No and `LG-02` = A. No `RECEIVER` query is built. MTM receiving rows remain the receiver proxy, and `LG-04` Part 2 compares the residual against the on-hand balance only.

**Why the join is not clean.** A `RECEIVER_LINE` quantity belongs to a purchase order line for one receipt event. An MTM row is one skid of that receipt. Visual holds nothing that maps a receiver quantity onto a specific skid, so any per-row attribution would be an inference dressed as evidence. Per-row attribution is exactly what the per-line write gate needs, and a group-level total adds nothing the movement ledger does not already provide.

`RECEIVER.RECEIVED_DATE` being nullable is a second, independent reason: a receipt event with no booked date cannot anchor a day-window comparison.

**Better alternative already present.** `INVENTORY_TRANS` carries `PART_ID`, `PURC_ORDER_ID`, `PURC_ORDER_LINE_NO`, `QTY`, `TRANSACTION_DATE`, `USER_ID`, and `TYPE`/`CLASS`. A receipt is `TYPE='I'`, `CLASS='R'` with a purchase order. That gives a dated, part-level inbound quantity, linked to the purchase order, from the same ledger the rest of the module already trusts. If a true received quantity is ever needed, it comes from there, not from `RECEIVER`.

**Alternatives considered.** Building the receiver query and attributing the PO-line quantity across MTM rows by proportion: rejected, it invents a split the data cannot support and would feed the write gate. Building it for group-level totals only: rejected, `INVENTORY_TRANS` already answers that at lower cost and without a nullable date.

## R-3: Visual `TRANSACTION_DATE` Semantics

**Evidence.** `.github/instructions/database/infor-visual-inventory-discrepancy-investigation.instructions.md` gives the ledger decode:

- Receipt: `TYPE='I'` with `CLASS='R'` and a purchase order.
- Location transfer: matched `TYPE='I'` and `TYPE='O'` legs with `CLASS='A'`, linked both ways by `TRANSFER_TRANS_ID`, netting to zero.
- Consumption: `TYPE='O'` with `CLASS='I'` at barcode-labor clock-out.

`INVENTORY_TRANS.TRANSACTION_DATE` is `datetime NOT NULL`. `18b_GetTransferPairDetails.sql` dates a movement as `COALESCE(dst.TRANSACTION_DATE, src.TRANSACTION_DATE)`, meaning the destination leg wins when the two legs disagree. `18_GetReceivingLocationTransferMovements.sql` and `18a` scope discovery with `CAST(TRANSACTION_DATE AS date) BETWEEN @ReceivedDate-2 AND @ReceivedDate+2`.

**Decision**: `TRANSACTION_DATE` is the posting time of the individual leg. For a transfer pair, the movement's date is the destination leg's date, which is when the material arrived. Windows stay anchored on the MTM receipt date, because that is the only date a receiving row can supply. Discovery stays at minus two to plus two days and strict validation at the receipt date and the day after, both configurable, neither widened by a sweep.

**Why.** Anchoring on the posting date of a Visual leg would make the window depend on when an operator happened to post, which is what the current implementation deliberately avoids. Taking the destination leg for recency matches how buckets report their latest move.

**Alternatives considered.** Using the source leg's date: rejected, a two-leg transfer can straddle midnight and the arrival date is the one that explains where material is. Anchoring windows on the Visual receipt transaction date: rejected, it is not always present and it is not what the row records.

## R-4: `CR_PART_LOCATION` Coverage and Freshness

**Evidence.**

- `docs/development/InforVisual/DatabaseCSVFiles/Views/CR_PART_LOCATION.csv` shows `CR_PART_LOCATION` is a view. Its columns are `ID` (the part), `WAREHOUSE_ID`, `LOCATION_ID`, `DESCRIPTION`, `QTY`, `COMMITTED_QTY`, `STATUS`, `LOCKED`, `TRANSIT`, `LAST_COUNT_DATE`, plus ordering fields.
- `docs/development/InforVisual/DatabaseCSVFiles/ColumnDetails/PART_LOCATION.csv` shows the base table with `PART_ID`, `WAREHOUSE_ID`, `LOCATION_ID`, `QTY` and the same ordering fields. `16_GetReceivingLocationEvidence.sql` reads `dbo.PART_LOCATION` directly with `pl.PART_ID`.
- The ledger playbook defines `CR_PART_LOCATION` as "current on-hand per (part, warehouse, location)".

**Coverage: yes.** A destination the module proposes is a location material was transferred into, so it holds a balance row while it holds stock. A destination with no balance row simply has zero on-hand, and that does not affect the gate, because Part 2 checks the **source** location only.

**Freshness: no, and this is the finding that changes the design.** `CR_PART_LOCATION` is a live snapshot at query time. It reflects the same day's movements only once they are posted, and it does not reflect a past day at all. Comparing a historical day's residual against today's on-hand would fail every day in a backfill sweep, which would break user story 6 on its first day.

**Decision**: Part 2 compares the residual against an **as-of balance**, reconstructed as live on-hand at the receiving location minus the signed net movement at that location after the end of the strict window. For a same-day run the post-window delta is empty and the figure is exactly the live on-hand the spec names. One new read-only query, `18c_GetLocationMovementAfterWindow.sql`, returns the signed net movement per location for the part after a given date.

**Why.** It preserves the spec's exactness, keeps every comparison in `decimal`, needs no new table, and reduces to the same-day behaviour the acceptance scenarios describe. It is also the only formulation that makes a sweep meaningful.

**Alternatives considered.** Live on-hand only: rejected, it fails every historical day. Rebuilding the whole balance history in application code: rejected, more work than the residual needs. Adding a balance snapshot table: rejected, `AR-07` and `DS-07` forbid new tables in this slice.

## R-5: Non-Writable Rate On A Live Day

**Not closeable from the repository.** It requires one production day of rows and the module running against it.

**What is known from the code.** Today's service sets `SavedRowQuantity = load.WeightQuantity`, and `WeightQuantity` is read as `weight_quantity` with `quantity` as fallback (`Dao_ReceivingLoad.cs:646`). The spec requires the **integer** `quantity` (FR-027, `LG-05` = B). So the change already swaps the field, and `C-04` predicts the consequence correctly: with a rounded integer, exact equality, and no unit gate, weight-based parts will land in `Not Enough` and `Extra Surplus` more often than the decimal comparison does today.

**Recorded as post-implementation measurement.** After the first live day, count the findings carrying `Not Enough` or `Extra Surplus` and divide by the day's row count. The decision that measurement governs is whether `LG-05` stays B (integer `quantity`) or moves to C (`weight_quantity` with an integer fallback). It does not block this slice: FR-027 fixes the field at B for now, and the measurement is the trigger to revisit it.

**Decision rule the implementer should apply.** If the non-writable rate on weight-based parts exceeds the rate on unit-count parts by more than the noise a single day can carry, reopen `LG-05` with the measured figure attached. Otherwise keep B.

## R-6: The 18a/18b Split Shape

**Evidence.** `18_GetReceivingLocationTransferMovements.sql`, `18a_GetTransferCandidatePairs.sql` and `18b_GetTransferPairDetails.sql` all exist. Production does **not** use the split: `Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs:473` loads the combined `18_GetReceivingLocationTransferMovements.sql`. The two-step path is authored but unwired.

Both `18` and `18a` decide direction from transaction id ordering, commented as "The lower `TRANSACTION_ID` row is the source location and the higher `TRANSACTION_ID` row is the destination." That is a heuristic. `R-DATA-09` says direction lives in `TYPE` (`I` in, `O` out).

**Does the split still fit after `LG-02` and `LG-04`?**

- `LG-02` = A means no receiver query, so the split's inputs are unchanged.
- `LG-04` needs three things from the movement set: the per-destination net, a strict-window reference, and detection of a destination that ran negative. `18b` returns both legs with source and destination ids, quantities, and dates, so all three are computable in memory, which is where the current code already computes the strict-window reference.

**Decision**: keep the two-step split, complete the wiring, and replace the id-ordering heuristic with direction from `TYPE`. `18a` returns the pair ids; `18b` labels each leg's role from `TYPE` rather than from id order.

**Why.** The split is the right shape for a bounded per-part call: discovery stays cheap and narrow, hydration stays limited to the ids that matter. The heuristic is the one piece that cannot survive `LG-04`: if a pair's out leg has the higher id, source and destination invert, both bucket direction and the residual are silently wrong, and the proof would certify a wrong answer. `R-DATA-09` names the correct discriminator and the column exists.

**Alternatives considered.** Keep the combined query: rejected, `DS-02` records that the split is the intended shape and the combined file duplicates `18a` and `18b` logic. Keep id ordering and add a validation: rejected, it detects the swap after the fact instead of avoiding it.

## R-7: Query 17 Callers

**Evidence.** `17_GetReceivingLocationTransactionHistory.sql` is reached through three plumbing members:

- `Module_Core/Contracts/Services/IService_InforVisual.cs:94`
- `Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs:382`, loading the file at line 396
- `Module_Core/Services/Database/Service_InforVisualConnect.cs:513` and `:589`

`Model_InforVisualLocationTransaction` is referenced only by those same four files, so it exists solely to carry this query's rows.

A repository-wide search for `GetReceivingLocationTransactionHistoryAsync` outside `bin` and `obj` returns those same three source files and nothing else. No ViewModel, service, or module calls it.

**Decision**: query 17 has zero feature callers and is deleted, together with the three plumbing members and `Model_InforVisualLocationTransaction`.

**Why.** Keeping a dead query means keeping a dead model, an interface member, a DAO method, and a service pass-through, and `R-SCOPE-04` says remove what belongs to the feature.

**Alternatives considered.** Leaving it: rejected, it is reconcile-specific plumbing with no remaining consumer.

## R-8: Numeric Persistence Of The Workflow Step

**Evidence.** `Enum_ReceivingWorkflowStep.ReconciliationReview = 12` is the last member, so removing it cannot renumber anything.

Consumers of the enum are `switch` arms and `nameof` comparisons. The two settings that could persist a step do not: `ViewModel_Settings_Receiving_WorkflowDefaults` stores `DefaultModeOnStartup` as a **string** (`"manual"`, `"edit"`, `"mode selection"`) via `ReceivingSettingsKeys.BusinessRules.DefaultModeOnStartup`, and it converts between the two with `nameof` arms at lines 287 and 288.

A search for the enum alongside `Serialize`, `Convert`, `Parse`, `Json`, `Save`, `Persist`, and `Store` returns nothing. The MySQL schema has no column for a workflow step.

**Decision**: nothing persists the numeric value. The member is deleted outright, with no reserved slot and no comment.

**Cascading surface the removal must also cover.** The step is not only the enum member:

- `Module_Core/Services/Help/Service_Help.cs` lines 160, 261 and 788, keyed `"Receiving.ReconciliationReview"`.
- `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs` lines 61, 104, 433, 459, 460 and 1052.
- `Module_Receiving/Views/View_Receiving_Workflow.xaml` (`ReconciliationReviewHost`) and its code-behind, 17 references.
- `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs` and `View_Receiving_ModeSelection.xaml` (the card bound to `ReconcileSavedLocationsCommand`).

## R-9: Receiving Proposed UI Documents

**Evidence.** `Module_Receiving/Proposed_UI_Changes.xaml.md` is 24,697 bytes with a `.html` render and a `.pdf`. Its section headings are EditMode toolbar and selection bar, EditModeDataGrid multi-select, batch edit overlay wireframe, and reprint batch filter chips, plus new converters and XAML risks. The word "reconcil" appears twice in the whole document.

**Decision**: not reconcile specific. It stays in `Module_Receiving` untouched. It is not moved, not deleted, and not part of the removal boundary.

**Why.** `R-SCOPE-04` removes only what belongs to the feature. Moving a Receiving edit-mode design note into a reconciliation module would misplace it, and deleting it would lose unrelated work.

## R-10: Project Memories Naming The Reconciliation Path

**Evidence.** Searching `.serena/memories` for `Service_ReceivingLocationReconciliation`, `IService_ReceivingLocationReconciliation`, `LocationReconciliation`, and the four query filenames returns nothing. One file mentions reconciliation at all: `infor-visual-inventory-discrepancy-playbook.md` line 58, which restates the generic ledger identity `receipts(CLASS R) - net WO consumption ≈ on-hand change`.

**Decision**: no memory file names the Receiving reconciliation path, so `MG-03` has nothing to rewrite and nothing to delete.

**Why.** The rule anticipated memories that would go stale. The check shows none would.

**Alternatives considered.** Proactively adding a memory for the new module: out of scope; memories are created when knowledge would otherwise be lost, and the module ships its own documentation.

## Decisions Taken During Research

These arose while closing the investigations and they shape the design, so they are recorded here beside the evidence that produced them.

### D-01: Module Boundaries Are Namespace Rules, Not Assembly References

**Evidence.** The repository has one production project, `MTM_Receiving_Application.csproj`, and a solution file containing only that project. Modules are folders compiled into a single assembly.

**Decision**: `Module_Reconciliation` enforces the boundary exactly as `R-MOD-07` describes (no `using` of `Module_Receiving`, no shared type, no shared procedure name, no shared settings key), and SC-008 is verified by building with the module's folder excluded from compilation and then with `Module_Receiving` excluded. It is not verified by removing a project reference, because there is no project reference to remove.

**Why.** Stating this up front stops a reviewer from looking for a `.csproj` that does not exist and stops an implementer from splitting the solution into projects, which would be a much larger change than the spec authorises.

### D-02: The Residual Is Compared Against An As-Of Balance

Follows from R-4. See that entry.

### D-03: Saved Locations Are Bound Before General Allocation

**Evidence.** `FR-029` requires a first-pass current-inventory comparison that short-circuits an already-correct line to `Unchanged` before allocation runs. `FR-036` says such a line never enters the review cycle even when movement evidence exists. Today's code allocates first and only then sets `Unchanged` when the chosen bucket happens to equal the saved location, so a row already sitting in a legitimate destination bucket can be moved off it and reported `Updated`.

**Decision**: before the deterministic allocation pass, every row whose saved location is a destination bucket with enough remaining quantity is bound to that bucket and resolves to `Unchanged`, and that quantity is deducted from the bucket. Only the remaining rows and remaining bucket quantities enter allocation. The documented allocation ordering still governs within that pass.

**Why.** This is a real behaviour change, and the spec asks for it explicitly in `FR-029` and `FR-036`, which `R-SCOPE-01` requires. It also stops the module rewriting a location to the place it is already at, which is the outcome the operator would find least believable.

### D-04: Movement Direction Comes From `TYPE`, Not From Id Ordering

Follows from R-6. See that entry.

### D-05: The Proof Uses The Integer Quantity, Which Is A Field Swap

Follows from R-1 and `LG-05`. `Model_ReceivingLoad.WeightQuantity` collapses `weight_quantity` and `quantity`, so the new module's own row model must project them separately. `Model_ReconciliationRow.Quantity` is the integer column and is the only field the proof reads. `WeightQuantity` is carried for display only.

### D-06: The Grouping Key Gains The Purchase Order

**Evidence.** Today's key is `new ReconciliationGroupKey(Normalize(tagged.Load.PartID), tagged.Load.ReceivedDate.Date)`, which merges two different purchase orders of the same part received on the same day. `LG-01` = C requires `(PO, PartID, ReceivedDate)`.

**Decision**: purchase order, part, and transaction date for purchase-order rows; part, transaction date, and vendor for non-purchase-order rows.

**Why.** Without the purchase order in the key, two orders competing for one statement of movement would corrupt every bucket in the group, and the residual check would then reject both.

### D-07: One Navigation Entry That Opens A Launcher Page

**Evidence.** `FR-062` requires the module to appear exactly once in navigation, in the Reporting group. Decision `UI-01` requires a **mode-select launcher page** for the module, copying the existing mode-select style and features. `SC-05` = C places the entry under Reporting. `MainWindow.xaml` already places the Reporting module's own entry (`Tag="ReportingMainPage"`) in the `FooterMenuItems` group.

**Decision**: the module's single navigation entry is a `NavigationViewItem` added to that footer group, labelled `Receiving Validator` with `from Infor Visual` as supporting text, opening a **launcher page** (`View_Reconciliation_Launcher`) built in the established mode-select style. The launcher owns the mode cards and opens the working page (`View_Reconciliation_Main`). No card is added to the Reporting module's own page.

**Why.** `UI-01` is a locked decision: the launcher is part of the requested surface, not an optional extra, and it is where future modes (for example the separate non-purchase-order service from `LG-13`) will appear without restructuring navigation. "Appears once" is satisfied by the single nav entry, not by collapsing the launcher into the working page — the launcher is reached only through that one entry.

**Superseded:** an earlier reading of this decision collapsed the launcher into the working page to avoid a second surface. That contradicts `UI-01` and is withdrawn.

### D-08: The Module Ships `Documentation/`, Not `docs/`

**Evidence.** `R-DOC-01` requires the full `Documentation/` tree with its mandatory files and calls it a deliverable rather than a follow-up. No module in the repository has that folder today; `Module_Reporting`, `Module_Reprint`, `Module_Scanner` and `Module_ShipRec_Tools` have `docs/`, and `Module_Receiving` has neither at its root.

**Decision**: the new module ships `Documentation/` as the rule states, and the moved algorithm document lands inside it. The legacy `docs/` folders are left alone.

**Why.** The rule is binding and its own text says the new module is where it starts. Copying a neighbour's legacy folder shape would knowingly break a rule on the one module that has no history to preserve.

### D-09: FR-054 Defaults To Any Authenticated User, With No Supervisor Gate

**Evidence.** FR-054 carries the slice's one open business question. `DS-06` records that the module supersedes the Receiving edit-mode ownership rule, and `C-06` asks for confirmation without resolving who may write.

**Decision**: any authenticated user may correct any row, and the write records the operator. A supervisor role is a future gate that can be added in the procedure and the service without changing the review flow.

**Why.** The spec's own assumption says no role gate is assumed and the review flow is unaffected either way. Defaulting to deny would leave the feature unable to perform the correction the request asks for. The single write procedure is the one place a gate would later be enforced, so nothing is designed out.

### D-10: The Module Owns Its Visual DAO, And Loads Its Own Query Files

**Evidence.** `R-MOD-07` forbids a shared DAO. `Dao_InforVisualConnection` in `Module_Core` currently holds both the connection guard and the receiving queries.

**Decision**: `Dao_ReconciliationInforVisual` takes the connection string through constructor injection, repeats the `ApplicationIntent=ReadOnly` guard, and loads `Reconciliation/18a`, `18b` and `18c` through `Helper_SqlQueryLoader`, which is a helper rather than a DAO. `Module_Receiving` keeps query 16 and its own DAO untouched.

**Why.** It satisfies "own data access" without duplicating the read-only guard's intent or inventing a parallel connection factory. `Helper_SqlQueryLoader` resolves embedded resources by dotted path, so a `Reconciliation/` subfolder needs only the folder name in the call.

### D-11: One Write Procedure Covers Both Row Sources

**Evidence.** Rows come from `receiving_label_data` (current labels) and `receiving_history`. `R-MOD-08` allows exactly one reconciliation-owned write procedure over receiving rows.

**Decision**: `sp_Reconciliation_ReceivingLocation_Update` takes the row source, the row id, the destination location, and the operator, and updates `initial_location` on the named row. It returns the affected row count so the caller can tell a successful write from a no-op.

**Why.** One procedure is the entire contract between the modules, and a discriminant column keeps it one procedure rather than two.

### D-12: The Outcome Type Is `Enum_ReconciliationOutcome`

**Evidence.** `R-NAM-01` gives `Enum_ReconciliationResolution` as a shape example. FR-032 calls the values "outcomes" and the Verbatim Constraints table names them as such.

**Decision**: the enum is `Enum_ReconciliationOutcome`. Its members are `Unchanged`, `Updated`, `ExtraSurplus`, `NotEnough`, `NotFound`, `Skipped`, `Error`, `ManuallySet`, spelled exactly as the constraints table spells them.

**Why.** The constraint table pins the display names, so the members follow those, and the type noun follows the spec's own word. The rule's example is a naming shape, not a pinned identifier.
