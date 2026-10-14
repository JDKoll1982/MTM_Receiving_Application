# Service and DAO Contract: Heat / Lot Management

The C# contract for the Heat / Lot Management feature as consumed by `ViewModel_Tool_LotMarking`,
`ViewModel_Settings_LotMarking`, and the receiving save path. Layering stays
View -> ViewModel -> Service -> DAO -> MySQL, and every MySQL call reaches a stored procedure.

## Layering

- `View_Tool_LotMarking` binds with `x:Bind` only. Its code-behind holds only `XamlRoot`, the help
  call, and the fuzzy-picker delegate, exactly as `View_Tool_OutsideServiceHistory` already does.
- `ViewModel_Tool_LotMarking : ViewModel_Tool_Base` owns presentation state and calls
  `IService_Tool_LotMarking` only. It never names a `Dao_` type and never names
  `Helper_Database_*`.
- `Service_Tool_LotMarking` holds the business rules: palette, normalization, resolution, override,
  lock, and the enabled part list.
- `Dao_Tool_LotMarking` is instance-based, takes the MySQL connection string in its constructor, is
  async throughout, and returns `Model_Dao_Result` or `Model_Dao_Result<T>` on every path.
- No raw MySQL SQL appears in any C# file.

## Helpers

### `Helper_LotNumberNormalizer`

- `Normalize(string? heat)` returns the matching key: trimmed, internal whitespace collapsed,
  upper-cased, and known vendor separators stripped. `ABC-123`, `abc123`, and `ABC 123` therefore
  produce one key.
- It returns `null` for a blank heat, for whitespace only, and for the `"Nothing Entered"` sentinel the
  receiving flow already substitutes at three call sites. Treating that sentinel as a real lot number
  would collapse every blank-heat line into one shared lot.
- The stored `heat` value is never rewritten. Existing rows and the reprint procedure copy `heat`
  verbatim.
- **Satisfies**: FR-003, FR-008, and the edge cases on spacing, casing, and separators.

### `Helper_LotMarkingResolver`

- `Resolve(...)` is pure and deterministic: it seeds a combination from the lot key, then probes
  forward past any combination already held by another lot, up to a bounded number of attempts, and
  accepts the seeded value when every attempt is taken.
- It never fails. When all 120 combinations are in use it still returns a marking, and the duplicate is
  surfaced in the tool instead of thrown at the operator.
- It is a pure static helper, so it is unit-testable without a database.
- **Satisfies**: FR-001, FR-002, FR-015, and the edge case requiring a marking even when the palette is
  exhausted.

## `Dao_Tool_LotMarking`

`Module_ShipRec_Tools/Data/Dao_Tool_LotMarking.cs`. Instance-based; constructor takes
`string connectionString`. Every method is `virtual async Task<Model_Dao_Result<...>>` so tests can
substitute it.

```csharp
public class Dao_Tool_LotMarking
{
    public Dao_Tool_LotMarking(string connectionString);

    Task<Model_Dao_Result<List<Model_Tool_LotMarking>>> GetAllAsync(string? supplierGroup);
    Task<Model_Dao_Result<List<Model_Tool_LotMarking>>> GetByHeatAsync(string heatSearch, int limit);
    Task<Model_Dao_Result<Model_Tool_LotMarking?>> ResolveAsync(string heatKey, string supplierGroup);
    Task<Model_Dao_Result<int>> InsertAsync(Model_Tool_LotMarking marking);
    Task<Model_Dao_Result> UpdateMarkingAsync(
        int id, string shapeCode, string colorCode, string reason, string modifiedBy);
    Task<Model_Dao_Result> SetLockedAsync(int id, bool isLocked, string modifiedBy);

    Task<Model_Dao_Result<List<Model_Tool_LotMarkingScope>>> GetScopeAsync();
    Task<Model_Dao_Result<List<Model_Tool_LotMarkingScope>>> SearchScopeAsync(
        string searchText, int limit);
    Task<Model_Dao_Result<int>> UpsertScopeAsync(string partId, string addedBy);
    Task<Model_Dao_Result> SetScopeEnabledAsync(string partId, bool isEnabled, string modifiedBy);
    Task<Model_Dao_Result> DeleteScopeAsync(string partId);
}
```

