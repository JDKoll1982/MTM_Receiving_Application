# Implementation Plan: Receiving Workflow Consolidation

**Branch**: `copilot/follow-instructions-in-agent-files` | **Date**: 2026-01-24 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/copilot/follow-instructions-in-agent-files/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Consolidate Module_Receiving wizard workflow from 12 steps to 3 steps by grouping related functionality: 
- **Step 1**: Order & Part Selection (PO Number, Part, Load Count)
- **Step 2**: Load Details Entry (Weight/Quantity, Heat Lot, Package Type, Packages Per Load) with bulk copy operations
- **Step 3**: Review & Save (Summary, validation, save to CSV and database)

Technical approach uses MVVM with CQRS/MediatR pattern, WinUI 3 DataGrid for multi-load entry, FluentValidation for real-time validation, and bulk copy operations with auto-fill tracking to preserve data integrity.

## Technical Context

**Language/Version**: C# 12 / .NET 8  
**Primary Dependencies**: WinUI 3 (Windows App SDK 1.6+), CommunityToolkit.Mvvm, MediatR, FluentValidation, Serilog, CsvHelper  
**Storage**: MySQL 8.0 (mtm_receiving_application - READ/WRITE), SQL Server (Infor Visual - READ ONLY)  
**Testing**: xUnit with FluentAssertions, Bogus for test data  
**Target Platform**: Windows 10/11 Desktop (version 1809 or later)  
**Project Type**: WinUI 3 Desktop Application  
**Performance Goals**: 
- UI responsiveness: < 100ms for all user interactions
- Bulk copy operations: < 1 second for up to 100 loads
- Save operations: < 3 seconds for complete workflow  
**Constraints**: 
- Read-only access to Infor Visual database (ApplicationIntent=ReadOnly)
- Must support offline data entry (save to CSV)
- Must maintain backward compatibility with existing data format
- No internet connectivity required  
**Scale/Scope**: 
- Up to 100 loads per receiving transaction
- Concurrent users: 5-10
- Estimated 50-100 transactions per day
- Data retention: All historical records

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify alignment with the constitution:

