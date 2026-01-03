# Implementation Plan: Multi-Step Receiving Label Entry Workflow

**Branch**: `003-database-foundation` | **Date**: December 17, 2025 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/003-database-foundation/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Implement a 9-step guided workflow for warehouse receiving operations within the WinUI 3 application. Users will enter PO numbers or non-PO items, select parts, specify load quantities and heat numbers, assign package types, review data in an editable grid with cascading updates, and save to CSV files and MySQL database. The workflow includes session persistence via JSON, part validation against Infor Visual (SQL Server), smart package type defaults, quick-select heat numbers, and comprehensive error handling.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0, WinUI 3 (Windows App SDK)  
**Primary Dependencies**: 
- WinUI 3 (Microsoft.WindowsAppSDK)
- CommunityToolkit.Mvvm (MVVM framework)
- MySql.Data or Dapper (MySQL connectivity)
- Microsoft.Data.SqlClient (Infor Visual SQL Server connectivity)
- System.Text.Json (session persistence)
- CsvHelper (CSV file generation)

**Storage**: 
- MySQL database (receiving_loads, package_type_preferences tables)
- SQL Server (Infor Visual - read-only queries for PO/Part data)
- Local CSV file (%APPDATA%\ReceivingData.csv)
- Network CSV file (\\mtmanu-fs01\...\JKOLL\ReceivingData.csv)
- Session JSON (%APPDATA%\MTM_Receiving_Application\session.json)

**Testing**: xUnit with FluentAssertions (unit tests), integration tests for database operations  
**Target Platform**: Windows 10 19041+ (Desktop application)  
**Project Type**: Single desktop application with MVVM architecture  
**Performance Goals**: 
- PO query response < 2 seconds for 95% of requests
- Part selection to next step < 500ms
- Review grid rendering < 1 second for 50 loads
- Save operation complete within 5 seconds for 50 loads
- UI remains responsive during database operations (async/await pattern)

**Constraints**: 
- Must integrate within existing MainWindow.xaml NavigationView
- Session must persist across application restarts
- Cascading updates in review grid must be atomic (all-or-nothing)
- Network CSV path may be unavailable (graceful degradation required)
- Follows existing MVVM patterns and dependency injection setup

**Scale/Scope**: 
- Single-user workstation application
- Typical session: 1-5 parts, 3-20 loads per part
- Max supported: 99 loads per part, unlimited parts per session
- Expected daily usage: 50-200 receiving entries per terminal

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Status**: ✅ PASSED (No constitution file defined - using best practices)

