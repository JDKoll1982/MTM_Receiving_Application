# Phase 0 Research: Receiving By Receiver Mode

**Feature**: `007-receiving-by-receiver` | **Plan**: [plan.md](./plan.md)

Each entry records a choice the codebase does not already settle, why it was made, and what else was
weighed. Everything else the plan needs was already fixed by the spec, the verified MTMFG schema, or the
existing receiving conventions.

## D1. The list is a workflow step, not a sub-mode of PO Entry

**Decision**: append `ReceiverSelection = 12` to `Enum_ReceivingWorkflowStep` and host
`ViewModel_Receiving_ReceiverSelection` exactly as `ViewModel_Receiving_POEntry` is hosted.

**Rationale**: Back from Load Entry has to land on the list (FR-014), and `GoToPreviousStep` routes by step.
A step value is also what the visibility switch, `ShowWorkflowBackButton`, and the header bar already read.
Appending rather than inserting keeps every existing numeric value where it is.

**Alternatives considered**: a flag on `POEntry` that swaps its content. Rejected: every step predicate in
the workflow view model would then need the flag, and Back still could not distinguish the two doors.

## D2. The entry mode is recorded on the service, not inferred from the step

**Decision**: `Service_ReceivingWorkflow` gains `IsByReceiverFlow`, set when the walk is opened by the new
mode or the stored default, and cleared when the walk is opened from Guided, Manual, or Edit. `ResolveActiveMode`
reads it before it falls back to the step.

**Rationale**: after a row is chosen the step is `LoadEntry`, which the existing `IsGuidedWalkStep` already
claims for Guided. Without the marker both chips light up and pressing Guided silently does nothing.

**Alternatives considered**: deriving the mode from a non-empty receiver number on the session. Rejected:
it couples mode identity to data that a later step could clear.

## D3. New row model with decimal quantities

**Decision**: `Model_InforVisualReceiverRow` carries the nine displayed values plus the identifiers used for
grouping and highlight, with `QtyReceived`, `QtyOrdered`, and `QtyRemaining` as `decimal`.

**Rationale**: the existing `Model_InforVisualPart.RemainingQuantity` is an `int`. Reusing it would truncate
a summed receipt quantity and would have nowhere to put the vendor, the received date, or the receiver number.
Quantities in MTMFG (`ORDER_QTY`, `TOTAL_RECEIVED_QTY`, `RECEIVED_QTY`) are decimal, and the delivery-schedule
row model already models them that way.

**Alternatives considered**: extending `Model_InforVisualPart` with receiver fields. Rejected: it is the part
page's model and is consumed by validation and load generation that expect a PO line, not a receipt.

## D4. One query, filtered and aggregated in SQL

**Decision**: `Database/InforVisualScripts/Queries/35_GetRecentReceivers.sql` returns the finished rows:
one row per receiver and part, `SUM(RECEIVED_QTY)` as quantity received, remaining clamped with a `CASE`,
`WHERE` on purge marker, part number, and warehouse and location, ordered by received date then receiver
number descending, `TOP (@MaxRows)`.

**Rationale**: every exclusion in the spec is a source-side rule, so applying it in SQL keeps the row count
bounded and removes the risk that a later filter re-introduces a row the rules exclude (the spec says the
search runs on the already-filtered list). The verified join path is
`RECEIVER.ID → RECEIVER_LINE.RECEIVER_ID`, then `PURC_ORDER_ID + PURC_ORDER_LINE_NO → PURC_ORDER_LINE`,
then `PURCHASE_ORDER.VENDOR_ID → VENDOR.ID`.

**Alternatives considered**: reading raw lines and grouping in C#. Rejected: it multiplies rows across the
wire for a list that is display-only, and it moves a rule the database can enforce into a view model.

## D5. A bounded window, filtered and paged in the view model

**Decision**: the query reads a bounded window (`@MaxRows`, 1000 rows) once on entry and on refresh. The view
model applies the contains-match search and the page slice over that window.

**Rationale**: the request asks for "the last # of receivers", so a window is the requirement rather than a
shortcut. Filtering 1000 rows in memory is well inside the one-second budget, and the pager then works on the
filtered count as FR-009 and FR-010 require.

`// simplified: in-memory search and paging over a 1000-row window; move the search term and paging into
@MaxRows and a server-side page parameter when the window stops covering the operator's history`.
Recorded as a concern with this plan.

**Alternatives considered**: passing the search term into SQL on every keystroke. Rejected for now: it puts a
round trip behind typing and needs cancellation to stay inside the one-second budget, for a window the
source can serve in one read.

