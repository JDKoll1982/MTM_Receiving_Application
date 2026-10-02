<!-- 
[DOC-META-START]
- File Name: BatchSkidEdit_UX_Specification.md
- Description: End-user UX specification for bulk multi-skid selection, bulk field editing, quantity reconciliation, and batch review inside Module_Receiving Edit Mode.
- Last Updated: 2026-10-02
- Quick TOC:
  - Line 30-38: # Batch Skid Edit — End-User UX Specification
  - Line 39-52: ## Why This Exists
  - Line 53-72: ## The Worked Example From the Attached Request
  - Line 73-104: ## Where This Lives
  - Line 105-176: ## Multi-Select Grid Mechanics
  - Line 177-227: ## The Selection Bar (Bulk Data Presentation)
  - Line 228-345: ## The Batch Edit Overlay
  - Line 346-395: ## Cascading Fields and Dependent Data
  - Line 396-441: ## Quantity Reconciliation
  - Line 442-472: ## Bulk Review Before Commit
  - Line 473-505: ## Bulk Actions Menu
  - Line 506-533: ## Operator Ergonomics and Accessibility
  - Line 534-556: ## Out of Scope
  - Line 557-585: ## Acceptance Criteria
- Critical Notes: Selection truth lives on Model_ReceivingLoad.IsSelected, not on the DataGrid, because the grid is paginated.
[DOC-META-END]
-->

# Batch Skid Edit — End-User UX Specification

Last Updated: 2026-10-02

Audience: implementation engineers turning this into XAML and view-model work, plus the receiving
operators who will actually use it.

## Why This Exists

Today Edit Mode can only present and change one skid at a time. Everything else is row-by-row:

- `EditModeDataGrid` is declared `SelectionMode="Single"` in `View_Receiving_EditMode.xaml`.
- The only working multi-row selection is the per-row `IsSelected` checkbox, and it feeds exactly
  two commands: `RemoveRowCommand` and `SaveCommand`.
- To retag three skids the operator opens each row's cell editor, types the new part, waits for the
  lookup to validate, and repeats. Three skids is tolerable; the requests that actually arrive are
  frequently ten or forty skids that all must move from one part number to another.

This document specifies a batch surface that presents many rows at once, edits them in one action,
prints a report of what changed, and re-labels them via the existing reprint queue.

## The Worked Example From the Attached Request

The attached material-transfer package is the canonical case and every design decision below is
tested against it:

1. Purchasing (Steph) requests the transfer of **154,540 lbs of `MMC000659` into `MMC000767`**.
2. The four receipts that should have been booked as `767` came in as `659` (PO-069993 and
   PO-069993, 9/28 and 9/29, four lines).
3. The printed **Warehouse Locations for Part MMC000659** report dated 10/2/2026 lists seven
   locations:

   | Warehouse ID | Location ID | Quantity | Status     | Locked |
   | ------------ | ----------- | -------- | ---------- | ------ |
   | 002          | NCM         | 25,574    | Unavailable | No     |
   | 002          | R-05        | 59,510    | Available   | No     |
   | 002          | V-B0-00     | 5,820     | Available   | No     |
   | 002          | V-C0-09     | 27,550    | Available   | No     |
   | 002          | V-F0-03     | 63,500    | Available   | No     |
   | 002          | V-F0-07     | 113,624   | Available   | No     |
   | 002          | WC          | 19,490.4  | Available   | No     |

4. The handwritten instruction says to **change the labels on the 3 highlighted locations from
   `MMC000659` to `MMC000767`**, "per Steph".
5. The same note then says: *"There is supposed to be 154,540 lbs. I can only find 136,500 lbs.
   feel free to check my math. I will fix the system when completed."*

Step 5 is the requirement that is currently impossible to satisfy with the shipped UI. The operator
is asked to permanently change physical identities while holding an unresolved arithmetic problem in
their head. The batch surface must do that arithmetic **for** them and must refuse to make the
imbalance invisible.

## Where This Lives