Each method maps to one procedure and translates `OUT p_status` / `OUT p_error_message` into the
result: `p_status = 0` becomes a success carrying `Data`, `p_status = 1` becomes a failure carrying
`p_error_message`. A `SIGNAL SQLSTATE '45000'` arrives as a failure with the procedure's
`MESSAGE_TEXT`, so the operator reads a business reason and never a MySQL error number.

| Method | Procedure |
|---|---|
| `GetAllAsync` | `sp_LotMarkings_GetAll` |
| `GetByHeatAsync` | `sp_LotMarkings_GetByHeat` |
| `ResolveAsync` | `sp_LotMarkings_Resolve` |
| `InsertAsync` | `sp_LotMarkings_Insert` |
| `UpdateMarkingAsync` | `sp_LotMarkings_UpdateMarking` |
| `SetLockedAsync` | `sp_LotMarkings_SetLocked` |
| `GetScopeAsync` | `sp_LotMarkingPartScope_GetAll` |
| `SearchScopeAsync` | `sp_LotMarkingPartScope_Search` |
| `UpsertScopeAsync` | `sp_LotMarkingPartScope_Upsert` |
| `SetScopeEnabledAsync` | `sp_LotMarkingPartScope_SetEnabled` |
| `DeleteScopeAsync` | `sp_LotMarkingPartScope_Delete` |

## `IService_Tool_LotMarking`

`Module_ShipRec_Tools/Contracts/IService_Tool_LotMarking.cs`.

```csharp
public interface IService_Tool_LotMarking
{
    Task<Model_Dao_Result<bool>> IsEnabledAsync();
    Task<Model_Dao_Result<Model_LotMarkingPalette>> GetPaletteAsync();
    Task<Model_Dao_Result<List<Model_Tool_LotMarking>>> GetMarkingsAsync(string? supplierGroup);
    Task<Model_Dao_Result<List<Model_Tool_LotMarking>>> FindByHeatAsync(string heatNumber);

    Task<Model_Dao_Result<Model_Tool_LotMarking?>> ResolveForReceiptAsync(
        string? heatLotNumber,
        string? supplierId,
        string? supplierName,
        string partId,
        string modifiedBy);

    Task<Model_Dao_Result> OverrideMarkingAsync(
        int id, string shapeGlyph, Enum_LotColor color, string reason, string modifiedBy);
    Task<Model_Dao_Result> SetLockedAsync(int id, bool isLocked, string modifiedBy);

    Task<Model_Dao_Result<List<Model_Tool_LotMarkingScope>>> GetPartsAsync();
    Task<Model_Dao_Result<List<Model_Tool_LotMarkingScope>>> SearchPartsAsync(
        string searchText, int limit);
    Task<Model_Dao_Result<int>> AddPartAsync(string partId, string addedBy);
    Task<Model_Dao_Result> SetPartEnabledAsync(string partId, bool isEnabled, string modifiedBy);
    Task<Model_Dao_Result> RemovePartAsync(string partId);
}
```

### Operations

#### `IsEnabledAsync`

- **Inputs**: none. **Returns**: the `LotMarking.Enabled` switch.
- **Failure modes**: a missing manifest entry resolves to the `true` default rather than an error.
- **Satisfies**: FR-013.

#### `GetPaletteAsync`

- **Inputs**: none. **Returns**: `Model_LotMarkingPalette` — the 10 shapes and 12 colors, each with its
  display name and list order, filtered and ordered by `LotMarking.Palette.EnabledShapes` and
  `LotMarking.Palette.EnabledColors`.
- **Failure modes**: an unset or unparseable palette setting falls back to the complete default set, so
  the palette is never empty.
- **Satisfies**: FR-021, and the palette is the input FR-018's override picks from.

#### `GetMarkingsAsync`

- **Inputs**: `supplierGroup` optional — `null` returns every lot. **Returns**: the ledger projection
  ordered newest first.
- **Failure modes**: a database failure returns a failed result; the view model shows it through
  `ShowStatus` and leaves the list empty rather than throwing.
- **Satisfies**: FR-018, FR-019.

