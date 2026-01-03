# Tasks: Dunnage Stored Procedures

**Input**: Design documents from `/specs/CompletedSpecs/005-dunnage-stored-procedures/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Integration tests included for each DAO as per plan.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Create directory structure for Stored Procedures in `Database/StoredProcedures/Dunnage/`
- [x] T002 Create directory structure for DAOs in `Data/Dunnage/`
- [x] T003 Create directory structure for Models in `Models/Dunnage/`

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Ensure database schema exists before creating procedures

- [x] T005 Verify or create dunnage tables schema in `Database/Schemas/04_create_dunnage_tables.sql` and deploy to database

## Phase 3: User Story 1 - Dunnage Types (P1)

**Goal**: Implement type management foundation
**Independent Test**: Can create, read, update, delete types and verify counts

- [x] T006 [US1] Create `Model_DunnageType.cs` in `Models/Dunnage/`
- [x] T007 [P] [US1] Create `sp_dunnage_types_get_all.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T008 [P] [US1] Create `sp_dunnage_types_get_by_id.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T009 [P] [US1] Create `sp_dunnage_types_insert.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T010 [P] [US1] Create `sp_dunnage_types_update.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T011 [P] [US1] Create `sp_dunnage_types_delete.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T012 [P] [US1] Create `sp_dunnage_types_count_parts.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T013 [P] [US1] Create `sp_dunnage_types_count_transactions.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T014 [US1] Deploy all Type stored procedures to database
- [x] T015 [US1] Create `Dao_DunnageType.cs` in `Data/Dunnage/` implementing all 7 methods

## Phase 4: User Story 2 - Dunnage Specs (P1)

**Goal**: Implement dynamic spec schema management
**Independent Test**: Can manage specs for types and verify JSON handling

- [x] T017 [US2] Create `Model_DunnageSpec.cs` in `Models/Dunnage/`
- [x] T018 [P] [US2] Create `sp_dunnage_specs_get_by_type.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T019 [P] [US2] Create `sp_dunnage_specs_get_all.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T020 [P] [US2] Create `sp_dunnage_specs_get_by_id.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T021 [P] [US2] Create `sp_dunnage_specs_insert.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T022 [P] [US2] Create `sp_dunnage_specs_update.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T023 [P] [US2] Create `sp_dunnage_specs_delete_by_id.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T024 [P] [US2] Create `sp_dunnage_specs_delete_by_type.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T025 [P] [US2] Create `sp_dunnage_specs_count_parts_using_spec.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T026 [US2] Deploy all Spec stored procedures to database
- [x] T027 [US2] Create `Dao_DunnageSpec.cs` in `Data/Dunnage/` implementing all 8 methods

## Phase 5: User Story 3 - Dunnage Parts (P1)

**Goal**: Implement master data management for parts
**Independent Test**: Can create parts with JSON spec values and search them

- [x] T029 [US3] Create `Model_DunnagePart.cs` in `Models/Dunnage/`
- [x] T030 [P] [US3] Create `sp_dunnage_parts_get_all.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T031 [P] [US3] Create `sp_dunnage_parts_get_by_type.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T032 [P] [US3] Create `sp_dunnage_parts_get_by_id.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T033 [P] [US3] Create `sp_dunnage_parts_insert.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T034 [P] [US3] Create `sp_dunnage_parts_update.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T035 [P] [US3] Create `sp_dunnage_parts_delete.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T036 [P] [US3] Create `sp_dunnage_parts_count_transactions.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T037 [P] [US3] Create `sp_dunnage_parts_search.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T038 [US3] Deploy all Part stored procedures to database
- [x] T039 [US3] Create `Dao_DunnagePart.cs` in `Data/Dunnage/` implementing all 8 methods

## Phase 6: User Story 4 - Dunnage Loads (P1)

**Goal**: Implement transaction recording
**Independent Test**: Can insert single and batch loads, and retrieve by date

- [x] T041 [US4] Create `Model_DunnageLoad.cs` in `Models/Dunnage/`
- [x] T042 [P] [US4] Create `sp_dunnage_loads_get_all.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T043 [P] [US4] Create `sp_dunnage_loads_get_by_date_range.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T044 [P] [US4] Create `sp_dunnage_loads_get_by_id.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T045 [P] [US4] Create `sp_dunnage_loads_insert.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T046 [P] [US4] Create `sp_dunnage_loads_insert_batch.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T047 [P] [US4] Create `sp_dunnage_loads_update.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T048 [P] [US4] Create `sp_dunnage_loads_delete.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T049 [US4] Deploy all Load stored procedures to database
- [x] T050 [US4] Create `Dao_DunnageLoad.cs` in `Data/Dunnage/` implementing all 7 methods

## Phase 7: User Story 5 - Inventoried Dunnage (P2)

**Goal**: Implement inventory notification management
**Independent Test**: Can manage inventory list and check status

- [x] T052 [US5] Create `Model_InventoriedDunnage.cs` in `Models/Dunnage/`
- [x] T053 [P] [US5] Create `sp_inventoried_dunnage_get_all.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T054 [P] [US5] Create `sp_inventoried_dunnage_check.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T055 [P] [US5] Create `sp_inventoried_dunnage_get_by_part.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T056 [P] [US5] Create `sp_inventoried_dunnage_insert.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T057 [P] [US5] Create `sp_inventoried_dunnage_update.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T058 [P] [US5] Create `sp_inventoried_dunnage_delete.sql` in `Database/StoredProcedures/Dunnage/`
- [x] T059 [US5] Deploy all Inventoried stored procedures to database
- [x] T060 [US5] Create `Dao_InventoriedDunnage.cs` in `Data/Dunnage/` implementing all 6 methods

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final integration and cleanup

- [x] T062 Update `Database/Deploy/Deploy-StoredProcedures.ps1` to include new dunnage procedures
- [x] T063 Run full integration test suite to ensure no regressions and verify all performance metrics (SC-002, SC-006)

## Dependencies

1. **US1 (Types)**: Foundation for all other stories
2. **US2 (Specs)**: Depends on US1 (TypeID)
3. **US3 (Parts)**: Depends on US1 (TypeID) and US2 (Schema validation)
4. **US4 (Loads)**: Depends on US3 (PartID)
5. **US5 (Inventoried)**: Depends on US3 (PartID)

## Parallel Execution Examples

- **Within US1**: T007-T013 (SP creation) can be done in parallel by different developers or agents.
- **Across Stories**: US2 and US3 can be started once US1 is stable, though US3 needs US2 for full validation logic.

## Implementation Strategy

1. **MVP**: Complete US1 (Types) and US3 (Parts) to allow basic part management.
2. **Full Feature**: Add US2 (Specs) and US4 (Loads) to enable full receiving workflow.
3. **Enhancement**: Add US5 (Inventoried) for ERP integration support.