| Concern              | Owning existing surface                        | Change type                |
| -------------------- | ---------------------------------------------- | -------------------------- |
| Grid and toolbar     | `Module_Receiving/Views/` `View_Receiving_EditMode.xaml` | Additive                |
| Selection state      | `Model_ReceivingLoad.IsSelected`               | Reuse as single source     |
| Batch commands       | `ViewModel_Receiving_EditMode`                 | New commands + partials    |
| Overlay dialog       | `Module_Receiving/Dialogs/`                    | New dialog                 |
| Persistence          | `IService_MySQL_Receiving`                     | Reuse, no new stored procs |
| Validation           | `IService_ReceivingValidation`, `Control_Shared_TypedLookupTextBox` | Reuse |

No new database objects are required. A batch apply is the same `UpdateReceivingLoadsAsync` or
`UpdateCurrentLabelDataAsync` call that `SaveAsync` already performs, just with more than one row in
the payload.

## Multi-Select Grid Mechanics

### Current versus proposed

| Element                     | Today               | Proposed                                   |
| ---------------------------------------------- | -------------------------- | ---------------------------- |
| `EditModeDataGrid.SelectionMode` | `Single`       | `Extended`                                 |
| Row checkbox (`IsSelected`) | Only driver of bulk | Mirrors grid selection, remains authoritative |
| Select all                  | `SelectAllCommand` toggles every loaded row regardless of page | Page-scoped, with an explicit "select all N results" promotion |
| Shift-click range           | Not supported       | Supported within the current page          |
| Ctrl-click toggle           | Not supported       | Supported                                  |
| Selection across pages      | Lost, because selection is visual only | Preserved, because it lives on the model |
| Toolbar                     | No batch entry point | New `Batch Edit` group with a live count  |

### The pagination trap (read this before implementing)

`ViewModel_Receiving_EditMode` paginates the grid (`PageSizeOptions` `20 / 50 / 100 / 200`, driven by
`IService_Pagination` and `FilterAndPaginate`). A `DataGrid` in `Extended` mode only knows about rows
that are currently realized. If selection is allowed to live **only** in `SelectedItems`, then:

- paging to page 2 and back silently drops the operator's selection, and
- `RemoveRowCommand` and `SaveCommand`, which already read `Loads.Where(l => l.IsSelected)`, would
  disagree with what the operator sees highlighted.

**Rule:** `Model_ReceivingLoad.IsSelected` stays the single source of truth. The grid is a view of
that state, not the owner of it.

### Selection state contract

```
GridView.SelectionChanged
    -> for each row leaving the grid: if it is no longer in SelectedItems, IsSelected = false
    -> for each row entering the grid: IsSelected = row is in SelectedItems
    -> row checkbox bound to IsSelected stays in visual agreement
    -> SelectedCount and the Selection Bar recompute
```

`Load_PropertyChanged` in the view model already watches `nameof(Model_ReceivingLoad.IsSelected)`;
that existing hook is where the batch command availability and the Selection Bar refresh should be
raised, so there is exactly one notification path.

### Selection rules

| Rule                                | Behavior                                                                 |
| ----------------------------------- | ------------------------------------------------------------------------ |
| Duplicate selection                 | Impossible — selection keys on `Model_ReceivingLoad.LoadID`               |
| Rows hidden by ownership rules      | Cannot be selected. `ApplyOwnershipVisibilityRules` already removes them before they reach the grid; the batch path must not reintroduce them. |
| Rows with an unacknowledged quality hold | Selectable, but marked with a QH badge in the Selection Bar and re-checked at confirm time |
| "Select all" on a filtered result   | First selects the visible page, then offers "Select all 128 results matching this filter" as an explicit second action. Never silently selects off-screen rows. |
| Removing a selected row             | Removes it from the selection and from `_allLoads`, matching existing `RemoveRowAsync` semantics |
| Refresh or reload                   | Selection is cleared, exactly as `HandleCurrentLabelQueueCleared` and load commands already do |

### Visual checked state

Three states must be visually distinct, because operators verify by eye:

