---
description: "Module Modernization Tasks Checklist Template - STRICT Constitutional Compliance"
format: "Checklist only - no code snippets, just task titles and checkboxes"
---

# Tasks Template: Module_{Feature} - Strict Constitutional Compliance

**Replace {Feature} with actual module name**

## How to Use This Template

1. Copy this entire template
2. Rename to `tasks.md` in your spec folder
3. Replace all `{Feature}` placeholders with actual module name
4. Update task counts and phases as needed
5. Check off multiple tasks at once as they complete
6. Keep format clean - checkboxes only, no inline code or snippets

---

## Phase 1: Foundation & Structure

**Duration:** 1 day | **Effort:** 2-3 hours | **Blocking:** YES

- [ ] T001 Create folder structure (Handlers/Commands, Handlers/Queries, Validators, Models/Requests)
- [ ] T002 Create handler base files (README.md templates)
- [ ] T003 Verify MediatR configuration in App.xaml.cs
- [ ] T004 Verify FluentValidation configuration in App.xaml.cs
- [ ] T005 Verify project builds with zero errors

---

## Phase 2: Principle I - MVVM & View Purity

**Duration:** 2-3 days | **Effort:** 10-12 hours | **Dependencies:** Phase 1 ✓

### Create ViewModels to Replace Code-Behind Logic

- [ ] T006 Write tests for shipment line add/remove operations
- [ ] T007 Create ViewModel_Feature_ShipmentLine_Edit.cs
- [ ] T008 Refactor code-behind: Remove business logic methods
- [ ] T009 Update XAML: Bind commands to RelayCommand
- [ ] T010 Write tests for part add/edit operations
- [ ] T011 Create ViewModel_Feature_Part_AddEdit.cs
- [ ] T012 Refactor code-behind: Remove dialog state management
- [ ] T013 Fix View_Feature_Settings: Remove OnPageLoaded async void event
- [ ] T014 Fix View_Feature_History: Remove OnPageLoaded async void event
- [ ] T015 Fix View_Feature_ShipmentEntry: Remove OnLoaded async void event

**Checkpoint:** All code-behind business logic removed ✓

---

## Phase 3: Principle III - CQRS & MediatR

**Duration:** 2 days | **Effort:** 16-18 hours | **Dependencies:** Phase 2 ✓

### Create Request Models (Commands & Queries)

- [ ] T016 Create AddLineCommand.cs
- [ ] T017 Create RemoveLineCommand.cs
- [ ] T018 Create UpdateFeatureCommand.cs
- [ ] T019 Create GetFeatureItemsQuery.cs
- [ ] T020 Create additional query/command models as needed

### Create FluentValidation Validators

- [ ] T021 Write validator tests
- [ ] T022 Create AddLineCommandValidator.cs
- [ ] T023 Create RemoveLineCommandValidator.cs
- [ ] T024 Create UpdateFeatureCommandValidator.cs
- [ ] T025 Create additional validators as needed

### Implement Command Handlers

- [ ] T026 Write handler tests
- [ ] T027 Create AddLineCommandHandler.cs
- [ ] T028 Create RemoveLineCommandHandler.cs
- [ ] T029 Create UpdateFeatureCommandHandler.cs
- [ ] T030 Create additional command handlers as needed

### Implement Query Handlers

- [ ] T031 Write query handler tests
- [ ] T032 Create GetFeatureItemsQueryHandler.cs
- [ ] T033 Create additional query handlers as needed

**Checkpoint:** All handlers and validators implemented ✓

---

## Phase 4: ViewModel Modernization

**Duration:** 1 day | **Effort:** 3-4 hours | **Dependencies:** Phase 3 ✓

### Update ViewModels to Use IMediator

- [ ] T034 Update ViewModel_Feature_Entry: Inject IMediator, remove service calls
- [ ] T035 Update ViewModel_Feature_Settings: Inject IMediator, remove service calls
- [ ] T036 Update ViewModel_Feature_History: Inject IMediator, remove service calls
- [ ] T037 Replace all service calls with mediator.Send(query/command)

**Checkpoint:** All ViewModels use IMediator exclusively ✓

---

## Phase 5: Dependency Injection Registration

**Duration:** 30 minutes | **Effort:** 1 hour | **Dependencies:** Phase 4 ✓

### Update App.xaml.cs

- [ ] T038 Register new ViewModels as Transient
- [ ] T039 Verify DAOs registered as Singleton
- [ ] T040 Verify MediatR handler auto-registration working
- [ ] T041 Verify FluentValidation auto-discovery working

**Checkpoint:** DI configuration complete ✓

---

## Phase 6: Testing & Quality Assurance

