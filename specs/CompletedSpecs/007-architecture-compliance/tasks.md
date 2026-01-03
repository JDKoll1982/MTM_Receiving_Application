# Tasks: Architecture Compliance Refactoring

**Feature Branch**: `007-architecture-compliance`
**Spec**: [spec.md](spec.md)
**Plan**: [plan.md](plan.md)
**Status**: Completed

## Phase 1: Setup

- [x] T001 Create directory `Models/InforVisual`
- [x] T002 Create directory `Data/InforVisual`
- [x] T003 Create directory `Data/Receiving` (if not exists)
- [x] T004 Create directory `Contracts/Services` (if not exists)

## Phase 2: Foundational (Models & New DAOs)

**Goal**: Create all new Data Models and Instance-Based DAOs required for Service delegation, and register them in DI. This unblocks User Stories 1 and 2.

- [x] T005 [P] Create `Model_InforVisualPO` in `Models/InforVisual/Model_InforVisualPO.cs`
- [x] T006 [P] Create `Model_InforVisualPart` in `Models/InforVisual/Model_InforVisualPart.cs`
- [x] T007 [P] Create `Model_PackageTypePreference` in `Models/Receiving/Model_PackageTypePreference.cs`
- [x] T008 [P] Create `Dao_ReceivingLoad` (Instance-based) in `Data/Receiving/Dao_ReceivingLoad.cs`
- [x] T009 [P] Create `Dao_PackageTypePreference` (Instance-based) in `Data/Receiving/Dao_PackageTypePreference.cs`
- [x] T010 [P] Create `Dao_InforVisualPO` (Instance-based, Read-Only) in `Data/InforVisual/Dao_InforVisualPO.cs`
- [x] T011 [P] Create `Dao_InforVisualPart` (Instance-based, Read-Only) in `Data/InforVisual/Dao_InforVisualPart.cs`
- [x] T012 Register new DAOs (`Dao_ReceivingLoad`, `Dao_PackageTypePreference`, `Dao_InforVisualPO`, `Dao_InforVisualPart`) as Singletons in `App.xaml.cs`
- [x] T013 Register existing DAOs (`Dao_User`, `Dao_ReceivingLine`) as Singletons in `App.xaml.cs`

## Phase 3: User Story 1 - ViewModel Architecture Compliance (P1)

**Goal**: Refactor ViewModels to use Services instead of DAOs directly.

- [x] T014 [US1] Create `IService_UserPreferences` interface in `Contracts/Services/IService_UserPreferences.cs`
- [x] T015 [US1] Implement `Service_UserPreferences` in `Services/Database/Service_UserPreferences.cs`
- [x] T016 [US1] Create `IService_MySQL_ReceivingLine` interface in `Contracts/Services/IService_MySQL_ReceivingLine.cs`
- [x] T017 [US1] Implement `Service_MySQL_ReceivingLine` in `Services/Receiving/Service_MySQL_ReceivingLine.cs`
- [x] T018 [US1] Register `IService_UserPreferences` and `IService_MySQL_ReceivingLine` in `App.xaml.cs`
- [x] T019 [US1] Refactor `ReceivingModeSelectionViewModel` to inject `IService_UserPreferences` and remove `Dao_User` usage in `ViewModels/Receiving/ReceivingModeSelectionViewModel.cs`
- [x] T020 [US1] Refactor `ReceivingLabelViewModel` to inject `IService_MySQL_ReceivingLine` and remove `Dao_ReceivingLine` usage in `ViewModels/Receiving/ReceivingLabelViewModel.cs`

## Phase 4: User Story 2 - Service-to-DAO Delegation (P1)

**Goal**: Refactor Services to delegate database operations to DAOs.

