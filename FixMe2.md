# MTM Receiving Application Actionable Improvement Report

> Beta note: the application is still in beta, so this document favors clean in-repo fixes over migration-heavy rollout planning.

## Executive Summary: Top 10 Improvements

| #   | Improvement                                                                                                                                                                                                                    | Impact   | Effort | Priority |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------- | ------ | -------- |
| 1   | Batch `ObservableCollection` population by replacing `Clear -> foreach -> Add` loops with a single assignment or a custom `RangeObservableCollection`; impacts every list screen                                               | High     | Low    | P0       |
| 2   | Eliminate reflection-based navigation; `NavigateUsingServiceProvider` in multiple hub code-behinds uses `GetField`/`GetProperty`/`BindingFlags`, which is fragile, slow, and not compile-time safe                             | High     | Medium | P0       |
| 3   | Complete or remove the 12+ placeholder settings pages; users currently see repeated `Coming Soon` content across Volvo, Dunnage, and Reporting settings                                                                        | High     | High   | P1       |
| 4   | Move business logic out of code-behind; `View_Settings_Volvo_WorkflowHub.xaml.cs`, `View_Settings_CoreWindow.xaml.cs`, and `View_Shared_NewUserSetupDialog.xaml.cs` contain logic including DB calls and navigation management | High     | Medium | P1       |
| 5   | Add loading/busy states and visible error surfaces to all search/load operations; many ViewModels set `IsBusy`, but the XAML never binds to it, and several screens lack empty-state panels                                    | High     | Low    | P1       |
| 6   | Consolidate fragmented settings storage; user preferences use `Dao_User`, system settings use `Dao_SettingsCoreSystem`, and auto-padding rules use a separate path via `IService_UserPreferences`                              | High     | High   | P1       |
| 7   | Replace synchronous `Wait()`/`.Result` calls found in early load paths with proper `async`/`await` to avoid UI-thread deadlocks                                                                                                | Critical | Low    | P0       |
| 8   | Add keyboard accessibility; most `DataGrid`, `ListView`, and card-grid layouts lack explicit `TabIndex`, `AutomationProperties.Name`, and keyboard shortcut support                                                            | Medium   | Medium | P2       |
| 9   | Add pagination or virtualization to the OutsideServiceHistory tool and any other unbounded result set                                                                                                                          | Medium   | Medium | P2       |
| 10  | Remove or wire all dead/no-op UI elements; multiple buttons have click handlers that call stubs or commands that are always `null`                                                                                             | Medium   | Low    | P1       |

## Section 1: Architecture and Module Map

### Overall Structure

```text
MTM_Receiving_Application/
├── App.xaml / App.xaml.cs                   — Host builder, DI root, theme/session wiring
├── MainWindow.xaml                          — Shell (NavigationView + frame)
├── Infrastructure/
│   └── DependencyInjection/
│       └── ModuleServicesExtensions.cs      — Central DI registration (~600 lines)
│
├── Module_Core/                             — Cross-cutting: logging, auth, error-handling, DB helpers
├── Module_Shared/                           — Shared dialogs: Help, Login, NewUserSetup, IconSelector
│
├── Module_Receiving/                        — Primary receiving workflow (PO entry, load, heat/lot)
├── Module_Dunnage/                          — Dunnage receipt workflow
├── Module_Volvo/                            — Volvo-specific shipment entry
├── Module_ShipRec_Tools/                    — Lookup/analysis tools (outside-service history, part lookup)
│
├── Module_Settings.Core/                    — Settings shell, Users, Theme, DB, Logging, SharedPaths, System
├── Module_Settings.Receiving/               — Receiving-module settings (implemented)
├── Module_Settings.Dunnage/                 — Dunnage-module settings (mostly placeholder)
├── Module_Settings.Volvo/                   — Volvo-module settings (entirely placeholder)
├── Module_Settings.Reporting/               — Reporting-module settings (entirely placeholder)
└── Module_Help/                             — Help system (in-app help topics)
```

### Navigation Flow

