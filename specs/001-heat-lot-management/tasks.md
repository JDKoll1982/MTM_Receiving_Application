# Tasks: Heat / Lot Management

**Feature**: `specs/001-heat-lot-management/heat-lot-management.spec.md`
**Plan**: `specs/001-heat-lot-management/plan.md`
**Created**: 2026-10-14
**Size**: `oversized`

## Scale note

This list spans four subsystems and about sixty files: two new MySQL tables with two new
procedure families, eleven changed receiving procedures plus the history view, the module's tool
page and its wiring, a new settings page with three name-keyed registration lists, and the tests
for each. Watch two things: the migration must ship the receiving procedures it invalidates, and the
marking must be written once at save time and copied verbatim through promotion and reprint.

## Phase 1: Setup

**Wave 1 — independent (different files):**

- [x] **T001** [P] Verify a green baseline before changing anything: `dotnet build MTM_Receiving_Application.slnx` and the focused test project both succeed.
- [x] **T002** [P] Confirm the next migration number is free and that the internal procedure name does not collide with the legacy sequence, then record it in the new migration's header · `Database/Database_Deployment/Sql_Files/Migrations/16_Migration_lot_markings.sql`

**⟶ Wait for Wave 1 to finish, then:**

**Checkpoint**: the repository builds and the migration number is settled.

## Phase 2: Foundational

Blocks every story. No user-story work begins until this phase is done.

**Files**: `Module_ShipRec_Tools/Enums/*`, `Module_ShipRec_Tools/Models/Model_LotMarking*`,
`Module_ShipRec_Tools/Helpers/*`, `Module_ShipRec_Tools/Data/Dao_Tool_LotMarking.cs`,
`Module_ShipRec_Tools/Contracts/IService_Tool_LotMarking.cs`,
`Module_ShipRec_Tools/Services/Service_Tool_LotMarking.cs`,
`Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs`,
`Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml(.cs)`, and every file under
`Database/Database_Deployment/Sql_Files/` that this feature touches.

**Wave 1 — independent (different files):**

- [x] **T003** [P] Add the shape vocabulary. Superseded 2026-10-14: shapes are emoji and the glyph is the persisted value, so no shape enum exists; the vocabulary lives in the palette · `Module_ShipRec_Tools/Models/Model_LotMarkingPalette.cs`
- [x] **T004** [P] Add the 12-value color enum, codes exactly as pinned in the spec · `Module_ShipRec_Tools/Enums/Enum_LotColor.cs`
- [x] **T005** [P] Add the marking-source enum · `Module_ShipRec_Tools/Enums/Enum_LotMarkingSource.cs`
- [x] **T006** [P] Add the collision-policy enum with `Allow`, `Probe`, `Warn` · `Module_ShipRec_Tools/Enums/Enum_LotMarkingCollisionPolicy.cs`
- [x] **T007** [P] Add the palette catalog: code, display name, glyph key, hex token, sort order, and lookups by code · `Module_ShipRec_Tools/Models/Model_LotMarkingPalette.cs`
- [x] **T008** [P] Add the observable ledger row model · `Module_ShipRec_Tools/Models/Model_Tool_LotMarking.cs`
- [x] **T009** [P] Add the observable enabled-part row model · `Module_ShipRec_Tools/Models/Model_Tool_LotMarkingScope.cs`
- [x] **T010** [P] Add the normalizer: trim, collapse internal whitespace, uppercase, strip vendor separators, and treat null, empty, whitespace, and the application's blank-heat sentinel as blank · `Module_ShipRec_Tools/Helpers/Helper_LotNumberNormalizer.cs`

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T011** Resolve depends on the palette, the enums, and the normalizer; implement the seeded hash plus the bounded probe with a locked row short-circuit · `Module_ShipRec_Tools/Helpers/Helper_LotMarkingResolver.cs`
- [x] **T012** Add the ledger table, exact columns and keys, `utf8mb4_unicode_ci` collation · `Database/Database_Deployment/Sql_Files/Schemas/50_Table_lot_markings.sql`
- [x] **T013** Add the enabled-part table, exact columns and unique key, `utf8mb4_unicode_ci` collation · `Database/Database_Deployment/Sql_Files/Schemas/51_Table_lot_marking_part_scope.sql`
- [x] **T014** Add both marking columns to the history table definition · `Database/Database_Deployment/Sql_Files/Schemas/10_Table_receiving_history.sql`
- [x] **T015** Add both marking columns to the queue table definition · `Database/Database_Deployment/Sql_Files/Schemas/11_Table_receiving_label_data.sql`