## D6. A receiver with no received date is excluded

**Decision**: the query requires `RECEIVER.RECEIVED_DATE IS NOT NULL`.

**Rationale**: `RECEIVED_DATE` is nullable, the list is ordered by it, and a receiver with no date has not
been received. The existing receiving-analytics query takes the same stance on `LAST_RECEIVED_DATE`.

**Alternatives considered**: sorting nulls last. Rejected: it puts a receiver that was never received on a
list whose whole purpose is "recently received".

## D7. A dedicated read-only DAO

**Decision**: `Module_Core/Data/InforVisual/Dao_InforVisualReceiver.cs`, constructed with the Infor Visual
connection string and the same `ValidateReadOnlyConnection` guard `Dao_InforVisualPO` uses, returning
`Model_Dao_Result<List<Model_InforVisualReceiverRow>>`.

**Rationale**: it follows the existing one-DAO-per-surface split (`Dao_InforVisualPO`, `Dao_InforVisualPart`)
and carries the read-only guard in its constructor, so no caller can reach the query through an unguarded path.
The alternative of adding it to the 92 KB `Dao_InforVisualConnection.cs` keeps the guard but makes that file
the home of one more unrelated surface.

**Alternatives considered**: extending `Dao_InforVisualConnection`. Rejected as above.

## D8. Load Entry shows the remaining quantity read-only

**Decision**: Load Entry gains a read-only "Qty Remaining on Line" line, bound to a new property fed from the
chosen row, visible only when the walk began from a receiver.

**Rationale**: FR-012 asks for the remaining quantity to be present on Load Entry, and Load Entry currently
captures only location and number of loads. Adding a second quantity input would change the guided walk's
validation and save path, which the spec forbids. A readout satisfies the requirement and leaves the walk
alone.

**Alternatives considered**: seeding `NumberOfLoads` from the remaining quantity. Rejected: the spec does not
ask for it, and it would change what a load means for PO walks too.

## D9. Pager is DevWinUI, page state is `Service_Pagination`

**Decision**: use `dev:PagerControl` with a page-size control offering 25, 50, 100 and 200 and opening on 50,
driven by a `Service_Pagination` instance as Edit Mode already does.

**Rationale**: the spec pins the offered counts and the default in its assumptions. The existing Edit Mode
offers 20/50/100/200 with a default of 20; that difference is deliberate here and stays inside this screen.

**Alternatives considered**: matching Edit Mode's 20/50/100/200. Rejected: the spec records 25/50/100/200 and
50 as the agreed behaviour.

## D10. The highlighted row is remembered on the service

**Decision**: the selection key (receiver id, part number, line number) is stored on the workflow service when
a row is chosen, and the list view model restores the highlight from it when it is shown again.

**Rationale**: view models are transient, so the list is rebuilt on the way back and cannot remember its own
selection. The service already holds the walk's state and is the only object that outlives the step.

**Alternatives considered**: keeping the list view model alive across steps. Rejected: it breaks the module's
transient registration and leaves a stale list in memory.

## D11. "by-receiver" is a first-class default token

**Decision**: the stored token is `by-receiver`, recognised in `StartWorkflowAsync`, in
`NormalizeDefaultMode`, and in the option lists of both settings view models.

**Rationale**: an unrecognised stored value is rewritten to `guided`, so a missing normaliser would silently
undo the operator's choice. The mode list exists twice today, so both copies change together.

**Alternatives considered**: storing `byreceiver`. Rejected: every other token is lower kebab or lower plain,
and the chip `Mode` value should read the same as the stored value.

## D12. The header step bar is built for the walk that was entered

**Decision**: the trail drawn for a by-receiver walk is Receivers → Loads → Weight/Qty → Heat/Lot → Package →
Review. The Guided walk keeps its existing trail unchanged.

**Rationale**: the trail labels also drive the active-step index. Leaving "Enter PO" as step one would tell the
operator they skipped a screen they never had.

**Alternatives considered**: hiding the bar in by-receiver mode. Rejected: the operator loses the only progress
indicator the walk has.

## D13. The mock-data path returns sample receivers

**Decision**: `Service_InforVisualConnect` keeps its `GetUseInforVisualMockData` branch, and the new method
returns a small fixed sample set through `Model_InforVisualMockDataCatalog` when it is on.

**Rationale**: every other Infor Visual method short-circuits for mock data, and a method that returns empty in
development would make this mode look broken in the only environment it can be exercised without MTMFG.

**Alternatives considered**: no mock branch. Rejected: it silently breaks the existing development toggle.