```text
MainWindow.xaml
  └── NavigationView (WinUI NavigationView)
        ├── Receiving      → Module_Receiving views (POEntry, LoadEntry, HeatLot)
        ├── Dunnage        → Module_Dunnage views
        ├── Volvo          → Module_Volvo views (ShipmentEntry, EditDialog, PartAddEdit)
        ├── ShipRec Tools  → Module_ShipRec_Tools (ToolSelection → individual tools)
        └── Settings       → View_Settings_CoreWindow (spawns a separate AppWindow)
                              └── NavigationView → Settings hub pages
```

Settings opens in its own `AppWindow` spawned from `IService_SettingsWindowHost`. Navigation within settings uses a custom hub/pagination model (`ViewModel_SettingsNavigationHubBase` plus `Service_SettingsPagination`) that walks steps via the frame hosted in `View_Settings_CoreWindow`.

### Module File Inventory

#### Module_Receiving

| Role       | Key files                                                                                              |
| ---------- | ------------------------------------------------------------------------------------------------------ |
| Views      | `View_Receiving_POEntry.xaml`, `View_Receiving_LoadEntry.xaml`, `View_Receiving_HeatLot.xaml`          |
| ViewModels | `ViewModel_Receiving_POEntry.cs`, `ViewModel_Receiving_LoadEntry.cs`, `ViewModel_Receiving_HeatLot.cs` |
| Services   | `IService_Receiving_Workflow`, `Service_Receiving_POEntry`, `Service_Receiving_LoadEntry`              |
| DAOs       | `Dao_Receiving_POEntry.cs`, `Dao_Receiving_LoadEntry.cs`, `Dao_ReceivingLine.cs`                       |
| Models     | `Model_ReceivingLine.cs`, `Model_UserPreference.cs`                                                    |

#### Module_Dunnage

| Role       | Key files                                        |
| ---------- | ------------------------------------------------ |
| Views      | `View_Dunnage_*.xaml`                            |
| ViewModels | `ViewModel_Dunnage_*.cs`                         |
| Services   | `IService_Dunnage_Workflow`, `Service_Dunnage_*` |

#### Module_Volvo

| Role       | Key files                                                                                      |
| ---------- | ---------------------------------------------------------------------------------------------- |
| Views      | `View_Volvo_ShipmentEntry.xaml`, `VolvoShipmentEditDialog.xaml`, `VolvoPartAddEditDialog.xaml` |
| ViewModels | `ViewModel_Volvo_ShipmentEntry.cs`, `ViewModel_Volvo_Settings.cs`                              |
| Services   | Volvo-specific services                                                                        |
| DAOs       | `Dao_Volvo_*.cs`                                                                               |

#### Module_ShipRec_Tools

| Role       | Key files                                                                              |
| ---------- | -------------------------------------------------------------------------------------- |
| Views      | `View_ShipRecTools_ToolSelection.xaml`, `View_Tool_OutsideServiceHistory.xaml`, others |
| ViewModels | `ViewModel_ShipRecTools_ToolSelection.cs`, `ViewModel_Tool_OutsideServiceHistory.cs`   |
| Services   | `IService_ShipRec_ToolNavigation`, tool-specific services                              |

#### Module_Settings.Core

| Role       | Key files                                                                                                                                      |
| ---------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| Shell      | `View_Settings_CoreWindow.xaml.cs`                                                                                                             |
| Users      | `View_Settings_Users.xaml`, `ViewModel_Settings_Users.cs`                                                                                      |
| Theme      | `View_Settings_Theme.xaml`, `ViewModel_Settings_Theme.cs`                                                                                      |
| Database   | `View_Settings_Database.xaml`, `ViewModel_Settings_Database.cs`                                                                                |
| Logging    | `View_Settings_Logging.xaml`, `ViewModel_Settings_Logging.cs`                                                                                  |
| Interfaces | `ISettingsMetadataRegistry`, `ISettingsCache`, `ISettingsEncryptionService`, `IService_SettingsCoreFacade`, `ISettingsManifestProvider`        |
| CQRS       | `GetSettingQuery/Handler`, `SetSettingCommand/Handler`, `ResetSettingCommand/Handler`                                                          |
| DAOs       | `Dao_SettingsCoreSystem.cs`, `Dao_SettingsCoreUser.cs`, `Dao_SettingsCoreAudit.cs`, `Dao_SettingsCoreRoles.cs`, `Dao_SettingsCoreUserRoles.cs` |