**⟶ Wait for T012 and T013 to finish, then:**

- [x] **T016** [P] Add the ledger procedure family: get all, get by heat, resolve, insert, update marking, set locked · `Database/Database_Deployment/Sql_Files/StoredProcedures/Settings/sp_LotMarkings_*.sql`
- [x] **T017** [P] Add the enabled-part procedure family: get all, search, upsert, set enabled, delete · `Database/Database_Deployment/Sql_Files/StoredProcedures/Settings/sp_LotMarkingPartScope_*.sql`

**⟶ Wait for Wave 2 to finish, then:**

- [x] **T018** Update the eleven receiving procedures and the history view so the two columns are enumerated everywhere, including both sides of promotion and reprint and the four union columns · `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_*.sql`, `Database/Database_Deployment/Sql_Files/Views/02_View_receiving_history.sql`

**⟶ Wait for T018 to finish, then:**

- [x] **T019** Write the migration: add both columns to both tables with the idempotent helper pattern, then re-create the receiving procedures the new columns invalidate, and drop every helper at the end · `Database/Database_Deployment/Sql_Files/Migrations/16_Migration_lot_markings.sql`

**⟶ Wait for Wave 3 to finish, then:**

**Wave 4 — independent (different files):**

- [x] **T020** [P] Add the DAO: instance class, injected connection string, one method per procedure, `Model_Dao_Result` on every path, no raw SQL · `Module_ShipRec_Tools/Data/Dao_Tool_LotMarking.cs`
- [x] **T021** [P] Add the settings keys for the feature plus the accessors on the existing Ship/Rec Tools settings surface · `Module_ShipRec_Tools/Settings/ShipRecToolsSettingsKeys.cs`, `Module_ShipRec_Tools/Contracts/IService_ShipRecToolsSettings.cs`, `Module_ShipRec_Tools/Services/Service_ShipRecToolsSettings.cs`
- [x] **T022** [P] Add the five manifest definitions under the existing `ShipRecTools` category, with the system-wide switch at `Admin` and the rest at `User` · `Module_Settings.Core/Defaults/settings.manifest.json`

**⟶ Wait for T020 to finish, then:**

- [x] **T023** Add the service contract and implementation: normalize, resolve the supplier, resolve the marking, persist only when absent, and log through the error handler · `Module_ShipRec_Tools/Contracts/IService_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Services/Service_Tool_LotMarking.cs`
- [x] **T024** Add the tool view model: heat search, marking list, override, lock, part list, palette legend, and the fuzzy-picker delegate contract · `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs`

**⟶ Wait for T024 to finish, then:**

- [x] **T025** Add the tool view and its code-behind: `x:Bind` only, with the picker delegate and the help call in code-behind · `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml`, `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml.cs`
- [x] **T026** Add the settings page view and view model, reusing the settings core facade and the existing Ship/Rec Tools settings pattern · `Module_Settings.Core/Views/View_Settings_LotMarking.xaml(.cs)`, `Module_Settings.Core/ViewModels/ViewModel_Settings_LotMarking.cs`

**⟶ Wait for Wave 4 to finish, then:**

**Wave 5 — independent (different files):**

