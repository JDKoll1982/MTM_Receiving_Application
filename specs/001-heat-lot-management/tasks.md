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

## Phase 9: User Story 7 — A wrong lot number is put right on the lots it already marked (P2)

**Files**: `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_HeatLotKeys_GetAll.sql`, `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_LotMarking_RealignRows.sql`, `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_LotMarking_ClearRows.sql`, `Database/Database_Deployment/Sql_Files/StoredProcedures/Settings/sp_LotMarkings_Delete.sql`, `Module_ShipRec_Tools/Models/Model_Tool_LotMarkingRealign.cs`, `Module_ShipRec_Tools/Contracts/IService_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Data/Dao_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Services/Service_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Dialogs/Dialog_LotMarkingRealignConfirm.xaml`, `Module_ShipRec_Tools/Dialogs/Dialog_LotMarkingRealignConfirm.xaml.cs`, `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml`, `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml.cs`

### Tests

- [x] **T061** [US7] Add the realign tests to the lot marking service tests: a corrected number that already exists takes that lot's marking, a number that does not exist becomes a new lot with a free shape and color, a lot whose number appears on no row is removed, a protected lot is left alone and reported, a blank number clears the row, and a second run reports nothing · `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/Service_Tool_LotMarkingTests.cs`

### Implementation

**Wave 1 — independent (different files):**

- [x] **T062** [P] [US7] Add the read procedure returning every distinct heat and supplier pair across the queue and history with its marking and row count, so the application can decide which rows disagree and which lots are orphaned. Normalization stays in the application, so the heat is returned exactly as captured · `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_HeatLotKeys_GetAll.sql`
- [x] **T063** [P] [US7] Add the write procedure that puts one shape and color onto every queue and history row matching one raw heat and supplier · `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_LotMarking_RealignRows.sql`
- [x] **T064** [P] [US7] Add the write procedure that clears the marking from every row matching one raw heat and supplier, for a number that was corrected to nothing · `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_LotMarking_ClearRows.sql`
- [x] **T065** [P] [US7] Add the procedure that removes one lot by id, so its shape and color are free for a later lot · `Database/Database_Deployment/Sql_Files/StoredProcedures/Settings/sp_LotMarkings_Delete.sql`
- [x] **T066** [P] [US7] Add the plan and preview models: one entry per row group with what will happen to it, and the counts the report shows · `Module_ShipRec_Tools/Models/Model_Tool_LotMarkingRealign.cs`
- [x] **T067** [P] [US7] Add the preview and apply operations to the lot marking service contract · `Module_ShipRec_Tools/Contracts/IService_Tool_LotMarking.cs`

**⟶ Wait for the whole of Wave 1 to finish, then:**

- [x] **T068** [US7] Add the data access methods for the three new procedures, reusing the existing insert and get-all for the lot record itself · `Module_ShipRec_Tools/Data/Dao_Tool_LotMarking.cs`

**⟶ Wait for T068 to finish, then:**

- [x] **T069** [US7] Add the service: build the preview from the ledger and the row groups, resolve a free shape and color for a number that does not exist yet, refuse to touch a protected lot, and apply the plan recording who ran it · `Module_ShipRec_Tools/Services/Service_Tool_LotMarking.cs`

**⟶ Wait for T069 to finish, then:**

- [x] **T070** [US7] Add the confirmation dialog listing the lots to be created, the lots to be removed, and the rows to be changed · `Module_ShipRec_Tools/Dialogs/Dialog_LotMarkingRealignConfirm.xaml`, `Module_ShipRec_Tools/Dialogs/Dialog_LotMarkingRealignConfirm.xaml.cs`

**⟶ Wait for T070 to finish, then:**

- [x] **T071** [US7] Add the preview and apply region to the view model, behind a confirm delegate, with the run wired to reload the list afterwards · `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs`

**⟶ Wait for T071 to finish, then:**

- [x] **T072** [US7] Add the Update All Lots button to the tool's top card and wire the dialog · `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml`, `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml.cs`