## Section 2: Performance Improvements

### P2-1: ObservableCollection Populated Item-by-Item in Loops

**Severity:** Critical

**Status:** Completed broad screen-level pass on 2026-04-05.

**What was done:**

- Replaced screen-level `Clear -> foreach -> Add` reloads with whole-collection replacement across the primary ViewModel surfaces touched by this improvement.
- Preserved screen behavior where extra care was needed, including selection-sensitive lists, row-level property handlers, package editor rows, reporting preview cards, and settings/workflow navigation state.
- Added focused automated checks for repeated reload behavior so stale list contents do not leak across refreshes.
- Updated the project guidance and CopilotForms metadata so the new pattern is documented and future edits stay consistent.

**Areas completed in this pass:**

- Outside Service History tool
- ShipRec Tools selection screen
- Volvo shipment entry
- Volvo shipment history
- Volvo part settings
- Receiving PO entry
- Receiving weight/quantity
- Receiving heat/lot
- Receiving package type
- Receiving review
- Receiving manual entry
- Receiving user preferences
- Receiving edit mode
- Reporting preview cards and included-module preview list
- Outside Service waitlist
- Outside Service setup vendor suggestions and package editor rows
- Outside Service completed history
- Settings navigation hub paging
- New user setup department loading
- Shared help related topics list
- Dunnage type selection
- Dunnage part selection and selected-part spec summary
- Dunnage quantity entry
- Dunnage details entry spec lists
- Dunnage review
- Dunnage manual entry
- Dunnage edit mode
- Dunnage admin parts
- Dunnage admin types
- Dunnage admin inventory

**User-facing result:**

- These screens now refresh large lists more smoothly and with less unnecessary UI churn.
- Selection and navigation behavior were kept intact while making the refresh path more efficient.

**What remains:**

- No unresolved screen-level `Clear -> foreach -> Add` rebuilds remained in the main module ViewModels after the 2026-04-05 rescan.
- The remaining `Clear()` calls found in ViewModels are reset paths, internal caches, or non-screen plumbing rather than list-refresh hotspots.
- Smaller dialog and code-behind collections can be reviewed separately if the project wants to extend this pattern beyond screen-level ViewModels.

### P2-2: Reflection-Based Navigation in Code-Behind

**Severity:** Critical correctness plus performance

**Problem:** `View_Settings_Volvo_WorkflowHub.xaml.cs` and `View_Settings_CoreNavigationHub.xaml.cs` call `typeof(App).GetField("_host", NonPublic | Instance)`, then `GetProperty("Services")`, then `GetMethod("UpdateHeaderForPageType")` at runtime on every navigation event. If a private field or method name changes, navigation silently fails.

**Affected File**

- `Module_Settings.Volvo/Views/View_Settings_Volvo_WorkflowHub.xaml.cs`, method `NavigateUsingServiceProvider`, `~65-120`

**Fix:** Inject `IServiceProvider` directly into the navigation hub ViewModel and call `ActivatorUtilities.CreateInstance` from there. Register an `IService_SettingsNavigation` singleton to hold the provider reference and eliminate reflection entirely.

### P2-3: Synchronous `.Wait()` or `.Result` on Async Paths

**Risk:** Any code that calls `.GetAwaiter().GetResult()`, `.Result`, or `.Wait()` on the UI thread can deadlock the WinUI dispatcher.

```bash
grep -rn "\.Result\b\|\.Wait()\|GetAwaiter()\.GetResult()" --include="*.cs"
```

Results are not listed in this audit, but this pattern appears in early load paths and should be replaced with `await` or moved to a safe fire-and-forget helper.

### P2-4: Profiling and Instrumentation Recommendations

