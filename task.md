# Tasks: Shutdown And Connection Cleanup Remediation

**Source**: [.github/audits/03262026-ShutdownAndConnectionCleanup-Audit.md](.github/audits/03262026-ShutdownAndConnectionCleanup-Audit.md)

**Goal**: Remove abrupt shutdown paths, centralize app teardown, harden database cleanup behavior, and eliminate workflow-related timer and event retention issues.

**Format**: `[ID] [Area] Description`

## Phase 1: Lifecycle Foundation

- [x] T001 [Core] Add a shared application shutdown contract and implementation in `Module_Core/Contracts/Services/IService_ApplicationShutdown.cs` and `Module_Core/Services/Service_ApplicationShutdown.cs`.
- [x] T002 [Core] Register shutdown coordination in `Infrastructure/DependencyInjection/CoreServiceExtensions.cs`.
- [x] T003 [Shell] Centralize shutdown in `App.xaml.cs`, including host start, host stop, host disposal, exit-code propagation, and connection-pool clearing.
- [x] T004 [Shell] Remove direct process termination from `MainWindow.xaml.cs` and route final exit through `App.xaml.cs`.
- [x] T005 [Shared] Change `Module_Shared/Views/View_Shared_SplashScreenWindow.xaml.cs` to request coordinated shutdown instead of calling `Application.Exit()` directly.

## Phase 2: Startup Exit Path Cleanup

- [x] T006 [Core] Replace `Environment.Exit(...)` and direct `Application.Exit()` startup branches in `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs` with shutdown requests and orderly window teardown.
- [x] T007 [Core] Add startup abort checks and failure cleanup in `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs` so startup cancellation and startup exceptions do not leave timer-backed session state behind.

## Phase 3: Database Safety And Ownership

- [x] T008 [Core] Enforce `ApplicationIntent=ReadOnly` in the constructor of `Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs`.
- [x] T009 [Volvo] Move Volvo shipment-line transaction ownership from `Module_Volvo/Services/Service_Volvo.cs` into `Module_Volvo/Data/Dao_VolvoShipmentLine.cs`.
- [x] T010 [Shell] Clear MySQL and SQL Server pools during coordinated shutdown in `App.xaml.cs`.

## Phase 4: Timer, Event, And Cancellation Cleanup

- [x] T011 [Core] Add weak event infrastructure in `Module_Core/Helpers/Events/WeakEventSource.cs`.
- [x] T012 [Receiving] Convert workflow step and status notifications in `Module_Receiving/Services/Service_ReceivingWorkflow.cs` to weak event delivery.
- [x] T013 [Dunnage] Convert guided-workflow notifications in `Module_Dunnage/Services/Service_DunnageWorkflow.cs` to weak event delivery and dispose singleton session-timeout subscriptions.
- [x] T014 [Dunnage] Convert admin-workflow notifications in `Module_Dunnage/Services/Service_DunnageAdminWorkflow.cs` to weak event delivery.
- [x] T015 [Core] Make `Module_Core/Services/Authentication/Service_UserSessionManager.cs` dispose its timer-backed monitoring state.
- [x] T016 [Core] Link `Module_Core/Services/VisualAutomation/Service_UIAutomation.cs` to the shared shutdown token so polling and field-fill waits can cancel during shutdown.
- [x] T017 [Dunnage] Dispose timer-backed dialog validation in `Module_Dunnage/ViewModels/ViewModel_Dunnage_AddTypeDialogViewModel.cs` and `Module_Dunnage/Views/View_Dunnage_Dialog_Dunnage_AddTypeDialog.xaml.cs`.
- [x] T018 [Dunnage] Remove obsolete manual unsubscribe logic from `Module_Dunnage/ViewModels/ViewModel_Dunnage_ReviewViewModel.cs` after weak workflow events are in place.

## Phase 5: Documentation And Metadata

- [x] T019 [Audit] Extend `.github/audits/03262026-ShutdownAndConnectionCleanup-Audit.md` with additional MySQL closure edge cases.
- [x] T020 [Docs] Update impacted CopilotForms module metadata under `docs/CopilotForms/data/module-metadata/` for Core, Shared, Receiving, Dunnage, and Volvo lifecycle changes.

## Phase 6: Validation

- [ ] T021 [Validation] Build the solution with the workspace `build` task.
- [ ] T022 [Validation] Run the relevant automated tests for the touched areas.
- [ ] T023 [Validation] Resolve any build or test regressions introduced by the remediation work.

## Validation Notes

- The remediation files themselves currently report no file-level IDE errors.
- A full solution build is still blocked by broad pre-existing WinUI/XAML generated duplicate-member errors under `obj/ARM64/Debug/net10.0-windows10.0.22621.0/`, including generated files for `View_Receiving_Workflow`, `View_Dunnage_QuickAddTypeDialog`, and several `Module_Settings.*` views.
- `dotnet clean MTM_Receiving_Application.slnx` completed successfully, but the duplicate generated-member build failure remained on the next rebuild.