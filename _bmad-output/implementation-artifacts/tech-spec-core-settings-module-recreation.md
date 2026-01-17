---
title: 'Core Settings Module Recreation'
slug: 'core-settings-module-recreation'
created: '2026-01-17'
status: 'ready-for-dev'
stepsCompleted: [1, 2, 3, 4]
tech_stack:
  - 'WinUI 3 (.NET 8)'
  - 'C# 12'
  - 'MVVM (CommunityToolkit.Mvvm)'
  - 'MediatR (CQRS)'
  - 'FluentValidation'
  - 'Serilog'
  - 'MySQL stored procedures'
files_to_modify:
  - 'MainWindow.xaml'
  - 'MainWindow.xaml.cs'
  - 'App.xaml.cs'
  - 'Module_Settings/ViewModels/ViewModel_Settings_DatabaseTest.cs'
  - 'Module_Settings/Views/View_Settings_DatabaseTest.xaml'
  - 'Module_Settings/docs/templates/MOCK_FILE_STRUCTURE.md'
code_patterns:
  - 'ViewModel -> IMediator (no direct DAO)'
  - 'Instance-based DAOs returning Model_Dao_Result'
  - 'x:Bind in all XAML'
  - 'Settings Window with its own NavigationView'
  - 'WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(1400, 900) for new windows'
test_patterns:
  - 'xUnit + FluentAssertions'
  - 'Validator unit tests (FluentValidation)'
  - 'Handler tests: unit if interface-only, integration if DAO concrete'
  - 'DAO integration tests against MySQL'
---

# Tech-Spec: Core Settings Module Recreation

**Created:** 2026-01-17

## Overview

### Problem Statement

The existing Module_Settings was created before the app became fully modular. It must be replaced with a new Core Settings Module that provides shared storage, validation, audit logging, caching, and developer tooling for settings that affect multiple modules. Feature modules must own their own settings files and UI and only rely on Core Settings for read/write access.

### Solution

Rebuild Module_Settings as a core infrastructure module using CQRS (MediatR), FluentValidation, Serilog, and instance-based DAOs backed by MySQL stored procedures. Provide a dedicated Settings Window with its own navigation hub for global settings (system/user, roles/privileges, core UI, DB/logging, shared file locations) and developer tooling. Remove all legacy settings UI and integration points, achieve a green build, then implement the new system.

### Scope

**In Scope:**

- Core/global settings affecting multiple modules (system settings, user settings, roles/privileges, core UI theme, core DB settings, core logging settings, shared file location settings)
- Settings metadata, validation, auditing, caching
- Core Settings Window (separate from MainWindow navigation)
- Developer DB test tooling in core settings
- Documentation for feature-module settings creation (templates + guide)

**Out of Scope:**

- Any feature-module settings UI or ViewModels
- Feature-module business settings (receiving/dunnage/routing/volvo/reporting)
- Application shell navigation changes beyond launching the Settings Window
- Data migration of old settings (delete old files, no migration)
- Reporting execution or sync workflows

## Context for Development

### Codebase Patterns

- Strict MVVM (ViewModel -> Service/Mediator -> DAO -> DB)
- Instance-based DAOs returning Model_Dao_Result (no exceptions)
- Stored procedures only for MySQL
- x:Bind for all XAML
- ViewModels are partial and use CommunityToolkit.Mvvm
- ViewModels typically inherit ViewModel_Shared_Base and use IService_ErrorHandler + IService_LoggingUtility
- DI registrations are centralized in App.xaml.cs
- New windows should call WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(1400, 900)

### Codebase Scan Findings

