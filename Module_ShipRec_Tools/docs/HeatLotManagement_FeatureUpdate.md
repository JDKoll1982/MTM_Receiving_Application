# HeatLotManagement_FeatureUpdate

- **Last Updated:** 2026-11-05
- **Module:** Ship/Rec Tools (`Module_ShipRec_Tools`)
- **Status:** Shipped 2026-10-15. Build green, 819 of 819 tests pass, 80 of 80 tasks. Lot marking backfill (user story 6) and the supplier group width fix landed 2026-10-14; Update All Lots (user story 7) landed 2026-10-15.
- **New tables:** `lot_markings`, `lot_marking_part_scope` (MySQL, `mtm_receiving_application` schema)
- **Existing lot key:** `receiving_label_data.heat` and `receiving_history.heat` already store the lot number — no new lot column is introduced

## Copy/Paste Prompt

You are a senior C# / WinUI 3 developer on the MTM Receiving Application (WinUI 3, .NET 10,
CommunityToolkit.Mvvm, MySQL 5.7 read/write via stored procedures only, Infor Visual SQL Server
read-only). Add a new Ship/Rec Tools tool called **Heat / Lot Management** that assigns a visual
marking — one Shape plus one Color — to every unique heat/lot number and supplier, so material
handlers can tell lots apart at a glance. The lot number already exists as the `heat` column on
`receiving_label_data` and `receiving_history`; do not add a second lot column. The marking is
derived deterministically from the normalized heat number plus the supplier, pinned in a MySQL
ledger table on first sight, and stored as `heat_shape_code` / `heat_color_code` on both receipt
tables. The feature applies only to part numbers in an inclusion list that starts empty, and parts
are chosen through the app's existing `Dialog_FuzzySearchPicker` modal. Both purchase-order and
non-PO receipts are in scope. Follow the existing `WeldedCoils` tool as the structural template,
keep MVVM boundaries (View -> ViewModel -> Service -> DAO -> stored procedures), use `x:Bind` in
XAML, and do not use raw MySQL SQL in C#.

## Feature

A new maintenance tool plus a deterministic lot-marking engine. Every unique combination of
normalized heat/lot number and supplier receives a stable Shape + Color marking. The marking is
restricted to enabled part numbers, persisted in a ledger, and carried onto receiving label data and
receiving history so it survives queue promotion and reprint.

## Implementation Order

Order: 3 of a 5-slice feature. Rationale — the palette/resolver and the ledger table must exist and
be testable before the receiving workflow can display markings, and before any later label-printing
work consumes the persisted columns.

1. Slice 1 — palette definitions, lot-number normalization helper, deterministic resolver (pure, unit-testable).
2. Slice 2 — schema + stored procedures + DAO for the ledger and the part-number scope list.
3. Slice 3 — this slice: the Heat / Lot Management tool (ViewModel + View), settings, wiring, help.
4. Slice 4 — receiving workflow surface (line marking display, load marking strip) and label-data/history column write-through.
5. Slice 5 — reprint, analytics, and cross-module parity (Volvo, Dunnage, Non-PO).

## Confirmed Decisions

All items below were confirmed by the product owner on 2026-10-14.

- Tool display name is **Heat / Lot Management**; tool key is `HeatLotManagement`.
- Marking = one Shape plus one Color, stored as stable string codes, never as display names or hex values.
- Assignment is **automatic and deterministic** from the normalized heat/lot number plus the supplier. Operators never choose a routine marking.
- A marking is scoped to a supplier. Two suppliers using the same lot number receive two distinct markings, so the ledger key is (normalized heat number, supplier).
- The supplier is identified by resolving the supplier name on the receipt to the official Infor Visual supplier id, not by comparing raw typed names.
- Non-PO receipts have no supplier, so they are treated as one shared `NONPO` supplier group.
- The ledger table is authoritative after first resolution. The hash is a seed, not a live recompute.
- Markings are **kept forever**. No purge or retention policy is applied.
- Collision policy is **Probe**. When a lot resolves to a shape/color pair already held by another matching lot, the resolver deterministically advances to the next free pair.
- A receipt line with **no heat/lot number gets no marking**; its shape and color columns stay NULL.
- Shape is the primary identifier; color is secondary. Shape never carries meaning by position alone.
- Marking is **in-app only** in this feature. Printed-label rendering is explicitly deferred, but the persisted columns must be data-ready for it.
- Colors — 12 total, duplicate Yellow removed: `BLUE`, `RED`, `GREEN`, `WHITE`, `YELLOW`, `BLACK`, `PURPLE`, `ORANGE`, `PINK`, `GRAY`, `BROWN`, `TEAL`.
- Shapes — 10 total, stored as the emoji itself: `⚡`, `💗`, `🌙`, `⭐`, `💎`, `⬛`, `🔺`, `⭕`, `❌`, `➡️`. Cylinder, Hexagon and Pentagon are dropped because no dependable Segoe UI Emoji character exists for them, and Oval became a circle for the same reason.
- The part-number scope list is an **inclusion list that starts empty**. A part number receives no marking until it is added. A global master switch disables the feature entirely.
- Parts are added **one at a time**, chosen through the app's existing fuzzy-search picker modal (`Dialog_FuzzySearchPicker`) rather than typed as free text.
- The marking appears **on each receipt line and on the load/receipt header**. A whole-load marking summary strip is not required.
- Any user of the Ship/Rec Tools module may override a marking. Overrides are still recorded with who, when, and why.
- The new receipt columns are named `heat_shape_code` and `heat_color_code`, matching the existing `heat` column.
- New settings live under the existing manifest category `ShipRecTools`, resolved through `IService_SettingsCoreFacade` — no bespoke settings table.
- First release covers **standard purchase-order receiving and non-PO receipts**. Volvo and Dunnage are out of scope.

## Current Repo State