The project does not have a populated constitution.md file, so standard best practices apply:
- **MVVM Architecture**: Feature follows existing MVVM patterns with ViewModels, Services, and Models
- **Dependency Injection**: Uses Microsoft.Extensions.DependencyInjection patterns already established in App.xaml.cs
- **Testing**: Unit tests for ViewModels and Services, integration tests for database operations
- **Error Handling**: Comprehensive error handling with user-friendly messages and graceful degradation
- **Code Organization**: Follows existing project structure (Models/, ViewModels/, Services/, Views/)
- **Async/Await**: All I/O operations (database, file) use async patterns to keep UI responsive

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
MTM_Receiving_Application/
├── Models/
│   └── Receiving/
│       ├── Model_ReceivingLoad.cs
│       ├── Model_ReceivingSession.cs
│       ├── Model_InforVisualPO.cs
│       ├── Model_InforVisualPart.cs
│       ├── Model_PackageTypePreference.cs
│       └── Model_HeatCheckboxItem.cs
├── ViewModels/
│   └── Receiving/
│       ├── ReceivingWorkflowViewModel.cs
│       ├── POEntryViewModel.cs
│       ├── LoadEntryViewModel.cs
│       ├── WeightQuantityViewModel.cs
│       ├── HeatLotViewModel.cs
│       ├── PackageTypeViewModel.cs
│       └── ReviewGridViewModel.cs
├── Views/
│   └── Receiving/
│       ├── ReceivingWorkflowView.xaml(.cs)
│       ├── POEntryView.xaml(.cs)
│       ├── LoadEntryView.xaml(.cs)
│       ├── WeightQuantityView.xaml(.cs)
│       ├── HeatLotView.xaml(.cs)
│       ├── PackageTypeView.xaml(.cs)
│       └── ReviewGridView.xaml(.cs)
├── Services/
│   ├── Database/
│   │   ├── Service_InforVisual.cs (queries PO/Part data from SQL Server)
│   │   ├── Service_MySQL_Receiving.cs (saves to MySQL receiving tables)
│   │   └── Service_MySQL_PackagePreferences.cs (package type preferences)
│   ├── Receiving/
│   │   ├── Service_SessionManager.cs (JSON persistence for sessions)
│   │   ├── Service_CSVWriter.cs (writes to local/network CSV)
│   │   ├── Service_ReceivingValidation.cs (validation logic)
│   │   └── Service_ReceivingWorkflow.cs (orchestrates workflow steps)
├── Contracts/
│   └── Services/
│       ├── IService_InforVisual.cs
│       ├── IService_MySQL_Receiving.cs
│       ├── IService_MySQL_PackagePreferences.cs
│       ├── IService_SessionManager.cs
│       ├── IService_CSVWriter.cs
│       ├── IService_ReceivingValidation.cs
│       └── IService_ReceivingWorkflow.cs
├── Database/
│   └── StoredProcedures/
│       └── Receiving/
│           ├── sp_GetPOWithParts.sql
│           ├── sp_GetPartByID.sql
│           ├── sp_GetReceivingByPOPartDate.sql
│           ├── sp_InsertReceivingLoad.sql
│           └── sp_GetPackageTypePreference.sql
```

**Structure Decision**: Single WinUI 3 desktop application following MVVM pattern. The feature integrates into the existing application structure with Models, ViewModels, Views, Services, and Contracts. Views are hosted within MainWindow.xaml's NavigationView. Database operations separated by source (Infor Visual read-only, MySQL read/write). Session management and CSV writing are isolated services for testability.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

**Status**: N/A - No constitution violations identified

This feature follows established patterns and best practices. No complexity justification required.

---

## Phase Completion Summary

### Phase 0: Research ✅ COMPLETE
- **Output**: [research.md](./research.md)
- **Key Decisions**:
  - UserControl-based navigation within single View
  - CommunityToolkit DataGrid for editable grid with cascading updates
  - System.Text.Json for session persistence
  - CsvHelper for CSV generation with graceful network fallback
  - SqlClient + stored procedures for Infor Visual
  - MySql.Data with transactions for receiving data
  - ViewModel-based package type defaults with DB persistence
  - CheckBox list for quick-select heat numbers

### Phase 1: Design & Contracts ✅ COMPLETE
- **Outputs**:
  - [data-model.md](./data-model.md) - Complete entity definitions with validation rules
  - [contracts/](./contracts/) - 7 service interface specifications (.md format to avoid compilation)
    - IService_InforVisual.md (PO/Part queries)
    - IService_MySQL_Receiving.md (save receiving loads)
    - IService_MySQL_PackagePreferences.md (package type persistence)
    - IService_SessionManager.md (JSON session persistence)
    - IService_CSVWriter.md (CSV file generation)
    - IService_ReceivingValidation.md (business rule validation)
    - IService_ReceivingWorkflow.md (workflow orchestration)
  - [quickstart.md](./quickstart.md) - Implementation guide for developers
  
- **Agent Context Updated**: ✅ GitHub Copilot context file updated with C# 12 / .NET 8.0, WinUI 3

### Phase 2: Task Breakdown ⏭️ NEXT STEP
- Run `/speckit.tasks` command to generate implementation tasks
- Tasks.md will break down implementation into concrete work items
- Includes test planning and acceptance criteria

---

## Constitution Check Re-evaluation (Post-Design)

**Status**: ✅ PASSED

Design maintains adherence to best practices:
- **MVVM Architecture**: Proper separation of concerns (Models, Views, ViewModels, Services)
- **Dependency Injection**: All services injected via interfaces
- **Testability**: Services isolated with clear contracts, mockable interfaces
- **Async/Await**: All I/O operations non-blocking
- **Error Handling**: Graceful degradation (network CSV fallback), validation gates
- **Code Organization**: Follows existing project structure conventions

No violations introduced during design phase.

---

## Next Actions

1. ✅ Phase 0 & 1 Complete - Planning and design artifacts generated
2. ⏭️ **Run `/speckit.tasks`** to generate implementation task breakdown
3. Begin implementation following quickstart.md
4. Create database migrations (SQL Server stored procs, MySQL tables)
5. Implement services with unit tests
6. Implement ViewModels with unit tests  
7. Implement Views with bindings
8. Integration testing
9. Manual testing against spec acceptance criteria

---

**Planning Complete**: December 17, 2025  
**Ready for**: Task breakdown and implementation  
**Branch**: 003-database-foundation  
**Spec**: [spec.md](./spec.md)