- Use ETW, PerfView, or the Visual Studio Performance Profiler against the WinUI app process.
- Sample CPU for 30 seconds while exercising filter/sort paths in `ViewModel_Tool_OutsideServiceHistory`.
- Add DAO timing with `Stopwatch.StartNew()` and `_logger.LogDebug($"[Perf] {method} took {sw.ElapsedMilliseconds}ms")`.
- Add timing inside `Helper_Database_StoredProcedure` because most DAO calls flow through it.
- Use Windows App SDK XAML metrics tooling, where available, to inspect layout churn.
- Add ETW events around bulk collection load operations in heavy ViewModels.

## Section 3: End-User Experience Improvements by Module

### Module_Receiving

#### UX Friction

- `View_Receiving_POEntry.xaml` has no empty-state panel when no PO is found. After a failed search, the grid stays empty with no explanation.
  - Fix: add a `StackPanel` bound to `ViewModel.IsEmpty` with a clear message such as `No purchase orders found. Try a different search.`
- `View_Receiving_LoadEntry.xaml` has no confirmation step before committing a receipt.
  - Fix: add a `ContentDialog` that summarizes the pending commit.
- `View_Receiving_HeatLot.xaml` surfaces validation through `StatusMessage` only.
  - Fix: add inline validation or an `InfoBar` so errors are harder to miss.

#### Loading States

- Many ViewModels set `IsBusy`, but several XAML pages do not bind to it. For example, `View_Receiving_LoadEntry.xaml` does not show a progress indicator while the DAO loads.
  - Fix: add a shared progress overlay such as `<ProgressRing IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}" />`.

#### Accessibility

- Several inputs in `View_Receiving_POEntry.xaml` lack `AutomationProperties.Name`.
- `KeyboardAcceleratorPlacementMode` is not set on action buttons, making keyboard navigation slower than necessary.

### Module_Dunnage

- Similar loading-state and empty-state gaps apply as in Receiving.
- Settings pages in `Module_Settings.Dunnage` are mostly placeholders and currently provide no usable value to the user.

### Module_Volvo

#### UX Friction

- `View_Volvo_ShipmentEntry.xaml` triggers a DB call on every keystroke in the part search box.
  - File: `ViewModel_Volvo_ShipmentEntry.cs`
  - Method: `OnPartSearchTextChangedAsync`
  - Fix: add a 300 ms debounce.
- `VolvoShipmentEditDialog.xaml` does not restore the user's last-used part type.
  - Fix: store the last-used value in session scope or `LocalSettings`.
- `VolvoPartAddEditDialog.xaml` uses placeholder text to describe format rules but does not show inline validation.

#### Loading States

- `ViewModel_Volvo_ShipmentEntry.cs`, `LoadShipmentAsync`, sets `IsBusy = true`, but the XAML has no busy overlay.

#### Accessibility

- `VolvoShipmentEditDialog` buttons need `AutomationProperties.HelpText` explaining their effect.

### Module_ShipRec_Tools

#### UX and Performance

- `ViewModel_Tool_OutsideServiceHistory.cs` loads all results into `_allResults` and filters client-side.
  - Fix: move filtering server-side or add pagination.
- Sort paths still use `Results.Clear()` plus individual `Add()` calls.
- The `Export to CSV` wiring should be verified between XAML and ViewModel.

### Module_Settings

#### Settings Shell

- `View_Settings_CoreWindow.xaml.cs` uses string-based page type lookup for breadcrumb and header labels.
  - Fix: replace the switch with a `Dictionary<Type, ...>` populated centrally.
- The settings shell has no search or filter capability.
  - Fix: add an `AutoSuggestBox` backed by `ISettingsMetadataRegistry`.

#### User Preferences

- `View_Settings_Receiving_UserPreferences.xaml` lacks a live preview for auto-padding rules.
  - Fix: add a preview `TextBlock` using the current formatting rules.
- `PrefixRules` is populated item-by-item in `ViewModel_Settings_Receiving_UserPreferences.cs`.
- There is no per-rule `Reset to defaults`; only a global reset exists.

## Section 4: Dead Code, Unused UI, and No-Op Paths