**Checkpoint**: Story 7 is independently functional. A corrected lot number can be put right across the queue and history, and lots that no longer exist are cleared out of the list.

## Phase 10: Polish

**Wave 1 — independent (different files):**

- [x] **T073** [P] Add the four new procedures to the hand-maintained procedure test inventory · `Database/Database_Deployment/StoredProcedure-Test-Checklist.md`
- [x] **T074** [P] Update the module feature document with the Update All Lots action and the superseded keep-forever rule · `Module_ShipRec_Tools/docs/HeatLotManagement_FeatureUpdate.md`

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T075** Regenerate the database object catalog and resolve anything it reports · `Database/Database_Deployment/sql-object-catalog.json`, `docs/database/**`
- [x] **T076** Validate against the Success Criteria: run the build and the focused test suites, and confirm each measurable outcome for Story 7 holds.

## Phase 12: User Story 8 — The lot list shows everything it holds (P2)

**Files**: `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_HeatLotKeys_GetAll.sql`, `Module_ShipRec_Tools/Models/Model_Tool_LotMarkingRealign.cs`, `Module_ShipRec_Tools/Models/Model_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Contracts/IService_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Data/Dao_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Services/Service_Tool_LotMarking.cs`, `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml`, `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/Service_Tool_LotMarkingTests.cs`

### Tests

- [ ] **T081** [US8] Add tests for the lot list read: a lot with receiving lines reports the right count and the right most recent date, a lot with none reports zero and no date, and reading the list changes no marking · `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/Service_Tool_LotMarkingTests.cs`

### Implementation

**Wave 1 — independent (different files):**

- [ ] **T082** [P] [US8] Extend the receiving group read to report the date of each group's most recent line, so the lot list can be built from the read the tool already needs, and leave the ledger's own read untouched because the resolver's probe uses it on every new lot · `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_HeatLotKeys_GetAll.sql`
- [ ] **T083** [P] [US8] Add the two display fields to the marking model and the ledger read to the service contract · `Module_ShipRec_Tools/Models/Model_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Contracts/IService_Tool_LotMarking.cs`

**⟶ Wait for the whole of Wave 1 to finish, then:**

- [ ] **T084** [US8] Add the data access method and the service operation for the ledger read, leaving the existing get-all alone because the resolver's probe reads its result on every new lot · `Module_ShipRec_Tools/Data/Dao_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Services/Service_Tool_LotMarking.cs`

**⟶ Wait for T084 to finish, then:**

- [ ] **T085** [US8] Point the list at the ledger read so each row carries its count and its date · `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs`

**⟶ Wait for T085 to finish, then:**

- [ ] **T086** [US8] Rebuild the lot list so every column stays reachable at any window width, and add the count and date columns · `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml`

**Checkpoint**: Story 8 is independently functional. Every column of the lot list can be read, and each lot says how many lines it covers and when it was last seen.

## Phase 13: User Story 9 — A reprinted label carries the marking and comes back complete (P2)

**Files**: `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_LabelData_InsertFromHistory.sql`, `Module_Receiving/Contracts/IService_Reprint_Receiving.cs`, `Module_Receiving/Services/Service_Reprint_Receiving.cs`, `MTM_Receiving_Application.Tests/Unit/Module_Reprint/Services/Service_ReprintTests.cs`

### Tests

- [ ] **T087** [US9] Add tests for reprint fidelity: the queued row is given the lot's recorded marking when the history row carries none, an existing marking is not replaced, a row with no lot number stays unmarked, and queuing twice is refused · `MTM_Receiving_Application.Tests/Unit/Module_Reprint/Services/Service_ReprintTests.cs`

### Implementation

**Wave 1 — independent (different files):**

