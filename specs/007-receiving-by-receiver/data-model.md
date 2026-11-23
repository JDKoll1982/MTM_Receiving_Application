# Data Model: Receiving By Receiver Mode

**Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md) | **Research**: [research.md](research.md)
**Created**: 2026-11-23

This feature introduces no database table, column, stored procedure, or schema change of any kind. Every
value it shows is read from Infor Visual through a new read-only query, and no write is ever issued against
the source system (FR-016). The receiving workflow session, the shell action bar state, and the receiving
mode default are reshaped, not replaced. The only brand-new persistent surface is the stored default token
`by-receiver`, which lives in the preference store the shell action bar feature already uses.

Types split exactly as the plan's Structure Decision states: the row model and the read-only DAO live in
`Module_Core` beside every other Infor Visual surface, the list view model and workflow behaviour live in
`Module_Receiving`, and the shell projection gains one mirror in `Module_Shared`.

## Entity Overview

```text
Model_InforVisualReceiverRow                 Module_Core, new: one listed receiver/part row (read-only)
        -> Dao_InforVisualReceiver            Module_Core, new: runs 35_GetRecentReceivers.sql, read-only
                -> IService_InforVisual.GetRecentReceiversAsync   new contract member
                        -> Service_InforVisualConnect             pass-through + mock branch
        -> ViewModel_Receiving_ReceiverSelection   Module_Receiving, new: search, paging, selection

Service_ReceivingWorkflow                    reshaped: IsByReceiverFlow, SelectedReceiver, highlight key
        -> Enum_ReceivingWorkflowStep        reshaped: ReceiverSelection = 12 appended
        -> Model_ReceivingSession            unchanged: receiver number informational, no new column

Model_ShellActionBarState                    reshaped: ByReceiver chip + IsByReceiverDefault
        -> ViewModel_Shared_ShellActionBar   reshaped: ByReceiver mirror
        -> default mode token                by-receiver, normalised in StartWorkflowAsync / NormalizeDefaultMode

RECEIVER / RECEIVER_LINE / PURC_ORDER_LINE / PURCHASE_ORDER / VENDOR   Infor Visual, read only
```

## Model_InforVisualReceiverRow

The one listed row, in `Module_Core/Models/InforVisual/`. It is a view of source-system data and is never
stored locally. Quantities are `decimal`, not the `int` the part-page model uses, because MTMFG quantities
are decimal and a summed receipt quantity must not truncate (D3).

| Field | Type | Notes |
| --- | --- | --- |
| `ReceiverNumber` | `string` | the receiver number shown in column one and matched by search |
| `ReceivedDate` | `DateTime` | ordering key; not a column, but drives FR-004 ordering |
| `PONumber` | `string` | from the purchase order the line belongs to |
| `POLineNumber` | `string` | the displayed line number; first PO line found when several lines share the part |
| `PartNumber` | `string` | the part the row receives; never empty (FR-005) |
| `VendorName` | `string` | vendor name, falling back to the vendor identifier when no name exists |
| `Warehouse` | `string` | the receiving line's warehouse; the inventory-receipt gate (FR-006) |
| `LocationReceivedTo` | `string` | the receiving line's location; blank allowed, in which case the warehouse alone shows |
| `QtyReceived` | `decimal` | `SUM(RECEIVED_QTY)` across the receiver's lines for this part (FR-008) |
| `QtyOrdered` | `decimal` | the PO line's ordered quantity |
| `QtyRemaining` | `decimal` | ordered minus total received on the PO line, clamped to zero (FR-013) |

The three identifier fields used for grouping and highlight — receiver id, part number, and line number —
are the row's selection key. They are carried beside the displayed values so the list view model can
restore the highlight and so the workflow can route Back to the row the operator picked (D10).

**Rules**

- One row is produced per receiver and part. A receiver with several lines for the same part collapses to
  one row whose `QtyReceived` is the sum of those lines, and whose `POLineNumber` is the first PO line
  found for that part (FR-008, Assumptions).
- A receiver with several distinct part numbers produces one row per part, each with its own PO line
  (Edge Cases).
- `QtyRemaining` is ordered quantity minus the PO line's total received quantity, and is zero, never
  negative, when the line is over-received (FR-013).
- `VendorName` uses the vendor name and falls back to the vendor identifier when no name exists
  (Assumptions).
- `LocationReceivedTo` may be blank while `Warehouse` is filled; that row is still an inventory receipt
  and displays the warehouse alone (Edge Cases).

## The read path

### 35_GetRecentReceivers.sql

The source of every row, in `Database/InforVisualScripts/Queries/`. It is a single read-only SELECT,
not a stored procedure — Infor Visual has none — and contains no statement that writes.

