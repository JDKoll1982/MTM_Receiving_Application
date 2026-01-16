# Implementation Plan: Module_Volvo CQRS Modernization

**Branch**: `001-volvo-modernization` | **Date**: January 16, 2026 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-volvo-modernization/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Refactor Module_Volvo from legacy service-based architecture to CQRS pattern using MediatR, implementing 12 query handlers and 9 command handlers with FluentValidation validators. Migrate ViewModels from direct service injection to IMediator-based operations while maintaining 100% functional parity with existing CSV label generation, email formatting, and component explosion calculations. Preserve all existing stored procedures and database schema. Achieve constitutional compliance by eliminating Principle III violations (ViewModels calling services directly), Principle I violations (Binding vs x:Bind), and Principle V violations (missing FluentValidation).

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**: MediatR 12.4.1, FluentValidation 11.10.0, CommunityToolkit.Mvvm 8.2+, MySql.Data 9.0+, Serilog 4.1.0  
**Storage**: MySQL 8.0 (mtm_receiving_application schema) with stored procedures (sp_volvo_*), no schema changes allowed  
**Testing**: xUnit with FluentAssertions, Moq for mocking, property-based tests for calculations, golden file tests for CSV/email  
**Target Platform**: Windows 10 1809+ / Windows 11 desktop (WinUI 3 / Windows App SDK 1.8+)  
**Project Type**: Desktop application - modular structure with Module_Volvo as feature module  
**Performance Goals**: <500ms for database operations (constitutional target), <10ms MediatR pipeline overhead, maintain or improve current shipment completion time  
**Constraints**: Zero database schema changes, byte-for-byte CSV/email parity with legacy, 100% functional preservation, no UI redesign  
**Scale/Scope**: 3 ViewModels (26+9+12 methods), 21 handlers (12 queries + 9 commands), 8 validators, 5 DAOs, 6 XAML views, ~3000 LOC in module

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Initial Check (Pre-Research):**

✅ **MVVM Purity** (Principle I):

- ViewModels are partial ✅ (all 3 ViewModels verified)
- ViewModels inherit ViewModel_Shared_Base ✅
- [ObservableProperty] and [RelayCommand] used ✅
- ❌ VIOLATION: 20 occurrences of `{Binding}` instead of `x:Bind` in DataGrid columns (Views: View_Volvo_ShipmentEntry, View_Volvo_History, View_Volvo_Settings, VolvoShipmentEditDialog)
- ✅ No business logic in code-behind detected

✅ **Data Access Integrity** (Principle II):

- All DAOs instance-based ✅ (Dao_VolvoShipment, Dao_VolvoShipmentLine, Dao_VolvoPart, Dao_VolvoPartComponent, Dao_VolvoSettings verified)
- All DAOs return Model_Dao_Result<T> ✅
- MySQL operations use stored procedures only ✅ (sp_volvo_* verified)
- No Infor Visual write operations N/A (module uses MySQL only)

❌ **CQRS/MediatR Usage** (Principle III) - **PRIMARY VIOLATION**:

- ViewModels inject IService_Volvo and IService_VolvoMasterData directly ❌
- No IMediator injection found ❌
- No query/command handlers exist (Handlers/Commands/ and Handlers/Queries/ folders empty) ❌
- **This is the core violation this modernization addresses**

✅ **DI Registration** (Principle IV):

- Services registered in App.xaml.cs ✅
- Constructor injection pattern used ✅
- Module boundaries respected ✅

❌ **Validation/Logging/Error Handling** (Principle V):

- IService_ErrorHandler used ✅
- Serilog structured logging via Module_Core ✅
- ❌ VIOLATION: No FluentValidation validators exist (validation embedded in services/ViewModels)

✅ **Security/Session Discipline** (Principle VI):

- Authentication patterns followed ✅ (IService_UserSessionManager injected)
- No secrets in code ✅

✅ **Library-First Approach** (Principle VII):

- Uses approved libraries (MediatR, FluentValidation already in Module_Core) ✅
- Ready to leverage Mapster, Ardalis.GuardClauses, Bogus for modernization ✅

**GATE RESULT: ❌ FAIL - 3 violations identified (expected for modernization)**

**Violations to Resolve:**

1. Principle III: Refactor ViewModels to use IMediator (11 query handlers + 8 command handlers)
2. Principle I: Migrate 20 `{Binding}` occurrences to `x:Bind`
3. Principle V: Create 8 FluentValidation validators for command handlers

**Post-Design Check (After Phase 1):**

✅ **Phase 1 Design Complete** - All constitutional requirements addressed in design:

- **Principle III (CQRS/MediatR)**: 21 handlers designed (12 queries + 9 commands) with IRequest<Model_Dao_Result<T>> pattern ✅
- **Principle I (MVVM Purity)**: DataGridTemplateColumn pattern documented for `x:Bind` migration ✅
- **Principle V (Validation)**: 8 FluentValidation validators designed (one per command) ✅

**Contracts Defined**:

- ✅ 9 command DTOs (requests/responses)
- ✅ 12 query DTOs (requests/responses)
- ✅ Shared DTOs (ShipmentLineDto, InitialShipmentData, ShipmentDetail, ImportPartsCsvResult)

**Data Model Documented**:

- ✅ Entity Relationship Diagram (PlantUML)
- ✅ 6 domain entities with validation rules
- ✅ Data flow diagrams for CQRS pipeline
- ✅ Migration strategy (5 phases)

**Quickstart Guide Created**:

- ✅ Step-by-step implementation guide
- ✅ First query handler example (GetInitialShipmentDataQuery)
- ✅ ViewModel refactoring example
- ✅ Golden file test pattern
- ✅ Troubleshooting guide

