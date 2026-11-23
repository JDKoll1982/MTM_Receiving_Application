# Tasks: Receiving By Receiver Mode

**Feature**: `007-receiving-by-receiver` | **Spec**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md)
**Size**: normal

## How the files are owned

The receiver list screen is one view model and one view, and four stories read it: US1 puts it on screen,
US2 searches it, US3 proves what the source returns, US4 pages it. Because one file cannot have four
owners, the whole list surface is built once in Phase 2 before any story starts, and no story phase edits
it again. US2, US3 and US4 therefore own their tests and their checkpoint; US5 owns the two settings
option lists, which are its only production files.

Test conventions: xUnit + FluentAssertions, `MethodName_ShouldResult_WhenCondition`, Infor Visual reached
through a Moq fake of `IService_InforVisual`. Integration tests implement `IAsyncLifetime` and skip unless
an Infor Visual test connection is configured, because the source is read-only and nothing can be seeded.

## Phase 1: Setup

Nothing to set up. No new package (DevWinUI 10.9.1 already supplies the pager), no new project, and no
configuration: `Database/InforVisualScripts/Queries/**/*.sql` is already an embedded-resource glob
(`MTM_Receiving_Application.csproj:376`), so a new `.sql` file ships without a project edit.

## Phase 2: Foundational

Files: `Module_Core/Models/Enums/Enum_ReceivingWorkflowStep.cs`,
`Module_Core/Models/InforVisual/Model_InforVisualReceiverRow.cs`,
`Database/InforVisualScripts/Queries/35_GetRecentReceivers.sql`,
`Module_Receiving/Settings/ReceivingSettingsKeys.cs`, `Module_Receiving/Settings/ReceivingSettingsDefaults.cs`,
`Module_Core/Contracts/Services/IService_InforVisual.cs`,
`Module_Core/Models/InforVisual/Model_InforVisualMockDataCatalog.cs`,
`Module_Settings.Core/Defaults/inforvisual-mock-data.json`,
`Module_Core/Data/InforVisual/Dao_InforVisualReceiver.cs`,
`Module_Receiving/Services/Service_ReceivingWorkflow.cs`,
`Module_Core/Services/Database/Service_InforVisualConnect.cs`,
`Module_Receiving/ViewModels/ViewModel_Receiving_ReceiverSelection.cs`,
`Module_Receiving/Views/View_Receiving_ReceiverSelection.xaml` (+ `.xaml.cs`),
`Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`

**Wave 1 — independent (different files):**

- [x] **T001** [P] Append `ReceiverSelection = 12` so every existing step value stays put · `Module_Core/Models/Enums/Enum_ReceivingWorkflowStep.cs`
- [x] **T002** [P] New read-only row model: `ReceiverNumber`, `ReceivedDate`, `PONumber`, `POLineNumber`, `PartNumber`, `VendorName`, `Warehouse`, `LocationReceivedTo`, and `QtyReceived`/`QtyOrdered`/`QtyRemaining` as `decimal`, plus the `receiver|part|line` selection key · `Module_Core/Models/InforVisual/Model_InforVisualReceiverRow.cs`
- [x] **T003** [P] New read-only query: inner joins `RECEIVER` → `RECEIVER_LINE` → `PURC_ORDER_LINE` → `PURCHASE_ORDER` → `VENDOR`, requires `RECEIVED_DATE IS NOT NULL`, non-blank part and warehouse, no purge marker, groups one row per receiver and part with `SUM(RECEIVED_QTY)`, clamps remaining with `CASE`, orders `RECEIVED_DATE DESC, RECEIVER.ID DESC`, bounds with `TOP (@MaxRows)` · `Database/InforVisualScripts/Queries/35_GetRecentReceivers.sql`
- [x] **T004** [P] Add receiver-list keys and defaults: the nine verbatim headers in order, search placeholder, rows-per-page label with `25/50/100/200` and a `50` default, refresh, previous/next page and page indicator, empty and failure copy, step title `Receiving - By Receiver`, and accessibility names · `Module_Receiving/Settings/ReceivingSettingsKeys.cs`, `Module_Receiving/Settings/ReceivingSettingsDefaults.cs`
- [x] **T005** [P] Declare `Task<Model_Dao_Result<List<Model_InforVisualReceiverRow>>> GetRecentReceiversAsync(int maxRows = 1000)` with the read-only doc comment · `Module_Core/Contracts/Services/IService_InforVisual.cs`
- [x] **T006** [P] Add a `RecentReceivers` sample set to the catalog model and its JSON defaults, so mock mode shows the mode working · `Module_Core/Models/InforVisual/Model_InforVisualMockDataCatalog.cs`, `Module_Settings.Core/Defaults/inforvisual-mock-data.json`

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T007** [P] (T002, T003) New dedicated DAO: `ValidateReadOnlyConnection` in the constructor exactly as `Dao_InforVisualPO` does, loads `35_GetRecentReceivers.sql` through `Helper_SqlQueryLoader.LoadAndPrepareQuery`, binds `@MaxRows`, maps every column, and returns failures as `Model_Dao_Result` rather than throwing · `Module_Core/Data/InforVisual/Dao_InforVisualReceiver.cs`
- [x] **T008** [P] (T001, T002) Workflow service: add `IsByReceiverFlow`, `SelectedReceiver`, `SelectedReceiverKey`, `CurrentReceiverRemainingQty`; open at `ReceiverSelection` and set the flag when the stored mode is `by-receiver`; accept `by-receiver` when normalising the stored default; route `GoToPreviousStep` from `LoadEntry` back to `ReceiverSelection` in this flow; clear all four in `ResetWorkflowAsync` · `Module_Receiving/Services/Service_ReceivingWorkflow.cs`