- **Unselected** — default row.
- **Selected for batch** — soft accent row background plus the checked leading checkbox.
- **Changed by the pending batch** — accent left edge bar (4px) plus a "modified" glyph, so a
  previewed change is never confused with a mere selection.

The header cell of the checkbox column becomes a tri-state select-all: unchecked, checked, and
indeterminate when only part of the page is selected.

## The Selection Bar (Bulk Data Presentation)

When `SelectedCount >= 1`, a summary strip appears between the toolbar and the grid. It replaces the
current bare result count as the place the operator reads *what am I holding*.

```
┌────────────────────────────────────────────────────────────────────────────────────────────┐
│  ☑ 3 skids selected                                                  [Clear selection]      │
│  Total 236,634 lb   ·   Parts 1 (MMC000659)   ·   Locations 3   ·   Ops none pending        │
│  Expected 154,540 lb      Variance  +82,094 lb   ⚠ does not balance                          │
│  [Batch Edit…] [Print Batch Report] [Reprint Labels] [Remove]                                 │
└────────────────────────────────────────────────────────────────────────────────────────────┘
```

| Field            | Source                                                                 |
| ---------------- | ---------------------------------------------------------------------- |
| Selected count   | Count of `Loads` where `IsSelected`                                     |
| Total quantity   | Sum of `WeightQuantity` over the selection, formatted with `Converter_DecimalToString` |
| Parts            | Distinct `PartID` values; a single value renders as the part, multiple render as "3 parts" with a tooltip |
| Locations        | Distinct `InitialLocation` values                                      |
| Expected total   | Operator-entered, persisted per session, not written to the database    |
| Variance         | `Total − Expected`, with sign, and a status glyph                       |

The bar is always visible when a selection exists, so the operator never has to open the overlay to
learn that their selection does not add up.

## The Batch Edit Overlay

### Trigger and container

- Primary trigger: `Batch Edit…` in the Selection Bar, also reachable from the `Edit` toolbar group.
- Container: a `ContentDialog` hosted by the Edit Mode page, following the existing pattern in
  `Module_Receiving/Dialogs/Dialog_Receiving_EditModeColumnChooser.xaml`.
- Size: `PrimaryButtonText` = `Apply to N skids`, `CloseButtonText` = `Cancel`. The dialog is wide
  (`MinWidth` 720) because it contains a field list and a preview.
- Modal by design: a batch change commits to many rows and must not race with cell edits.

### Field application model

The overlay does **not** present a blank form. It presents the union of editable fields, each with an
explicit "apply this field" toggle. An unticked field is untouched on every selected row. This is the
"cascading/batch override" contract.

| Apply | Field | Current value | New value editor |
| -------- | ---------------------- | ------------------------------ | ---------------------------- |
| ☑ | Part ID | `MMC000659` | Typed lookup, `LookupType` PartNumber |
| ☐ | Heat / Lot # | *(3 different values)* | Text box |
| ☐ | Location | `V-F0-03`, `V-F0-07`, `WC` | Typed lookup, `WarehouseCode` 002 |
| ☐ | Weight / Qty | `236,634` | Numeric box plus operation selector |
| ☐ | Pkgs / Load | `12` | Numeric box |
| ☐ | Package Type | `Skid` | Combo from `PackageTypes` |
| ☐ | PO Number | *(mixed)* | Text box with PO normalization |
| ☐ | User-set variable | *(mixed)* | Text box |
| ☐ | Customer variable | *(mixed)* | Text box |

Cascading behaviour per field, kept out of the table so the columns stay wide enough to read:

- **Part ID** cascades description, UOM, part type, and quality hold, re-deriving each per row.
- **Heat / Lot #** blank is normalized to `Nothing Entered` at save, matching `SaveAsync`.
- **Location** blank means "leave each row's own value".
- **Weight / Qty** supports the operations in the next section.
- **PO Number** reuses the `Model_ReceivingLoad` PO handling.
- **User-set variable** honors `UserSetVariableFieldVisibility`.

### Mixed-value semantics

