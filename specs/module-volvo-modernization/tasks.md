---
description: "Module_Volvo Modernization - Strict Constitutional Compliance Tasks"
status: "ready-for-execution"
created: "2026-01-16"
template: "_bmad/module-agents/templates/tasktemplates/module-modernization-tasks.md"
---

# Tasks: Module_Volvo - Strict Constitutional Compliance Modernization

**Authority:** Constitution.md (7 Core Principles)  
**Target:** ZERO Constitutional Deviations  
**Violations Found:** 18 total  
**Phases:** 9 sequential phases with parallel opportunities  
**Total Tasks:** 80  
**Estimated Effort:** 34-44 hours (~1 week)

---

## Quick Stats

| Metric | Value |
|--------|-------|
| **CRITICAL Violations** | 8 (Principle I) |
| **HIGH Violations** | 6 (Principle III) |
| **MEDIUM Violations** | 4 (Principle V) |
| **Parallelizable Tasks** | 24 |
| **Sequential Tasks** | 56 |
| **Target Test Coverage** | ≥ 80% |
| **Target Compliance** | 100% (ZERO deviations) |

---

## Phase 1: Foundation & Structure

**Duration:** 1 day | **Effort:** 2-3 hours | **Blocking:** YES

- [ ] T001 Create folder structure (Handlers/Commands, Handlers/Queries, Validators, Models/Requests)
- [ ] T002 Create handler base files (README.md templates)
- [ ] T003 Verify MediatR configuration in App.xaml.cs
- [ ] T004 Verify FluentValidation configuration in App.xaml.cs
- [ ] T005 Verify project builds with zero errors

**Checkpoint:** Foundation ready ✓

---

## Phase 2: Principle I - MVVM & View Purity (8 Critical/High Violations)

**Duration:** 2-3 days | **Effort:** 10-12 hours | **Dependencies:** Phase 1 ✓

### Create ViewModels to Replace Code-Behind Logic

- [ ] T006 Write tests for shipment line add/remove operations
- [ ] T007 Create ViewModel_VolvoShipmentLine_Edit.cs
- [ ] T008 Refactor VolvoShipmentEditDialog.xaml.cs: Remove Click handlers (lines 21-22)
- [ ] T009 Refactor VolvoShipmentEditDialog.xaml.cs: Remove LoadShipment method (lines 28-43)
- [ ] T010 Refactor VolvoShipmentEditDialog.xaml.cs: Remove GetUpdatedShipment method (lines 50-54)
- [ ] T011 Update VolvoShipmentEditDialog.xaml: Bind buttons to RelayCommand

### Create Part Edit ViewModel

- [ ] T012 Write tests for part add/edit operations
- [ ] T013 Create ViewModel_VolvoPart_AddEdit.cs
- [ ] T014 Refactor VolvoPartAddEditDialog.xaml.cs: Remove InitializeForAdd method (lines 23-31)
- [ ] T015 Refactor VolvoPartAddEditDialog.xaml.cs: Remove InitializeForEdit method (lines 33-48)
- [ ] T016 Refactor VolvoPartAddEditDialog.xaml.cs: Remove OnSaveClicked method (lines 50-59)
- [ ] T017 Update VolvoPartAddEditDialog.xaml: Bind dialog to ViewModel

### Remove Async Void Anti-Pattern

- [ ] T018 Remove OnPageLoaded async void from View_Volvo_Settings.xaml.cs (lines 19-21)
- [ ] T019 Remove OnPageLoaded async void from View_Volvo_History.xaml.cs
- [ ] T020 Remove OnLoaded async void from View_Volvo_ShipmentEntry.xaml.cs

**Checkpoint:** All code-behind business logic removed ✓

---

## Phase 3: Principle III - CQRS & MediatR (6 High Violations)

**Duration:** 2 days | **Effort:** 16-18 hours | **Dependencies:** Phase 2 ✓

### Create Request Models (Commands & Queries)

- [ ] T021 Create AddShipmentLineCommand.cs
- [ ] T022 Create RemoveShipmentLineCommand.cs
- [ ] T023 Create UpdateShipmentCommand.cs
- [ ] T024 Create GetVolvoPartsQuery.cs
- [ ] T025 Create GetVolvoShipmentsQuery.cs
- [ ] T026 Create GetShipmentHistoryQuery.cs