**⟶ Wait for T007 to finish, then:**

- [x] **T009** (T005, T006, T007) Implement the pass-through and the `GetUseInforVisualMockData` branch that returns the sample receivers · `Module_Core/Services/Database/Service_InforVisualConnect.cs`

**⟶ Wait for T009 to finish, then:**

- [x] **T010** (T004, T008, T009) List view model: read one bounded 1000-row window, drive Idle/Loading/Loaded/Empty/Failed/Chosen, filter on a contains match of the receiver number, slice the page with `Service_Pagination` and the chosen page size, reset to page one on a search or page-size change, refresh and retry in place, and on selection fill the nine values on the workflow service before stepping to `LoadEntry`; restore the highlight from `SelectedReceiverKey` · `Module_Receiving/ViewModels/ViewModel_Receiving_ReceiverSelection.cs`

**⟶ Wait for T010 to finish, then:**

- [x] **T011** (T010) List view: the nine columns in the contract order, search box, rows-per-page control, refresh, pager with current page and total, and the empty and failure surfaces with Retry, all through `x:Bind` with no code-behind logic · `Module_Receiving/Views/View_Receiving_ReceiverSelection.xaml`, `Module_Receiving/Views/View_Receiving_ReceiverSelection.xaml.cs`

**⟶ Wait for T011 to finish, then:**

- [x] **T012** (T011) Register `ViewModel_Receiving_ReceiverSelection` and `View_Receiving_ReceiverSelection` as transient, beside the other receiving steps · `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`

## Phase 3: User Story 1 — Start a guided entry from a receiver instead of a PO number (P1)

**Goal**: the operator picks a recent receiver row and lands on Load Entry with that row already carried.

**Independent Test**: enter Receiving, switch to By Receiver, select a row, confirm the walk resumes at
Load Entry with the row's part, location and remaining quantity present, complete through Review and save.

Files: `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs`,
`Module_Receiving/Views/View_Receiving_Workflow.xaml` (+ `.xaml.cs`),
`Module_Receiving/ViewModels/ViewModel_Receiving_LoadEntry.cs`,
`Module_Receiving/Views/View_Receiving_LoadEntry.xaml`,
`Module_Core/Models/UI/Model_ShellActionBarState.cs`,
`Module_Shared/ViewModels/ViewModel_Shared_ShellActionBar.cs`, `MainWindow.xaml`

### Tests