- [ ] **T088** [P] [US9] Fix the reprint insert so it carries every column of the history row, stops filling the weight from the quantity, stops discarding the part type, and carries the skid sequence and skid total · `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_LabelData_InsertFromHistory.sql`
- [ ] **T089** [P] [US9] Add the marking push to the reprint service: after a row is queued, resolve the marking for its lot number and supplier and write it onto the queued row, leaving the history row untouched · `Module_Receiving/Contracts/IService_Reprint_Receiving.cs`, `Module_Receiving/Services/Service_Reprint_Receiving.cs`

**Checkpoint**: Story 9 is independently functional. A reprinted row matches its history row value for value and carries the lot's marking.

## Phase 14: Polish

**Wave 1 — independent (different files):**

- [ ] **T090** [P] Check the hand-maintained procedure test inventory still counts every procedure, since this work extends one rather than adding one · `Database/Database_Deployment/StoredProcedure-Test-Checklist.md`
- [ ] **T091** [P] Update the module feature document with the two new columns, the reprint fidelity fix and the reprint marking push · `Module_ShipRec_Tools/docs/HeatLotManagement_FeatureUpdate.md`

**⟶ Wait for Wave 1 to finish, then:**

- [ ] **T092** Regenerate the database object catalog and resolve anything it reports · `Database/Database_Deployment/sql-object-catalog.json`, `docs/database/**`
- [ ] **T093** Validate: run the build and the focused tests, then load the changed procedures against the test database to confirm they create cleanly, and confirm each measurable outcome for Stories 8 and 9 holds.

## Dependencies & Execution Order

Setup → Foundational → Story 1 → Story 2 → Story 3 → Story 4 → Story 5 → Story 6 → Story 7 → Story 8 → Story 9 → Polish. Story 1 and Story 2 are both priority one and both depend only on Foundational; Story 2 may run alongside Story 1. Stories 3, 4 and 5 each depend on the marking existing on a row, which Story 1 delivers. Stories 6 and 7 both repair markings across the queue and history rather than creating them, so both depend on Story 4 having taught the receiving rows to carry a marking through history, and neither depends on the other. Story 8 reads the ledger and the receiving rows together and touches no marking, so it depends only on Story 1. Story 9 changes the reprint copy and needs the resolver, so it depends on Story 1 for the resolver and on Story 4 for the marking columns reaching history in the first place.

Waves within a phase are the dependency order: Foundational wave 5 is the only one that must wait for
all four earlier waves, and every story phase follows the same pattern of independent files first, then
the file that joins them.

Ownership is disjoint: the tool page and its wiring belong to Foundational, the receiving model and
queue DAO to Story 1, the part-list surface to Story 2, the receiving display files to Story 3, the
reprint path to Story 4, and the override surface to Story 5. No path appears under two phases.

## Phase 11: User Story 6 — back-recorded

User Story 6 was implemented and shipped on 2026-10-14 under the specification that added it, before
this file was extended, so it had requirements but no entries here. This phase records it after the
fact. Every entry is complete.

**Files**: `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_UnmarkedLotMarkings_Preview.sql`, `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_UnmarkedLotMarkings_Apply.sql`, `Module_ShipRec_Tools/Models/Model_Tool_LotMarkingBackfill.cs`, `Module_ShipRec_Tools/Dialogs/Dialog_LotMarkingBackfillConfirm.xaml`, `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml`

- [x] **T077** [US6] Add the preview procedure listing the unmarked rows of one part grouped by the heat as captured and the supplier, read only · `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_UnmarkedLotMarkings_Preview.sql`
- [x] **T078** [US6] Add the apply procedure filling one lot across the queue and history in one statement, refusing a part that is not enabled, a blank heat, or a missing shape or color · `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_UnmarkedLotMarkings_Apply.sql`
- [x] **T079** [US6] Add the data access methods, the models, and the service operations, resolving each lot once and writing every spelling of it · `Module_ShipRec_Tools/Data/Dao_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Services/Service_Tool_LotMarking.cs`
- [x] **T080** [US6] Add the fill-in command, its confirmation dialog, its button, and the eight service tests covering one lot written once, a blank heat skipped, and a second run writing nothing · `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/Service_Tool_LotMarkingTests.cs`