When the selected rows disagree on a field, the "current value" cell must say so plainly rather
than showing the first row's value:

- All rows equal → show the value.
- All rows empty → show `(blank)` in secondary text color.
- Rows differ → show `(mixed — 3 values)` in italic secondary color, with a tooltip listing up to
  ten distinct values.

If a field is ticked while its current value is mixed, the typed value overwrites every selected row.
That is intentional and must be stated in the dialog: `Applies to all 3 selected skids`.

### Quantity operations

Overwriting is often the wrong operation for a transfer, so the numeric fields expose an operation
selector rather than a bare value:

| Operation      | Meaning                                       | Example                        |
| ---------------- | ------------------------------------------- | ------------------------------ |
| `Set to`       | Replace every row's value                     | Set Weight/Qty to 63,500       |
| `Add`          | Add the amount to every row                   | Add 500 corrective lb          |
| `Subtract`     | Subtract the amount from every row            | Subtract 19,490.4              |
| `Scale by`     | Multiply every row by a factor                | Scale by 1.02                  |
| `Distribute`   | Set one row's value and let the rest adjust to reach a target total | Set the 3 rows to total 154,540 |

`Distribute` is the operation that matches the attached request most literally: the operator knows
the correct grand total but not the per-skid split. It requires a per-row remainder preview and is
the only operation that must show its arithmetic before commit.

### Overlay regions

| Region          | Purpose                                                                  |
| --------------- | ------------------------------------------------------------------------ |
| Header          | "Batch edit 3 skids" plus the source mode (`Memory`, `Current Labels`, or `History`) so the operator knows what they are about to overwrite |
| Field list      | The table above                                                          |
| Reconciliation  | Expected total, selected total, variance, balance status                 |
| Preview grid    | `Skid | Field | Before | After` for every row that will change, scrollable, with a "show only changed fields" toggle |
| Validation pane | Live per-field status; see `Module_Shared/Proposed_Validation_Safeguards.md` |
| Footer          | `Apply to 3 skids` (primary, disabled while validation is failing or in flight) and `Cancel` |

## Cascading Fields and Dependent Data

A batch Part ID change is not a string replacement. The following must cascade and be re-resolved for
each affected row, not copied from the first row:

1. `PartDescription`, `PartType`, `UnitOfMeasure`, and `QtyOrdered` are re-derived from the part.
2. `IsQualityHoldRequired` and `QualityHoldRestrictionType` are re-derived through
   `IService_ReceivingValidation.IsRestrictedPartAsync`, which `SaveAsync` already calls per load.
3. `WeightPerPackage` and package metadata are kept, because the physical skid does not change.
4. PO linkage is preserved unless the operator ticks PO Number, because a retag is a correction of
   identity, not a re-receipt.
5. `InitialLocation` is preserved per row, because the three skids sit in three different locations.

The preview grid must display these cascaded values, so the operator sees the part description flip
from the 659 description to the 767 description before committing.

## Quantity Reconciliation

Reconciliation is a first-class field, not a decoration. It directly answers the handwritten
"feel free to check my math" note.

| Control        | Behavior                                                                 |
| -------------- | ------------------------------------------------------------------------ |
| Expected total | Operator types `154540` from the transfer request. Persisted per session only. |
| Selected total | Computed sum of `WeightQuantity` across the selection                    |
| Variance       | `Selected − Expected`, always signed, never hidden                       |
| Balance state  | `Balanced` (variance 0), `Over` (positive), `Short` (negative)           |

### Severity rules

| Condition                            | Presentation                                                        | Apply button |
| ------------------------------------ | ------------------------------------------------------------------- | ------------ |
| No expected total entered            | Neutral; reconciliation reads "No expected total entered"            | Enabled      |
| Variance is zero                     | Green check, "Balances to 154,540 lb"                                | Enabled      |
| Variance within the configured tolerance | Amber, "Short by 12 lb (within 25 lb tolerance)"                  | Enabled      |
| Variance outside tolerance           | Amber banner, "Short by 18,040 lb — 3 skids total 136,500 lb"        | Enabled, but requires an explicit confirmation checkbox |
| Expected total entered, zero rows selected | Not reachable; the overlay requires a selection                 | Disabled     |

