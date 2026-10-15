# HeatLotManagement_FeatureUpdate

- **Last Updated:** 2026-10-14
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
still shown, and the list reports every part that carries it.

Choosing a part in the part list reads the marking list again and narrows it to that part alone. That
is honoured whether or not the part is still enabled, because choosing a part is deliberate and a
removed part's recorded markings must stay reachable.

Two boundaries are worth stating, because each looks like an omission:

- A lot whose number appears on **no** receiving row is still shown. It cannot be attributed to a
  part, so hiding it would hide the lot a corrected number used to describe. This keeps story 8's
  requirement that such a lot stay in the list rather than disappearing.
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