**Checkpoint**: Story 6 is independently functional. An enabled part's unmarked lots can be filled in on demand.

## Phase 15: User Story 10 — Only the parts on the list are marked, and the list shows one material's lots (P1)

The gate this story restores was already specified by FR-009 and by Story 2's first acceptance
scenario, and never implemented. So this phase makes existing requirements true rather than adding a
capability, and it keeps the numbering it was given when it was appended.

**Files**: `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_HeatLotKeys_GetAll.sql`, `Module_ShipRec_Tools/Models/Model_Tool_LotMarkingRealign.cs`, `Module_ShipRec_Tools/Models/Model_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Data/Dao_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Contracts/IService_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Services/Service_Tool_LotMarking.cs`, `Module_Receiving/Services/Service_MySQL_Receiving.cs`, `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml`, `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/Service_Tool_LotMarkingTests.cs`, `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarkingTests.cs`

`sp_Receiving_HeatLotKeys_GetAll.sql` is also owned by T082 in Phase 12, which is the same single
`SELECT` and is already applied. Phase 15 extends that statement rather than replacing it, so T095
must not run before T082 has landed.

### Tests

- [x] **T094** [US10] Add tests for the part gate and the list scope: a part that is not on the list creates no marking, a part that is creates one, an already-recorded marking is returned whatever the part, a lot an enabled part carries is listed, a lot carried only by other parts is not, choosing a part narrows the list, a lot with no receiving rows stays listed, an unreadable enabled list creates nothing, and the parts a lot is attributed to are read from its receiving rows rather than from the marking · `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/Service_Tool_LotMarkingTests.cs`

### Implementation

**Wave 1 — independent (different files):**

- [x] **T095** [P] [US10] Carry the part number of each group out of the receiving group read, grouping by it so one lot can be attributed to every part that carries it, and leaving the two counts and the date summed across those parts so no line is counted twice · `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_HeatLotKeys_GetAll.sql`
- [x] **T096** [P] [US10] Add the part number to the receiving group record · `Module_ShipRec_Tools/Models/Model_Tool_LotMarkingRealign.cs`
- [x] **T097** [P] [US10] Add the parts field to the marking model, rendered as a dash when the lot is attributed to none, following the count and date fields beside it · `Module_ShipRec_Tools/Models/Model_Tool_LotMarking.cs`

**⟶ Wait for the whole of Wave 1 to finish, then:**

- [x] **T098** [US10] Map the new column in the data access layer, and put the part number on the resolve and ledger operations in the contract · `Module_ShipRec_Tools/Data/Dao_Tool_LotMarking.cs`, `Module_ShipRec_Tools/Contracts/IService_Tool_LotMarking.cs`

**⟶ Wait for T098 to finish, then:**

- [x] **T099** [US10] Enforce the enabled list when a marking would be created and not when one is read, so a recorded marking is still returned for material no longer on the list, and scope the ledger to the lots that at least one enabled part carries while still showing a lot that no receiving row carries because it cannot be attributed to a part · `Module_ShipRec_Tools/Services/Service_Tool_LotMarking.cs`

**⟶ Wait for T099 to finish, then:**

**Wave 2 — independent (different files):**

- [x] **T100** [P] [US10] Pass the load's part number into the resolver at the receiving call site, which is the value the gate was missing · `Module_Receiving/Services/Service_MySQL_Receiving.cs`
- [x] **T101** [P] [US10] Read the marking list again when a part is chosen so the list follows the selection, pass the chosen part into every ledger read including the heat search, and word an empty search so it says the list covers only the parts on the enabled list · `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs`

**⟶ Wait for Wave 2 to finish, then:**

- [x] **T102** [US10] Add the parts column to the lot list, between the supplier and the line count · `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml`
- [x] **T103** [US10] Update the view model tests for the ledger operation's new part argument · `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarkingTests.cs`

**Checkpoint**: Story 10 is independently functional. No marking is created for a part that is not on the enabled list, and the lot list shows only the lots of material that is, narrowing to one part when one is chosen.
