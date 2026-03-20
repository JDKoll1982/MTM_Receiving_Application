
# Things To Change V2

Last Updated: 2026-03-19

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

- [ ] Remove the Manual Entry mock-location popup from the screen without breaking location validation or preset-location editing.
- [ ] Seed Manual Mode with 10 blank rows on first entry and focus Row 1, PO Number.
- [ ] Add row-level PO-driven part selection modal behavior for Manual Mode.
- [ ] Add a row-level or page-level Non-PO mode toggle to Manual Mode.
- [ ] Refactor Auto-Fill so it respects PO-selected parts and Non-PO behavior.
- [ ] Update validation and save-path assumptions for mixed PO and Non-PO Manual Mode rows.
- [ ] Update `Module_Reporting` receiving summaries, receiving detail display, and preview/copy formatting so Manual Mode PO-selected and Non-PO rows report correctly.
- [ ] Add or update tests and any affected module documentation after implementation.

## Existing Implementation Map

### Primary UI

- `Module_Receiving/Views/View_Receiving_ManualEntry.xaml`
  - line 63: `InfoBar` currently displays the mock location list popup.
  - line 88: PO Number column is a `DataGridTemplateColumn`.
  - line 102: PO edit textbox uses `LostFocus="PONumberTextBox_LostFocus"`.
  - line 111: Part Number column is currently a plain editable `TextBox` column.
  - line 126: Part Number edit textbox uses `LostFocus="PartIDTextBox_LostFocus"`.
  - lines 185-212: Location column switches between `ComboBox` and `TextBox` based on mock-location mode.

- `Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs`
  - line 24: `View_Receiving_ManualEntry` code-behind class.
  - line 55: subscribes to `ViewModel.Loads.CollectionChanged`.
  - line 99: `Loads_CollectionChanged(...)` handles focus/edit activation for new rows.
  - line 260: `SelectFirstEditableCell(DataGrid grid)` currently focuses the first editable column, not specifically the PO column.
  - line 341: `PONumberTextBox_LostFocus(...)` formats the PO number only; it does not load PO parts.
  - line 381: `FormatPONumber(string input)` canonicalizes `PO-######` and `PO-######B` formats.
  - line 424: `PartIDTextBox_LostFocus(...)` pads part number text and applies default location; this is the current seam that conflicts with the requested PO-part modal flow.
  - line 457: `LocationTextBox_LostFocus(...)` validates location and shows warning status.

### Manual Entry ViewModel

- `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs`
  - lines 19-24: core dependencies are `_workflowService`, `_validationService`, `_inforVisualService`, `_windowService`, `_helpService`, `_receivingSettings`.
  - line 27: `_loads` is the backing `ObservableCollection<Model_ReceivingLoad>`.
  - lines 75-77: `IsMockLocationMode` and `MockLocationListText` drive the popup that needs removal.
  - line 79: `PresetLocations` is still needed by the Location column when mock data is enabled.
  - line 101: `_loads` is initialized from `_workflowService.CurrentSession.Loads`.
  - lines 137-284: `AutoFillAsync()` currently fills `PartID`, `PoNumber`, `WeightQuantity`, `HeatLotNumber`, `InitialLocation`, `PackagesPerLoad`, and `PackageTypeName` by copying prior rows.
  - lines 286-290: `AddRow()` delegates to `AddNewLoad()`.
  - lines 292-357: `AddMultipleRowsAsync()` uses a `ContentDialog` to ask how many rows to create.
  - line 359: `MaxManualEntryRows` constant.
  - lines 361-389: `AddNewLoad()` creates a new `Model_ReceivingLoad`, assigns `LoadNumber`, and appends the row to both `Loads` and `_workflowService.CurrentSession.Loads`.
  - lines 445-593: `SaveAsync()` validates each row before persisting.
  - line 520: `SaveAsync()` currently validates PO numbers through `_validationService.ValidatePONumber(...)`.
  - line 536: `SaveAsync()` currently validates locations through `_validationService.ValidateLocationAsync(...)`.
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

#### Current behavior