The verified join path is `RECEIVER.ID -> RECEIVER_LINE.RECEIVER_ID`, then
`PURC_ORDER_ID + PURC_ORDER_LINE_NO -> PURC_ORDER_LINE`, then `PURCHASE_ORDER.VENDOR_ID -> VENDOR.ID`
(D4).

The query applies every source-side exclusion, so the search can never re-surface a row the rules exclude:

| Rule | Source predicate |
| --- | --- |
| Received, not just entered | `RECEIVER.RECEIVED_DATE IS NOT NULL` (D6) |
| Part exists | part number is not null and not empty (FR-005) |
| Inventory is added | line names a warehouse; location may be blank (FR-006, Edge Cases) |
| Not removed | receiver not marked for removal or purge (FR-007) |
| PO line still exists | the join to `PURC_ORDER_LINE` must resolve, so a vanished line drops the row (Edge Cases) |

The result shape:

| Field | Derivation |
| --- | --- |
| `ReceiverNumber` | from `RECEIVER` |
| `ReceivedDate` | `RECEIVER.RECEIVED_DATE` |
| `PONumber` | from `PURCHASE_ORDER` |
| `POLineNumber` | first `PURC_ORDER_LINE.LINE_NO` for the part within the receiver |
| `PartNumber` | from `PURC_ORDER_LINE` |
| `VendorName` | vendor name with identifier fallback |
| `Warehouse`, `LocationReceivedTo` | from `RECEIVER_LINE` |
| `QtyReceived` | `SUM(RECEIVER_LINE.RECEIVED_QTY)` per receiver and part |
| `QtyOrdered` | `PURC_ORDER_LINE.ORDER_QTY` |
| `QtyRemaining` | `ORDER_QTY - TOTAL_RECEIVED_QTY`, clamped to zero with a `CASE` (FR-013) |

Rows are ordered by `ReceivedDate` descending, then `ReceiverNumber` descending, and the window is bounded
with `TOP (@MaxRows)` at 1000 rows (D4, D5, FR-004).

### Dao_InforVisualReceiver

`Module_Core/Data/InforVisual/Dao_InforVisualReceiver.cs`, constructed with the Infor Visual connection
string and the same `ValidateReadOnlyConnection` guard `Dao_InforVisualPO` carries. It returns
`Model_Dao_Result<List<Model_InforVisualReceiverRow>>`, so a source failure is a result, never an
exception (D7).

`IService_InforVisual` gains `GetRecentReceiversAsync`, and `Service_InforVisualConnect` adds the
pass-through. Its existing `GetUseInforVisualMockData` branch returns a small fixed sample set through
`Model_InforVisualMockDataCatalog`, so the mode is exercisable without MTMFG (D13).

## Enum_ReceivingWorkflowStep

Reshaped by appending one member. Appending rather than inserting keeps every existing numeric value
where it is, so the step bar, the keyboard gates, and the persisted session are unaffected (D1).

| Member | Value | Notes |
| --- | --- | --- |
| `ManualEntry` through `Complete` | 1..11 | unchanged |
| `ReceiverSelection` | 12 | appended; the By Receiver list screen |

**Rule**: `ReceiverSelection` is hosted by the workflow exactly as `POEntry` is, so
`GoToPreviousStep`, the visibility switch, `ShowWorkflowBackButton`, and the header bar all read it
without a special case.

## Receiving workflow session

`Service_ReceivingWorkflow` keeps its existing session and gains the receiver state the walk needs. The
persisted `Model_ReceivingSession` is unchanged; the receiver number is informational on later steps and
the saved record keeps the existing receiving schema (Assumptions).

| Member | Type | Notes |
| --- | --- | --- |
| `IsByReceiverFlow` | `bool` | set when the walk opens from the new mode or the stored default; cleared when opened from Guided, Manual, or Edit (D2) |
| `SelectedReceiver` | `Model_InforVisualReceiverRow?` | the chosen row, carried into workflow state (FR-011) |
| selection key | receiver id + part number + line number | stored on the service so the transient list view model can restore the highlight on the way back (D10) |

**Rules**

- `ResolveActiveMode` reads `IsByReceiverFlow` before it falls back to the step, so Guided does not light
  up for the whole by-receiver walk once the step is Load Entry (D2).
- Selecting a row fills `CurrentPONumber` and `CurrentPart` before the step changes, because
  `GenerateLoads` reads both (plan, call paths).
- Back from Load Entry, when the walk began from a receiver row, routes to `ReceiverSelection` with the
  row still highlighted; the list requires no re-selection (FR-014, Assumptions).

