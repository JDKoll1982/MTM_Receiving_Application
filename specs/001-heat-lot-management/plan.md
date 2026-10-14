# Implementation Plan: Heat / Lot Management

**Feature**: `specs/001-heat-lot-management/heat-lot-management.spec.md`
**Created**: 2026-10-14
**Size**: `oversized`

## Scale note

This change spans four subsystems: two MySQL tables with two new stored-procedure families, twelve
existing database objects that enumerate columns explicitly, a new Ship/Rec Tools tool with its own
palette, resolver, DAO, service, view model and view, and a new settings page in `Module_Settings.Core`
with a part-number list carried as data rather than as a setting. A reader should watch two seams in
particular: the receiving write path, where a marking must be written once and then copied verbatim
through promotion and reprint rather than re-resolved, and the migration, which must ship the
re-deployed procedures it invalidates.

## Summary

Give every unique combination of heat number and supplier a permanent visual marking made of one
shape and one color, chosen automatically the first time the lot is seen. The marking is resolved by
a pure deterministic resolver in `Module_ShipRec_Tools`, pinned in a new MySQL ledger table, and
written onto the receiving queue row so it travels into receiving history and back out on a reprint
without ever being recomputed. Only part numbers on an enabled list receive markings, and the list is
maintained from a new settings page using the application's existing fuzzy-search picker.

No new frameworks or packages are introduced. The feature follows the module's established tool
pattern.

## Project Structure

```text
Module_ShipRec_Tools/
  Enums/
    Enum_LotColor.cs                      (new)
    Enum_LotMarkingSource.cs              (new)
    Enum_LotMarkingCollisionPolicy.cs     (new)
  Models/
    Model_LotMarkingPalette.cs            (new)
    Model_Tool_LotMarking.cs              (new)
    Model_Tool_LotMarkingScope.cs         (new)
  Helpers/
    Helper_LotNumberNormalizer.cs         (new)
    Helper_LotMarkingResolver.cs          (new)
  Data/
    Dao_Tool_LotMarking.cs                (new)
  Contracts/
    IService_Tool_LotMarking.cs           (new)
    IService_ShipRecToolsSettings.cs      (change: add accessors)
  Services/
    Service_Tool_LotMarking.cs            (new)
    Service_ShipRecTools_Navigation.cs    (change: register tool)
    Service_ShipRecToolsSettings.cs       (change: add accessors)
  Settings/
    ShipRecToolsSettingsKeys.cs           (change: add LotMarking keys)
  ViewModels/
    ViewModel_Tool_LotMarking.cs          (new)
    ViewModel_ShipRecTools_Main.cs        (change: visibility flag + branch)
  Views/
    View_Tool_LotMarking.xaml             (new)
    View_Tool_LotMarking.xaml.cs          (new)
    View_ShipRecTools_Main.xaml           (change: host control)
    View_ShipRecTools_Main.xaml.cs        (change: five-point wiring)

Module_Settings.Core/
  Defaults/settings.manifest.json         (change: ShipRecTools LotMarking entries)
  Views/View_Settings_LotMarking.xaml     (new)
  Views/View_Settings_LotMarking.xaml.cs  (new)
  ViewModels/ViewModel_Settings_LotMarking.cs (new)
  Views/View_Settings_CoreNavigationHub.xaml(.cs) (change: hub button + handler)
  Views/View_Settings_CoreWindow.xaml.cs  (change: page header switch)

Module_Core/Services/Help/Service_Help.cs (change: ShipRecTools.LotMarking article)
MainWindow.xaml.cs                        (change: settings destination entry)
Infrastructure/DependencyInjection/ModuleServicesExtensions.cs (change: four registrations)

Database/Database_Deployment/Sql_Files/
  Schemas/50_Table_lot_markings.sql        (new)
  Schemas/51_Table_lot_marking_part_scope.sql (new)
  Schemas/10_Table_receiving_history.sql   (change: two columns)
  Schemas/11_Table_receiving_label_data.sql (change: two columns)
  StoredProcedures/Settings/sp_LotMarkings_*.sql  (new family)
  StoredProcedures/Settings/sp_LotMarkingPartScope_*.sql (new family)
  StoredProcedures/Receiving/sp_Receiving_LabelData_*.sql (change)
  StoredProcedures/Receiving/sp_Receiving_Load_*.sql      (change)
  StoredProcedures/Receiving/sp_Receiving_History_*.sql   (change)
  Views/02_View_receiving_history.sql      (change: UNION gains four columns)
  Migrations/16_Migration_lot_markings.sql (new)

Database/SyncTool/sync_policy.yml          (change: allow-list the two new tables)

MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/
  Helpers/Helper_LotMarkingResolverTests.cs    (new)
  Services/Service_Tool_LotMarkingTests.cs     (new)
  ViewModels/ViewModel_Tool_LotMarkingTests.cs (new)
  ViewModels/ViewModel_ShipRecTools_MainTests.cs (change: new case)
MTM_Receiving_Application.Tests/Integration/Database/
  LotMarkingStoredProceduresIntegrationTests.cs (new)
```