### Create FluentValidation Validators

- [ ] T027 Write validator tests for all commands
- [ ] T028 Create AddShipmentLineCommandValidator.cs
- [ ] T029 Create RemoveShipmentLineCommandValidator.cs
- [ ] T030 Create UpdateShipmentCommandValidator.cs
- [ ] T031 Create AddVolvoPartCommandValidator.cs
- [ ] T032 Create UpdateVolvoPartCommandValidator.cs

### Implement Command Handlers

- [ ] T033 Write handler tests for all commands
- [ ] T034 Create AddShipmentLineCommandHandler.cs
- [ ] T035 Create RemoveShipmentLineCommandHandler.cs
- [ ] T036 Create UpdateShipmentCommandHandler.cs
- [ ] T037 Create AddVolvoPartCommandHandler.cs
- [ ] T038 Create UpdateVolvoPartCommandHandler.cs

### Implement Query Handlers

- [ ] T039 Write tests for all query handlers
- [ ] T040 Create GetVolvoPartsQueryHandler.cs
- [ ] T041 Create GetVolvoShipmentsQueryHandler.cs
- [ ] T042 Create GetShipmentHistoryQueryHandler.cs

**Checkpoint:** All handlers and validators implemented ✓

---

## Phase 4: ViewModel Modernization

**Duration:** 1 day | **Effort:** 3-4 hours | **Dependencies:** Phase 3 ✓

### Update ViewModels to Use IMediator

- [ ] T043 Update ViewModel_Volvo_ShipmentEntry: Inject IMediator
- [ ] T044 Update ViewModel_Volvo_ShipmentEntry: Replace service calls with mediator.Send()
- [ ] T045 Update ViewModel_Volvo_Settings: Inject IMediator
- [ ] T046 Update ViewModel_Volvo_Settings: Replace service calls with mediator.Send()
- [ ] T047 Update ViewModel_Volvo_History: Inject IMediator
- [ ] T048 Update ViewModel_Volvo_History: Replace service calls with mediator.Send()

**Checkpoint:** All ViewModels use IMediator exclusively ✓

---

## Phase 5: Dependency Injection Registration

**Duration:** 30 minutes | **Effort:** 1 hour | **Dependencies:** Phase 4 ✓

### Update App.xaml.cs

- [ ] T049 Register ViewModel_VolvoShipmentLine_Edit as Transient
- [ ] T050 Register ViewModel_VolvoPart_AddEdit as Transient
- [ ] T051 Verify all DAOs registered as Singleton
- [ ] T052 Verify MediatR handler auto-registration working
- [ ] T053 Verify FluentValidation auto-discovery working
- [ ] T054 Verify project builds with zero errors

**Checkpoint:** DI configuration complete ✓

---

## Phase 6: Testing & Quality Assurance

**Duration:** 1-2 days | **Effort:** 4-6 hours | **Dependencies:** Phase 5 ✓

### Unit Testing

- [ ] T055 Run all existing Volvo tests - verify zero failures
- [ ] T056 Run new handler tests - verify passing
- [ ] T057 Run new validator tests - verify passing
- [ ] T058 Run new ViewModel tests - verify passing
- [ ] T059 Calculate test coverage - target ≥ 80%

### Build Verification

- [ ] T060 Clean build (dotnet clean && dotnet build)
- [ ] T061 Verify zero errors
- [ ] T062 Verify zero warnings
- [ ] T063 Run full test suite - all tests passing
- [ ] T064 Verify no regressions in other modules

**Checkpoint:** Test coverage ≥ 80%, all tests passing ✓

---

## Phase 7: Documentation Updates

**Duration:** 1 day | **Effort:** 2-3 hours | **Dependencies:** Phase 6 ✓

### Create/Update Documentation

- [ ] T065 Update or create Module_Volvo/README.md
- [ ] T066 Create Module_Volvo/ARCHITECTURE.md (CQRS pattern)
- [ ] T067 Create Module_Volvo/HANDLERS_REFERENCE.md (handler list)
- [ ] T068 Update SETTABLE_OBJECTS_REPORT.md (if auto-generated, re-run)
- [ ] T069 Create workflow diagrams (PlantUML)