- [x] **T013** [P] [US1] FR-001: the shell snapshot offers a visible chip labelled exactly `By Receiver`, and `MainWindow.xaml` places its chip between `ShellGuidedButton` and `ShellManualButton` · `MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_ShellByReceiverTests.cs`
- [x] **T014** [P] [US1] FR-002: activating By Receiver leaves the current step at `ReceiverSelection`, the list visible and PO entry hidden, while Guided still opens on PO entry · `MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_ByReceiverEntryTests.cs`
- [x] **T015** [P] [US1] FR-003: the list view declares the nine headers verbatim and in order, `Receiver Number` through `Qty Remaining on Line` · `MTM_Receiving_Application.Tests/Unit/Module_Receiving/Views/View_Receiving_ReceiverSelectionColumnsTests.cs`
- [x] **T016** [P] [US1] FR-011: selecting a row puts receiver number, PO number, line number, part number, vendor, location received to, qty received, qty ordered and qty remaining on the workflow state · `MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_ReceiverSelectionCarryTests.cs`
- [x] **T017** [P] [US1] FR-012: Load Entry reached from a row pre-fills the location received to, shows the row's remaining quantity in a read-only `Qty Remaining on Line` readout, and hides that readout in a Guided walk · `MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_LoadEntryByReceiverTests.cs`
- [x] **T018** [P] [US1] FR-014: Back from Load Entry in a by-receiver walk returns to `ReceiverSelection` with `SelectedReceiverKey` unchanged, and a Guided walk still returns to its own previous step · `MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_ByReceiverBackTests.cs`
- [x] **T019** [P] [US1] FR-016: the DAO constructor rejects a connection string whose `ApplicationIntent` is not `ReadOnly`, and the query file contains no `INSERT`, `UPDATE`, `DELETE`, `MERGE` or `EXEC` · `MTM_Receiving_Application.Tests/Unit/Module_Core/Data/Dao_InforVisualReceiverTests.cs`
- [x] **T020** [P] [US1] FR-017: entering the mode reads the source once, and Refresh reads it again without leaving the mode · `MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_ByReceiverRefreshTests.cs`
- [x] **T021** [P] [US1] FR-018: a failed source read leaves the list in its failed state with a readable message and a Retry that issues a second read and recovers · `MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_ByReceiverFailureTests.cs`

**⟶ Wait for the tests above to fail, then:**

### Implementation

**Wave 1 — independent (different files):**

- [x] **T022** [P] [US1] Workflow view model: host `ReceiverSelection` like `POEntry` (`IsReceiverSelectionVisible`, the step switch), add the By Receiver chip and its default checkbox, draw the by-receiver trail starting at Receivers, make `ResolveActiveMode` read `IsByReceiverFlow` before the step, and enter the mode through the service · `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs`
- [x] **T023** [P] [US1] Load Entry view model: expose `QtyRemainingOnLine` and `ShowQtyRemainingOnLine` from the chosen row and the by-receiver flow, refreshed on step entry · `Module_Receiving/ViewModels/ViewModel_Receiving_LoadEntry.cs`
- [x] **T024** [P] [US1] Shell snapshot: add `ByReceiver` and `IsByReceiverDefault`, defaulting to hidden and false · `Module_Core/Models/UI/Model_ShellActionBarState.cs`
- [x] **T025** [P] [US1] Draw the By Receiver chip between the Guided and Manual chips, with its own divider and default badge · `MainWindow.xaml`

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T026** [P] (T024) Mirror `ByReceiver` on the shared shell view model the bar binds to · `Module_Shared/ViewModels/ViewModel_Shared_ShellActionBar.cs`
- [x] **T027** [P] (T022) Host the receiver list in a `ContentControl` beside `POEntryHost`, bound to the new visibility, and resolve its view in code-behind as the other steps are · `Module_Receiving/Views/View_Receiving_Workflow.xaml`, `Module_Receiving/Views/View_Receiving_Workflow.xaml.cs`
- [x] **T028** [P] (T023) Draw the read-only `Qty Remaining on Line` readout on Load Entry, visible only in a by-receiver walk · `Module_Receiving/Views/View_Receiving_LoadEntry.xaml`

**Checkpoint**: By Receiver is reachable between Guided and Manual, the list is its first screen, a chosen
row resumes the walk at Load Entry with location and remaining quantity present, Back returns to the
highlighted row, and US1 is independently functional and testable.

## Phase 4: User Story 2 — Find one receiver by typing its number (P1)

**Goal**: typing part of a receiver number narrows the list; clearing it restores the whole list.

**Independent Test**: type a known receiver number and confirm only rows whose number contains those
digits remain, then clear the box and confirm the full list returns on page one.

Files: none of its own beyond tests — the search path lives in
`ViewModel_Receiving_ReceiverSelection.cs`, owned by Phase 2.

### Tests