- Main settings entry point is the NavigationView Settings button in MainWindow; current handler has a TODO to navigate settings.
- Main window currently navigates to Module_Settings.Views.View_Settings_DatabaseTest for the DB test tool.
- App.xaml.cs registers legacy settings views (View_Settings_Workflow, View_Settings_ModeSelection, View_Settings_DunnageMode, View_Settings_Placeholder) but they are commented out.
- ViewModel_Settings_DatabaseTest uses instance-based DAOs directly (legacy pattern) and inherits ViewModel_Shared_Base.
- No project-context.md found in repo.
- No other code references to View_Settings_* were found outside MainWindow/App.xaml.cs (other mentions are in docs/_bmad memory files).

## PlantUML WBS (Required Structure) - Required File Structure

```plantuml
@startwbs
* Module_Settings
** Module_Settings.Core
*** Data
*** Enums
*** Interfaces
*** Models
*** Services
*** ViewModels
*** Views
*** docs
**** templates
** Module_Settings.DeveloperTools
*** Data
*** Enums
*** Interfaces
*** Models
*** Services
*** ViewModels
*** Views
*** docs
**** templates
** Module_Settings.{FeatureName}
*** Data
*** Enums
*** Interfaces
*** Models
*** Services
*** ViewModels
*** Views
*** docs
**** templates
@endwbs
```

### Files to Reference

| File | Purpose |
| ---- | ------- |
| specs/005-settings-system-redesign/SPECIFICATION.md | Source for settings schema, RBAC, audit/logging patterns to adapt for core/global scope |
| docs/Module_Prep/MASTER_PROMPT.md | Module rebuild guidance and documentation requirements |
| AGENTS.md | Project constraints and patterns (window sizing, MVVM rules) |
| docs/source-tree-analysis.md | Existing module layout references for Module_Settings |
| Module_Volvo/docs/copilot/SETTABLE_OBJECTS.md | Reference pattern for settings inventory documentation (to generalize) |
| MainWindow.xaml | NavigationView with Settings button entry point |
| MainWindow.xaml.cs | Settings selection handler and DB test navigation |
| App.xaml.cs | DI registration pattern and legacy settings view registrations |
| Module_Settings/ViewModels/ViewModel_Settings_DatabaseTest.cs | Existing DB test view pattern (to recreate in DeveloperTools) |
| Module_Settings/Views/View_Settings_DatabaseTest.xaml | Existing DB test view UI (to recreate in DeveloperTools) |
| Module_Settings/docs/templates/MOCK_FILE_STRUCTURE.md | Required module layout template |

### Technical Decisions

- Core Settings UI is a separate Window with its own NavigationView and ViewModel (no reuse of MainWindow context control)
- Settings Window launches from the MainWindow NavigationView Settings button (IsSettingsSelected)
- Delete all legacy settings UI/files; green build before implementing new system
- Any existing module settings integration with old Module_Settings should be commented/TODOed for later refactor
- Core exposes a thin service facade; MediatR and FluentValidation remain internal to Module_Settings
- Feature modules must maintain their own settings inventory using a template stored in Module_Settings/docs/templates
- Core vs Feature boundary is enforced: Core only hosts global settings and tooling; feature modules own their settings UI and domain keys
- Settings Window UX must be discoverable but isolated; use standard WinUI 3 styling and no shared navigation context
- Settings Window must set size via WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(1400, 900)
- No new dependencies beyond MediatR, FluentValidation, and Serilog unless explicitly approved
- Metadata registry is code-first primary with optional manifest fallback; DB-only metadata is not preferred
- File structure must follow the Module_Settings.Core + Module_Settings.DeveloperTools + Module_Settings.{FeatureName} pattern, each with the same subfolder schema (Data/Models/Services/ViewModels/Views/Enums/Interfaces)
- DB Test tool lives in Module_Settings.DeveloperTools
- Manifest fallback behavior: on missing setting, write manifest default to DB and use it for session; on corrupted value, prompt user (setting name + expected type + returned type), reset to manifest default in DB, and use manifest value for session

### Architecture Decision Records (ADRs)