**Checkpoint:** All documentation updated ✓

---

## Phase 8: Constitutional Compliance Audit

**Duration:** 2 hours | **Effort:** 2 hours | **Dependencies:** Phase 7 ✓

### Verify Principle I: MVVM & View Purity

- [ ] T070 Verify all ViewModels are partial classes
- [ ] T071 Verify no business logic in .xaml.cs code-behind
- [ ] T072 Verify all Views use x:Bind (not Binding)
- [ ] T073 Verify no Click event handlers remain
- [ ] T074 Verify no async void events remain

### Verify Principle III: CQRS & MediatR

- [ ] T075 Verify all handlers exist and implemented
- [ ] T076 Verify ViewModels use IMediator only
- [ ] T077 Verify no direct service calls from ViewModels
- [ ] T078 Verify handlers never throw exceptions

### Verify Principle V: Validation & Error Handling

- [ ] T079 Verify all commands have validators
- [ ] T080 Verify handlers return Model_Dao_Result
- [ ] T081 Verify logging present in handlers

### Final Build Check

- [ ] T082 Final clean build - zero errors
- [ ] T083 Final full test suite - all passing
- [ ] T084 Constitutional compliance verified - ZERO deviations

**Checkpoint:** Constitutional compliance VERIFIED ✓

---

## Phase 9: End-to-End Testing & Deployment Ready

**Duration:** 1 day | **Effort:** 4-5 hours | **Dependencies:** Phase 8 ✓

### Workflow Testing

- [ ] T085 Create smoke test for shipment entry workflow
- [ ] T086 Create smoke test for settings management workflow
- [ ] T087 Create smoke test for history viewing workflow
- [ ] T088 Verify all user workflows work identically to before

### Performance Verification

- [ ] T089 Measure handler execution time (target < 100ms)
- [ ] T090 Measure validation time (target < 10ms)
- [ ] T091 Measure query performance (target < 50ms)
- [ ] T092 Measure end-to-end workflow (target < 200ms)

### Deployment Readiness

- [ ] T093 Verify migrations applied
- [ ] T094 Verify connection strings configured
- [ ] T095 Verify logging configured and working
- [ ] T096 Verify error handling operational
- [ ] T097 Module ready for production deployment

**Checkpoint:** Module production-ready ✓

---

## Summary Table

| Phase | Focus | Tasks | Effort | Status |
|-------|-------|-------|--------|--------|
| 1 | Foundation | T001-T005 | 2-3 hrs | Pending |
| 2 | Principle I (MVVM) | T006-T020 | 10-12 hrs | Pending |
| 3 | Principle III (CQRS) | T021-T042 | 16-18 hrs | Pending |
| 4 | ViewModels | T043-T048 | 3-4 hrs | Pending |
| 5 | DI Registration | T049-T054 | 1 hr | Pending |
| 6 | Testing | T055-T064 | 4-6 hrs | Pending |
| 7 | Documentation | T065-T069 | 2-3 hrs | Pending |
| 8 | Compliance Audit | T070-T084 | 2 hrs | Pending |
| 9 | Deployment Ready | T085-T097 | 4-5 hrs | Pending |
| **TOTAL** | **All Phases** | **97 tasks** | **34-44 hrs** | **Ready** |

---

## How to Complete Tasks

### Batch Completion Example

Select multiple lines and replace at once:

```
Before (3 tasks pending):
- [ ] T001 Task description
- [ ] T002 Task description
- [ ] T003 Task description

After (all 3 completed):
- [x] T001 Task description
- [x] T002 Task description
- [x] T003 Task description
```

Your editor can do multi-line find/replace to complete entire phases at once.

---

## Execution Notes

**Start With:** Phase 1, Task T001  
**Complete:** In order (each phase depends on previous)  
**Parallelize:** Tasks marked similar effort in same phase can run side-by-side  
**Validate:** At each checkpoint before proceeding  
**Stop Points:** After each phase completes, review checkpoint before next phase  

---

**Status:** ✅ Ready for Execution  
**Template Used:** `_bmad/module-agents/templates/tasktemplates/module-modernization-tasks.md`  
**Last Updated:** 2026-01-16  
**Authority:** Module Rebuilder Agent - Strict Constitutional Compliance Mode