#### `FindByHeatAsync`

- **Inputs**: `heatNumber` — the operator's search text, which may be partial and unnormalized.
  **Returns**: matching lots.
- **Failure modes**: no match is a successful empty result, not a failure; the tool shows its empty
  state.
- **Satisfies**: FR-017, SC-009.

#### `ResolveForReceiptAsync`

This is the only method on the receiving write path and the only place a marking is ever created.

1. Return a successful result with `null` data when `heatLotNumber` normalizes to `null`, when the
   feature switch is off, or when `partId` is not on the enabled list. No codes are written and the
   receipt proceeds untouched.
2. Resolve `supplierId` to the official supplier record through the existing Infor Visual vendor fuzzy
   search, taking the id when exactly one match is returned. On zero or multiple matches, fall back to
   the normalized `supplierName`. On a receipt with no supplier at all, use `NONPO`.
3. Read the ledger with the normalized heat key, then write through `Helper_LotMarkingResolver` only
   when the lot is absent, and return the winning row.

- **Inputs**: `heatLotNumber`, `supplierId`, `supplierName`, `partId`, `modifiedBy`.
- **Returns**: the pinned ledger row whose `heat_shape_code` and `heat_color_code` the caller writes
  onto the queue row, or `null` data for "no marking required".
- **Failure modes**: all expected conditions — blank heat, feature off, part not listed, unresolvable
  supplier, palette exhausted — return success with `null` data or a fallback group, never a failure.
  A genuine database failure returns a failed result, and the caller still saves the receipt with both
  codes left NULL.
- **Satisfies**: FR-001, FR-002, FR-003, FR-005, FR-006, FR-007, FR-008, FR-009, FR-010, FR-013,
  FR-015.

#### `OverrideMarkingAsync`

- **Inputs**: `id`, `shape`, `color`, `reason`, `modifiedBy`. `reason` is required by the procedure, so
  the view model disables Save until it is non-blank.
- **Returns**: a bare `Model_Dao_Result`. **Failure modes**: a failed result carrying the procedure's
  `'A reason is required to change a lot marking'` or `'Lot marking not found'` message.
- **Behaviour**: the service does not consult `is_locked` before delegating. FR-019 constrains the
  system, not the operator, so a locked row stays overridable by hand and stays locked afterwards.
- **Satisfies**: FR-018, FR-020.

#### `SetLockedAsync`

- **Inputs**: `id`, `isLocked`, `modifiedBy`. **Returns**: a bare `Model_Dao_Result`.
- **Failure modes**: a failed result carrying `'Lot marking not found'`.
- **Satisfies**: FR-019.

#### Part list operations

- `GetPartsAsync` returns the whole list for the enabled-part view. **Satisfies**: FR-009, FR-010.
- `SearchPartsAsync` searches rows already on the list so a removed part can be found and re-enabled.
  The candidate list a user picks a **new** part from comes from the existing part fuzzy search.
  **Satisfies**: FR-011.
- `AddPartAsync` is idempotent; adding a part that is already listed reactivates it rather than
  failing. **Satisfies**: FR-009, FR-011.
- `SetPartEnabledAsync` is the tool's Remove and Re-enable action. It deactivates and never deletes, so
  recorded markings are untouched. **Satisfies**: FR-012.
- `RemovePartAsync` is the hard delete used only to clear a row added by mistake.
  **Satisfies**: FR-012.
- **Failure modes**: a blank part number or an unknown part number returns a failed result carrying the
  procedure's message. A repeated add is not a failure.

## Receiving Integration

FR-014 and FR-016 require the marking to reach the queue row, the history row, and the reprint grid,
so the receiving path changes alongside the new tool. The layering does not change: the receiving
service calls `IService_Tool_LotMarking`, a contract, rather than reaching into the tool's internals.

- `Model_ReceivingLoad` gains `HeatShapeCode` and `HeatColorCode`, observable, nullable strings.
- `Service_MySQL_Receiving` resolves once per load inside `SaveReceivingLoadsAsync` and
  `UpdateReceivingLoadsAsync`, before it calls the DAO. It resolves **once per load save**, never on
  promotion and never on reprint.