**Duration:** 1-2 days | **Effort:** 4-6 hours | **Dependencies:** Phase 5 ✓

### Unit Testing

- [ ] T042 Run all existing tests - verify zero failures
- [ ] T043 Run new handler tests - verify passing
- [ ] T044 Run new validator tests - verify passing
- [ ] T045 Calculate test coverage - target ≥ 80%

### Build Verification

- [ ] T046 Clean build (dotnet clean && dotnet build)
- [ ] T047 Verify zero errors
- [ ] T048 Verify zero warnings (or approved only)
- [ ] T049 Run full test suite - all tests passing

**Checkpoint:** Test coverage ≥ 80%, all tests passing ✓

---

## Phase 7: Documentation Updates

**Duration:** 1 day | **Effort:** 2-3 hours | **Dependencies:** Phase 6 ✓

### Create/Update Documentation

- [ ] T050 Update or create README.md for module
- [ ] T051 Create ARCHITECTURE.md (CQRS pattern explanation)
- [ ] T052 Create HANDLERS_REFERENCE.md (list all handlers)
- [ ] T053 Update SETTABLE_OBJECTS_REPORT.md (if auto-generated, re-run)
- [ ] T054 Add handler documentation with input/output structures

**Checkpoint:** All documentation updated ✓

---

## Phase 8: Constitutional Compliance Audit

**Duration:** 2 hours | **Effort:** 2 hours | **Dependencies:** Phase 7 ✓

### Verify ZERO Deviations from Constitution

#### Principle I: MVVM & View Purity

- [ ] T055 Verify all ViewModels are partial classes
- [ ] T056 Verify no business logic in .xaml.cs code-behind
- [ ] T057 Verify all Views use x:Bind (not Binding)
- [ ] T058 Verify no Click event handlers remain
- [ ] T059 Verify no async void events remain

#### Principle III: CQRS & MediatR

- [ ] T060 Verify all handlers exist and implemented
- [ ] T061 Verify ViewModels use IMediator only
- [ ] T062 Verify no direct service calls from ViewModels

#### Principle V: Validation & Error Handling

- [ ] T063 Verify all commands have validators
- [ ] T064 Verify handlers return Model_Dao_Result (never throw)
- [ ] T065 Verify logging present in handlers

#### Final Build Check

- [ ] T066 Final clean build - zero errors
- [ ] T067 Final full test suite - all passing
- [ ] T068 Constitutional compliance verified - ZERO deviations

**Checkpoint:** Constitutional compliance VERIFIED ✓

---

## Phase 9: End-to-End Testing & Deployment Ready

**Duration:** 1 day | **Effort:** 4-5 hours | **Dependencies:** Phase 8 ✓

### Workflow Testing

- [ ] T069 Create smoke test for primary workflow
- [ ] T070 Create smoke test for secondary workflow
- [ ] T071 Verify all user workflows work identically to before

### Performance Verification

- [ ] T072 Measure handler execution time (target < 100ms)
- [ ] T073 Measure validation time (target < 10ms)
- [ ] T074 Measure query time (target < 50ms)
- [ ] T075 Measure end-to-end workflow (target < 200ms)

### Deployment Readiness

- [ ] T076 Verify migrations applied
- [ ] T077 Verify connection strings configured
- [ ] T078 Verify logging configured and working
- [ ] T079 Verify error handling operational
- [ ] T080 Module ready for production deployment

**Checkpoint:** Module production-ready ✓

---

## Summary

| Phase | Tasks | Effort | Status |
|-------|-------|--------|--------|
| 1 - Foundation | T001-T005 | 2-3 hrs | Pending |
| 2 - MVVM | T006-T015 | 10-12 hrs | Pending |
| 3 - CQRS | T016-T033 | 16-18 hrs | Pending |
| 4 - ViewModels | T034-T037 | 3-4 hrs | Pending |
| 5 - DI | T038-T041 | 1 hr | Pending |
| 6 - Testing | T042-T049 | 4-6 hrs | Pending |
| 7 - Docs | T050-T054 | 2-3 hrs | Pending |
| 8 - Audit | T055-T068 | 2 hrs | Pending |
| 9 - Deployment | T069-T080 | 4-5 hrs | Pending |
| **TOTAL** | **80 tasks** | **34-44 hrs** | **Ready** |

---

## How to Complete Tasks

Format for checking off multiple tasks:

```
Before:
- [ ] T001
- [ ] T002
- [ ] T003

After completing all three:
- [x] T001
- [x] T002
- [x] T003
```

You can select and replace multiple lines at once in your editor to complete batches of tasks together.

---

**Template Version:** 1.0  
**Created:** 2026-01-16  
**Authority:** Module Rebuilder Agent - Strict Constitutional Compliance Mode