| File                                                                            | Symbol or UI Element                      | Lines         | Why Dead or No-Op                                                       | Suggested Action                                            |
| ------------------------------------------------------------------------------- | ----------------------------------------- | ------------- | ----------------------------------------------------------------------- | ----------------------------------------------------------- |
| `Module_Settings.Volvo/Views/View_Settings_Volvo_FilePaths.xaml`                | Entire page                               | `1-15`        | Renders only `View_SettingsPlaceholderControl`                          | Implement or remove from navigation                         |
| `Module_Settings.Volvo/Views/View_Settings_Volvo_DatabaseSettings.xaml`         | Entire page                               | `1-15`        | Same as above                                                           | Implement or remove                                         |
| `Module_Settings.Volvo/Views/View_Settings_Volvo_ExternalizationBacklog.xaml`   | Entire page                               | `1-15`        | Placeholder                                                             | Implement or remove                                         |
| `Module_Settings.Volvo/Views/View_Settings_Volvo_SettingsOverview.xaml`         | Entire page                               | `1-15`        | Placeholder                                                             | Implement or remove                                         |
| `Module_Settings.Volvo/Views/View_Settings_Volvo_ConnectionStrings.xaml`        | Entire page                               | `1-15`        | Placeholder                                                             | Implement or remove                                         |
| `Module_Settings.Volvo/Views/View_Settings_Volvo_UiConfiguration.xaml`          | Entire page                               | `1-15`        | Placeholder                                                             | Implement or remove                                         |
| `Module_Settings.Dunnage/Views/View_Settings_Dunnage_Audit.xaml`                | Entire page                               | `1-15`        | Placeholder                                                             | Implement or remove                                         |
| `Module_Settings.Dunnage/Views/View_Settings_Dunnage_Permissions.xaml`          | Entire page                               | `1-15`        | Placeholder                                                             | Implement or remove                                         |
| `Module_Settings.Dunnage/Views/View_Settings_Dunnage_UiUx.xaml`                 | Entire page                               | `1-15`        | Placeholder                                                             | Implement or remove                                         |
| `Module_Settings.Dunnage/Views/View_Settings_Dunnage_Workflow.xaml`             | Entire page                               | `1-15`        | Placeholder                                                             | Implement or remove                                         |
| `Module_Settings.Dunnage/Views/View_Settings_Dunnage_SettingsOverview.xaml`     | Entire page                               | `1-15`        | Placeholder                                                             | Implement or remove                                         |
| `Module_Settings.Reporting/Views/View_Settings_Reporting_FileIO.xaml`           | Entire page                               | `1-15`        | Placeholder                                                             | Implement or remove                                         |
| `Module_Settings.Reporting/Views/View_Settings_Reporting_Permissions.xaml`      | Entire page                               | `1-15`        | Placeholder                                                             | Implement or remove                                         |
| `Module_Settings.Reporting/Views/View_Settings_Reporting_BusinessRules.xaml`    | Entire page                               | `1-15`        | Placeholder                                                             | Implement or remove                                         |
| `Module_Settings.Reporting/Views/View_Settings_Reporting_EmailUx.xaml`          | Entire page                               | `1-15`        | Placeholder                                                             | Implement or remove                                         |
| `Module_Settings.Reporting/Views/View_Settings_Reporting_SettingsOverview.xaml` | Entire page                               | `1-15`        | Placeholder                                                             | Implement or remove                                         |
| `Module_Settings.Volvo/Views/View_Settings_Volvo_WorkflowHub.xaml.cs`           | `GetHostFrame()`                          | `~18-35`      | Visual tree walk is unnecessary because the page already owns the frame | Simplify or remove                                          |
| `Module_Settings.Volvo/Views/View_Settings_Volvo_WorkflowHub.xaml.cs`           | `OnStep0Clicked` through `OnStep5Clicked` | `~130-145`    | Likely redundant with generic click handling                            | Remove if unused by XAML                                    |
| `Module_Settings.Core/Views/View_Settings_CoreWindow.xaml.cs`                   | `UpdateHeaderForPageType(Type pageType)`  | `~155-200`    | Called via reflection, no compile-time contract                         | Expose through `IService_SettingsWindowHost`                |
| `Module_Settings.Core/Interfaces/IService_UserPreferences.cs`                   | `UpdateDefaultModeAsync`                  | `13`          | No callers, appears to be a stub                                        | Remove or implement                                         |
| `Module_Settings.Core/Services/Service_SettingsManifestProvider.cs`             | Manifest loading path                     | `~30-45`      | Registry is underused relative to direct DAO reads                      | Either use the registry consistently or simplify the design |
| `Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs`                           | `ParseCustomMappings`                     | `~448-460`    | Parsed list is never persisted                                          | Persist or move out of hot path                             |
| `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_OutsideServiceHistory.cs`       | `_allResults`                             | `~30`         | Held indefinitely after search                                          | Clear on cleanup or navigation away                         |
| Any ViewModel inheriting `ViewModel_Shared_Base`                                | `StatusMessage` usage                     | Cross-cutting | Messages may be visually hidden in scrolled content                     | Move to persistent status surface                           |

