# Things To Change V2

Last Updated: 2026-03-20

## AI Agent Instructions

This file is a living implementation-planning document for `Module_Receiving` Manual Mode. Update it before and after code changes.

1. Treat this file as a scoped engineering brief, not a scratchpad.
2. Before editing this file, verify all file names, symbols, dependencies, and line numbers against the current repo state. Do not invent methods, classes, or bindings.
3. When adding or updating entries, always include:
   - actual file paths
   - actual method/property/field names
   - current line numbers as of the edit date
   - impacted dependencies and services
   - validation notes or follow-up tasks
4. Keep all checklist items unchecked until the code change is implemented and validated.
5. If code moves and line numbers drift, update the references in this file in the same change.
6. Preserve the project architecture: `View -> ViewModel -> Service -> DAO -> Database`.
7. For Manual Mode work, prefer changes in these layers first:
   - `Module_Receiving/Views/View_Receiving_ManualEntry.xaml`
   - `Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs`
   - `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs`
   - `Module_Receiving/Models/Model_ReceivingLoad.cs`
   - `Module_Receiving/Services/Service_ReceivingValidation.cs`
   - `Module_Receiving/Services/Service_ReceivingWorkflow.cs`
8. If a new dialog is added, record the DI registration update in `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`.
9. After implementation, replace planning-only notes with outcome notes, and add any new impacted files discovered during the change.

## Checklist

- [x] Remove the Manual Entry mock-location popup from the screen without breaking location validation or preset-location editing.
- [x] Seed Manual Mode with 10 blank rows on first entry and focus Row 1, PO Number.
- [x] Add row-level PO-driven part selection modal behavior for Manual Mode.
- [x] Add a row-level or page-level Non-PO mode toggle to Manual Mode.
- [x] Refactor Auto-Fill so it respects PO-selected parts and Non-PO behavior.
- [x] Update validation and save-path assumptions for mixed PO and Non-PO Manual Mode rows.
- [x] Update `Module_Reporting` receiving summaries, receiving detail display, and preview/copy formatting so Manual Mode PO-selected and Non-PO rows report correctly.
- [x] Add or update tests and any affected module documentation after implementation.

## Existing Implementation Map

### Primary UI

- `Module_Receiving/Views/View_Receiving_ManualEntry.xaml`
  - line 66: `ItemsSource` still binds directly to `ViewModel.Loads`.
  - line 77: `LoadingRow="ManualEntryDataGrid_LoadingRow"` remains active for row-level quality hold highlighting.
  - line 73: a row-level `DataGridCheckBoxColumn` now binds each row to `IsNonPOItem`.
  - line 82: `PoNumberColumn` is now a named `DataGridTemplateColumn`, which gives the code-behind a stable focus target.
  - lines 86-110: the PO column now shows the actual PO only for PO-backed rows and renders `Non-PO` for rows flagged `IsNonPOItem`.
  - line 102: PO edit textbox uses `LostFocus="PONumberTextBox_LostFocus"`.
  - line 105: `PartIdColumn` is now a named `DataGridTemplateColumn`, which is the fallback focus target when PO is not the preferred edit surface.
  - line 111: Part Number column is still a plain editable `TextBox` column.
  - lines 114 and 125: the Part Number cell template and edit template now both route `DoubleTapped` to `PartIdCell_DoubleTapped` so PO-backed rows can reopen part selection.
  - line 126: Part Number edit textbox uses `LostFocus="PartIDTextBox_LostFocus"`.
  - lines 185-212: Location column switches between `ComboBox` and `TextBox` based on mock-location mode.
  - outcome: the mock-location `InfoBar` has been removed from the screen; the location column behavior remains intact.