- `Dao_ReceivingLabelData` passes the two codes to `sp_Receiving_LabelData_Insert` and
  `sp_Receiving_LabelData_Update`, reads them back in its `GetAllAsync` projection, and copies them
  through `sp_Receiving_LabelData_ClearToHistory` and `sp_Receiving_LabelData_InsertFromHistory`.
- `Dao_ReceivingLoad` carries the two codes on its `GetAll`, `Insert`, and `Update` calls, which reach
  `sp_Receiving_Load_GetAll`, `sp_Receiving_Load_Insert`, and `sp_Receiving_Load_Update`.
- The reprint read path returns the two codes from `sp_Receiving_History_GetForReprint`, and
  `Model_ReprintSelectableRow` gains the two fields plus mapper support, because that mapper currently
  discards the `heat` the procedure already returns.
- Nothing re-resolves on promotion or on reprint. `sp_Receiving_History_Get` and
  `sp_Receiving_History_GetForReprint` only read what was stored.

## Settings Accessors

`IService_ShipRecToolsSettings` and `Service_ShipRecToolsSettings` gain five read accessors and five
matching save accessors. Each save returns a bare `Model_Dao_Result`.

```csharp
Task<Model_Dao_Result<bool>> GetLotMarkingEnabledAsync();
Task<Model_Dao_Result<Enum_LotMarkingCollisionPolicy>> GetLotMarkingCollisionPolicyAsync();
Task<Model_Dao_Result<List<Model_LotMarkingShape>>> GetLotMarkingEnabledShapesAsync();
Task<Model_Dao_Result<List<Enum_LotColor>>> GetLotMarkingEnabledColorsAsync();
Task<Model_Dao_Result<bool>> GetLotMarkingShapeFirstAsync();

Task<Model_Dao_Result> SaveLotMarkingEnabledAsync(bool enabled);
Task<Model_Dao_Result> SaveLotMarkingCollisionPolicyAsync(Enum_LotMarkingCollisionPolicy policy);
Task<Model_Dao_Result> SaveLotMarkingEnabledShapesAsync(IReadOnlyList<Model_LotMarkingShape> shapes);
Task<Model_Dao_Result> SaveLotMarkingEnabledColorsAsync(IReadOnlyList<Enum_LotColor> colors);
Task<Model_Dao_Result> SaveLotMarkingShapeFirstAsync(bool shapeFirst);
```

- `ShipRecToolsSettingsKeys` gains a nested `LotMarking` class holding the five key constants. The
  existing `ShipRecToolsSettingsKeys.Category` value `"ShipRecTools"` is reused unchanged; no enum or
  parser change is needed.
- A missing manifest entry or an unparseable value resolves to the documented default and reports
  success, so a bad value can never blank the palette or stop a receipt.
- **Failure modes**: a genuine save failure returns a failed result and leaves the stored value
  untouched; the settings page shows it through the shared error path.
- **Satisfies**: FR-013, FR-018, FR-021.

## Registration

`Infrastructure/DependencyInjection/ModuleServicesExtensions.cs` gains four registrations inside the
existing `AddShipRecToolsModule` extension, reusing its `mySqlConnectionString` local.

1. `Dao_Tool_LotMarking` — singleton, constructed from `mySqlConnectionString`.
2. `IService_Tool_LotMarking` -> `Service_Tool_LotMarking` — singleton.
3. `ViewModel_Tool_LotMarking` — transient.
4. `View_Tool_LotMarking` — transient.

No service is registered directly in `App.xaml.cs`.

## Global Failure Rules

- Expected failures return a failed `Model_Dao_Result` and never throw. Validators run at the service
  boundary rather than trusting a caller.
- An unexpected exception is caught in the view model and routed to `IService_ErrorHandler`; a DAO
  never calls a user-facing surface.
- A marking problem never fails a receipt. When `ResolveForReceiptAsync` returns `null` data or a
  failed result, the two codes stay NULL, the line still saves, and the receipt completes
  (FR-008, SC-008).
- An Infor Visual failure during supplier resolution falls back to the normalized supplier name rather
  than failing the receipt, because the feature must never block receiving.
- Async methods end with `Async`.
