# Advanced Button — Bulk Location to Items To Send

Last Updated: 2026-08-24

Add an **Advanced** button to the Scanner Workbench that navigates to a full-page two-step
wizard for moving parts from a source location into the Items To Send table.

## Button Placement

- [x] Add an **Advanced** button immediately to the left of the **Manage Items** button in the
      Items To Send header.
- [x] Keep the existing Manage Items button position and behavior unchanged.

## Page vs Modal

- [x] Host the wizard as a full page (`View_Scanner_AdvancedBulkMove`) inside the scanner
      module host instead of a `ContentDialog`, so it has more room for the search box and the
      parts/destination lists.
- [x] Add a fuzzy **From Location** search box (`AutoSuggestBox` with a `Find` query icon) plus
      an explicit **Search** button in Step 1.
- [x] The workbench **Advanced** button navigates to the page; completed destination rows are
      handed back to the workbench through `IService_ScannerNavigation.PendingBulkMoveDestinations`.

## Step 1 — Select Source Location and Parts

- [x] The page shows a **From Location** search box with fuzzy location suggestions.
- [x] Searching a From Location pulls up all part numbers currently in that location.
- [x] Display the transaction/quantity amount for each part number (for example, `PartA` has
      `5000` in `RECV`).
- [x] Provide a checkbox for each part so the user can select any or all parts.
- [x] Add a **number box** next to the checkbox column, defaulting to `1`, to control how many
      destination entries are created for that part.
- [x] Add a **Next** button that accepts the selected parts and advances to Step 2.

## Step 2 — Add To Locations

- [x] After Next, the page content switches to an **Add To Locations** view.
- [x] Add one line for each selected part where the user can enter the destination location.
- [x] When a part's number box is greater than `1`, give that part that many destination rows
      (for example, move `2500` to `V-A1-01` and `2500` to `V-A1-02`).
- [x] Add a final **Save** button that sends all completed rows back to the Items To Send table.

## Acceptance Criteria

- [x] Advanced button navigates to the page (replacing the modal) on the left of Manage Items.
- [x] From Location search filters parts to that location.
- [x] Part rows show available quantity and a selection checkbox.
- [x] Number box defaults to `1` and expands Step 2 rows accordingly.
- [x] Step 2 Save returns all filled rows to the Items To Send table.
- [x] Existing Manage Items behavior is not regressed.
- [x] Build succeeds and Module_Scanner tests pass (116/116).

## 📑 Technical Research Notes

Last Updated: 2026-08-24

### File Targets

- **View (XAML):** `Module_Scanner/Views/View_Scanner_Workbench.xaml`
  - The `Items To Send` header grid is `x:Name="ItemsHeaderGrid"` at approximately lines
    222–248 (inside `ItemsToSendCard` → `ItemsCardContentGrid`).
  - The **Advanced** button lives at `Grid.Column="1"` (bound to
    `ViewModel.AdvancedBulkMoveCommand`), **Manage Items** at `Grid.Column="2"`, and **Remove
    Selected** at `Grid.Column="3"`.
- **Page host:** `Module_Scanner/Views/View_Scanner_Main.xaml(.cs)`
  - Adds an `AdvancedBulkMoveHost` `ContentControl`; the host constructor injects
    `View_Scanner_AdvancedBulkMove` and assigns it. Visibility is driven by
    `ViewModel_Scanner_Main.IsAdvancedBulkMoveVisible`.
- **Page:** `Module_Scanner/Views/View_Scanner_AdvancedBulkMove.xaml(.cs)`
  - Full-page two-step wizard. Step 1 has a fuzzy location `AutoSuggestBox`
    (`FromLocationSearchBox`, `QueryIcon="Find"`) plus an explicit **Search** button that runs
    `SearchPartsCommand`; the parts list uses `Parts` with checkbox + entry-count `NumberBox`.
    Step 2 uses `Destinations` with To-location `TextBox` and qty `NumberBox`. Footer holds
    Back (Step 2 only), Cancel, and a Next/Save primary button whose label/icon change with
    `IsStepTwo`.
- **Page ViewModel:** `Module_Scanner/ViewModels/ViewModel_Scanner_AdvancedBulkMove.cs`
  - `SearchPartsCommand` → `_validationService.GetPartsInLocationAsync(location, warehouse)`.
  - `UpdateLocationSuggestionsAsync` → `_validationService.GetLocationSuggestionsAsync`.
  - `NextStepCommand` builds destinations (Step 1 → 2) or validates + hands rows back
    (Step 2 save). `BackToStepOneCommand` and `CancelCommand` return to the workbench.
- **Navigation contract:** `Module_Scanner/Contracts/IService_ScannerNavigation.cs`
  - Adds `ShowAdvancedBulkMove()` and the `PendingBulkMoveDestinations` payload. The workbench
    subscribes to `PropertyChanged` and stages the payload when it returns to the workbench.
- **Row model:** `Module_Scanner/Models/Model_ScannerBatchItem.cs`
  - Key properties: `PayloadPartId`, `PayloadQuantity` (string), `PayloadFromWarehouse`,
    `PayloadFromLocation`, `PayloadToWarehouse`, `PayloadToLocation`, `SequenceNumber`,
    `ValidationState`, `FuzzyMatchedFromLocation`, `FuzzyMatchedToLocation`, `ItemId`.