- `Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs`
  - line 24: `View_Receiving_ManualEntry` code-behind class.
  - line 47: subscribes to `Loaded` through `View_Receiving_ManualEntry_Loaded` for initial row focus.
  - line 55: subscribes to `ViewModel.Loads.CollectionChanged`.
  - line 95: `View_Receiving_ManualEntry_Loaded(...)` now selects Row 1 and queues initial focus to the selected row's preferred entry column.
  - line 110: `Loads_CollectionChanged(...)` now scrolls new rows into the selected row's preferred entry column.
  - line 280: `SelectPreferredEditableCell(DataGrid grid)` now prefers PO or Part Number based on the selected row's `IsNonPOItem` value.
  - line 307: `GetPreferredEntryColumn(...)` centralizes row-level focus rules.
  - line 350: row-level `IsNonPOItem` changes now re-run preferred-cell selection for the selected row.
  - line 386: `PONumberTextBox_LostFocus(...)` now formats the PO number and then calls into the ViewModel to select a PO part when needed.
  - line 381: `FormatPONumber(string input)` canonicalizes `PO-######` and `PO-######B` formats.
  - line 456: `PartIDTextBox_LostFocus(...)` now only formats free-text part values and applies default location logic; it no longer opens the PO part picker on cell exit.
  - line 510: `PartIdCell_DoubleTapped(...)` is now the explicit gesture that reopens PO part selection for PO-backed rows.
  - line 457: `LocationTextBox_LostFocus(...)` validates location and shows warning status.

### Manual Entry ViewModel

- `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs`
  - lines 19-24: core dependencies are `_workflowService`, `_validationService`, `_inforVisualService`, `_windowService`, `_helpService`, `_receivingSettings`.
  - line 19: `InitialManualEntryRows` now defines the first-entry seed count as `10`.
  - line 27: `_loads` is the backing `ObservableCollection<Model_ReceivingLoad>`.
  - line 53: `ManualEntryColumnNonPoText` supplies the row-level Non-PO column header.
  - lines 75-77: `IsMockLocationMode` and `MockLocationListText` drive the popup that needs removal.
  - line 79: `PresetLocations` is still needed by the Location column when mock data is enabled.
  - line 94: `_loads` is initialized from `_workflowService.CurrentSession.Loads`.
  - line 95: `_loads.CollectionChanged += Loads_CollectionChanged` now attaches row-level mode normalization.
  - line 110: `EnsureInitialManualRows()` is called from the constructor.
  - line 115: `EnsureInitialManualRows()` seeds 10 rows only when the current session is empty and selects the first row.
  - line 173: `AutoFillAsync()` now only copy-fills `PartID` for Non-PO rows and still skips PO copy-fill for rows marked `IsNonPOItem`.
  - lines 286-290: `AddRow()` delegates to `AddNewLoad()`.
  - lines 292-357: `AddMultipleRowsAsync()` uses a `ContentDialog` to ask how many rows to create.
  - line 359: `MaxManualEntryRows` constant.
  - line 407: `AddNewLoad()` now inherits `IsNonPOItem` from the selected row to support mixed-mode repetitive entry.
  - lines 445-593: `SaveAsync()` validates each row before persisting.
  - line 578: `SaveAsync()` now skips PO validation for rows marked `IsNonPOItem`.
  - line 536: `SaveAsync()` currently validates locations through `_validationService.ValidateLocationAsync(...)`.
  - lines 770-804: `Loads_CollectionChanged(...)`, `AttachLoadHandlers(...)`, `DetachLoadHandlers(...)`, and `Load_PropertyChanged(...)` now normalize row state when `IsNonPOItem` changes.
  - line 806: `NormalizeLoadForMode(...)` clears stale PO metadata whenever a row flips mode.
  - line 799: `TrySelectPartForPoAsync(...)` now loads PO parts, opens the shared picker dialog, applies the selected part back onto the row, and enforces reselection when the PO changes.
  - line 933: `ClearPoSelectedPart(...)` clears stale PO-selected part state when a row’s PO changes or Non-PO mode is enabled.
  - lines 718-736: `ApplyDefaultLocationFromPartAsync(Model_ReceivingLoad load)` already reacts to `PartID` changes and may need to stay compatible with modal-selected parts.