**Structure Decision**: all new application code lives in the existing `Module_ShipRec_Tools` module,
which already owns every ship/rec tool and already contains `Enums`, `Models`, `Helpers`, `Data`,
`Contracts`, `Services`, `Settings`, `ViewModels` and `Views` folders. The settings page goes in
`Module_Settings.Core` because that is where every existing Ship/Rec Tools settings page lives, and
`Module_Settings.Core` already references `Module_ShipRec_Tools` types. No new project is created.

## Flow

The block generator (`plan-blocks.py`) is missing its part files in this installation, so these
blocks are written by hand in the same shape.

```text
calls
Receive one line
  ViewModel_Receiving_*  ->  Service_MySQL_Receiving  ->  Dao_ReceivingLabelData  ->  sp_Receiving_LabelData_Insert
Resolve a marking
  Service_Tool_LotMarking  ->  Helper_LotNumberNormalizer.Normalize
                           ->  Dao_Tool_LotMarking (ledger read)
                           ->  Helper_LotMarkingResolver.Resolve (seed, then probe)
                           ->  Dao_Tool_LotMarking (ledger write, only when absent)
Promote to history
  Dao_ReceivingLabelData  ->  sp_Receiving_LabelData_ClearToHistory   (copies stored codes, never re-resolves)
Reprint
  Dao_ReceivingLoad.GetReprintHistoryAsync  ->  sp_Receiving_History_GetForReprint
  Dao_ReceivingLabelData.InsertFromHistoryAsync  ->  sp_Receiving_LabelData_InsertFromHistory
```

```text
states
Unmarked: an enabled part whose line carries no heat number, or a part not on the enabled list. No codes written.
Resolved: the lot has a ledger row and its codes are stored on the receiving queue row.
Promoted: the codes arrive on the receiving history row unchanged.
Reprinted: the codes return to the queue unchanged, and are never re-resolved for a reprint row.
Locked: a ledger row a user has protected. The resolver returns it untouched, probes nothing.
```

```text
screens
screen lot-marking-tool    The tool page: heat search, marking list, override and lock, palette legend, part list.
screen lot-marking-settings The settings page: feature switch, collision behaviour, display options, part list.
screen receiving-line      An existing receipt line grid, with the shape and color shown per lot.
screen receiving-header    The existing per-step receipt header card, with the receipt's markings.
screen reprint-grid        The existing reprint grid, with the marking for each history row.
```

## Constitution Check

| Principle | Assessment |
|---|---|
| I. MVVM and View Purity | PASS. New view model is `partial`, inherits `ViewModel_Tool_Base`, uses `[ObservableProperty]` and `[RelayCommand]`. The view binds with `x:Bind` only. Code-behind holds only `XamlRoot`, the help call, and the fuzzy-picker delegate, exactly as the existing `View_Tool_OutsideServiceHistory` already does. |
| II. Data Access Integrity | PASS. All MySQL access is through stored procedures. The new DAO is instance-based, async, and returns `Model_Dao_Result` on every path. Infor Visual is read only. No static DAOs, no raw SQL. |
| III. CQRS + Mediator First | JUSTIFIED VIOLATION. The new tool exposes a multi-method service rather than MediatR commands and queries. See Complexity Tracking. |
| IV. Dependency Injection and Modular Boundaries | PASS. Four registrations are added to the existing `AddShipRecToolsModule` extension, reusing its `mySqlConnectionString` local. DAO and service are singleton, view model and view transient. All new logic stays inside its module. |
| V. Validation, Errors, and Structured Logging | PASS. Every service path returns a typed result, errors go through `IService_ErrorHandler`, and the resolver validates its inputs at the boundary rather than trusting callers. |
| VI. Security and Session Discipline | PASS. No secrets are added. The ledger records who changed a marking and when. The system-wide switch is the only `Admin`-level setting in the category, so a normal user cannot disable the feature for everyone. Infor Visual remains read only. |
| VII. Library-First Reuse | PASS. No new package. The part picker reuses `Dialog_FuzzySearchPicker`, the settings layer reuses `IService_SettingsCoreFacade`, and the tool reuses `ViewModel_Tool_Base`. |

Gate result: PASS with one justified violation, re-checked after Phase 1 design and unchanged.

## Complexity Tracking

| Violation | Why needed | Simpler alternative rejected |
|---|---|---|
| Principle III: the tool is a multi-method `IService_Tool_LotMarking` instead of MediatR commands and queries | Every existing Ship/Rec Tools tool works this way, and the settings layer it depends on is already a MediatR facade. Introducing handlers here would produce one-implementation commands and a second orchestration style inside a single module. | MediatR handlers for assign, override, lock, and scope CRUD. Rejected: it duplicates the module's established pattern, adds ceremony with no second consumer, and would not reduce coupling because the DAO call boundary is identical either way. |