- MVVM purity (partial ViewModels, x:Bind only, no code-behind business logic).
- Data access integrity (MySQL stored procedures only; Infor Visual read-only; instance DAOs returning Model_Dao_Result; no static DAOs).
- CQRS/MediatR usage with pipeline behaviors (validation/logging/audit) and mediator-first ViewModels.
- DI registration in App.xaml.cs with module boundaries (Module_Core infra only; module-specific logic stays local).
- Validation/logging/error handling (FluentValidation + IService_ErrorHandler + Serilog structured logs; no exception leakage to UI).
- Security/session discipline (auth tiers, timeouts, auditability; no secrets in code).
- Library-first approach (use approved libraries before writing custom services: MediatR, FluentValidation, Serilog, CsvHelper, AutoMapper/Mapster, Scrutor, Polly, Ardalis.GuardClauses, FluentAssertions/Bogus).

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Module_Receiving/
├── ViewModels/
│   ├── ViewModel_Receiving_Workflow.cs                    # Main 3-step workflow ViewModel
│   ├── ViewModel_Receiving_Workflow_LoadDetail.cs         # Individual load detail ViewModel
│   └── ViewModel_Receiving_ManualEntry.cs                 # Manual mode ViewModel
├── Views/
│   ├── View_Receiving_Workflow_Step1.xaml                 # Step 1: Order & Part
│   ├── View_Receiving_Workflow_Step2.xaml                 # Step 2: Load Details (DataGrid)
│   └── View_Receiving_Workflow_Step3.xaml                 # Step 3: Review & Save
├── Commands/
│   ├── StartWorkflowCommand.cs
│   ├── UpdateStep1Command.cs
│   ├── UpdateLoadDetailCommand.cs
│   ├── CopyToLoadsCommand.cs
│   ├── ClearAutoFilledDataCommand.cs
│   ├── ForceOverwriteCommand.cs
│   ├── ChangeCopySourceCommand.cs
│   ├── NavigateToStepCommand.cs
│   └── SaveWorkflowCommand.cs
├── Queries/
│   ├── GetSessionQuery.cs
│   ├── GetLoadDetailsQuery.cs
│   ├── GetValidationStatusQuery.cs
│   ├── GetCopyPreviewQuery.cs
│   └── GetPartLookupQuery.cs
├── Handlers/
│   ├── StartWorkflowCommandHandler.cs
│   ├── UpdateStep1CommandHandler.cs
│   ├── UpdateLoadDetailCommandHandler.cs
│   ├── CopyToLoadsCommandHandler.cs
│   ├── ClearAutoFilledDataCommandHandler.cs
│   ├── ForceOverwriteCommandHandler.cs
│   ├── ChangeCopySourceCommandHandler.cs
│   ├── NavigateToStepCommandHandler.cs
│   ├── SaveWorkflowCommandHandler.cs
│   ├── GetSessionQueryHandler.cs
│   ├── GetLoadDetailsQueryHandler.cs
│   ├── GetValidationStatusQueryHandler.cs
│   ├── GetCopyPreviewQueryHandler.cs
│   └── GetPartLookupQueryHandler.cs
├── Validators/
│   ├── StartWorkflowCommandValidator.cs
│   ├── UpdateStep1CommandValidator.cs
│   ├── UpdateLoadDetailCommandValidator.cs
│   ├── CopyToLoadsCommandValidator.cs
│   ├── ClearAutoFilledDataCommandValidator.cs
│   └── SaveWorkflowCommandValidator.cs
├── Models/
│   ├── ReceivingWorkflowSession.cs
│   ├── LoadDetail.cs
│   ├── ValidationError.cs
│   ├── CopyOperationResult.cs
│   ├── CopyPreview.cs
│   └── SaveResult.cs
├── Services/
│   ├── IService_ReceivingWorkflow.cs
│   └── Service_ReceivingWorkflow.cs
├── Data/
│   ├── Dao_ReceivingWorkflowSession.cs
│   └── Dao_ReceivingLoadDetail.cs
└── Behaviors/
    ├── ValidationBehavior.cs                               # MediatR pipeline behavior
    ├── LoggingBehavior.cs                                  # MediatR pipeline behavior
    └── TransactionBehavior.cs                              # MediatR pipeline behavior

Database/
└── StoredProcedures/
    ├── sp_Create_Receiving_Session.sql
    ├── sp_Update_Load_Detail.sql
    ├── sp_Get_Session_With_Loads.sql
    ├── sp_Save_Completed_Transaction.sql
    ├── sp_Copy_To_Loads.sql
    └── sp_Clear_AutoFilled_Data.sql

MTM_Receiving_Application.Tests/
├── Module_Receiving/
│   ├── Commands/
│   │   ├── UpdateStep1CommandHandlerTests.cs
│   │   ├── CopyToLoadsCommandHandlerTests.cs
│   │   └── SaveWorkflowCommandHandlerTests.cs
│   ├── Validators/
│   │   ├── UpdateStep1CommandValidatorTests.cs
│   │   └── CopyToLoadsCommandValidatorTests.cs
│   ├── ViewModels/
│   │   └── ViewModel_Receiving_WorkflowTests.cs
│   └── Integration/
│       ├── Dao_ReceivingWorkflowSessionIntegrationTests.cs
│       └── ReceivingWorkflow_EndToEnd_Tests.cs
```

**Structure Decision**: Using WinUI 3 desktop application structure with MVVM + CQRS pattern. Commands/Queries/Handlers are organized separately from ViewModels for clear separation. Data access layer (DAOs) uses stored procedures per constitution. Tests are organized by layer (Commands/Validators/ViewModels/Integration).


## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