### Guided PO Entry Logic That Should Be Reused Conceptually

- `Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs`
  - lines 39-51: existing PO entry state includes `_isNonPOItem` and `_selectedPart`.
  - lines 230-291: `LoadPOAsync()` loads the PO through `_inforVisualService.GetPOWithPartsAsync(PoNumber)`, populates `Parts`, and raises workflow status.
  - lines 293-303: `ToggleNonPO()` resets PO-specific state and flips `_workflowService.IsNonPOItem`.
  - line 409: `OnPoNumberChanged(...)` writes the validated PO back to `_workflowService.CurrentPONumber`.
  - lines 432-449: `OnSelectedPartChanged(Model_InforVisualPart? value)` writes the part back to `_workflowService.CurrentPart`, updates `_workflowService.CurrentLocation`, and infers package type from the part prefix.

### Model and Validation Dependencies

- `Module_Receiving/Models/Model_ReceivingLoad.cs`
  - line 20: `_partID`
  - line 26: `_poNumber`
  - line 29: `_poLineNumber`
  - line 32: `_selectedPartSourcePONumber` tracks which PO produced the current row-level selected part.
  - line 41: `_initialLocation`
  - line 59: `_isNonPOItem`
  - line 191: `PONumberDisplay`

- `Module_Receiving/Services/Service_ReceivingValidation.cs`
  - lines 27-35: `_presetLocations` static mock-location list.
  - line 37: `UseMockLocationList`
  - line 39: `PresetLocations`
  - line 49: mock-location mode is driven by `InforVisualSettings.UseMockData`.
  - line 76: `ValidatePONumber(string poNumber)` is the current PO validation gate used by Manual Entry save.
  - lines 194-214: `ValidateLocationAsync(...)` performs location validation and still depends on the preset list in mock mode.

- `Module_Receiving/Contracts/IService_ReceivingWorkflow.cs`
  - line 46: `CurrentPONumber`
  - line 51: `CurrentPart`
  - line 56: `IsNonPOItem`
  - line 118: `AddCurrentPartToSessionAsync()`

- `Module_Receiving/Services/Service_ReceivingWorkflow.cs`
  - lines 209-222: guided workflow requires `CurrentPONumber` and `CurrentPart` unless `IsNonPOItem` is true.
  - lines 357-369: `GenerateLoads()` copies PO/part state into generated `Model_ReceivingLoad` rows.
  - lines 417-425: `AddCurrentPartToSessionAsync()` clears `CurrentPONumber`, `CurrentPart`, and `IsNonPOItem` for the next guided batch.

### Navigation and Registration

- `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs`
  - line 141: `SelectManualModeAsync()` is the entry point into Manual Mode.
  - line 149: `_workflowService.GoToStep(Enum_ReceivingWorkflowStep.ManualEntry)` activates the Manual Entry screen.

- `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs`
  - lines 266-268: Manual Entry visibility and title are set when `CurrentStep == Enum_ReceivingWorkflowStep.ManualEntry`.