- The popup is not a modal dialog; it is an `InfoBar` in `Module_Receiving/Views/View_Receiving_ManualEntry.xaml:63`.
- It is shown whenever `ViewModel.IsMockLocationMode` is true.
- The displayed text comes from `ViewModel.MockLocationListText` in `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs:76`.

#### Required change

- Remove the `InfoBar` from the Manual Entry screen.
- Do not remove `PresetLocations`, `UseMockLocationList`, or the location `ComboBox` behavior unless the implementation intentionally redesigns mock-location entry.
- Keep `Service_ReceivingValidation.ValidateLocationAsync(...)` intact unless there is an explicit requirement to change validation rules.

#### Impacted files

- `Module_Receiving/Views/View_Receiving_ManualEntry.xaml`
- `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs`
- `Module_Receiving/Services/Service_ReceivingValidation.cs` (reference only unless validation behavior changes)

### 2. Seed Manual Mode with 10 blank rows and focus Row 1 PO Number

#### Current behavior

- Manual Mode navigation starts in `ViewModel_Receiving_ModeSelection.SelectManualModeAsync()` at `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs:141`.
- The Manual Entry viewmodel currently loads existing rows from `_workflowService.CurrentSession.Loads` at `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs:101`.
- Row creation happens through `AddNewLoad()` at `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs:361`.
- UI focus on new rows is handled in `Loads_CollectionChanged(...)` and `SelectFirstEditableCell(...)` in `Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs:99` and `:260`.

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

#### Current behavior

- Manual Entry currently treats PO Number as formatting-only on `LostFocus` in `View_Receiving_ManualEntry.xaml.cs:341`.
- Manual Entry currently treats Part Number as a free-entry textbox in `View_Receiving_ManualEntry.xaml:111-132`.
- Guided PO mode already knows how to load PO parts in `ViewModel_Receiving_POEntry.LoadPOAsync()` in `Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs:230-291`.

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

#### Current behavior

- Guided PO entry already supports Non-PO mode through `ToggleNonPO()` in `Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs:293-303`.
- `Model_ReceivingLoad` already has `IsNonPOItem` at `Module_Receiving/Models/Model_ReceivingLoad.cs:59`.
- Manual Entry save currently validates `PoNumber` for each row in `ViewModel_Receiving_ManualEntry.SaveAsync()` at `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs:520`.

#### Required change

- Add a Non-PO checkbox to Manual Entry.
- When checked:
  - hide or remove the PO Number column from the Manual Entry grid
  - skip PO validation for affected rows
  - allow free-text part entry in the Part Number column
  - avoid PO-part modal behavior
- Decide whether Non-PO is page-wide for all Manual Mode rows or row-specific per load. The current request reads page-wide, but the row model already supports row-specific storage.

#### Important design note

- If the checkbox is page-wide, the implementation should still write `Model_ReceivingLoad.IsNonPOItem` per row so persistence and save validation remain explicit.
- If the checkbox is row-specific, the XAML and auto-fill logic will need more conditional behavior.

#### Impacted files

- `Module_Receiving/Views/View_Receiving_ManualEntry.xaml`
- `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs`
- `Module_Receiving/Models/Model_ReceivingLoad.cs`
- `Module_Receiving/Services/Service_ReceivingValidation.cs`

### 5. Refactor Auto-Fill to respect PO/Non-PO rules

#### Current behavior

- `AutoFillAsync()` in `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs:137-284` currently:
  - fills blank `PartID` from the immediate predecessor
  - fills blank `PoNumber`, `WeightQuantity`, `HeatLotNumber`, `InitialLocation`, `PackagesPerLoad`, and `PackageTypeName` from the nearest previous row with the same `PartID`

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

- Manual Mode is about to support a mix of PO-backed and Non-PO receiving rows.
- Reporting currently has no explicit `IsNonPOItem` or `PoLineNumber` field on `Model_ReportRow`, so if the receiving history query/save path starts storing those semantics and they need to be shown or summarized, the reporting DTO/query layer will need to expose them.
- If Manual Mode changes how `PartID`, `PoNumber`, `PoLineNumber`, `PackagesPerLoad`, or inferred package metadata are populated, the Receiving summary and Receiving detail table may become misleading unless reporting is updated in the same implementation wave.