- **Bulk-move models:** `Module_Scanner/Models/Model_ScannerBulkMovePart.cs` (Step 1 row:
  `PartId`, `PartDescription`, `WarehouseCode`, `LocationId`, `Quantity`, `IsSelected`,
  `EntryCount`) and `Model_ScannerBulkMoveDestination.cs` (Step 2 row: `PartId`,
  `FromLocation`, `ToLocation`, `QuantityNumber`, computed `Quantity`).

### XAML & UI Strategy

- **Header layout:** `ItemsHeaderGrid` is a `Grid` with three columns:
  `* , Auto, Auto`. The title `TextBlock` fills column 0; **Manage Items** and **Remove
  Selected** occupy columns 1 and 2.
- **Safe injection point:** Add the new **Advanced** button as `Grid.Column="1"` and shift
  **Manage Items** to `Grid.Column="2"` and **Remove Selected** to `Grid.Column="3"` (or insert
  the new button with `Margin="0,0,8,0"` before Manage Items). Add one `Auto` column; do not
  touch the `*` title column so alignment is preserved:

  ```xml
  <Button Grid.Column="1" Style="{StaticResource ScannerActionButtonStyle}"
          Command="{x:Bind ViewModel.AdvancedBulkMoveCommand}" Content="Advanced" />
  ```

- **Page mechanism:** Host the wizard as a full page (more room than a modal):
  - The page lives in the scanner module host (`View_Scanner_Main`), so it gets the standard
    header/back workflow automatically.
  - Step 1 / Step 2 panels toggle via `IsStepTwo` bound to
    `Converter_BooleanToVisibility` / `Converter_InverseBooleanToVisibility`.
  - The step header text and the primary button label/icon (`Next` → `Save`) are bound to
    `StepHeaderText`, `NextButtonText`, and `NextButtonIcon` observables.
  - The page ViewModel keeps validation/persistence orchestration; the workbench stages the
    returned snapshot (cloned rows via `GetDestinations()`).

### Data & SQL Architecture

- **Inventory lookup service:** `IService_InforVisual.GetMaterialAvailabilityCurrentStockAsync(
  string? locationId, string? partId, string warehouseCode)` implemented by
  `Service_InforVisualConnect` (delegates to `Dao_InforVisualConnection`) and
  `Service_ScannerValidation` (which wraps `IService_InforVisual`).
  - Pass `locationId = "RECV"`, `partId = null`, `warehouseCode = "002"` to get **all parts in
    that location with on-hand quantity**.
  - Returns `List<Model_InforVisualMaterialLocationRow>` with `PartId`, `PartDescription`,
    `WarehouseCode`, `LocationId`, `Quantity`, `CommittedQuantity`.
- **SQL source:** `Database/InforVisualScripts/Queries/18_GetMaterialAvailabilityCurrentStock.sql`
  - Uses `dbo.CR_PART_LOCATION` joined to `dbo.PART`; filters to positive `QTY`; when
    `@PartId IS NULL` and `@LocationId` is set, it returns all parts in that location. This
    satisfies the Step 1 "all part numbers in that location with quantity" requirement with no
    new query needed.
- **Location resolution:** `Service_ScannerValidation.FormatLocation` applies the shared dash
  rule (e.g. `V-A1-01`); destination entry can reuse the same formatting for Step 2 rows.

### Step-by-Step Implementation Map

1. **ViewModel — Step 1 state:** `AdvancedBulkMoveCommand` (workbench) checks for an active
   session and calls `_navigationService.ShowAdvancedBulkMove()`. The page ViewModel loads
   parts via `_validationService.GetPartsInLocationAsync(location, warehouse)`.
   - Map each returned `Model_InforVisualMaterialLocationRow` into a lightweight selectable row
     (`Model_ScannerBulkMovePart`: `PartId`, `PartDescription`, `Quantity`, `IsSelected`,
     `EntryCount = 1`).
   - The page binds the checkbox column to `IsSelected` and a `NumberBox` to `EntryCount`
     (default `1`), next to the checkbox column as required.
2. **ViewModel — Step 2 state:** On Step 1 Next, keep only rows where `IsSelected`. Build a
   destination-row list by expanding each selected part `EntryCount` times:
   - Example: `PartA`, quantity `5000`, `EntryCount = 2` → two destination rows
     (`PartA → ?`, `PartA → ?`). Each row carries `PartId`, the shared `FromLocation`, and an
     editable `ToLocation` (blank by default) plus an editable per-row quantity (initialize to
     `quantity / EntryCount`, e.g. `2500` / `2500`, so the split is meaningful by default).
   - Add a `NumberBox`/`TextBox` per destination row so the user can adjust each split.
3. **ViewModel — Save back:** On Step 2 Save, the page sets
   `_navigationService.PendingBulkMoveDestinations = GetDestinations()` (cloned rows) and calls
   `ShowWorkbench()`. The workbench observes the navigation change and runs
   `StageAdvancedBulkMoveDestinationsAsync`, transforming each destination row into a
   `Model_ScannerBatchItem`:
   - `PayloadPartId = PartId`
   - `PayloadFromWarehouse = warehouse`, `PayloadFromLocation = FromLocation`
   - `PayloadToWarehouse = warehouse`, `PayloadToLocation = entered ToLocation`
   - `PayloadQuantity = entered quantity`
   - It appends to `CurrentSession.Items`, validates each row via `ValidateNewItemAsync`, and
     persists via `ReplaceSessionItemsAsync` (the Manage Items pattern).
4. **View:** Add the Advanced button; wire `AdvancedBulkMoveCommand`. The page toggles its
   internal Step 1 / Step 2 content and returns the final cloned rows for the workbench to
   commit. Do not mutate `SessionItems` from inside the page — return snapshots only.