The tool must never auto-correct the expected total to match the found total. If the operator cannot
reconcile, the batch applies and the imbalance is recorded and reported — matching the note's own
"feel free to check my math" spirit — but it is impossible to miss.

Tolerance is a setting, not a constant, so it can be tuned without a rebuild.

## Bulk Review Before Commit

| Step | Operator sees                                                        |
| ---- | -------------------------------------------------------------------- |
| 1    | Select rows in the grid; Selection Bar totals update live             |
| 2    | Enter expected total; variance appears                                 |
| 3    | Open `Batch Edit…`; tick fields; type values                          |
| 4    | Preview grid lists every before/after pair                            |
| 5    | Validation pane resolves to green or lists blockers                    |
| 6    | `Apply to 3 skids` writes the changes into the in-memory rows only     |
| 7    | Dialog closes; changed rows carry the modified marker; Save is pending |
| 8    | Operator runs `Save & Finish` (`SaveAsync`) to persist                 |

Step 6 writing only into memory is the safety property: nothing reaches MySQL until the existing
`SaveAsync` persistence path runs, so `Cancel` and app close remain non-destructive.

## Bulk Actions Menu

Because the request is broader than editing, the Selection Bar carries the full bulk surface:

| Action                | Effect                                                                  |
| --------------------- | ----------------------------------------------------------------------- |
| Batch Edit…           | Opens the overlay described above                                       |
| Print Batch Report    | Builds a printable change report (see `BatchReprint_Automation_Spec.md`) |
| Reprint Labels        | Hands the affected skids to `Module_Reprint`                            |
| Remove                | Existing `RemoveRowCommand` semantics, unchanged                        |
| Clear selection       | Unchecks every selected row                                             |
| Copy as table         | Optional: clipboard export of the selection, reusing the reporting clipboard pattern |

## Operator Ergonomics and Accessibility

| Requirement            | Detail                                                                 |
| ---------------------- | ---------------------------------------------------------------------- |
| Scanner-first          | Part ID and Location editors accept a scanner wedge into the typed lookup control directly |
| Keyboard only          | `Ctrl+A` selects the page, `Space` toggles the focused row, `Shift+Space` extends, `Ctrl+Shift+E` opens Batch Edit |
| Touch                  | 40px minimum hit targets; checkbox column width 50px stays as-is        |
| Screen reader          | `AutomationProperties.Name` on every toolbar and dialog control; Selection Bar announced via a polite live region on count change |
| Narrow window          | Field list and preview stack vertically under 900px window width using `VisualStateManager`, matching the adaptive pattern in `View_Reprint_ModulePage.xaml` |
| Long operations        | Progress ring plus status text; the window never blocks                 |

## Out of Scope

- No schema or stored-procedure changes. Batch apply reuses `UpdateReceivingLoadsAsync` and
  `UpdateCurrentLabelDataAsync`.
- No change to `SaveAsync` source-mode semantics for `Memory`, `CurrentLabels`, or `History`.
- No change to ownership visibility rules; they remain the gate on what is selectable.
- No bulk create of new skids. This surface edits existing rows.
- No Infor Visual writes. That database remains read-only.

## Acceptance Criteria

- [ ] Three skids can be filtered, multi-selected, retagged to `MMC000767`, previewed, and applied in
      one dialog interaction.
- [ ] Selection survives paging between pages of the grid.
- [ ] The Selection Bar total matches the sum of the checked rows at all times.
- [ ] Entering `154540` as the expected total immediately surfaces the variance and its severity.
- [ ] An out-of-tolerance variance cannot be committed without an explicit acknowledgement.
- [ ] Mixed-value fields render as mixed, never as the first row's value.
- [ ] No database write occurs until `Save & Finish` is pressed.
- [ ] All new labels come from `ReceivingSettingsKeys.UiText`, consistent with existing Edit Mode text.