## Load Entry readout

`ViewModel_Receiving_LoadEntry` gains one read-only value, fed from the chosen row, visible only when the
walk began from a receiver (D8). It is a display, not an input, so the guided walk's validation and save
path stay exactly as they are.

| Field | Type | Notes |
| --- | --- | --- |
| `QtyRemainingOnLine` | `decimal` | the chosen row's remaining quantity, shown read-only (FR-012) |
| `ShowQtyRemainingOnLine` | `bool` | true only in a by-receiver flow; hidden for PO, Manual, and Edit walks |

## Receiving mode and shell action bar

The mode list gains a fifth entry positioned between Guided and Manual (FR-001). The shell action bar
feature's snapshot is extended rather than redesigned.

| Member | Notes |
| --- | --- |
| `Model_ShellActionBarState.ByReceiver` | new `Model_ShellModeAction`, mode value `by-receiver`, positioned between Guided and Manual |
| `Model_ShellActionBarState.IsByReceiverDefault` | which checkbox is ticked; at most one default is true |
| `ViewModel_Shared_ShellActionBar.ByReceiver` | the projection mirror the bar binds with `x:Bind` |
| `MainWindow.xaml` | the chip, between Guided and Manual |

**Rules**

- Pressing the By Receiver chip enters the mode for the current session and never changes the stored
  default; the checkbox is a separate control that persists it.
- At most one of `IsGuidedDefault`, `IsByReceiverDefault`, `IsManualDefault`, and `IsEditDefault` is true.

## User default receiving mode

The stored default now recognises a fifth token. An unrecognised stored value is rewritten to `guided`,
so the normaliser must accept `by-receiver` or it silently undoes the operator's choice (D11).

| Value | Meaning |
| --- | --- |
| `guided` | the guided walk |
| `by-receiver` | By Receiver, the new list-first walk |
| `manual` | Manual Entry |
| `edit` | Edit Mode |
| `null` / empty / unrecognised | no stored default; resolves to `guided` |

`by-receiver` is recognised in `StartWorkflowAsync`, in `NormalizeDefaultMode`, and in the option lists of
both settings view models, so the token, the chip, and the stored value read the same (D11).

**State transitions**

```text
NoDefault --open Receiving--> Guided (stored back as guided)
DefaultByReceiver --open Receiving--> ReceiverSelection (list first)
AnyMode --tick another checkbox--> NewDefault (previous default cleared)
OnlyDefault --untick last checkbox--> NoDefault
```

## Settings

The page size and the list text live under `ReceivingSettingsKeys` and `ReceivingSettingsDefaults` in
`Module_Receiving/Settings/`.

| Setting | Values | Default |
| --- | --- | --- |
| Page size | 25, 50, 100, 200 | 50 (Assumptions) |
| Receiver list text | column headers, search, pager, empty and failure wording | from `ReceivingSettingsKeys` |

The column headers are fixed verbatim, in order: `Receiver Number`, `PO #`, `Line #`, `Part Number`,
`Vendor`, `Location Received to`, `Qty Received`, `Qty Ordered`, `Qty Remaining on Line`.

## List lifecycle

The receiver list view model has six states, matching the plan's States section. Search and paging run
over the in-memory window, so changing the page size or the search text returns to the first page without
leaving `Loaded` (D5, Edge Cases).

- **Idle**: the mode is not open and nothing has been read.
- **Loading**: the query is in flight and the list is still empty.
- **Loaded**: rows are on screen; search and paging are live.
- **Empty**: the query succeeded and nothing matched the search; an empty-state message is shown, never an
  error (FR-009, US2-3).
- **Failed**: the query failed; a readable message with a Retry action is shown (FR-018).
- **Chosen**: a row is held in workflow state and Load Entry is next.

Transitions:

| From | To | When |
| --- | --- | --- |
| Idle | Loading | the mode is entered or the list is refreshed (FR-017) |
| Loading | Loaded | rows returned |
| Loading | Empty | no rows returned |
| Loading | Failed | the source returned a failure |
| Empty | Loading | the search is cleared or Retry is pressed |
| Failed | Loading | Retry is pressed |
| Loaded | Chosen | a row is selected |
| Chosen | Loaded | Back from Load Entry |

## Reused types

The feature reuses the existing `Service_Pagination` for page state (driven by a DevWinUI `PagerControl`,
already referenced), the existing `IService_ErrorHandler` for the failure and Retry surface, the existing
`Helper_SqlQueryLoader` for the SQL file, and the existing `PreviousStepCommand` / `NextStepCommand` the
walk already owns. No new package, table, column, or stored procedure is added.