- [x] **T027** [P] Register the tool with its key, title, category and icon · `Module_ShipRec_Tools/Services/Service_ShipRecTools_Navigation.cs`
- [x] **T028** [P] Add the tool's visibility flag, its navigation case and its hide branch · `Module_ShipRec_Tools/ViewModels/ViewModel_ShipRecTools_Main.cs`
- [x] **T029** [P] Add the tool's host control to the main tool view · `Module_ShipRec_Tools/Views/View_ShipRecTools_Main.xaml`
- [x] **T030** [P] Complete the five-point wiring in the main tool code-behind · `Module_ShipRec_Tools/Views/View_ShipRecTools_Main.xaml.cs`
- [x] **T031** [P] Register the DAO, service, view model and view inside the Ship/Rec Tools module block, reusing its existing connection-string local · `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
- [x] **T032** [P] Add the help article for the tool · `Module_Core/Services/Help/Service_Help.cs`

**⟶ Wait for Wave 5 to finish, then:**

- [x] **T033** Complete the three name-keyed settings registrations so the page is reachable, titled, and findable from settings search · `Module_Settings.Core/Views/View_Settings_CoreNavigationHub.xaml(.cs)`, `Module_Settings.Core/Views/View_Settings_CoreWindow.xaml.cs`, `MainWindow.xaml.cs`

**Checkpoint**: the tool opens, resolves a marking, and the settings page opens and saves.

## Phase 3: User Story 1 — A lot gets its marking automatically (P1)

**Files**: `Module_Receiving/Models/Model_ReceivingLoad.cs`, `Module_Receiving/Data/Dao_ReceivingLabelData.cs`, `Module_Receiving/Data/Dao_ReceivingLoad.cs`, `Module_Receiving/Services/Service_MySQL_Receiving.cs`, `Module_Receiving/Services/Service_ReceivingWorkflow.cs`

### Tests

- [x] **T034** [US1] Write failing tests first: a marking is resolved once and reused for the same heat number and supplier, and a second supplier gets a second marking · `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/Service_Tool_LotMarkingTests.cs`

### Implementation

**Wave 1 — independent (different files):**

- [x] **T035** [P] [US1] Add the two marking properties to the shared receiving row model and mark any display-only addition so it is not session-persisted · `Module_Receiving/Models/Model_ReceivingLoad.cs`
- [x] **T036** [P] [US1] Add the two marking parameters to the queue DAO's save and update calls and read them back in the row mapper · `Module_Receiving/Data/Dao_ReceivingLabelData.cs`

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T037** [US1] Resolve and attach the marking on the receiving save path before the row is written, and skip resolution entirely when the heat number is blank or the part is not enabled · `Module_Receiving/Services/Service_MySQL_Receiving.cs`, `Module_Receiving/Services/Service_ReceivingWorkflow.cs`

**⟶ Wait for T034 and T037 to finish, then:**

- [x] **T038** [US1] Add the resolver unit tests: determinism, normalization collapse, blank and sentinel handling, supplier separation, the sentinel group, probe behaviour, and a locked row never moving · `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Helpers/Helper_LotMarkingResolverTests.cs`

**Checkpoint**: Story 1 is independently functional. A received line carries a marking, and the same lot always resolves to the same one.

## Phase 4: User Story 2 — Choosing which parts get markings (P1)

**Files**: `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml(.cs)`, `Module_ShipRec_Tools/Services/Service_Tool_LotMarking.cs`

### Tests

- [x] **T039** [US2] Add the part-list tests: an empty list marks nothing, an added part marks, a removed part keeps existing markings · `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarkingTests.cs`

### Implementation

**Wave 1:**

- [x] **T040** [US2] Add the enabled-part operations to the service: list, search, add, deactivate, reactivate · `Module_ShipRec_Tools/Services/Service_Tool_LotMarking.cs`

**⟶ Wait for T040 to finish, then:**

- [x] **T041** [US2] Add the part-list region to the view model, including the picker delegate call to turn a search term into a confirmed part number · `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs`

**⟶ Wait for T041 to finish, then:**

- [x] **T042** [US2] Add the part-list region to the view: search box, add button, list with a per-row removal, bound with `x:Bind` · `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml`, `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml.cs`

**Checkpoint**: Story 2 is independently functional. Parts can be added and removed and the marking gate follows the list.

## Phase 5: User Story 3 — Seeing the marking where the work happens (P2)

**Files**: `Module_Receiving/ViewModels/ViewModel_Receiving_EditMode.cs`, `Module_Receiving/Views/View_Receiving_EditMode.xaml`, `Module_Receiving/ViewModels/ViewModel_Receiving_Review.cs`, `Module_Receiving/Views/View_Receiving_Review.xaml`, `Module_Receiving/Views/View_Receiving_HeatLot.xaml`, `Module_Receiving/Views/View_Receiving_LoadEntry.xaml`, `Module_Receiving/Views/View_Receiving_WeightQuantity.xaml`, `Module_Receiving/Views/View_Receiving_PackageType.xaml`

### Implementation

**Wave 1 — independent (different files):**

- [x] **T043** [P] [US3] Show the shape and color per row in the edit-mode grid, alongside the existing heat/lot column · `Module_Receiving/Views/View_Receiving_EditMode.xaml`
- [x] **T044** [P] [US3] Show the shape and color on the review table and the single-entry card · `Module_Receiving/Views/View_Receiving_Review.xaml`
- [x] **T045** [P] [US3] Add the receipt-level marking display to the shared header card shown by each guided step · `Module_Receiving/Views/View_Receiving_HeatLot.xaml`, `Module_Receiving/Views/View_Receiving_LoadEntry.xaml`, `Module_Receiving/Views/View_Receiving_WeightQuantity.xaml`, `Module_Receiving/Views/View_Receiving_PackageType.xaml`

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T046** [US3] Expose the receipt's distinct lot markings from the edit-mode and review view models · `Module_Receiving/ViewModels/ViewModel_Receiving_EditMode.cs`, `Module_Receiving/ViewModels/ViewModel_Receiving_Review.cs`

**Checkpoint**: Story 3 is independently functional. A handler sees each lot's marking on the line and at the receipt level.

## Phase 6: User Story 4 — The marking stays put through history and reprint (P2)

**Files**: `Module_Receiving/Data/Dao_ReceivingLoad.cs`, `Module_Core/Models/Reprint/Model_ReprintHistoryRow.cs`, `Module_Reprint/Services/Service_Reprint_Receiving.cs`, `Module_Reprint/Views/*`

### Tests

- [x] **T047** [US4] Add integration tests that insert a marked queue row, promote it, assert the codes arrive on history unchanged, then re-queue it and assert the codes come back · `MTM_Receiving_Application.Tests/Integration/Database/LotMarkingStoredProceduresIntegrationTests.cs`

### Implementation

**Wave 1 — independent (different files):**

- [x] **T048** [P] [US4] Read the two marking columns in the history-side mapper and pass them on the history update path · `Module_Receiving/Data/Dao_ReceivingLoad.cs`
- [x] **T049** [P] [US4] Add the marking fields to the reprinted-row model · `Module_Core/Models/Reprint/Model_ReprintHistoryRow.cs`

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T050** [US4] Populate the marking on the reprint row and show it in the reprint grid so a reprinted receipt shows the original marking · `Module_Reprint/Services/Service_Reprint_Receiving.cs`, `Module_Reprint/Views/*`

**Checkpoint**: Story 4 is independently functional. Promotion and reprint never change or drop a marking.

## Phase 7: User Story 5 — Looking up, correcting, and protecting a marking (P3)

**Files**: `Module_ShipRec_Tools/Services/Service_Tool_LotMarking.cs`, `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml`

### Tests

- [x] **T051** [US5] Add the override and lock tests: an override records the acting user and the reason, a locked row is returned untouched, and the palette legend lists every shape and color · `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/Service_Tool_LotMarkingTests.cs`

### Implementation

**Wave 1:**

- [x] **T052** [US5] Add the override and lock operations to the service, requiring a reason and recording the acting user · `Module_ShipRec_Tools/Services/Service_Tool_LotMarking.cs`

**⟶ Wait for T052 to finish, then:**

- [x] **T053** [US5] Add the search, override and lock region to the view model, plus the palette legend collection · `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs`

**⟶ Wait for T053 to finish, then:**

- [x] **T054** [US5] Add the search, override, lock and legend region to the view · `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml`

**Checkpoint**: Story 5 is independently functional. A lot can be found, corrected, and protected.

## Phase 8: Polish

**Wave 1 — independent (different files):**

- [x] **T055** [P] Allow-list the two new tables in the sync policy with their direction and a written reason · `Database/SyncTool/sync_policy.yml`
- [x] **T056** [P] Add the new procedures to the hand-maintained procedure test inventory · `Database/Database_Deployment/StoredProcedure-Test-Checklist.md`
- [x] **T057** [P] Add the new case and its expectation to the main tool view model tests · `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_ShipRecTools_MainTests.cs`
- [x] **T058** [P] Update the module feature document to match the shipped state · `Module_ShipRec_Tools/docs/HeatLotManagement_FeatureUpdate.md`

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T059** Regenerate the database object catalog and the generated database reference pages, then resolve any undeclared-parameter or missing-table error they report · `Database/Database_Deployment/sql-object-catalog.json`, `docs/database/**`
- [x] **T060** Validate against the Success Criteria: run the build and the focused test suites, and confirm each measurable outcome in the spec holds.

## Dependencies & Execution Order

Setup → Foundational → Story 1 → Story 2 → Story 3 → Story 4 → Story 5 → Polish. Story 1 and Story 2 are both priority one and both depend only on Foundational; Story 2 may run alongside Story 1. Stories 3, 4 and 5 each depend on the marking existing on a row, which Story 1 delivers.

Waves within a phase are the dependency order: Foundational wave 5 is the only one that must wait for
all four earlier waves, and every story phase follows the same pattern of independent files first, then
the file that joins them.

Ownership is disjoint: the tool page and its wiring belong to Foundational, the receiving model and
queue DAO to Story 1, the part-list surface to Story 2, the receiving display files to Story 3, the
reprint path to Story 4, and the override surface to Story 5. No path appears under two phases.