- [x] **T029** [US2] FR-009: typing digits keeps only rows whose receiver number contains them, clearing the box restores the full list at page one, a no-match search shows the empty message rather than an error, and the pager total follows the filtered count · `MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_ReceiverSearchTests.cs`

### Implementation

None of its own. Phase 2's list view model already carries the contains-match filter and the page-one
reset; if T029 fails, the defect is fixed there, not in a second search surface.

**Checkpoint**: US2 is independently functional and testable against the shared list surface.

## Phase 5: User Story 3 — Only inventory receipts appear, newest first (P2)

**Goal**: every row is something the operator can receive against, and the newest receipt is first.

**Independent Test**: compare the rows shown against the same window in Infor Visual and confirm no row
lacks a part or a receiving location, none carries a removal marker, and the newest received date is first.

Files: none of its own beyond tests — the rules are the source predicates in
`35_GetRecentReceivers.sql`, owned by Phase 2.

### Tests

- [x] **T030** [US3] Shared integration harness: `IAsyncLifetime`, resolves a read-only Infor Visual connection from `MTM_INFORVISUAL_TEST_CONNECTION`, and skips the caller when it is unset · `MTM_Receiving_Application.Tests/Integration/InforVisual/InforVisualReceiverTestConnection.cs`

**⟶ Wait for T030 to finish, then:**

- [x] **T031** [P] [US3] FR-004: over the returned window, `ReceivedDate` never increases and equal dates run by receiver number descending · `MTM_Receiving_Application.Tests/Integration/InforVisual/Dao_InforVisualReceiverOrderingTests.cs`
- [x] **T032** [P] [US3] FR-005: no returned row has a blank part number · `MTM_Receiving_Application.Tests/Integration/InforVisual/Dao_InforVisualReceiverPartNumberTests.cs`
- [x] **T033** [P] [US3] FR-006: no returned row has a blank warehouse, and a blank location still yields the warehouse · `MTM_Receiving_Application.Tests/Integration/InforVisual/Dao_InforVisualReceiverInventoryTests.cs`
- [x] **T034** [P] [US3] FR-007: no returned receiver carries a purge or removal marker in the source · `MTM_Receiving_Application.Tests/Integration/InforVisual/Dao_InforVisualReceiverRemovalTests.cs`
- [x] **T035** [P] [US3] FR-008: no (receiver, part) pair repeats, and for one row a read-only recount of that receiver's `RECEIVED_QTY` lines for the part equals the row's qty received · `MTM_Receiving_Application.Tests/Integration/InforVisual/Dao_InforVisualReceiverMergeTests.cs`
- [x] **T036** [P] [US3] FR-013: no row shows a qty remaining below zero, and an over-received line returns exactly zero · `MTM_Receiving_Application.Tests/Integration/InforVisual/Dao_InforVisualReceiverRemainingTests.cs`

### Implementation

None of its own. The predicates, grouping, clamp and ordering are in the query built by T003 and read by
T007; if a T031–T036 test fails, the defect is fixed in that query, not worked around in the view model.

**Checkpoint**: US3's source rules are proven against the live schema, and US3 is independently testable.

## Phase 6: User Story 4 — Choose how many receivers a page holds (P2)

**Goal**: the operator sets rows per page and moves between pages of the filtered list.

**Independent Test**: change the page size, confirm the row count matches, page forward and back, and
confirm the receiver range stays consistent.

Files: none of its own beyond tests — the page slice lives in
`ViewModel_Receiving_ReceiverSelection.cs`, owned by Phase 2.

### Tests

- [x] **T037** [US4] FR-010: the page-size control offers `25`, `50`, `100`, `200` and opens on `50`; changing it redisplays from page one with that many rows; next and previous move the page and update the indicator; one page of results leaves the controls present but inactive; a row selected on page two is the row carried · `MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_ReceiverPagingTests.cs`

### Implementation

None of its own. The page slice, page-size reset and pager state are in Phase 2's list view model and its
view; a failure here is fixed there so one page model serves every reader.

**Checkpoint**: US4 is independently functional and testable against the shared list surface.

## Phase 7: User Story 5 — Keep "By Receiver" as a personal default (P3)

**Goal**: `By Receiver` is one of the modes a user can store as their default, and Receiving opens in it.

**Independent Test**: set By Receiver as the default in Receiving settings, reopen Receiving and confirm
the receiver list appears without touching the mode controls, then set the default back.