**GATE RESULT: ✅ PASS - Design resolves all 3 constitutional violations**

## Project Structure

### Documentation (this feature)

```text
specs/001-volvo-modernization/
├── spec.md              # Feature specification ✅ COMPLETE
├── plan.md              # This file (/speckit.plan command output) ✅ COMPLETE
├── research.md          # Phase 0 output (/speckit.plan command) ✅ COMPLETE
├── data-model.md        # Phase 1 output (/speckit.plan command) ✅ COMPLETE
├── quickstart.md        # Phase 1 output (/speckit.plan command) ✅ COMPLETE
├── contracts/           # Phase 1 output (/speckit.plan command) ✅ COMPLETE
│   └── README.md        # API contracts documentation
└── checklists/
    └── requirements.md  # Quality validation checklist ✅ COMPLETE
```

### Source Code (Module_Volvo structure)

```text
Module_Volvo/
├── Contracts/
│   ├── IService_Volvo.cs                    # [TO DEPRECATE] Legacy service interface
│   └── IService_VolvoMasterData.cs          # [TO DEPRECATE] Legacy service interface
│
├── Data/                                    # ✅ DAOs (Instance-based, Model_Dao_Result compliant)
│   ├── Dao_VolvoPart.cs
│   ├── Dao_VolvoPartComponent.cs
│   ├── Dao_VolvoSettings.cs
│   ├── Dao_VolvoShipment.cs
│   └── Dao_VolvoShipmentLine.cs
│
├── Handlers/                                # [NEW] CQRS handlers
│   ├── Commands/                            # [NEW] 9 command handlers
│   │   ├── AddPartToShipmentCommand.cs
│   │   ├── RemovePartFromShipmentCommand.cs
│   │   ├── SavePendingShipmentCommand.cs
│   │   ├── CompleteShipmentCommand.cs
│   │   ├── UpdateShipmentCommand.cs
│   │   ├── AddVolvoPartCommand.cs
│   │   ├── UpdateVolvoPartCommand.cs
│   │   ├── DeactivateVolvoPartCommand.cs
│   │   └── ImportPartsCsvCommand.cs
│   │
│   └── Queries/                             # [NEW] 12 query handlers
│       ├── GetInitialShipmentDataQuery.cs
│       ├── GetPendingShipmentQuery.cs
│       ├── SearchVolvoPartsQuery.cs
│       ├── GenerateLabelCsvQuery.cs
│       ├── FormatEmailDataQuery.cs
│       ├── GetRecentShipmentsQuery.cs
│       ├── GetShipmentHistoryQuery.cs
│       ├── GetShipmentDetailQuery.cs
│       ├── GetAllVolvoPartsQuery.cs
│       ├── GetPartComponentsQuery.cs
│       ├── ExportPartsCsvQuery.cs
│       └── ExportShipmentsQuery.cs
│
├── Models/                                  # ✅ Existing models (no changes)
│   ├── Model_VolvoPart.cs
│   ├── Model_VolvoPartComponent.cs
│   ├── Model_VolvoShipment.cs
│   ├── Model_VolvoShipmentLine.cs
│   ├── Model_VolvoEmailData.cs
│   ├── Model_VolvoSetting.cs
│   └── VolvoShipmentStatus.cs
│
├── Requests/                                # [NEW] Command/Query DTOs
│   ├── Commands/                            # [NEW] 9 command DTOs
│   └── Queries/                             # [NEW] 12 query DTOs
│
├── Services/
│   ├── Service_Volvo.cs                     # [TO DEPRECATE] Mark [Obsolete] after migration
│   ├── Service_VolvoMasterData.cs           # [TO DEPRECATE] Mark [Obsolete] after migration
│   └── Service_VolvoAuthorization.cs        # ✅ Keep (auth logic remains service-based)
│
├── Validators/                              # [NEW] 8 FluentValidation validators
│   ├── AddPartToShipmentCommandValidator.cs
│   ├── SavePendingShipmentCommandValidator.cs
│   ├── CompleteShipmentCommandValidator.cs
│   ├── UpdateShipmentCommandValidator.cs
│   ├── AddVolvoPartCommandValidator.cs
│   ├── UpdateVolvoPartCommandValidator.cs
│   ├── DeactivateVolvoPartCommandValidator.cs
│   └── ImportPartsCsvCommandValidator.cs
│
├── ViewModels/                              # [TO REFACTOR] Inject IMediator, remove service calls
│   ├── ViewModel_Volvo_ShipmentEntry.cs     # 26 methods → 11 MediatR calls + 3 local + 1 nav
│   ├── ViewModel_Volvo_History.cs           # 9 methods → 5 MediatR calls + 1 nav
│   └── ViewModel_Volvo_Settings.cs          # 12 methods → 8 MediatR calls
│
└── Views/                                   # [TO REFACTOR] Migrate {Binding} → x:Bind (20 occurrences)
    ├── View_Volvo_ShipmentEntry.xaml
    ├── View_Volvo_History.xaml
    ├── View_Volvo_Settings.xaml
    ├── VolvoPartAddEditDialog.xaml
    └── VolvoShipmentEditDialog.xaml

Module_Volvo.Tests/                          # [NEW] Test project
├── Handlers/
│   ├── Commands/                            # Unit tests for command handlers
│   └── Queries/                             # Unit tests for query handlers
├── Validators/                              # Unit tests for validators
├── Integration/                             # Integration tests (handler → service → DAO → DB)
├── PropertyBased/                           # Property-based tests for calculations
└── GoldenFiles/                             # Golden file tests for CSV/email format
    ├── expected_label.csv
    └── expected_email.html
```

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