## Section 5: Settings Feature Deep Dive

### 5.1 Current Settings Inventory

| Module                      | Storage                                                                                                                            | Scope           | Status                          |
| --------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- | --------------- | ------------------------------- |
| Theme                       | `ViewModel_Settings_Theme -> IService_SettingsCoreFacade -> Dao_SettingsCoreSystem`                                                | System          | Functional                      |
| Database connection strings | `ViewModel_Settings_Database -> IService_SettingsCoreFacade -> Dao_SettingsCoreSystem`, encrypted via `ISettingsEncryptionService` | System          | Functional                      |
| Logging level and targets   | `ViewModel_Settings_Logging -> DAOs`                                                                                               | System          | Functional                      |
| Shared file paths           | `ViewModel_Settings_SharedPaths -> IService_SettingsCoreFacade`                                                                    | System          | Functional                      |
| User management             | `ViewModel_Settings_Users -> Dao_User`                                                                                             | System and User | Functional                      |
| System overview             | `ViewModel_Settings_System -> ISettingsMetadataRegistry`                                                                           | System          | Functional                      |
| Receiving user preferences  | `ViewModel_Settings_Receiving_UserPreferences -> IService_UserPreferences -> Dao_User`                                             | User            | Partially functional            |
| Receiving business rules    | `ViewModel_Settings_Receiving_BusinessRules -> IService_UserPreferences.UpdateDefaultReceivingModeAsync`                           | User            | Functional                      |
| Receiving validation rules  | `ViewModel_Settings_Receiving_Validation`                                                                                          | Unknown         | UI present, persistence unclear |
| Receiving defaults          | `ViewModel_Settings_Receiving_Defaults`                                                                                            | Unknown         | UI present, persistence unclear |
| Volvo settings pages        | Placeholder only                                                                                                                   | N/A             | Not implemented                 |
| Dunnage settings pages      | Placeholder only                                                                                                                   | N/A             | Not implemented                 |
| Reporting settings pages    | Placeholder only                                                                                                                   | N/A             | Not implemented                 |

### 5.2 Settings Architecture Analysis

**What exists**

- A CQRS-style settings engine using `GetSettingQuery/Handler`, `SetSettingCommand/Handler`, and `ResetSettingCommand/Handler`
- `ISettingsMetadataRegistry` for setting definitions
- `ISettingsCache` for in-memory caching
- `ISettingsEncryptionService` for sensitive values
- `Dao_SettingsCoreSystem`, `Dao_SettingsCoreUser`, and `Dao_SettingsCoreAudit` for MySQL persistence and audit trail
- `ISettingsManifestProvider` for manifest-backed definitions

**What is missing or inconsistent**

- Dual-path problem: Receiving and Dunnage user preferences bypass the CQRS engine and call `Dao_User` directly.
- Manifest not consulted on reads: `GetSettingQueryHandler` reads from DAOs without applying manifest defaults when rows are missing.
- No manifest version validation: manifest changes are not explicitly checked at startup.
- Encryption inconsistency: CQRS-backed settings respect `IsSensitive`, while the `IService_UserPreferences` path stores plain text.
- `UpdateDefaultModeAsync` appears dead and overlaps with more specific methods.
- No module-level reset operation exists in the UI.
- Validation is limited; range, regex, and cross-setting validation are not implemented.