- **ADR-001:** Core Settings uses a separate Settings Window with its own NavigationView and ViewModel.
- **ADR-002:** No migration. Delete legacy settings files, achieve green build, then implement new settings system.
- **ADR-003:** Code-first metadata registry for settings definitions (feature modules register keys/defaults/types).
- **ADR-004:** Core UI scope is limited to global categories only (system/user/roles/theme/db/logging/shared paths).
- **ADR-005:** Settable Objects Inventory template lives in Module_Settings/docs/templates and is mandatory for feature modules.
- **ADR-006:** Manifest fallback behavior is permitted to self-heal missing/corrupt settings at runtime.

## Implementation Plan

### Tasks

- [ ] Task 1: Purge legacy Module_Settings and reach green build
  - File: Module_Settings/**
  - Action: Delete legacy Settings Views/ViewModels/Services/Interfaces/Enums/Data/Models (old workflow, mode selection, dunnage mode, placeholder, old DAOs/models). Keep only new Module_Settings.Core/DeveloperTools structure to be rebuilt.
  - Notes: Comment/TODO any feature-module references to old settings APIs.
- [ ] Task 2: Update MainWindow settings entry point
  - File: MainWindow.xaml.cs
  - Action: When `IsSettingsSelected`, launch the separate Core Settings Window (single-instance behavior). Remove any in-frame settings navigation.
  - Notes: Use `WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(1400, 900)` for the Settings Window.
- [ ] Task 3: DI cleanup and new registrations
  - File: App.xaml.cs
  - Action: Remove legacy Module_Settings registrations. Register Core Settings Window, Core Settings ViewModels/Services/DAOs, and DeveloperTools DB Test View/ViewModel.
  - Notes: No DI structure changes beyond new registrations.
- [ ] Task 4: Establish Module_Settings.Core structure and metadata registry
  - File: Module_Settings.Core/**
  - Action: Create Core module folders (Data/Enums/Interfaces/Models/Services/ViewModels/Views/docs/templates) and implement code-first settings metadata registry.
  - Notes: Define manifest defaults file location and format for fallback (module-local, versioned).
- [ ] Task 5: Implement Core settings data contracts and DAOs
  - File: Module_Settings.Core/Models/*, Module_Settings.Core/Data/*
  - Action: Create models for system/user settings, metadata, audit log; implement instance-based DAOs using stored procedures only.
  - Notes: DAOs return Model_Dao_Result and never throw.
- [ ] Task 6: Implement CQRS handlers, validators, and pipeline behaviors
  - File: Module_Settings.Core/Services/**, Module_Settings.Core/Validators/**
  - Action: Add MediatR handlers for get/set/reset, audit behavior, validation behavior, and safe logging (no secrets).
  - Notes: Enforce permission checks in handlers.
- [ ] Task 7: Build Core Settings Window and pages
  - File: Module_Settings.Core/Views/**, Module_Settings.Core/ViewModels/**
  - Action: Create Settings Window shell with NavigationView and global pages (System, Users/Privileges, UI Theme, Database, Logging, Shared Paths).
  - Notes: Use x:Bind, no code-behind logic beyond wiring; isolated from MainWindow nav context.
- [ ] Task 8: Recreate DeveloperTools DB Test tool
  - File: Module_Settings.DeveloperTools/Views/**, Module_Settings.DeveloperTools/ViewModels/**
  - Action: Re-implement DB test workflow and redesign UI from scratch using the existing DB test as reference.
  - Notes: New namespace/module; do not relocate old files.
- [ ] Task 9: Manifest fallback flow
  - File: Module_Settings.Core/Services/**
  - Action: Implement startup setting resolution: DB → manifest default on missing → prompt and reset on corrupted value.
  - Notes: Prompt must include setting name, intended type, returned type.
- [ ] Task 10: Documentation and templates
  - File: Module_Settings/docs/templates/**
  - Action: Add Feature Settings Implementation Guide, Settable Objects Inventory template, and mock file structure.
  - Notes: Must match Module_Settings.Core + Module_Settings.DeveloperTools + Module_Settings.{FeatureName} layout.
- [ ] Task 11: Tests
  - File: MTM_Receiving_Application.Tests/**
  - Action: Add unit tests for validators/handlers and integration tests for DAOs.
  - Notes: Follow testing-strategy.instructions.md decision tree.

### Acceptance Criteria

- [ ] AC 1: Given legacy Module_Settings files are removed, when the app builds, then the solution compiles and runs (green state).
- [ ] AC 2: Given a user clicks the Settings button in MainWindow, when invoked, then a separate Core Settings Window opens (single instance) with NavigationView.
- [ ] AC 3: Given a core setting is missing in DB, when the app loads, then the manifest default is written to DB and used for the session.
- [ ] AC 4: Given a core setting value is corrupted, when the app loads, then the user is prompted with setting name + expected type + returned type, and the setting resets to manifest default.
- [ ] AC 5: Given valid permissions, when a setting is changed, then validation runs, audit logs are recorded, and cache is invalidated.
- [ ] AC 6: Given the DB Test tool is accessed (debug menu), when opened, then the DeveloperTools DB Test view is displayed (new implementation).
- [ ] AC 7: Given feature modules need settings UI, when implemented, then they follow Module_Settings.{FeatureName} structure and use the inventory template.
- [ ] AC 8: Given new docs are required, when the spec is complete, then templates exist under Module_Settings/docs/templates.

## Additional Context

### Dependencies

- MediatR, FluentValidation, Serilog (Module_Settings internal; no new packages unless approved)
- MySQL stored procedures (core settings + audit)
- Manifest defaults file (module-local, versioned)

### Testing Strategy

- Unit tests for validators and handlers that use interface-only dependencies
- Integration tests for handlers/DAOs that use concrete DAOs or DB access
- Manual tests: Settings button opens separate window; missing/corrupt setting fallback flow; DB Test tool launches in debug builds
- Follow testing strategy instructions for classification and naming

### Notes

- Core/global categories must be explicitly listed and frozen in the spec
- No migration of existing settings; removal precedes rebuild
- Settings inventory template should mirror the SETTABLE_OBJECTS pattern in a generic, module-agnostic form
- If any potentially module-specific setting is discovered, confirm with the user before designating it as out of scope
- Purpose first principle: Core Settings exists for global safety and consistency; anything module-specific requires user confirmation before exclusion
- Lessons learned from prior redesign: keep RBAC/lock-unlock/audit/validation; avoid multi-module UI inside Core Settings
- Startup resolution flow: App loads → get setting from DB → if missing, write manifest default to DB and use for session → if corrupted, prompt user with setting name + expected type + returned type, reset to manifest default in DB, use manifest value for session
- Definition of "recreate": capture the existing workflow intent, then redesign the UI and re-implement from scratch (no direct relocation).
- Single-instance behavior for Settings Window should focus existing window if already open.

### Pre-mortem Risks & Mitigations

- **Risk:** Scope creep turns Core Settings into a platform.
  - **Mitigation:** Hard boundary section + acceptance gates for global-only categories.
- **Risk:** App fails to build after deleting legacy settings.
  - **Mitigation:** Comment/TODO any legacy integration points and verify green build before rebuild.
- **Risk:** Settings Window becomes a second app shell.
  - **Mitigation:** Isolated window, single-instance behavior, limited navigation scope.
- **Risk:** Feature module settings drift without consistent documentation.
  - **Mitigation:** Enforce Settable Objects Inventory template + feature settings guide.

### Failure Modes & Handling

- **Missing setting key:** Return failure result and log; do not crash UI.
- **Validation failures:** Block write, show inline error; no silent fallback.
- **Sensitive value exposure:** Mask in UI; use change-password dialog for secrets.
- **Cache staleness:** Invalidate on write; avoid stale overrides.
- **Privilege escalation risk:** Enforce permission checks in handlers; audit every write; never log raw secrets.