Files: `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_WorkflowDefaults.cs`,
`Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_BusinessRules.cs`

### Tests

- [x] **T038** [US5] FR-015: `By Receiver` is offered in the workflow default mode list, a stored `by-receiver` default opens `StartWorkflowAsync` at `ReceiverSelection` with `IsByReceiverFlow` set, and switching to Guided and back reloads the list without error · `MTM_Receiving_Application.Tests/Unit/Module_Settings/Receiving/ViewModels/ViewModel_Settings_Receiving_ByReceiverDefaultTests.cs`

**⟶ Wait for T038 to fail, then:**

### Implementation

**Wave 1 — independent (different files):**

- [x] **T039** [P] [US5] Add the `By Receiver` option with value `by-receiver` to the workflow defaults mode list · `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_WorkflowDefaults.cs`
- [x] **T040** [P] [US5] Add the `By Receiver` option with value `by-receiver` to the business rules default mode list, so the two copies agree · `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_BusinessRules.cs`

**Checkpoint**: a user who stores By Receiver reaches the list on opening Receiving, and US5 is
independently functional and testable.

## Phase 8: Polish

**Wave 1 — independent (different files):**

- [x] **T041** [P] Record the new mode under `Unreleased` in the changelog, in the same shape as the neighbouring entries: what changed, how the mode decides, the new settings keys, and the files touched · `CHANGELOG.md`
- [x] **T042** [P] Add By Receiver to the help pages that describe choosing a receiving mode and starting the guided walk, matching the surrounding wording · `docs/help/mode-selection.html`, `docs/help/guided-wizard-po-entry.html`

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T043** Validate against the Success Criteria: build the solution, run the full test suite and the lint and format pass, and confirm SC-001 (three actions to a started entry), SC-002 and SC-003 (list and search inside their time budget), SC-004 and SC-005 (no bare or removed rows, dates never increase), SC-006 (zero writes against Infor Visual) and SC-007 (stored default reaches the list every time)

## Dependencies & Execution Order

Phase order: Setup (nothing) → Phase 2 Foundational → Phase 3 US1 → Phase 4 US2 → Phase 5 US3 →
Phase 6 US4 → Phase 7 US5 → Phase 8 Polish. No story starts before Phase 2 finishes, because every story
reads the list surface or the workflow state built there.

- **Phase 2**: Wave 1 (T001–T006, six independent files) → Wave 2 (T007 and T008, different files) → T009 → T010 (needs the settings text and both service paths) → T011 (binds the view model) → T012 (registers the pair).
- **Phase 3 (US1)**: tests T013–T021 (nine independent files, written to fail) → implementation Wave 1 (T022–T025) → Wave 2 (T026–T028, each on one Wave 1 task).
- **Phase 4 (US2)**: T029 alone, against Phase 2's list view model.
- **Phase 5 (US3)**: T030 (harness) → T031–T036 (six independent test files).
- **Phase 6 (US4)**: T037 alone, against Phase 2's list view model.
- **Phase 7 (US5)**: T038 (test) → T039 and T040 in parallel.
- **Phase 8 (Polish)**: T041 and T042 in parallel → T043.

The three phases with no production files of their own (US2, US3, US4) exist because the surface they
describe is shared; their tasks still close their requirements, and a failure in them is a defect in
Phase 2, not a reason to fork the surface.

## Phase 9: Convergence

Found by the convergence assessment of the built code against spec, plan, and tasks.

- [x] **T044** When Back from Load Entry returns to the By Receiver list, move the pager to the page that holds `SelectedReceiverKey` before restoring `SelectedRow` (instead of re-reading at page one), so the row the operator picked on a later page is visibly highlighted rather than the list dropping to page one with no highlight per FR-014 / US1/AC5 (partial) · `Module_Receiving/ViewModels/ViewModel_Receiving_ReceiverSelection.cs`
- [x] **T045** Show the warehouse in the `Location Received to` column when the receipt line's location is blank and its warehouse is filled — a display value only, leaving the raw location for the carry into Load Entry — so a warehouse-only receipt reads as the warehouse alone per spec Edge Cases (partial) · `Module_Core/Models/InforVisual/Model_InforVisualReceiverRow.cs`, `Module_Receiving/Views/View_Receiving_ReceiverSelection.xaml`
