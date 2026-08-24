# Advanced Button — Bulk Location to Items To Send

Last Updated: 2026-08-24

Add an **Advanced** button to the Scanner Workbench that opens a two-step modal for moving
parts from a source location into the Items To Send table.

## Button Placement

- [ ] Add an **Advanced** button immediately to the left of the **Manage Items** button in the
      Items To Send header.
- [ ] Keep the existing Manage Items button position and behavior unchanged.

## Step 1 — Select Source Location and Parts

- [ ] Opening the modal shows a **From Location** selector.
- [ ] Selecting a From Location pulls up all part numbers currently in that location.
- [ ] Display the transaction/quantity amount for each part number (for example, `PartA` has
      `5000` in `RECV`).
- [ ] Provide a checkbox for each part so the user can select any or all parts.
- [ ] Add a **number box** next to the checkbox column, defaulting to `1`, to control how many
      destination entries are created for that part.
- [ ] Add a **Save** button that accepts the selected parts and advances to Step 2.

## Step 2 — Add To Locations

- [ ] After Save, the modal content switches to an **Add To Locations** view.
- [ ] Add one line for each selected part where the user can enter the destination location.
- [ ] When a part's number box is greater than `1`, give that part that many destination rows
      (for example, move `2500` to `V-A1-01` and `2500` to `V-A1-02`).
- [ ] Add a final **Save** button that sends all completed rows back to the Items To Send table.

## Acceptance Criteria

- [ ] Advanced button opens the modal on the left of Manage Items.
- [ ] From Location selection filters parts to that location.
- [ ] Part rows show available quantity and a selection checkbox.
- [ ] Number box defaults to `1` and expands Step 2 rows accordingly.
- [ ] Step 2 Save returns all filled rows to the Items To Send table.
- [ ] Existing Manage Items behavior is not regressed.
- [ ] Research complete — technical notes grounded in actual code files (see below).

## 📑 Technical Research Notes

Last Updated: 2026-08-24

### File Targets

- **View (XAML):** `Module_Scanner/Views/View_Scanner_Workbench.xaml`
  - The `Items To Send` header grid is `x:Name="ItemsHeaderGrid"` at approximately lines
    222–248 (inside `ItemsToSendCard` → `ItemsCardContentGrid`).
  - The existing **Manage Items** button lives at `Grid.Column="1"` (approx. lines 230–235) and
    binds to `ViewModel.ManageItemsDialogCommand`. The **Remove Selected** button is at
    `Grid.Column="2"`.
- **ViewModel:** `Module_Scanner/ViewModels/ViewModel_Scanner_Workbench.cs`
  - Hosts `SessionItems` (the grid's `ItemsSource`) and `CurrentSession`.
  - Existing dialog orchestration to model after: `ManageItemsDialogCoreAsync()` (approx. line
    581) — resolves `XamlRoot`, constructs a `ContentDialog`, applies the shared theme helper,
    awaits `ShowAsync()`, and reconciles results back into the session.
- **Row model:** `Module_Scanner/Models/Model_ScannerBatchItem.cs`
  - Key properties: `PayloadPartId`, `PayloadQuantity` (string), `PayloadFromWarehouse`,
    `PayloadFromLocation`, `PayloadToWarehouse`, `PayloadToLocation`, `SequenceNumber`,
    `ValidationState`, `FuzzyMatchedFromLocation`, `FuzzyMatchedToLocation`, `ItemId`.
- **Dialog pattern reference:** `Module_Scanner/Views/View_Scanner_ManageItemsDialog.xaml(.cs)`
  - The established `ContentDialog` subclass pattern: view-specific code-behind, `ObservableCollection<T>`
    working set, `GetItemsSnapshot()` returning cloned models, `PrimaryButtonText="Apply"` /
    `CloseButtonText="Cancel"`.

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

- **Dialog mechanism:** Use a `ContentDialog` subclass (consistent with
  `View_Scanner_ManageItemsDialog` and the repo dialog guidance):
  - Set `XamlRoot` from `IService_Window.GetXamlRoot()`.
  - Apply `Helper_UI_ContentDialogTheme.ApplyTheme(dialog, xamlRoot)`.
  - The two-step wizard can be one dialog whose content `Visibility` toggles between a
    Step 1 panel and a Step 2 panel, or two chained dialogs. A single dialog with an internal
    step state is simpler and matches the "contents of the modal window should then change"
    requirement.
  - Keep validation/persistence in the ViewModel/service layers; the dialog returns a
    snapshot (cloned rows) via a method like `GetResultRows()`.

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

1. **ViewModel — Step 1 state:** Add `AdvancedBulkMoveCommand`. On execution, open the dialog
   and load parts via `_validationService` → `GetMaterialAvailabilityCurrentStockAsync(
   selectedFromLocation, null, warehouse)`.
   - Map each returned `Model_InforVisualMaterialLocationRow` into a lightweight selectable row
     (e.g. `PartId`, `PartDescription`, `Quantity`, `bool IsSelected`, `int EntryCount = 1`).
   - The dialog binds the checkbox column to `IsSelected` and a `NumberBox` to `EntryCount`
     (default `1`), next to the checkbox column as required.
2. **ViewModel — Step 2 state:** On Step 1 Save, keep only rows where `IsSelected`. Build a
   destination-row list by expanding each selected part `EntryCount` times:
   - Example: `PartA`, quantity `5000`, `EntryCount = 2` → two destination rows
     (`PartA → ?`, `PartA → ?`). Each row carries `PartId`, the shared `FromLocation`, and an
     editable `ToLocation` (blank by default) plus an editable per-row quantity (initialize to
     `quantity / EntryCount`, e.g. `2500` / `2500`, so the split is meaningful by default).
   - Add a `NumberBox`/`TextBox` per destination row so the user can adjust each split.
3. **ViewModel — Save back:** On Step 2 Save, transform each destination row into a
   `Model_ScannerBatchItem`:
   - `PayloadPartId = PartId`
   - `PayloadFromWarehouse = warehouse`, `PayloadFromLocation = FromLocation`
   - `PayloadToWarehouse = warehouse`, `PayloadToLocation = entered ToLocation`
   - `PayloadQuantity = entered quantity`
   - Reuse the existing add/persist path (like `AddDraftItemAsync`) so rows flow into
     `SessionItems`/`CurrentSession.Items` and are validated/persisted consistently, or append
     to `CurrentSession.Items` and persist via `ReplaceSessionItemsAsync` (the Manage Items
     pattern).
4. **View:** Add the Advanced button; wire `AdvancedBulkMoveCommand`. The dialog toggles its
   internal Step 1 / Step 2 content and returns the final cloned rows for the ViewModel to
   commit. Do not mutate `SessionItems` from inside the dialog — return snapshots only.