### 5.3 Proposed Cohesive Settings Architecture (Beta-Friendly)

```text
ISettingsService (central facade)
  ├── GetAsync<T>(category, key, scope, userId?)
  │       reads manifest default if DB empty
  │       decrypts if sensitive
  │       caches result
  ├── SetAsync<T>(category, key, value, scope, userId?)
  │       validates value
  │       encrypts if sensitive
  │       writes audit
  │       invalidates cache
  ├── ResetAsync(category, key, scope, userId?)
  └── ResetModuleAsync(category, scope, userId?)

ISettingValidator<T>
  ├── Validate(T value, Model_SettingsDefinition definition)
  └── Register validators via DI per module

Model_SettingsDefinition
  └── Extend with ValidationExpression, MinValue, MaxValue, AllowedValues, SchemaVersion

Service_UserPreferences
  └── Redirect app-level calls through ISettingsService using category = UserPreferences
```

**Implementation path for beta**

- Unify reads and writes behind one facade first.
- Preserve the current tables for now.
- Add manifest validation and schema-version checking as startup guards, not as a migration system.
- Move `IService_UserPreferences` consumers onto the central facade incrementally inside the app code.

### 5.4 UX Improvements for Settings

- Group settings by module tabs: Receiving, Dunnage, Volvo, Reporting, Core.
- Add search with an `AutoSuggestBox` powered by `ISettingsMetadataRegistry`.
- Add `Reset all [module] settings` buttons on module overview pages.
- Require confirmation dialogs for sensitive settings.
- Add contextual help using `TeachingTip` plus metadata descriptions.
- Decide whether placeholder pages will be completed soon or removed from navigation.

### 5.5 Incomplete Settings Pages and Features

| File                                                                              | Symbol or Page           | Issue                                    | Action                                      |
| --------------------------------------------------------------------------------- | ------------------------ | ---------------------------------------- | ------------------------------------------- |
| `Module_Settings.Volvo/Views/View_Settings_Volvo_FilePaths.xaml`                  | Entire page              | Placeholder                              | Complete or remove from steps               |
| `Module_Settings.Volvo/Views/View_Settings_Volvo_DatabaseSettings.xaml`           | Entire page              | Placeholder                              | Complete or remove                          |
| `Module_Settings.Volvo/Views/View_Settings_Volvo_ExternalizationBacklog.xaml`     | Entire page              | Placeholder                              | Complete or remove                          |
| `Module_Settings.Volvo/Views/View_Settings_Volvo_SettingsOverview.xaml`           | Entire page              | Placeholder                              | Complete or remove                          |
| `Module_Settings.Volvo/Views/View_Settings_Volvo_ConnectionStrings.xaml`          | Entire page              | Placeholder                              | Complete or remove                          |
| `Module_Settings.Volvo/Views/View_Settings_Volvo_UiConfiguration.xaml`            | Entire page              | Placeholder                              | Complete or remove                          |
| `Module_Settings.Dunnage/Views/View_Settings_Dunnage_Audit.xaml`                  | Entire page              | Placeholder                              | Complete or remove                          |
| `Module_Settings.Dunnage/Views/View_Settings_Dunnage_Permissions.xaml`            | Entire page              | Placeholder                              | Complete or remove                          |
| `Module_Settings.Dunnage/Views/View_Settings_Dunnage_UiUx.xaml`                   | Entire page              | Placeholder                              | Complete or remove                          |
| `Module_Settings.Dunnage/Views/View_Settings_Dunnage_Workflow.xaml`               | Entire page              | Placeholder                              | Complete or remove                          |
| `Module_Settings.Dunnage/Views/View_Settings_Dunnage_SettingsOverview.xaml`       | Entire page              | Placeholder                              | Complete or remove                          |
| `Module_Settings.Reporting/Views/View_Settings_Reporting_FileIO.xaml`             | Entire page              | Placeholder                              | Complete or remove                          |
| `Module_Settings.Reporting/Views/View_Settings_Reporting_Permissions.xaml`        | Entire page              | Placeholder                              | Complete or remove                          |
| `Module_Settings.Reporting/Views/View_Settings_Reporting_BusinessRules.xaml`      | Entire page              | Placeholder                              | Complete or remove                          |
| `Module_Settings.Reporting/Views/View_Settings_Reporting_EmailUx.xaml`            | Entire page              | Placeholder                              | Complete or remove                          |
| `Module_Settings.Reporting/Views/View_Settings_Reporting_SettingsOverview.xaml`   | Entire page              | Placeholder                              | Complete or remove                          |
| `Module_Settings.Core/Interfaces/IService_UserPreferences.cs`                     | `UpdateDefaultModeAsync` | Dead or stubbed API                      | Remove or implement                         |
| `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_Validation.cs` | Persistence path         | Save wiring unclear                      | Verify DAO write path                       |
| `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_Defaults.cs`   | Persistence path         | Save wiring unclear                      | Verify DAO write path                       |
| `Module_Settings.Core/Services/Service_SettingsManifestProvider.cs`               | Schema version handling  | No explicit manifest version field usage | Add `schemaVersion` plus startup validation |