- `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
  - line 109: `IService_ReceivingValidation` registration.
  - line 110: `IService_ReceivingWorkflow` registration.
  - line 120: `ViewModel_Receiving_ManualEntry` registration.
  - line 132: `View_Receiving_ManualEntry` registration.

## Detailed Change Plan

### 1. Remove the Manual Entry location popup

#### Outcome

- Completed on 2026-03-20.
- The mock-location `InfoBar` has been removed from `Module_Receiving/Views/View_Receiving_ManualEntry.xaml`.
- `ViewModel.IsMockLocationMode`, `ViewModel.MockLocationListText`, `PresetLocations`, and the Location column edit behavior were left intact so mock-location editing and validation still work.
- `Module_Receiving/Services/Service_ReceivingValidation.cs:194-214` remains the active location validation path.

#### Validation

- `get_errors` returned no errors for:
  - `Module_Receiving/Views/View_Receiving_ManualEntry.xaml`
  - `Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs`
  - `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs`
- `dotnet build MTM_Receiving_Application.slnx` succeeded on 2026-03-20.

#### Required change

- Remove the `InfoBar` from the Manual Entry screen.
- Do not remove `PresetLocations`, `UseMockLocationList`, or the location `ComboBox` behavior unless the implementation intentionally redesigns mock-location entry.
- Keep `Service_ReceivingValidation.ValidateLocationAsync(...)` intact unless there is an explicit requirement to change validation rules.

#### Impacted files

- `Module_Receiving/Views/View_Receiving_ManualEntry.xaml`
- `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs`
- `Module_Receiving/Services/Service_ReceivingValidation.cs` (reference only unless validation behavior changes)

### 2. Seed Manual Mode with 10 blank rows and focus Row 1 PO Number

#### Outcome

- Completed on 2026-03-20.
- `ViewModel_Receiving_ManualEntry` now calls `EnsureInitialManualRows()` from the constructor at `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs:110`.
- `EnsureInitialManualRows()` at `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs:115` creates exactly 10 rows only when `Loads.Count == 0`.
- Existing sessions are preserved because the seed logic exits immediately when rows already exist.
- Initial focus is now handled by `View_Receiving_ManualEntry_Loaded(...)` at `Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs:100`, which selects Row 1, scrolls it into view, and routes edit focus to `PoNumberColumn`.
- New row focus also now prefers the PO column through `SelectPreferredEditableCell(...)` at `Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs:276`.

#### Validation

- `get_errors` returned no errors for the touched files.
- `dotnet build MTM_Receiving_Application.slnx` succeeded on 2026-03-20.
- Follow-up manual validation still recommended:
  - confirm first entry into Manual Mode shows 10 rows
  - confirm re-entering Manual Mode with an existing session does not duplicate rows
  - confirm Row 1 enters edit mode on the PO field when the view first appears

#### Required change

- On first entry into Manual Mode, if the session has no rows, create exactly 10 rows.
- Do not duplicate rows when the user leaves and re-enters Manual Mode with an existing session.
- After the rows are seeded, focus Row 1, PO Number specifically, not just the first editable column.
- If Non-PO mode is active at load time, the initial focus may need to move to Part Number instead of PO Number.

#### Most likely implementation seam

- Seed rows in `ViewModel_Receiving_ManualEntry` so the source of truth stays in `Loads` and `_workflowService.CurrentSession.Loads`.
- Update `SelectFirstEditableCell(...)` or add a new targeted focus helper in `View_Receiving_ManualEntry.xaml.cs` so focus is column-aware.

#### Impacted files

- `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs`
- `Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs` (only if an explicit initialization call is added there)

### 3. Add PO-driven Part Number modal to Manual Mode

#### Outcome

- Completed on 2026-03-20.
- `PONumberTextBox_LostFocus(...)` in `Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs:386` now formats the PO number and then calls `ViewModel.TrySelectPartForPoAsync(load)`.
- `TrySelectPartForPoAsync(...)` in `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs:799`:
  - loads the PO through `_inforVisualService.GetPOWithPartsAsync(...)`
  - enriches remaining quantity per part where needed
  - opens `Dialog_FuzzySearchPicker` with the available PO parts
  - writes the selected part back into `PartID`
  - writes `PoLineNumber`
  - records `SelectedPartSourcePONumber`
  - copies descriptive PO metadata onto the row
  - preserves or applies default location behavior from the selected part
- `PartIdCell_DoubleTapped(...)` in `Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs:510` lets users reopen part selection by double-clicking the Part cell.
- If the row PO changes, stale PO-selected part state is cleared through `ClearPoSelectedPart(...)` before reselection.
- If the PO has no selectable parts, `ShowPoHasNoUsablePartsDialogAsync(...)` tells the user to use Non-PO mode instead.

#### Validation

- `dotnet build MTM_Receiving_Application.slnx` succeeded on 2026-03-20 after the PO picker implementation.
- Manual verification still recommended:
  - entering a valid PO should open the picker
  - selecting a part should populate `PartID` and `PoLineNumber`
  - double-clicking the Part cell should reopen the picker
  - changing the PO should force a new part selection

#### Required change

- In Manual Mode, when a row receives a PO Number and the row is not Non-PO, open a modal dialog listing all parts for that PO.
- When the user selects a part, update that row's `PartID` and also record row-level PO metadata if needed, including `PoLineNumber` in `Model_ReceivingLoad`.
- Make the Part Number cell act like a selection surface instead of plain text for PO-backed rows.
- Allow the user to reopen the part selector by double-clicking the Part Number cell.
- If the PO changes after part selection, clear the row's previously selected part and require reselection.
- If the PO has no parts, show a graceful message telling the user the PO cannot be used in this module and to use Non-PO mode instead.

#### Recommended reuse points

- Reuse `_inforVisualService.GetPOWithPartsAsync(...)` logic already used by `ViewModel_Receiving_POEntry.LoadPOAsync()`.
- Reuse the `ContentDialog` + `XamlRoot` pattern already used in `ViewModel_Receiving_ManualEntry.AddMultipleRowsAsync()` at `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs:292-328`.
- Reuse the part side effects from `OnSelectedPartChanged(...)` in `ViewModel_Receiving_POEntry.cs:432-449`, especially location defaulting and package-type inference.

#### Likely new artifacts

- A new receiving dialog view and, if needed, a dialog viewmodel for PO-part selection.
- DI registration in `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`.
- Possibly a new row-level state property if the selected part needs richer metadata than `PartID` and `PoLineNumber`.

#### Impacted files

- `Module_Receiving/Views/View_Receiving_ManualEntry.xaml`
- `Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs`
- `Module_Receiving/Models/Model_ReceivingLoad.cs`
- `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`

### 4. Add Non-PO mode to Manual Entry

#### Outcome

- Completed on 2026-03-20 as row-level mixed-mode UX.
- The earlier page-level Manual Entry toggle was replaced with a per-row `Non-PO?` checkbox column.
- Each row now owns its own `IsNonPOItem` state.
- Switching a row to Non-PO clears PO-specific metadata for that row only.
- Switching a row back to PO-backed mode clears stale PO-derived row state and returns focus to that row's PO field.
- New rows inherit the selected row's `IsNonPOItem` value so repeated PO or Non-PO entry is faster.
- `SelectPreferredEditableCell(...)` in `Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs` now prefers PO or Part Number per row.
- `SaveAsync()` now skips PO validation for rows flagged `IsNonPOItem`.

#### Validation

- `dotnet build MTM_Receiving_Application.slnx` succeeded on 2026-03-20 after the Non-PO implementation.
- Manual verification still recommended:
  - toggling `Non-PO?` on one row should not affect neighboring rows
  - new rows should inherit the selected row's mode
  - toggling a row to Non-PO should move focus to Part Number for that row
  - toggling a row back to PO-backed mode should move focus to PO Number for that row
  - save should not reject Non-PO rows for missing PO values

#### Required change

- Completed. Manual Entry now uses row-specific `IsNonPOItem` behavior instead of a page-wide toggle.

#### Important design note

- The implementation now uses the row-specific path, and XAML focus/edit behavior was updated accordingly.

#### Impacted files

- `Module_Receiving/Views/View_Receiving_ManualEntry.xaml`
- `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs`
- `Module_Receiving/Models/Model_ReceivingLoad.cs`
- `Module_Receiving/Services/Service_ReceivingValidation.cs`

### 5. Refactor Auto-Fill to respect PO/Non-PO rules

#### Outcome

- Completed on 2026-03-20 for the high-risk identity fields.
- `AutoFillAsync()` in `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs:173` now only copy-fills blank `PartID` for rows marked `IsNonPOItem`.
- PO-backed rows no longer get `PartID` copied down from previous rows, which prevents bypassing the new explicit PO part-selection flow.
- `AutoFillAsync()` also now skips PO number copy-fill for rows marked `IsNonPOItem`.
- Helpful non-identity copy-fill behavior remains in place for fields such as quantity, heat/lot, location, packages per load, and package type where safe.

#### Validation

- Build succeeded after the Auto-Fill changes.
- Manual verification still recommended for mixed-row scenarios where PO-backed and Non-PO rows appear in the same Manual Entry session.

#### Required change

- Do not auto-fill `PartID` into PO-backed rows if the row is supposed to select the part from a PO modal.
- If a row is Non-PO, allow current free-text part auto-fill rules only if they still make sense after the redesign.
- Do not auto-fill a stale `PoNumber` or `PartID` combination that would bypass the new required re-selection behavior.
- Preserve helpful fills for non-identity fields such as `WeightQuantity`, `HeatLotNumber`, `InitialLocation`, `PackagesPerLoad`, and `PackageTypeName` where safe.

#### Suggested rule split

- PO-backed rows:
  - `PoNumber` may fill from a prior similar row only if that does not bypass mandatory part selection.
  - `PartID` should come from explicit selection, not blind copy-down.
- Non-PO rows:
  - `PartID` can remain eligible for copy-down if approved by the final UX decision.

#### Impacted files

- `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs`
- `Module_Receiving/Models/Model_ReceivingLoad.cs`

### 6. Update `Module_Reporting` after Manual Mode row semantics change

#### Outcome

- Completed on 2026-03-20 for current Manual Mode PO/Non-PO reporting semantics.
- `Module_Receiving/Services/Service_ReceivingValidation.cs:356` in `ValidateReceivingLoad(...)` now treats PO-backed rows and Non-PO rows differently at the shared validation layer.
- `Module_Receiving/Services/Service_ReceivingValidation.cs:431` in `ValidateSession(...)` now aggregates those row-level rules before Manual Entry save proceeds.
- `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs:616` in `SaveAsync()` now validates the mixed-mode session before save and only defaults blank Heat/Lot and Location values after validation succeeds.
- `Module_Receiving/Data/Dao_ReceivingLabelData.cs:153` and `Module_Receiving/Data/Dao_ReceivingLabelData.cs:172` persist `po_line_number` and `is_non_po_item` on insert, and `Module_Receiving/Data/Dao_ReceivingLabelData.cs:353` and `Module_Receiving/Data/Dao_ReceivingLabelData.cs:372` preserve those same fields on update.
- `Module_Reporting/Data/Dao_Reporting.cs` already projects `po_line_number` and `is_non_po_item` into `Model_ReportRow`.
- `Module_Core/Models/Reporting/Model_ReportRow.cs:64` in `DisplayPo` now renders `Non-PO` for receiving rows flagged `IsNonPOItem` and includes `PO / Line` text for PO-backed rows.
- `Module_Reporting/Services/Service_Reporting.cs:397` in `BuildReceivingSummaryTable(...)` now assigns receiving rows into an explicit `Non-PO` summary bucket instead of silently mixing them into prefix buckets or `Other`.
- `Module_Reporting/Services/Service_Reporting.cs:167` in `FormatForEmailAsync(...)` continues to use the row display properties, so preview/copy output stays aligned with the new `DisplayPo` behavior without adding a separate detail-rendering fork.

#### Validation

- `get_errors` returned no errors for:
  - `Module_Reporting/Services/Service_Reporting.cs`
  - `MTM_Receiving_Application.Tests/Unit/Module_Reporting/Services/Service_ReportingTests.cs`
- `dotnet build MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj` succeeded on 2026-03-20 after the reporting summary change and new tests were added.
- Focused test files now cover the reporting/display rules:
  - `MTM_Receiving_Application.Tests/Unit/Module_Core/Models/Reporting/Model_ReportRowTests.cs`
  - `MTM_Receiving_Application.Tests/Unit/Module_Receiving/Services/Service_ReceivingValidationTests.cs`
  - `MTM_Receiving_Application.Tests/Unit/Module_Reporting/Services/Service_ReportingTests.cs:25`
  - `MTM_Receiving_Application.Tests/Unit/Module_Reporting/Services/Service_ReportingTests.cs:73`
- Focused `runTests` execution is currently blocked by the existing WinAppSDK test-host environment issue (`System.BadImageFormatException` during module initialization), so test execution evidence is currently limited to successful compilation of the test project.

#### Current behavior

- `Module_Reporting/Services/Service_Reporting.cs`
  - line 86: `NormalizePONumber(string? poNumber)` normalizes any populated PO text before reporting output.
  - line 126: `BuildSummaryTablesAsync(...)` builds summary tables from `Model_ReportSection` rows.
  - line 149: `FormatForEmailAsync(...)` renders the copied report HTML and plain text from reporting sections and summary tables.
  - line 325: `BuildReceivingSummaryTable(...)` groups Receiving rows by configured part-prefix rules and currently assumes receiving rows can be bucketed from `PartNumber` + `Quantity` or `WeightLbs`.
  - line 634: `GetSummaryValue(Model_ReportRow row)` currently treats Receiving rows as `Quantity ?? WeightLbs ?? 0m`.
  - line 680: `AppendSectionStart(...)` is the shared HTML card wrapper used by the copied report output.

- `Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs`
  - line 246: `CopyEmailFormatAsync()` copies only the currently included preview-module cards to the clipboard.
  - lines 260-261: copied output is built from `IncludedPreviewModuleCards.Select(card => card.DetailSection)` and `IncludedPreviewModuleCards.Select(card => card.SummaryTable)`.
  - line 322: `BuildPreviewState(...)` builds the preview state from `Model_ReportSection` rows and reporting summary tables.
  - line 369: `CreatePreviewModuleCard(...)` constructs the per-module preview card used by the preview dialog.
  - line 448: `SyncMainModuleSelection(...)` syncs preview include toggles back to the main screen module checkboxes.
  - line 478: `SyncPreviewModuleSelection(...)` syncs the main screen module checkboxes into the preview cards.

- `Module_Reporting/Views/View_Reporting_PreviewDialog.xaml`
  - line 57: `Included Modules` selector is the current preview-time include/exclude surface.
  - line 79: the grouped module cards are rendered from `ViewModel.IncludedPreviewModuleCards`.
  - line 100: `Summary` table heading in the preview card.
  - line 178: `Detailed Activity` table heading in the preview card.

- `Module_Reporting/Models/Model_ReportingPreviewModuleCard.cs`
  - line 9: `Model_ReportingPreviewModuleCard` is the new grouped preview-card model.
  - line 12: `IsIncluded` controls whether a module remains in the preview/copy output.
  - line 24: `SummaryTable` holds the per-module summary table.
  - line 26: `DetailSection` holds the per-module detail table rows.

- `Module_Core/Models/Reporting/Model_ReportRow.cs`
  - line 10: `PONumber`
  - line 12: `PartNumber`
  - line 24: `SourceModule`
  - line 50: `PackagesPerLoad`
  - line 60: `DisplayPo`
  - line 62: `DisplayPartOrDunnage`
  - line 90: `DisplayLoadsOrSkids`
  - line 122: `DisplayUnitsPerSkid`

#### Why reporting is impacted by the Manual Mode changes

- Manual Mode now supports a mix of PO-backed and Non-PO receiving rows.
- Reporting now has explicit `IsNonPOItem` and `POLineNumber` fields on `Model_ReportRow`, and the DAO/query path currently projects both values into the reporting DTO.
- Receiving detail output now shows `Non-PO` or `PO / Line` through `Model_ReportRow.DisplayPo`, and receiving summaries now split Non-PO rows into a dedicated summary bucket.

#### Required change

- Completed for the current Receiving Manual Mode scope:
  - the receiving history query path already exposes row-level `IsNonPOItem` and `PoLineNumber`
  - `Model_ReportRow` already surfaces the correct PO/Non-PO detail text through `DisplayPo`
  - `Service_Reporting.BuildReceivingSummaryTable(...)` now provides a dedicated `Non-PO` summary column pair
  - preview and copied HTML/plain-text output remain aligned because both continue to consume the same reporting rows and summary tables

#### Most likely implementation seams

- Receiving summary logic
  - `Module_Reporting/Services/Service_Reporting.cs:397` in `BuildReceivingSummaryTable(...)`
  - completed: Non-PO rows now render in an explicit `Non-PO` summary column pair.

- Receiving detail display
  - `Module_Core/Models/Reporting/Model_ReportRow.cs:64` in `DisplayPo`
  - `Module_Core/Models/Reporting/Model_ReportRow.cs:95` in `DisplayPartOrDunnage`
  - `Module_Core/Models/Reporting/Model_ReportRow.cs:128` in `DisplayLoadsOrSkids`
  - completed for current scope: Non-PO rows now display an explicit `Non-PO` indicator instead of a misleading blank PO field.

- Preview + copy parity
  - `Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs:322` in `BuildPreviewState(...)`
  - `Module_Reporting/Views/View_Reporting_PreviewDialog.xaml:79` for grouped module-card rendering
  - `Module_Reporting/Services/Service_Reporting.cs:167` in `FormatForEmailAsync(...)`
  - completed for current scope: preview and copied output both consume the same `DisplayPo` and summary-table data, so no extra rendering divergence was introduced.

#### Impacted files

- `Module_Reporting/Services/Service_Reporting.cs`
- `Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs`
- `Module_Reporting/Views/View_Reporting_PreviewDialog.xaml`
- `Module_Reporting/Models/Model_ReportingPreviewModuleCard.cs` (if preview card metadata needs to carry additional receiving-specific state)
- `Module_Core/Models/Reporting/Model_ReportRow.cs`
- `Module_Reporting/Data/Dao_Reporting.cs` or the underlying reporting query source if new receiving fields must be projected into report rows

#### Validation notes / follow-up tasks

- Validate against deployed reporting stored procedures/views that archived Manual Mode rows still populate `po_line_number` and `is_non_po_item` consistently in production data.
- If a future product decision adds Receiving-specific detail columns to the preview card, update both preview and copied HTML/plain-text output in the same change.

## Validation Notes For Future Implementation

- Verified on 2026-03-20:
  - Manual Entry mock-location popup removed from the screen.
  - Manual Mode now seeds 10 rows on first empty entry.
  - Initial focus logic now prefers the PO column.
  - PO entry in Manual Mode now launches a part-selection picker and writes `PoLineNumber` back onto the row.
  - Part cells now reopen PO-backed part selection on double-click.
  - Manual Entry now supports row-level mixed PO and Non-PO entry through the `Non-PO?` row column.
  - Auto-Fill no longer copy-fills `PartID` into PO-backed rows.
  - Shared receiving validation now enforces PO-backed versus Non-PO row rules before save.
  - Receiving reporting detail rows now show explicit `Non-PO` and `PO / Line` values.
  - Receiving reporting summaries now show a dedicated `Non-PO` bucket.
  - Focused reporting tests were added and the test project builds successfully.
  - Solution build succeeded after the first-slice changes.
- Re-run focused reporting and Manual Mode tests once the WinAppSDK test-host `BadImageFormatException` environment issue is resolved.
- Confirm that any new dialog sets `XamlRoot` correctly before `ShowAsync()`.
- Confirm that row focus still works after seeding 10 rows and after opening/closing the modal selector.
- Confirm that changing a PO clears stale `PartID` and `PoLineNumber` values on the same row.
- Confirm that mock-location mode still supports the Location column even after the popup is removed.