- [x] T021 [US2] Refactor `Service_MySQL_Receiving` to inject and use `Dao_ReceivingLoad` in `Services/Receiving/Service_MySQL_Receiving.cs`
- [x] T022 [US2] Refactor `Service_MySQL_PackagePreferences` to inject and use `Dao_PackageTypePreference` in `Services/Database/Service_MySQL_PackagePreferences.cs`
- [x] T023 [US2] Refactor `Service_InforVisual` to inject and use `Dao_InforVisualPO` and `Dao_InforVisualPart` in `Services/Database/Service_InforVisual.cs`

## Phase 5: User Story 3 - Instance-Based DAO Pattern (P2)

**Goal**: Convert existing static DAOs (Dunnage domain) to instance-based pattern and update dependent services.

- [x] T024 [US3] Convert `Dao_DunnageLoad` to instance-based class in `Data/Dunnage/Dao_DunnageLoad.cs`
- [x] T025 [US3] Convert `Dao_DunnageType` to instance-based class in `Data/Dunnage/Dao_DunnageType.cs`
- [x] T026 [US3] Convert `Dao_DunnagePart` to instance-based class in `Data/Dunnage/Dao_DunnagePart.cs`
- [x] T027 [US3] Convert `Dao_DunnageSpec` to instance-based class in `Data/Dunnage/Dao_DunnageSpec.cs`
- [x] T028 [US3] Convert `Dao_InventoriedDunnage` to instance-based class in `Data/Dunnage/Dao_InventoriedDunnage.cs`
- [x] T029 [US3] Register all converted Dunnage DAOs as Singletons in `App.xaml.cs`
- [x] T030 [US3] Refactor `Service_MySQL_Dunnage` to inject and use the new instance-based Dunnage DAOs in `Services/Database/Service_MySQL_Dunnage.cs`


## Phase 7: User Story 5 - Architecture Documentation (P3)

**Goal**: Create comprehensive architecture documentation to prevent future violations.

- [x] T033 [US5] Create `architecture-refactoring-guide.instructions.md` in `.github/instructions/architecture-refactoring-guide.instructions.md`
- [x] T034 [US5] Create `service-dao-pattern.instructions.md` in `.github/instructions/service-dao-pattern.instructions.md`
- [x] T035 [US5] Create `dependency-analysis.instructions.md` in `.github/instructions/dependency-analysis.instructions.md`
- [x] T036 [US5] Create `dao-instance-pattern.instructions.md` in `.github/instructions/dao-instance-pattern.instructions.md`
- [x] T037 [US5] Update `mvvm-pattern.instructions.md` with ViewModel→Service→DAO diagram in `.github/instructions/mvvm-pattern.instructions.md`
- [x] T038 [US5] Update `dao-pattern.instructions.md` to deprecate static pattern in `.github/instructions/dao-pattern.instructions.md`

## Phase 8: Polish & Cross-Cutting Concerns

- [x] T039 Run `dotnet build` and verify 0 errors/warnings
- [x] T040 Run `dotnet test` and verify all tests pass
- [x] T041 Run dependency analysis (if tool available) to verify no ViewModel→DAO or Service→Database violations

## Dependencies

- US1 depends on Foundational (Models, DAOs)
- US2 depends on Foundational (Models, DAOs)
- US3 depends on Foundational (DI Registration pattern)
- US4 depends on Foundational (Infor Visual DAOs)

## Parallel Execution Examples

- **Models & DAOs**: T005, T006, T007, T008, T009, T010, T011 can be implemented in parallel.
- **Services**: T015 and T017 can be implemented in parallel once interfaces are defined.
- **DAO Conversion**: T024-T028 can be implemented in parallel.
- **Documentation**: T033-T036 can be implemented in parallel with code changes.

## Implementation Strategy

1. **Foundation First**: Establish the new Models and DAOs to support the Service layer.
2. **Critical Violations (P1)**: Fix ViewModel and Service violations immediately (US1, US2).
3. **Refactoring (P2)**: Convert remaining static DAOs (US3) and verify Infor Visual constraints (US4).
4. **Documentation (P3)**: Solidify the patterns with documentation to prevent regression.