#### Required change

- Review the Receiving history query path that feeds `Module_Reporting` and confirm whether new Manual Mode semantics require new reporting fields, especially:
  - row-level `IsNonPOItem`
  - row-level `PoLineNumber`
  - any persisted indicator that a part came from PO modal selection rather than free entry
- Update `Module_Core/Models/Reporting/Model_ReportRow.cs` if new Receiving report fields are needed for display or summarization.
- Update `Module_Reporting/Services/Service_Reporting.cs` so Receiving rows are handled correctly when:
  - `PONumber` is blank because a row is Non-PO
  - a Non-PO sentinel label should be shown instead of an empty PO cell
  - prefix-based Receiving summaries need a dedicated Non-PO bucket rather than silently falling into `Other`
  - PO-selected parts carry package/location defaults that change the meaning of `DisplayLoadsOrSkids` or `DisplayUnitsPerSkid`
- Update both rendering paths together if Receiving report columns change:
  - preview path in `Module_Reporting/Views/View_Reporting_PreviewDialog.xaml`
  - copied HTML/plain-text path in `Module_Reporting/Services/Service_Reporting.cs:149`
- Keep the preview and copied report column order, labels, and per-module card layout aligned. Do not let preview diverge from copied output again.

#### Most likely implementation seams

- Receiving summary logic
  - `Module_Reporting/Services/Service_Reporting.cs:325` in `BuildReceivingSummaryTable(...)`
  - validate whether Non-PO rows should remain in `Other` or get an explicit `Non-PO` summary column pair.

- Receiving detail display
  - `Module_Core/Models/Reporting/Model_ReportRow.cs:60` in `DisplayPo`
  - `Module_Core/Models/Reporting/Model_ReportRow.cs:62` in `DisplayPartOrDunnage`
  - `Module_Core/Models/Reporting/Model_ReportRow.cs:90` in `DisplayLoadsOrSkids`
  - add explicit display behavior for Non-PO rows if blanks are ambiguous in reports.

- Preview + copy parity
  - `Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs:322` in `BuildPreviewState(...)`
  - `Module_Reporting/Views/View_Reporting_PreviewDialog.xaml:79` for grouped module-card rendering
  - `Module_Reporting/Services/Service_Reporting.cs:149` in `FormatForEmailAsync(...)`
  - if a Receiving-specific column is added, update both surfaces in the same change.

#### Impacted files

- `Module_Reporting/Services/Service_Reporting.cs`
- `Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs`
- `Module_Reporting/Views/View_Reporting_PreviewDialog.xaml`
- `Module_Reporting/Models/Model_ReportingPreviewModuleCard.cs` (if preview card metadata needs to carry additional receiving-specific state)
- `Module_Core/Models/Reporting/Model_ReportRow.cs`
- `Module_Reporting/Data/Dao_Reporting.cs` or the underlying reporting query source if new receiving fields must be projected into report rows

#### Validation notes / follow-up tasks

- Validate that PO-backed Manual Mode rows still summarize under the correct Receiving prefix bucket after part selection is introduced.
- Validate that Non-PO rows do not display a misleading blank PO field in either preview or copied HTML if the product decision is to show a `Non-PO` indicator.
- Validate that any new receiving row fields added for reporting are populated by both existing Receiving flows and the new Manual Mode flow.
- Validate that preview cards and copied HTML still match after any Receiving detail-table column changes.

## Validation Notes For Future Implementation

- Re-run row-save validation paths in `ViewModel_Receiving_ManualEntry.SaveAsync()` after the Non-PO and modal-selection changes.
- Confirm that any new dialog sets `XamlRoot` correctly before `ShowAsync()`.
- Confirm that row focus still works after seeding 10 rows and after opening/closing the modal selector.
- Confirm that changing a PO clears stale `PartID` and `PoLineNumber` values on the same row.
- Confirm that mock-location mode still supports the Location column even after the popup is removed.