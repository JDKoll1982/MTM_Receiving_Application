# Scanner Feature Design Pack

Last Updated: 2026-07-21

This folder expands the approved memorandum in specs/ScannerFeature/MainPM.md into implementation-ready documentation artifacts for MTM.

## Purpose

These documents define the required models, services, view-models, views, and connected shared services needed to deliver fast batch item submission while preserving MTM architecture constraints.

## Scope And Guardrails

- Keep MVVM flow: View -> ViewModel -> Service -> DAO -> Database.
- MySQL 5.7 writes through stored procedures and DAO result models.
- No raw MySQL DML in C# for this feature.
- Infor Visual SQL Server stays read-only.
- Win32 and UI Automation behind service contracts.
- Keep code-behind free of business logic.

## UI Mockups

- All UI elements must match the approved mockups as closely as possible.
- All UI elements must support strict resize compatibility using both vertical and horizontal stretch behavior.

Mockup Files:

- docs/features/receiving/scanner-feature/mockups/01-scanner-workbench.png
- docs/features/receiving/scanner-feature/mockups/02-scanner-history.png
- docs/features/receiving/scanner-feature/mockups/03-scanner-settings.png
- docs/features/receiving/scanner-feature/mockups/04-modal-manage-batch-rows.png
- docs/features/receiving/scanner-feature/mockups/05-modal-save-draft.png
- docs/features/receiving/scanner-feature/mockups/06-modal-load-draft.png
- docs/features/receiving/scanner-feature/mockups/07-modal-configure-hotkeys.png

## Document Map

- 01-model-batch-session.md
- 02-model-batch-item.md
- 03-model-automation-profile.md
- 04-service-batch-orchestration.md
- 05-service-native-input-engine.md
- 06-service-persistence-and-history.md
- 07-viewmodel-scanner-workbench.md
- 08-viewmodel-scanner-history.md
- 09-viewmodel-scanner-settings.md
- 10-view-scanner-workbench.md
- 11-view-scanner-history.md
- 12-view-scanner-settings.md
- 13-connected-services-module-core.md
- 14-connected-services-module-shared.md

## Delivery Intent

Each file is written as a direct implementation handoff with:

- required responsibilities
- explicit data content
- state transitions and validation rules
- integration touchpoints
- use-versus-modify guidance for existing services

## Recommended Implementation Order

1. Finalize `Module_Scanner` contracts, models, and service interfaces for batch session, item validation, automation profile, and run history flows, and create corresponding tests in `MTM_Receiving_Application.Tests`.
   - contract tests for request/response and DTO boundary expectations
   - model mapping tests for scanner state and persistence shape conversions
   - interface wiring tests to confirm DI resolution and expected service registrations
2. Implement DAO layer in `Module_Scanner/Data` for MySQL stored-procedure access only, returning `Model_Dao_Result` patterns for expected failures.
3. Implement scanner services in `Module_Scanner/Services` (batch orchestration, native input automation, persistence/history, validation) using the DAO layer and existing Module_Core helper abstractions.
4. Implement and wire ViewModel behavior in `Module_Scanner/ViewModels`, including command flow, state transitions, and non-blocking add-line validation status/notes updates.
5. Implement scanner views in `Module_Scanner/Views` to match approved mockups, preserving x:Bind usage and resize/stretch behavior.
6. Integrate shell navigation and dependency injection registrations (already scaffolded) and then validate route activation and bottom navigation commands across workbench, history, and settings pages.
7. Add and run tests in `MTM_Receiving_Application.Tests`:
   - unit tests for ViewModels and interface-only services
   - integration tests for handlers/services/DAOs that hit real persistence boundaries
   - validation tests for add-line fuzzy checks and source-quantity sufficiency result mapping to Status/Notes
8. Validate end-to-end scanner workflow behavior manually against the target ERP window profile (`VMINVENT.exe` + `Inventory Transfers`) and update documentation only where behavior differs from the design pack.

## Workflow Gaps Resolved In Mockups

- The workbench now requires a clear `Manage Items` entry point rather than implying inline row creation from the main grid alone.
- History views must show full from/to warehouse and location traceability for retry and support workflows.
- Manage-items guidance must make it clear that warehouse values come from settings while item-level location and quantity remain the active editable values.
- Save and send workflows must keep manual ERP validation and save behavior visible; no auto-finalize or `Alt+S` workflow is implied.
- New-item validation must check Infor Visual using existing fuzzy-check patterns for part and location matching, plus source on-hand quantity validation, and surface the result through Status and Notes instead of blocking the add.
- Scanner feature ownership is expected to live in `Module_Scanner`, not `Module_Receiving`.

## Current Scaffold Status

- `Module_Scanner` navigation skeleton is in place for workbench, history, and settings pages.
- Bottom-page navigation command bindings are now aligned with generated RelayCommand properties across all three scanner page ViewModels.
- Step 1 (contracts/models/service-interface baseline) is implemented with scanner-focused unit tests in `MTM_Receiving_Application.Tests/Unit/Module_Scanner`.
- Step 2 (DAO layer) is implemented in `Module_Scanner/Data` using MySQL stored procedures only and `Model_Dao_Result` failure patterns, with DAO validation/mapping unit tests in `MTM_Receiving_Application.Tests/Unit/Module_Scanner/Data`.
- Scanner pages remain intentionally under-construction placeholders until service and workflow implementation is completed.