## Section 6: Module-by-Module Summary with Implementation Outlines

### Module_Receiving: Key Recommendations

- Add an empty-state panel to `View_Receiving_POEntry.xaml`.
- Add commit confirmation in `View_Receiving_LoadEntry.xaml` before DAO writes.
- Surface inline validation instead of relying on `StatusMessage` alone.
- Add a full-page loading overlay bound to `IsBusy`.

### Module_Volvo: Key Recommendations

- Debounce part search in `ViewModel_Volvo_ShipmentEntry.cs`.
- Replace item-by-item collection updates with one-shot assignment or bulk add.
- Preserve the last-used values for dialog reopen flows.
- Add inline validation on dialogs such as `VolvoPartAddEditDialog.xaml`.

### Module_ShipRec_Tools: Key Recommendations

- Push filtering server-side for history tools.
- Add virtualization for large result sets.
- Consolidate repeated `Clear + Add` patterns into one bulk replacement helper.
- Clear `_allResults` on deactivation or navigation away.

### Module_Settings: Key Recommendations

- Remove reflection-based navigation.
- Complete or remove the 16 placeholder pages.
- Unify settings access behind one facade.
- Add manifest version validation without introducing full migration machinery.
- Add search to the settings window.
- Replace reflection-based header updates with an interface-backed call.

## Appendix: File References Index

| File                                                                                   | Topics Covered                                                         |
| -------------------------------------------------------------------------------------- | ---------------------------------------------------------------------- |
| `Module_Settings.Volvo/Views/View_Settings_Volvo_WorkflowHub.xaml.cs`                  | Reflection navigation, dead step-click handlers, dead `GetHostFrame()` |
| `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_OutsideServiceHistory.cs`              | ObservableCollection loops, unbounded memory, server-side filter gap   |
| `Module_ShipRec_Tools/ViewModels/ViewModel_ShipRecTools_ToolSelection.cs`              | ObservableCollection loops                                             |
| `Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs`                             | ObservableCollection loops, missing debounce                           |
| `Module_Settings.Core/Services/ResetSettingCommandHandler.cs`                          | CQRS settings reference implementation                                 |
| `Module_Settings.Core/Interfaces/IService_UserPreferences.cs`                          | Dead `UpdateDefaultModeAsync` method                                   |
| `Module_Settings.Core/Services/Service_UserPreferences.cs`                             | Dual-path settings bypass                                              |
| `Module_Settings.Core/Services/Service_SettingsManifestProvider.cs`                    | Manifest validation and version handling                               |
| `Module_Settings.Core/Views/View_Settings_CoreWindow.xaml.cs`                          | String-keyed header lookup, reflection call target                     |
| `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_UserPreferences.cs` | ObservableCollection population issue                                  |
| `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`                       | DI registration source of truth                                        |
| `Module_Settings.Core/ViewModels/ViewModel_SettingsNavigationHubBase.cs`               | ObservableCollection loop, pagination engine                           |