- `Module_ShipRec_Tools/Services/Service_ShipRecTools_Navigation.cs` — tool registry; existing keys include `WeldedCoils`.
- `Module_ShipRec_Tools/ViewModels/ViewModel_ShipRecTools_Main.cs` — one `Is*Visible` flag per tool plus a `switch` on the tool key.
- `Module_ShipRec_Tools/Views/View_ShipRecTools_Main.xaml` — one `x:Name="*Host"` ContentControl per tool, bound to the matching `Is*Visible` flag.
- `Module_ShipRec_Tools/Views/View_Tool_WeldedCoils.xaml.cs` — shows the per-tool help call: `ShowHelpAsync("ShipRecTools.WeldedCoils", XamlRoot)`.
- `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs` — per-tool registrations near lines 608 (service), 663 (view model), 674 (view).
- `Module_Core/Services/Help/Service_Help.cs` — help article keys near line 1070 (`ShipRecTools.WeldedCoils`).
- `Module_ShipRec_Tools/Settings/ShipRecToolsSettingsKeys.cs` and `Services/Service_ShipRecToolsSettings.cs` — existing settings-key facade pattern for Ship/Rec tools.
- `Module_Settings.Core/Defaults/settings.manifest.json` — definitions with `category`, `key`, `defaultValue`, `dataType`, `scope`, `permissionLevel`, `isSensitive`; category `ShipRecTools` already in use.
- `Module_Settings.Core/ViewModels/ViewModel_Settings_MaterialAvailabilityBoardFields.cs` — existing Ship/Rec tools settings page host.
- `Database/Database_Deployment/Sql_Files/Schemas/` — numbered table scripts, highest is `49_Table_settings_weldedcoils.sql`.
- `Database/Database_Deployment/Sql_Files/Migrations/` — numbered migrations, highest file prefix is `15`.
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/` — `sp_Receiving_LabelData_*` and `sp_Receiving_History_*` procedure family.
- `Database/Database_Deployment/Sql_Files/Schemas/11_Table_receiving_label_data.sql` line 22 — `heat VARCHAR(100) NULL COMMENT 'Heat/lot number for material traceability'`. **The lot number column already exists and is called `heat`.**
- `Database/Database_Deployment/Sql_Files/Schemas/10_Table_receiving_history.sql` line 13 — the same `heat VARCHAR(100)` column.
- `receiving_label_data.po_vendor` (line 12) and `receiving_label_data.vendor_name` (line 31, commented "used for non-PO items"); `receiving_history.po_vendor` (line 11) and `receiving_history.vendor_name` (line 20) — the supplier is stored as a **name only**. Neither receipt table has a supplier id column, so the supplier id must be resolved during assignment.
- `Module_Core/Dialogs/Dialog_FuzzySearchPicker.xaml` — the shared fuzzy-search picker modal used by other text boxes in the app.
- `Module_ShipRec_Tools/Views/View_Tool_OutsideServiceHistory.xaml.cs` line 41 — in-tool precedent: `ShowFuzzyPickerDialogAsync` is implemented in the view and assigned to the ViewModel's picker delegate in the constructor.
- `Module_Shared/Services/Lookup/Strategy_SharedPartNumberLookup.cs` and `Module_Shared/Services/Lookup/Service_SharedLookupWorkflow.cs` — shared part-number lookup feeding that picker.
- `sp_Receiving_NonPO_Upsert` inserts into `receiving_non_po_entries`, which is a saved reference-reason lookup table (`value`, `created_by`, `use_count`), not the non-PO receipt itself. Non-PO receipts land in `receiving_label_data` with `vendor_name` populated.
- `Module_Receiving/Data/Dao_ReceivingLabelData.cs` — calls `sp_Receiving_LabelData_Insert` (line 185), `sp_Receiving_LabelData_ClearToHistory` (line 240), `sp_Receiving_LabelData_InsertFromHistory` (line 628).
- `Module_Receiving/Data/Dao_ReceivingLoad.cs` — also calls `sp_Receiving_LabelData_Insert` (line 156) and `sp_Receiving_LabelData_ClearToHistory` (line 553).
- `Database/SyncTool/sync_policy.yml` — has an explicit per-table policy entry for `receiving_history`.
- `Database/Database_Deployment/sql-object-catalog.json` and `Database/Database_Deployment/StoredProcedure-Test-Checklist.md` — generated/tracked DB artifacts that enumerate tables and procedures.
- `docs/database/` — generated per-table reference pages (`receiving_history/index.html`, `receiving_label_data/index.html`).
- `MTM_Receiving_Application.Tests/Integration/Database/StoredProcedures/StoredProcedureInitialCoverageIntegrationTests.cs` — direct INSERTs into `receiving_history` and `receiving_label_data`.

## Required Outcome

A single tool page where a user can see every lot's Shape + Color marking, search or filter by heat
number, override or lock a marking, and maintain the part-number inclusion list by picking parts
from the existing fuzzy-search modal. Every receiving label data row and receiving history row
carries the marking codes, and those codes survive the queue -> history promotion and the reprint
round trip for both purchase-order and non-PO receipts.

## Primary Change Areas

- `Module_ShipRec_Tools/Models/Model_Tool_LotMarking.cs` — ledger row model.
- `Module_ShipRec_Tools/Models/Model_Tool_LotMarkingScope.cs` — part-number scope row model.
- `Module_ShipRec_Tools/Models/Model_LotMarkingPalette.cs` — shape/color catalog with code, display name, glyph key, hex token, sort order.
- `Module_ShipRec_Tools/Enums/Enum_LotColor.cs`, `Enum_LotMarkingSource.cs`, `Enum_LotMarkingCollisionPolicy.cs` — stable code enums.
- `Module_ShipRec_Tools/Helpers/Helper_LotNumberNormalizer.cs` — canonical lot token; the single source of truth for normalization.
- `Module_ShipRec_Tools/Helpers/Helper_LotMarkingResolver.cs` — deterministic seed + collision probe; pure and unit-testable.
- `Module_ShipRec_Tools/Services/Service_Tool_LotMarking.cs` — resolves the receipt supplier name to an Infor Visual supplier id, and falls back to the `NONPO` group when no supplier applies.
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs` — owns the part picker delegate contract (`ShowFuzzyPickerAsync`).
- `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml.cs` — implements that delegate with `Dialog_FuzzySearchPicker`, exactly as `View_Tool_OutsideServiceHistory.xaml.cs` does.
- `Module_ShipRec_Tools/Data/Dao_Tool_LotMarking.cs` — instance DAO, injected connection string, returns `Model_Dao_Result` / `Model_Dao_Result<T>`.
- `Module_ShipRec_Tools/Contracts/IService_Tool_LotMarking.cs`, `Services/Service_Tool_LotMarking.cs` — orchestration, normalization enforcement, logging via `IService_ErrorHandler`.
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs` — inherits `ViewModel_Tool_Base`; search, override, lock, scope CRUD.
- `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml` and `.xaml.cs` — tool page; code-behind only wires `XamlRoot` and the help call.
- `Module_ShipRec_Tools/Services/Service_ShipRecTools_Navigation.cs` — add the `HeatLotManagement` tool key.
- `Module_ShipRec_Tools/ViewModels/ViewModel_ShipRecTools_Main.cs` — add `IsHeatLotManagementVisible` and the `case "HeatLotManagement"` branch.
- `Module_ShipRec_Tools/Views/View_ShipRecTools_Main.xaml` and `.xaml.cs` — add the `HeatLotManagementHost` ContentControl and activation hook.
- `Module_ShipRec_Tools/Settings/ShipRecToolsSettingsKeys.cs` and `Services/Service_ShipRecToolsSettings.cs` — add `LotMarking.*` keys and accessors.
- `Module_Settings.Core/Defaults/settings.manifest.json` — add the `ShipRecTools` / `LotMarking.*` definitions.
- `Module_Core/Services/Help/Service_Help.cs` — add the `ShipRecTools.LotMarking` article.
- `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs` — register service, view model, and view.
- `Module_Receiving/Data/Dao_ReceivingLabelData.cs`, `Module_Receiving/Data/Dao_ReceivingLoad.cs` — pass shape/color codes through insert, update, promote, and reprint paths.

## Files That Must Change

- `Module_ShipRec_Tools/Services/Service_ShipRecTools_Navigation.cs`
- `Module_ShipRec_Tools/Settings/ShipRecToolsSettingsKeys.cs`
- `Module_ShipRec_Tools/Services/Service_ShipRecToolsSettings.cs`
- `Module_ShipRec_Tools/ViewModels/ViewModel_ShipRecTools_Main.cs`
- `Module_ShipRec_Tools/Views/View_ShipRecTools_Main.xaml`
- `Module_ShipRec_Tools/Views/View_ShipRecTools_Main.xaml.cs`
- `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
- `Module_Core/Services/Help/Service_Help.cs`
- `Module_Settings.Core/Defaults/settings.manifest.json`
- `Module_Receiving/Data/Dao_ReceivingLabelData.cs`
- `Module_Receiving/Data/Dao_ReceivingLoad.cs`
- `Module_ShipRec_Tools/Enums/Enum_LotColor.cs`
- `Module_ShipRec_Tools/Enums/Enum_LotMarkingSource.cs`
- `Module_ShipRec_Tools/Enums/Enum_LotMarkingCollisionPolicy.cs`
- `Module_ShipRec_Tools/Models/Model_LotMarkingPalette.cs`
- `Module_ShipRec_Tools/Models/Model_Tool_LotMarking.cs`
- `Module_ShipRec_Tools/Models/Model_Tool_LotMarkingScope.cs`
- `Module_ShipRec_Tools/Helpers/Helper_LotNumberNormalizer.cs`
- `Module_ShipRec_Tools/Helpers/Helper_LotMarkingResolver.cs`
- `Module_ShipRec_Tools/Data/Dao_Tool_LotMarking.cs`
- `Module_ShipRec_Tools/Contracts/IService_Tool_LotMarking.cs`
- `Module_ShipRec_Tools/Services/Service_Tool_LotMarking.cs`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs`
- `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml`
- `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml.cs`
- `Database/Database_Deployment/Sql_Files/Schemas/50_Table_lot_markings.sql`
- `Database/Database_Deployment/Sql_Files/Schemas/51_Table_lot_marking_part_scope.sql`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_LotMarkings_*.sql`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Settings/sp_LotMarkingPartScope_*.sql`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_LabelData_Insert.sql`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_LabelData_Update.sql`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_LabelData_ClearToHistory.sql`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_LabelData_InsertFromHistory.sql`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_History_Get.sql`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_History_GetForReprint.sql`
- `Database/Database_Deployment/Sql_Files/Migrations/16_Migration_lot_markings.sql`
- `Database/Database_Deployment/Sql_Files/Migrations/17_Migration_lot_markings_supplier_group_width.sql`
- `Database/SyncTool/sync_policy.yml`
- `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Helpers/Helper_LotMarkingResolverTests.cs`
- `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/Service_Tool_LotMarkingTests.cs`
- `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarkingTests.cs`

## Recommended Target Shape

Ledger table. Keyed on the normalized heat/lot number **plus the supplier**, so two suppliers using
the same lot number get two separate markings while one supplier's lot keeps exactly one marking
forever. The supplier column is never NULL: non-PO receipts use the literal sentinel `NONPO`,
because MySQL unique keys treat NULLs as distinct and would otherwise permit duplicate rows.

```sql
CREATE TABLE lot_markings (
    id                     INT(11)      NOT NULL AUTO_INCREMENT,
    heat_number            VARCHAR(100) NOT NULL COMMENT 'heat column value as captured from the receipt',
    heat_number_normalized VARCHAR(100) NOT NULL COMMENT 'Canonical uppercase token used for matching',
    supplier_id            VARCHAR(15)  NOT NULL DEFAULT 'NONPO' COMMENT 'Infor Visual VENDOR.ID, or NONPO',
    supplier_name          VARCHAR(255) NULL COMMENT 'Supplier name as seen on the receipt, for display and audit',
    shape_code             VARCHAR(32)  NOT NULL COMMENT 'Assigned shape emoji, e.g. ⚡',
    color_code             VARCHAR(32)  NOT NULL COMMENT 'Enum_LotColor code, e.g. TEAL',
    marking_source         VARCHAR(16)  NOT NULL DEFAULT 'Auto' COMMENT 'Auto | Manual | Import',
    is_locked              TINYINT(1)   NOT NULL DEFAULT 0 COMMENT '1 = never recompute or probe',
    first_seen_part_number VARCHAR(50)  NULL,
    resolved_by            VARCHAR(50)  NULL COMMENT 'User id when manually set or overridden',
    resolved_at            DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    notes                  VARCHAR(255) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_lot_markings_heat_supplier (heat_number_normalized, supplier_id),
    KEY ix_lot_markings_marking (shape_code, color_code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

Scope table. Inclusion list — a part number not present here gets no marking at all.

```sql
CREATE TABLE lot_marking_part_scope (
    id         INT(11)     NOT NULL AUTO_INCREMENT,
    part_id    VARCHAR(50) NOT NULL,
    is_enabled TINYINT(1)  NOT NULL DEFAULT 1,
    added_by   VARCHAR(50) NULL,
    added_at   DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uq_lot_marking_part_scope_part (part_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

Column additions. Shape and color travel as a pair and are both nullable, so existing rows and
non-scoped part numbers stay untouched. The names follow the existing `heat` column that already
holds the lot number; no new lot column is introduced.

```sql
ALTER TABLE receiving_label_data
    ADD COLUMN heat_shape_code VARCHAR(32) NULL COMMENT 'Heat/Lot marking shape code',
    ADD COLUMN heat_color_code VARCHAR(32) NULL COMMENT 'Heat/Lot marking color code';

ALTER TABLE receiving_history
    ADD COLUMN heat_shape_code VARCHAR(32) NULL,
    ADD COLUMN heat_color_code VARCHAR(32) NULL;
```

Resolver. Deterministic seed from the normalized heat number plus supplier, then a bounded probe
against live collisions, then persist. The hash is culture-invariant and never depends on current
time or user identity.

```csharp
public static class Helper_LotMarkingResolver
{
    public static Model_LotMarking Resolve(
        string heatNumber,
        string supplierId,
        IReadOnlyList<Model_LotMarking> liveMarkings,
        Enum_LotMarkingCollisionPolicy policy);
}

public static class Helper_LotNumberNormalizer
{
    public static string Normalize(string heatNumber);
}
```

Settings manifest entry, matching the existing `ShipRecTools` category shape.

```json
{
  "category": "ShipRecTools",
  "key": "LotMarking.CollisionPolicy",
  "displayName": "Lot marking collision policy",
  "defaultValue": "Probe",
  "dataType": "String",
  "scope": "System",
  "permissionLevel": "Admin",
  "isSensitive": false,
  "validationRules": "Allow|Probe|Warn"
}
```

## Implementation Steps

1. Confirm the next migration number first. File prefixes run `01`-`15`, but the internal procedure names diverge (`01_Migration_receiving_label_queue_history_alignment.sql` uses `sp_mig38`, `09` uses `sp_mig09`, `12` uses `sp_mig12`). Verify against whatever ledger records applied migrations before naming a new one, or you may collide on a procedure name.
2. Add `Enum_LotColor`, `Enum_LotMarkingSource`, and `Enum_LotMarkingCollisionPolicy`, and fill in the 10 emoji shapes and 12 colors. There is no shape enum.
3. Add `Model_LotMarkingPalette` holding code, display name, glyph key, hex token, and sort order per value; expose shape and color as `IReadOnlyList` lookups by code.
4. Add `Helper_LotNumberNormalizer.Normalize` — trim, collapse internal whitespace, uppercase, and strip known vendor separators. Make it the only normalization path in the app.
5. Add `Helper_LotMarkingResolver.Resolve` — FNV-1a 32-bit hash over the normalized heat token combined with the supplier id, `shapeIndex = hash % shapeCount`, `colorIndex = (hash / shapeCount) % colorCount`, then apply the collision policy.
6. Write the unit tests before wiring any UI: normalization collapse, determinism across repeated calls, two suppliers producing two markings, `NONPO` fallback, distribution sanity, and probe behavior.
7. Create `50_Table_lot_markings.sql` and `51_Table_lot_marking_part_scope.sql`, then the `sp_LotMarkings_*` and `sp_LotMarkingPartScope_*` procedures.
8. Add the `heat_shape_code` and `heat_color_code` columns to `receiving_label_data` and `receiving_history` in a new migration, using the `sp_migNN_add_column_if_missing` idempotency pattern already used in `01_Migration_receiving_label_queue_history_alignment.sql`.
9. Update `sp_Receiving_LabelData_Insert`, `sp_Receiving_LabelData_Update`, `sp_Receiving_LabelData_ClearToHistory`, `sp_Receiving_LabelData_InsertFromHistory`, `sp_Receiving_History_Get`, and `sp_Receiving_History_GetForReprint` to carry the two columns.
10. Add `Dao_Tool_LotMarking` calling only the new procedures, returning `Model_Dao_Result` patterns on every path.
11. Add supplier resolution: turn the receipt's `po_vendor` or `vendor_name` into an Infor Visual supplier id using the existing supplier lookup, and fall back to the literal `NONPO` group when the receipt has no supplier.
12. Add `IService_Tool_LotMarking` and `Service_Tool_LotMarking` — normalize the incoming heat number, resolve the supplier, resolve the marking, persist, and log via `IService_ErrorHandler`.
13. Add `ViewModel_Tool_LotMarking` inheriting `ViewModel_Tool_Base`: search, override marking, lock/unlock, scope add via the part picker, scope remove, palette legend.
14. Add `View_Tool_LotMarking.xaml` with `x:Bind` only, plus code-behind implementing `ShowFuzzyPickerAsync` with `Dialog_FuzzySearchPicker` and the `ShipRecTools.LotMarking` help call.
15. Add `ShipRecToolsSettingsKeys.LotMarking.*` and the matching `Service_ShipRecToolsSettings` accessors, then the `settings.manifest.json` entries.
16. Wire the tool: navigation key, `IsLotMarkingVisible`, `LotMarkingHost`, main-view activation, the three DI registrations, and the `Service_Help` article.
17. Update the receiving write paths in `Dao_ReceivingLabelData` and `Dao_ReceivingLoad` so the codes are passed on insert, promote, and reprint. Purchase-order and non-PO receipts share `receiving_label_data`, so one write path covers both.
18. Add the display surface: the marking on each receipt line and on the load/receipt header.
19. Add the new tables and columns to `Database/SyncTool/sync_policy.yml`, then refresh `sql-object-catalog.json`, `StoredProcedure-Test-Checklist.md`, and the generated `docs/database/` pages.
20. Run the focused test slice and the affected stored-procedure integration tests.

## Task Checklist

- [ ] Next migration number confirmed against the applied-migration ledger before authoring.
- [ ] Palette models added with 10 emoji shapes and 12 colors.
- [ ] Heat-number normalization helper added and unit-tested.
- [ ] Deterministic resolver added and unit-tested, including supplier separation and probe behavior.
- [ ] Ledger table and scope table created with the (heat, supplier) unique key.
- [ ] `sp_LotMarkings_*` procedures created and covered.
- [ ] `sp_LotMarkingPartScope_*` procedures created and covered.
- [ ] `heat_shape_code` and `heat_color_code` added to `receiving_label_data` and `receiving_history` via migration.
- [ ] All six affected receiving procedures updated to carry the columns.
- [ ] Supplier name resolved to an Infor Visual supplier id, with `NONPO` fallback.
- [ ] DAO, service, view model, and view added.
- [ ] Part picker wired to `Dialog_FuzzySearchPicker` through the view's delegate.
- [ ] Settings keys, accessors, and manifest entries added.
- [ ] Help article added for `ShipRecTools.LotMarking`.
- [ ] Tool wired into navigation, main view, and DI.
- [ ] Marking displayed on each receipt line and on the load/receipt header.
- [ ] Receiving write paths pass the codes through insert, promote, and reprint for PO and non-PO receipts.
- [ ] `sync_policy.yml` and generated DB artifacts refreshed.
- [ ] Module doc updated with the final decision set.

## Validation

- Resolving the same heat number and supplier 100 times returns an identical shape and color pair.
- Resolving differently formatted variants of one heat number — different casing, extra whitespace, hyphen variants — returns one ledger row and one marking.
- The same heat number from two different suppliers returns two ledger rows and two different markings.
- A non-PO receipt with no supplier resolves into the `NONPO` group and reuses the same marking on repeat.
- A receipt line with a blank heat number produces no marking and writes NULL columns.
- A part number absent from `lot_marking_part_scope` produces no marking and writes NULL columns.
- A locked marking is returned unchanged even when the resolver would have probed to a different value.
- Adding a value to the palette does not change any marking already present in the ledger.
- A label data row inserted with a marking retains both codes after `sp_Receiving_LabelData_ClearToHistory` promotes it to history.
- A reprint round trip through `sp_Receiving_History_GetForReprint` and `sp_Receiving_LabelData_InsertFromHistory` preserves both codes.
- The part picker opens the shared fuzzy-search modal and adds the confirmed part number to the scope list.
- `dotnet build MTM_Receiving_Application.slnx` succeeds.
- The focused unit and integration slice passes.

## Guardrails

- No raw MySQL SQL in C#. Every write goes through a stored procedure.
- No `Dao_*` reference from any ViewModel; the ViewModel talks only to `IService_Tool_LotMarking`.
- No `x:Bind` violations — no runtime `{Binding}`, no business logic in `.xaml.cs` beyond the picker delegate, `XamlRoot`, and the help call.
- Never store display names or hex values in the database. Codes only.
- Never recompute a marking that already exists in the ledger. The ledger wins.
- Never key the ledger on the raw captured heat string; always key on the normalized token.
- Never allow a NULL into the ledger's supplier column — use the `NONPO` sentinel, or MySQL will accept duplicate rows.
- Do not add a second lot-number column. Reuse the existing `heat` column.
- Do not hand-roll a part-number search box. Reuse `Dialog_FuzzySearchPicker` and the shared part-number lookup.
- Do not add the marking to any printed label in this slice — the persisted columns are the only label-side change.
- Do not widen the feature to Volvo or Dunnage. Both are explicitly out of scope for the first release.
- Do not add a bespoke settings table; route settings through the existing settings core facade.
- Do not add NuGet packages for icon rendering; use existing project assets or vector paths.
- Do not apply a retention or purge job — markings are kept forever.

## Completion Criteria

- The Heat / Lot Management tool is reachable from the Ship/Rec Tools selection page and its help button opens the new article.
- A user can search by heat number, see each marking rendered as shape plus color, override or lock a marking, and maintain the part-number inclusion list by picking parts from the fuzzy-search modal.
- Two suppliers using the same heat number receive two different markings.
- Enabled part numbers receive markings during receiving; disabled part numbers and blank heat numbers do not.
- Marking codes are persisted on both receiving label data and receiving history and survive promotion and reprint, for purchase-order and non-PO receipts alike.
- All settings are editable through the settings core and governed by the existing permission model.
- Tests exist and pass for the resolver, the service, the view model, and the new stored procedures.
- Database artifacts and module documentation are updated to match the shipped state.

## Lot Marking Backfill (user story 6, added 2026-10-14)

A lot is only marked when its receipt is saved and its part is already on the enabled list. Receipts
taken before a part was added therefore stayed unmarked forever, leaving an operator looking at a
part with some lots marked and others blank. The tool can now fill those in, one enabled part at a
time, from the part list on the tool page.

- **Scope**: unmarked rows in both `receiving_label_data` and `receiving_history`, for one part that
  is already on the enabled list. A part that is not enabled cannot be filled in.
- **Preview first**: the action reports how many distinct lots and how many rows it would touch and
  writes nothing until the user confirms. Cancel is the dialog's default button, because this writes
  to historical records.
- **Only NULLs are filled.** A row that already carries a marking is never touched, so no handler ever
  sees a marking change under them, and running the action a second time writes nothing.
- **One lot, one marking**: rows are grouped by the normalized heat number plus the normalized
  supplier, so two raw spellings that normalize to one lot receive the same shape and color.
- **Blank heat stays unmarked**, including the `"Nothing Entered"` sentinel, matching the save path.

New database surface, both under `Sql_Files/StoredProcedures/Receiving/`:

- `sp_Receiving_UnmarkedLotMarkings_Preview` — read only. Lists the unmarked rows grouped by raw heat
  and supplier with a row count, so the application can count distinct lots. The heat is returned as
  captured because normalization belongs to the application.
- `sp_Receiving_UnmarkedLotMarkings_Apply` — fills one lot in a single statement across both tables,
  and refuses when the part is not enabled, when the heat is blank, or when a shape or color is
  missing.

The ledger stays authoritative throughout: a lot that already has a row, locked or not, is filled in
with that row's existing codes rather than a new pair.

## Supplier Group Width Fix (2026-10-14)

The tool refused to record a marking on real data with:

> Stored procedure 'sp_LotMarkings_Resolve' failed: Data too long for column 'p_supplier_id' at row 1

The supplier group is the official supplier id when the Infor Visual vendor search returns exactly one
match, and the normalized vendor name otherwise. The name is the common case: a fuzzy vendor search
that returns nothing, or several vendors, falls back to it. Vendor names are `VARCHAR(255)` on both
receiving tables, but the group column and the `p_supplier_id` parameter of both lot marking
procedures were declared `VARCHAR(15)`, which is the width of an id rather than of a name. MySQL
validated the value against the declared parameter and rejected it with error 1406 before any marking
could be written, so the failure happened at the procedure boundary and the column would have refused
the value too.

Shortening the name to make it fit was rejected deliberately. Two vendors can share their first
fifteen characters, for example two divisions of one manufacturer. A shortened key would merge them
into a single lot and give them one shared marking, which is what the rule against that exists to
prevent. The group key has to carry the whole name.

- `lot_markings.supplier_id` and the `p_supplier_id` parameter of `sp_LotMarkings_Resolve` and
  `sp_LotMarkings_Insert` are now `VARCHAR(255)`.
- `17_Migration_lot_markings_supplier_group_width.sql` widens an installed database. It is idempotent
  and widens only, and it also pins the table to `ROW_FORMAT DYNAMIC`, because the wider key makes
  `uq_lot_markings_heat_supplier` 1420 bytes in utf8mb4 and that needs the larger key limit.
- The migration cannot widen a parameter, because procedures are deployed from their own files. It
  ends with a verification query that reports the declared width of the column and of both
  parameters, so a hand run can see whether the procedures have been re-created yet.

The contract for this feature asked for a group column at `VARCHAR(255)` from the start. The schema
shipped it at `VARCHAR(15)`, and no test could catch the difference, because the tests run against an
in-memory ledger that has no column width. The regression guard added alongside the fix,
`ResolveMarkingAsync_ShouldKeepTheWholeSupplierName_WhenTwoVendorsShareAnIdLengthPrefix`, pins the
intent instead: it asserts that two vendors sharing a fifteen character prefix stay two separate lots,
so a future attempt to shorten the key fails a test rather than merging two suppliers' markings.

## Update All Lots (user story 7, added 2026-10-15)

A lot number can be typed wrong, and its marking is then chosen from a number the material never had.
Correcting the number in Edit Mode rewrites it on both the queue row and the history row but leaves the
marking behind, so the row keeps a shape and color belonging to no lot and the list keeps a lot that no
longer describes anything.

**Update All Lots**, a button in the tool's top card, puts that right. It reports first and writes
nothing until the report is confirmed. It does three things:

1. Where the corrected lot number already exists as a lot, the rows are changed to show that lot's
   shape and color, so all the material for one number shows one marking.
2. Where the corrected lot number carries no marking, the number becomes a lot and is given a shape and
   color chosen the same way a receipt chooses one, by the same shared resolver, so a later receipt of
   that number agrees with the rows.
3. Where a lot's number appears on no receiving row at all, the lot is removed from the list and its
   shape and color are freed for a later lot.

Boundaries, each a deliberate decision rather than an accident:

- Only rows that already carry a marking are changed. Update All Lots puts markings right and never
  introduces one; the fill-in action covers unmarked rows.
- A row whose lot number was cleared to blank loses its marking, and a blank value never becomes a lot.
- A protected lot is neither changed nor removed, and is reported instead.
- A lot number that still appears on even one row is never removed.

## Only the listed parts are marked (user story 10, added 2026-10-15)

The feature is opt-in per part number, and that opt-in was not working. Every receipt that carried a
lot number was given a marking, whatever part it was for. The enabled list was read only by the
fill-in action and by the tool's own part list, never by the path that creates a marking.

The list of recorded markings showed the result: lots belonging to parts nobody had approved, sitting
alongside the ones that were. On the production database at the time of the fix, 35 markings existed,
of which 11 belonged only to parts that were not on the list.

**What was wrong, precisely.** `ResolveMarkingAsync` had no part parameter at all, so it could not
consult the list and could not record which part had created a lot. `lot_markings.first_seen_part_number`
existed, the data access layer accepted it, and the model carried it; the automatic path passed null.
The method's own remark claimed that "part not listed" produced no marking, and nothing implemented
that claim.

**The fix** gives the resolver the part number from the receipt and consults the enabled list at the
one moment it matters:

- A marking is created only for a part on the enabled list. A part that is absent, removed, or not
  listed gets none.
- The check happens **after** the ledger read, not before it. A lot that already has a marking keeps it
  whatever part arrives against it, so removing a part cannot repaint a label a handler has already
  seen, and a marking that a handler has already been shown is never withdrawn.
- A receipt carrying no part number gets no marking, because the enabled list is a list of part
  numbers and a marking nothing can be attributed to cannot be shown to belong to anything.
- If the enabled list cannot be read, the marking is refused rather than risked. The receipt is still
  saved; it is simply saved unmarked.

**The list is now scoped to the same rule.** `sp_Receiving_HeatLotKeys_GetAll` carries each group's
part number, and the tool keeps a lot when at least one enabled part carries it. A lot is carried by a
set of parts rather than owned by one, so a lot used by both a listed part and an unlisted one is
still shown. (Since 2026-11-03 the parts the row reports are narrowed to the listed ones — see
[Tool Layout Rework](#tool-layout-rework-2026-11-03).)

Choosing a part in the part list reads the marking list again and narrows it to that part alone. That
is honoured whether or not the part is still enabled, because choosing a part is deliberate and a
removed part's recorded markings must stay reachable.

Two boundaries are worth stating, because each looks like an omission:

- A lot whose number appears on **no** receiving row is still shown. It cannot be attributed to a
  part, so hiding it would hide the lot a corrected number used to describe. (Superseded 2026-11-03:
  such a lot is no longer shown — see [Tool Layout Rework](#tool-layout-rework-2026-11-03).)
- A lot's parts are read from the receiving rows that carry its number, not from
  `first_seen_part_number`. The column is right only for lots created after this change, and the
  attribution has to be right for every lot, including the ones recorded before the list was
  consulted.

**What was deliberately not done.** The markings already recorded against unlisted material are left
exactly as they are, and nothing new is created for that material. Removing them would change what a
label already on the floor says. They are simply out of the default list, and reachable by choosing
the part number that carries them. Nothing was deleted or altered in production for this change.

**Tests.** `Service_Tool_LotMarkingTests` covers the gate (an unlisted part creates nothing, a removed
part creates nothing, a missing part number creates nothing, an unreadable list creates nothing, a
recorded marking is returned whatever the part, and the creating part is recorded) and the scope (a
lot an enabled part carries is listed, one carried only by unlisted parts is not, a lot with no rows
is kept, choosing a part narrows, a removed part's lots stay reachable, the parts are read from rows,
and the heat search still filters within the scope). `ViewModel_Tool_LotMarkingTests` covers the list
following the chosen part and asking for the whole scope when nothing is chosen.
- It never runs as part of saving an edit. It is only ever run by a person.
- It never changes a lot number, a supplier, or any other receiving value.

This supersedes the earlier rule that recorded markings are kept forever, for exactly one case: a lot
whose number appears on no receiving row. Removal is a deletion, which is what frees the pair and lets
that number be received again as a new lot. The list gives the removed number and the shape and color it
was holding as the trace that survives.

Two costs come with the behaviour. Both are known rather than overlooked:

- A label printed before the correction still shows the marking chosen from the wrong number, so the
  floor and the tool disagree until that label is reprinted. Reprinting is not handled here.
- Freeing a pair means a later lot can be given a shape and color a printed label still shows, so one
  pair can mean two lots at different times.

The four new procedures are `sp_Receiving_HeatLotKeys_GetAll` (read only: every distinct heat and
supplier pair across the queue and history with its marking and row count),
`sp_Receiving_LotMarking_RealignRows`, `sp_Receiving_LotMarking_ClearRows` and `sp_LotMarkings_Delete`.
Both write procedures touch only rows that already carry a marking, and each runs in a transaction so
the queue and history cannot end up disagreeing. `sp_LotMarkings_Delete` refuses a protected lot, so no
code path can remove one by mistake.

One thing is not resolved, and it is the piece most likely to misbehave on real data. A lot's supplier
group is the official supplier id when the receiving side resolved the vendor to a single match, and
the normalized supplier name otherwise, while a receiving row only ever carries the name. The update
therefore matches a row to a lot on either the id or the name, which is deliberately forgiving, because
failing to find a lot that exists would create a second lot for it and give one physical lot two
markings. Whether the receiving side and these repair actions should key the group the same way is a
question for the user.

## Tool Layout Rework (2026-11-03)

The page was rearranged around what a handler does while standing at a rack: look up a lot, read its
shape and color, print the sheet, and only rarely change something. The two editing surfaces moved out
of the way so the list itself fills the page.

**The page is now search plus the lot list.** The bottom card and the side panel are gone from the main
page, so the list runs the full width and the rows are easier to scan.

**Editing moved into modal windows.** Both surfaces open over the main window and the main window stops
taking clicks while one is open:

- **Part numbers that receive markings** — `View_Tool_LotMarkingPartsWindow` hosting
  `View_Tool_LotMarkingPartsDialog`. Adding, filtering, removing, re-enabling and filling unmarked lots
  all happen here.
- **Change this marking** — `View_Tool_LotMarkingChangeWindow` hosting
  `View_Tool_LotMarkingChangeDialog`. The shape, the color, the reason, and the lock are edited here.
  The window closes by itself once a change is saved, because the lot it was opened for has been
  reloaded and there is nothing left to edit.

These are real `Window` hosts, following the same shape as
`View_Volvo_ShipmentHistoryDetailWindow`, not `ContentDialog`s. A `ContentDialog` cannot hold a list
this tall without turning into a scrolling box inside a scrolling page.

**"Change this marking" is a button that enables on a selection.** It sits in the top card beside the
other actions and stays disabled until a row is chosen, so it can never open with no lot to edit.

**Every button on the page is an icon button with a tooltip drawn from the help content.** All labels
come from `IService_Help.GetTooltip`, so the wording lives with the rest of the app's tooltips instead
of in the markup. Each button also carries an `AutomationProperties.Name` so it is readable to a screen
reader.

**The list columns changed.**

- The single "Marked" column became **Shape** (the emoji) and **Color** (a swatch drawn from the palette
  plus the color code), so the two halves of a marking can be read separately.
- "Heat as captured" became **Heat**.
- "Lines" became **Skids**. Each receiving row is one skid, so the count was already a skid count; only
  the label was wrong.
- The "State" column was removed. A locked lot is still reported in the marking window, where the lock
  is changed.

**Only listed parts appear in the results, and only their skids are counted.** This tightens the rule
from [Only the listed parts are marked](#only-the-listed-parts-are-marked-user-story-10-added-2026-10-15):

- The `Parts` column reports only part numbers that are on the enabled list. When a lot is shared with
  an unlisted part, the unlisted part number no longer appears, because it is not material this page
  answers for.
- The **Skids** count and the **Last line** date cover only the skids the listed parts carry. A lot
  shared with an unlisted part therefore no longer reports the whole lot's total.
- A lot that no listed part receives is not shown at all. That includes a lot whose number appears on
  no receiving row, which was previously kept so a corrected number could still be traced. The
  superseding decision is that the list answers for listed material only; `Update All Lots` still
  reports and removes such lots, so the trace is not lost.

Choosing a part still narrows the list to that part alone, and is still honoured whether or not the
part is enabled.

**Printing (previously promised, now built).** The print button in the top card prints exactly what the
list is showing, so a heat search or a chosen part is reflected in the sheet. The report uses the same
columns as the grid — Shape, Color, Heat, Supplier, Parts, Skids, Last line — plus the print date, the
user, and the search text. It is built as a `Model_FormattedReportDocument` and printed through the
module's existing HTML path (`Helper_PrintDocumentHtml` + `Helper_HtmlReportExport`), which opens the
print dialog; that is the same path the Delivery Schedule, Receiving Analytics and Material
Availability Board use, so there is one printing behaviour in the module rather than two. The button is
disabled while the list is empty.

**Tests.** `Service_Tool_LotMarkingTests` was updated for the narrowed scope: a lot shared with an
unlisted part reports only the listed part and only that part's skids, and a lot no receiving row
carries is no longer listed. `ViewModel_Tool_LotMarkingTests` supplies the new help service dependency.


### The modal windows use DevWinUI ModalWindow (2026-11-03)

The two editing windows are `DevWinUI.ModalWindow` subclasses rather than plain `Window` hosts.

**Why it changed.** The first version blocked the main window by turning off hit testing on its content
root and releasing that in the window's `Closed` handler. Two things were wrong with it. The block was a
single switch with a single release path, so a close that missed that path left the application painted
but deaf to every click — a freeze the user could only clear by restarting. And the windows extended
content into the title bar while drawing no caption buttons of their own, so the minimize, maximize and
close buttons were drawn faintly over the page and were easy to miss.

**What `ModalWindow` gives.** It creates its window with `OverlappedPresenter.CreateForDialog()` and sets
`IsModal = true`, having first set the main window as the window's owner. Windows itself then disables the
main window for as long as the modal is open and re-enables it when the modal closes, and it brings the
main window back to the foreground. Nothing in this application decides when the main window may be used,
so no application bug can leave it unable to take a click.

Modal windows use the dialog presenter, so they show a system title bar with a normal close button but no
minimize or maximize. That is deliberate: a modal window with a minimize button can be sent behind the
window it is blocking, which leaves the user with a disabled window and nothing to click.

**The title bar is left to Windows.** Neither window calls `ExtendsContentIntoTitleBar`, and neither
content page declares a title bar element. The system draws the title, the icon and the caption buttons,
so they are always visible and always match the rest of the desktop.

**Verifying it.** Confirmed against a running build by enumerating the process's windows: while the modal
is open the modal window reports itself enabled and the main window reports itself disabled, and once the
modal closes the main window is enabled again and the part list reloads. The window also stayed open for
39 seconds untouched before being closed, so nothing closes it on its own.

### Platform bump required by DevWinUI (2026-11-03)

`DevWinUI` 10.9.1 depends on `Microsoft.WindowsAppSDK.WinUI` 2.3.6 or later, while this solution pinned the
`Microsoft.WindowsAppSDK` metapackage at 2.2.0, which resolves WinUI 2.2.1. The metapackage was moved to
**2.4.0**, the version whose WinUI is 2.3.6. The resolved graph is now: WindowsAppSDK 2.4.0, WinUI 2.3.6,
Runtime 2.4.0, DevWinUI 10.9.1 and DevWinUI.Base 10.9.1.

A note for anyone building this after the bump: the first build after the change produced a broken output
folder, because the incremental build removed the old runtime files without laying down the new ones, and
the app failed at startup with `Class not registered (0x80040154)` from `Application.Start`. A clean rebuild
(`dotnet build -t:Rebuild`) produced a complete output and the app started normally. If the same error
appears, clean the output rather than suspecting the code.

The publish scripts under `Database/Publish_and_Installation/` read the target framework and runtime
identifier from the project file, so they needed no change.

## Filter window: dates, sort order, and columns (2026-11-03)

The list was narrowed and ordered by whatever the code felt like showing. A **Filters** button in the top
card now opens a window that decides all three of what appears, in what order, and with which columns —
and the printed sheet follows the same choices, so a sheet can no longer disagree with the screen.

**A dedicated button, not the print button.** Printing deliberately stays a one-click action: it prints
what is already on screen. Putting the options behind the print button would have meant a dialog between
a user and a sheet they can already see, and would have left the on-screen list unable to be filtered at
all. So the Filter button narrows the list, and Print reproduces it.

**The window holds three groups.**

- **Which dates to include.** Two `CalendarDatePicker`s plus a **Quick date range** menu with the same
  nine presets the rest of the app offers — Last 7/30/90 Days, This/Last Month, This/Last Quarter, This
  Year, Last 12 Months — built from `Helper_DateRangePresets`, so the wording and the arithmetic cannot
  drift from Receiving Analytics.
- **What to sort by.** The six choices a handler actually asks for: most recently received, heat / lot
  number, most skids, supplier name, part number, and shape and color, each with a Highest-first /
  Lowest-first switch. Every order falls back to the heat number to break ties, so two lots that share a
  sorted value keep one order between reads instead of shuffling after an unrelated edit.
- **Which columns to show.** A checkbox per column. The same keys drive the grid and the report, from
  `Model_Tool_LotMarkingColumnCatalog`.

**A lot is listed when any of its skids was received inside the range.** That is the reading that answers
"what came in last week", and it is why the range is applied to the receiving rows rather than to the
lot's most recent date. Because the rows are narrowed before anything is counted, the **Skids** figure and
the **Last received** date describe the skids inside the window rather than the lot's whole history.

**The choices are per user and are stored in the database.** Five settings were added under
`ShipRecTools`: `LotMarking.List.FromDate`, `.ToDate`, `.SortField`, `.SortDescending` and
`.VisibleColumnKeys`. All five are user-scoped and carry the User permission level, so one person's date
range never narrows another's list. They read and write through `IService_SettingsCoreFacade` using the
signed-in employee number, and the store writes its own default the first time a key is read, so no seed
script is needed. Verified end to end: a range applied in one session came back selected after a restart.

**Two guards worth knowing about.**

- A range that runs backwards is refused before anything is saved or read, with a message naming the
  problem. It is a typo, not an empty result, and it should not become the saved preference.
- Turning every column off is refused: the stored set falls back to showing every column, because an
  empty grid is something a user cannot recover from without opening the Filter window again.

### The stored procedure needed a date range

`sp_Receiving_HeatLotKeys_GetAll` returned each group's newest receipt date only, so a range could not be
answered server-side. It now takes two optional `IN DATETIME` parameters, `p_from_date` and `p_to_date`,
and applies them before grouping, which is what makes the counts exact for the window. Both default to
NULL, and a NULL pair reads every row exactly as before, so any caller that does not pass them is
unaffected. `p_to_date` is treated as a whole day, so a receipt at 23:59 on the final day is included.

`Dao_Tool_LotMarking.GetReceivingHeatLotKeysAsync` gained the matching optional parameters, and the two
repair paths — Update All Lots and its preview — keep calling it with none, because they must see the
whole record rather than one window of it.

**The application also applies the range itself.** `Service_Tool_LotMarking` drops any returned row whose
date falls outside the chosen window. With the current procedure this removes nothing, since the rows
arrive already bounded. It runs anyway because of what happened during development: against a database
still running the earlier procedure, MySQL silently ignored the two new parameters and the list returned
every lot with the filter appearing to be applied. The client-side check turns that silent wrong answer
into a correct one, and the procedure remains the authority.

**Deploying it.** `Database/Scripts/Deploy/sp_Receiving_HeatLotKeys_GetAll_Deploy.sql` carries the
definition with test blocks for the schema, the parameter list, a NULL pair, and a one-day range. Run it
against `mtm_receiving_application_test` first, then change its one `USE` line to
`mtm_receiving_application`. It DROPs and re-CREATEs, so it is safe to run twice.

**Tests.** `Service_Tool_LotMarkingTests` covers the range (only in-range lots listed), the ordering
(newest first by default, most skids first when asked), and a backwards range. `ViewModel_Tool_LotMarkingTests`
covers applying a filter (the range, the sort field and the hidden column are all saved, and the hidden
column stops being reported as shown), refusing a backwards range without saving or reading, Reset
restoring every column, and printing honouring both the hidden column and the date line.

## The list reads the receiving lines, not just the ledger (2026-11-05)

A handler reported that the date filter was not showing the material that actually came in. For part
`MMC0000848` between 2026-10-06 and 2026-11-06 the Receiving Edit grid held five lots — `344934` BLACK
on 6 skids, `345121` ORANGE on 4, `346732` TEAL and RED on 4, `346733` PINK on 1, and `842T30290` GREEN
on 6 — and the lot list showed three.

**Why two lots went missing.** The list was built from the ledger and the receiving lines only made its
rows heavier: a ledger row was narrowed to the lines whose `(normalized heat, supplier group)` matched it
exactly, and a ledger row that matched nothing was dropped. Three receipts saved on 2026-11-05 supplied
the two ways that could fail:

- The `842T30290` marking was recorded against supplier group `VID-001080-01`, because the Infor Visual
  vendor lookup resolved the vendor to its official id on that save and to its name on every earlier
  one. The lines carry `MST Steel Corporation`, so nothing matched and 6 skids of green material were
  invisible. The same split gave `346732` a second marking, because the lot already held one.
- The `346733` marking was recorded under `T46733`, one character off the number the lines carry, so it
  could not match either — and because the list was ledger-driven, a lot with no matching ledger row
  had no way onto it at all.

**What reads the list now.** `Service_Tool_LotMarking.GetLedgerAsync` reads the receiving lines for the
range, groups them into lots by normalized number and supplier, and reports one row per lot. Nothing else
decides whether material appears. The ledger is still what supplies the marking, in two passes: the entry
recorded against the same supplier first, then an entry for the same number recorded against another
supplier spelling, because a vendor keyed as an id on one receipt and as a name on another is one vendor.
An entry is used once, so one marking can never appear against two lots.

A lot the ledger does not know shows the **shape and color its own lines carry**, which is what a handler
saw on the receipt, and carries no ledger id. That is the intended worst case: an unreachable marking
costs a recorded marking, not a missing lot.

**What that changed on screen.**

- The **Change this marking** button stays off for a lot with no recorded marking, and the save path says
  why if it is somehow reached. Nothing can be edited on a marking that does not exist yet; Update all
  lots is what records one.
- The empty-list wording now says no lots came in for the listed parts in those dates, rather than no
  marks were found, because the list no longer only holds lots that carry a marking.

**Repairing the three ledger rows.** `Database/Database_Deployment/Sql_Files/Migrations/18_Migration_lot_markings_reachable_marks.sql`
re-keys the two markings that no line can reach and corrects the misspelled number, keeping the shapes and
colors a handler has already seen. It leads with read-only diagnostics, runs the two updates inside a
transaction behind their own guards, then verifies, and it commits nothing on its own. Row 118, the second
marking for `346732`, is deliberately left alone and reported as a duplicate: which of the two markings the
lot should keep is a decision for a person, and the file's optional section holds the guarded delete plus
the instruction to run Update all lots afterwards. Verified on a disposable copy of the schema: the
diagnostic goes from three rows to one, and a second run changes no row.

The underlying split was fixed the same day, in the next subsection: the resolver now looks under both supplier
spellings before it creates anything, so a lot no longer gains a second marking when one receipt keys it by
the official id and the next by the name. The supplier key is still two-sided; what changed is that a second
marking is no longer created because of it.

**Tests.** `Service_Tool_LotMarkingTests` gained four cases: a lot with no recorded marking is listed with
its own codes, a marking keyed by supplier id is still found from lines that carry the name, a marking
under a differently spelled number does not hide the lot, and one marking is never put against two lots.

### The one search box covers part numbers too (2026-11-05)

The list holds every enabled part's lots, and the only way to narrow it to one material was the part list
dialog — which is a maintenance screen, not something a handler opens to read the list. The search box now
matches **part numbers as well as lot numbers**, so typing `MMC0000848`, or just `848`, leaves only that
part's lots and typing `3467` still narrows by lot number.

The match is the same on both sides: the row must contain what was typed, ignoring case, so a part number
can be typed whole or by its tail, and the lot number is also compared with its separators removed. The two
kinds of search are not combined into an AND — one box answers "which material is this" whichever way the
handler happens to know it. The box, its watermark, the tooltip, the empty-list wording and the printed
sheet's search line all say search rather than heat search, because a sheet that records "heat search:
MMC0000848" would misdescribe what was narrowed.

### A lot that already has a marking keeps it (2026-11-05)

Saving a receipt for `346732` produced two rows with a black square and RED next to two rows with a triangle
and TEAL, for one lot. The lot already had a marking; the save gave it a second one anyway.

**Why.** `ResolveMarkingAsync` looked the lot up under one key: the normalized heat number and the resolved
supplier group. The group is the official vendor id when the Infor Visual lookup resolves to exactly one
vendor, and the vendor name otherwise. `346732` was first recorded against `MST STEEL CORPORATION` and the
next save resolved the same vendor to `VID-001080-01`, so the lookup found nothing, the probe handed out the
next free pair, and a second marking was written for a lot that already had one. Nothing in the resolver
knew the two keys meant one vendor.

**What it does now.** Before creating anything, the resolver makes a second read under the supplier name the
receipt itself carries, so a lot recorded under the name is found by a save keyed on the id and the other way
round. The probe only runs when both reads come back empty, which is the case for a genuinely different
supplier, so two suppliers sharing a heat number still get two markings — the confirmed decision that a
marking is scoped to a supplier is unchanged. A marking created from now on records the receipt's supplier
name rather than the key in `supplier_name`, which is what keeps the two spellings related for the next read.

**Update All Lots was leaving the damage behind.** It gathered a lot's rows, compared the recorded marking
with the *first* row only, and called the lot correct when that one row matched. A lot whose rows carried two
markings therefore stayed in two markings for as long as the first row happened to be the right one. Every row
of the lot is compared now, and the raw spellings are gathered by the pair the write matches on, so a lot is
written once and the reported row count is the number of rows that actually changed rather than a multiple of
it.

**Tests.** `Service_Tool_LotMarkingTests` gained a case for the reported bug — a lot recorded under the
vendor name is returned unchanged when the same receipt resolves the vendor to its id, and nothing is
inserted — and a case for a lot whose rows carry two markings being rewritten in full by Update all lots.
