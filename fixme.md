# MTM Receiving Application Startup Improvement Report

> Beta note: the application is still in beta, so this document favors clean in-repo fixes over migration-heavy rollout planning.

> Deduplication note: a strict comparison against `FixMe2.md` found related themes but no exact duplicate startup fixes to remove. This report keeps the startup-specific items and now uses the same report format.

## Executive Summary: Top 10 Startup Improvements

| #   | Improvement                                                                                                         | Impact | Effort | Priority |
| --- | ------------------------------------------------------------------------------------------------------------------- | ------ | ------ | -------- |
| 1   | Rework first-login settings initialization so new users do not wait on hundreds of sequential default-setting calls | High   | Medium | P0       |
| 2   | Lazy-load module root views and workflow subtrees so each module resolves only the active screen or step on demand  | High   | High   | P0       |
| 3   | Batch and cache receiving settings reads for the initial receiving screen                                           | High   | Medium | P0       |
| 4   | Remove blocking startup writes from authentication, workstation detection, and session logging                      | High   | Medium | P0       |
| 5   | Replace constructor-launched async initialization with explicit awaited lifecycle hooks                             | High   | Medium | P1       |
| 6   | Add timeouts and degraded-mode fallbacks for startup database calls                                                 | High   | Medium | P1       |
| 7   | Collapse duplicate role bootstrap queries and assign the default role only once                                     | Medium | Medium | P1       |
| 8   | Make stored-procedure retry policy startup-aware and operation-specific                                             | Medium | Medium | P1       |
| 9   | Remove artificial splash and dialog delays from the critical path                                                   | Medium | Low    | P1       |
| 10  | Add structured startup phase telemetry and timing                                                                   | Medium | Low    | P2       |

## Section 1: Startup Scope and Critical Paths

### Current Scope

This report stays intentionally narrow: it covers startup latency, startup reliability, and the first-screen readiness path. Unlike `FixMe2.md`, which surveys the whole application, this document focuses on the sequence from application launch through authentication, session setup, and first visible screen readiness.

### Startup Flow Map

```text
App launch
  -> Host build and dependency injection registration
  -> Authentication and workstation detection
  -> Session bootstrap
	  -> Settings initialization
	  -> Role and privilege bootstrap
	  -> Optional new-user or shared-terminal flows
  -> Main window activation
	  -> Navigate to View_Receiving_Workflow
	  -> Construct receiving workflow steps and ViewModels
  -> First screen becomes interactive
```

### Startup Improvement Inventory

| Area                         | Improvement IDs        | Main Goal                                              |
| ---------------------------- | ---------------------- | ------------------------------------------------------ |
| Settings initialization      | `S2-1`, `S2-3`, `S3-4` | Remove serial reads and make defaulting predictable    |
| Module first-screen load     | `S2-2`, `S3-1`, `S3-5` | Stop hidden work from inactive module screens or steps |
| Authentication and session   | `S2-4`, `S2-5`, `S2-6` | Remove avoidable writes and duplicate bootstrap calls  |
| Dialog and exceptional flows | `S2-7`, `S3-2`, `S3-3` | Bound slow paths and keep startup usable               |
| Diagnostics and protection   | `S4-1`, `S4-2`         | Make regressions visible and enforce budgets           |

## Section 2: Critical-Path Performance Improvements

### S2-1: Rework New User First-Login Settings Initialization

**Priority:** High  
**Effort:** Medium

**Affected Files**

- `Module_Core/Services/Authentication/Service_UserLoginCoordinator.cs`
- `Module_Settings.Core/Services/Service_SettingsCoreFacade.cs`
- `Module_Settings.Core/Services/GetSettingQueryHandler.cs`
- `Module_Settings.Core/Defaults/settings.manifest.json`

**Problem:**  
During first login for a brand-new user, `InitializeAuthenticatedSessionAsync` waits for `InitializeSettingsDefaultsAsync`, which calls `InitializeDefaultsAsync`. That method loops through every settings definition in the settings manifest and sends a `GetSettingQuery` for each one. For a new user this causes a cache miss, a user-settings lookup, and then a default write for each missing setting. Because this is done sequentially for every setting, the user waits through hundreds of serial database operations before entering the application.

**Recommended Alternatives:**

1. Preferred: return manifest defaults in memory and only persist settings when the user explicitly changes them.
2. Add a bulk seed stored procedure that inserts all missing defaults for a user in one database call.
3. Move settings default initialization off the critical login path and run it in the background after the session is created.
4. As a short-term mitigation, parallelize the existing per-setting initialization flow.

**Expected Outcome:**  
First login for new users will no longer block on hundreds of per-setting serial database round-trips. New users should reach the main application much faster, and settings persistence can be handled lazily, in bulk, or off the critical path.

**Workflow Before:**

1. User authenticates successfully.
2. Session creation starts.
3. Settings initialization loops over the full manifest.
4. Each setting triggers lookup, miss handling, default write, audit write, and cache population.
5. Main window stays blocked until the full loop completes.

**Workflow After:**

1. User authenticates successfully.
2. Session creation starts.
3. Effective defaults are resolved from memory or seeded in one bulk/background operation.
4. Only required startup settings are materialized immediately.
5. Main window opens while non-critical persistence finishes lazily or not at all unless the user changes a value.

**User Experience Change:**

- Brand-new users stop seeing an unusually long first login.
- The application feels ready almost as quickly for new users as it does for returning users.
- First-run no longer feels like the app is frozen while invisible setup work happens.

### S2-2: Clarify Startup Scope and Lazy-Load Module Root Views and Workflow Subtrees

**Priority:** High  
**Effort:** High

**Affected Files**

- `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
- `Module_Receiving/Views/View_Receiving_Workflow.xaml.cs`
- `MainWindow.xaml.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs`
- `Module_Receiving/Views/View_Receiving_ModeSelection.xaml.cs`
- `Module_Receiving/Views/View_Receiving_POEntry.xaml.cs`
- `Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs`
- `Module_Receiving/Views/View_Receiving_EditMode.xaml.cs`
- `Module_Dunnage/Views/View_Dunnage_WorkflowView.xaml.cs`
- `Module_OutsideService/Views/View_OutsideService_Main.xaml.cs`
- `Module_Volvo/Views/View_Volvo_ShipmentEntry.xaml.cs`
- `Module_Volvo/Views/View_Volvo_History.xaml.cs`
- `Module_Reporting/Views/View_Reporting_Main.xaml.cs`
- `Module_ShipRec_Tools/Views/View_ShipRecTools_Main.xaml.cs`
- `Module_Settings.Core/Views/View_Settings_CoreWindow.xaml.cs`

**Problem:**  
The startup script in `App.xaml.cs` and `ModuleServicesExtensions.cs` registers all feature modules with dependency injection, but that registration step does not instantiate every module UI on launch. The actual eager-loading problem starts when a module root view is activated from `MainWindow.xaml.cs` and that root view immediately constructs a large internal subtree or preloads state that the user cannot see yet. The first and most visible example is `MainWindow_Activated` calling `NavigateWithDI(typeof(View_Receiving_Workflow))`, then `View_Receiving_Workflow` constructing and assigning the entire receiving workflow subtree at once: mode selection, manual entry, edit mode, PO entry, load entry, weight/quantity, heat/lot, package type, and review. The same lazy-loading principle should be applied to every other main-window module entry point and to settings navigation hosts: resolve the module root page on navigation, but only resolve child screens, dialogs, and heavy state when the user actually enters that branch.

**Recommended Alternatives:**

1. Preferred: keep module registration as-is, but change each module root view so it resolves and hosts only the active screen or active workflow step at startup.
2. Apply the receiving refactor first, then use the same pattern for Dunnage, OutsideService, Volvo, Reporting, ShipRec Tools, and settings hubs where child views or heavy state are eagerly composed.
3. Keep a small lazy cache so already-opened module screens or workflow steps can be reused without forcing every child view to be created on first navigation.
4. Replace constructor injection of child views with a factory, keyed resolver, or navigation-based step loader so module root views remain cheap to activate.
5. If full lazy loading is too large a change initially, at least defer edit, review, history, admin, and other non-default paths because they are not needed for the first screen the user sees.

**Expected Outcome:**  
The startup discussion will be accurate: all modules remain registered, but only the module root page and its active child screen are resolved when a module is opened. The first screen for each module will render faster, hidden screens will stop doing work before they are visible, and startup cost will better match the screen the user actually sees.

**Workflow Before:**

1. App startup builds the host and registers all modules in DI.
2. Main window activates or the user selects a module from navigation.
3. `NavigateWithDI` resolves the module root view.
4. That module root view eagerly resolves child pages, workflow steps, dialogs, or related ViewModels that are not yet visible.
5. Hidden module branches begin initialization even though the user is still on the first visible screen for that module.

**Workflow After:**

1. App startup builds the host and registers all modules in DI.
2. Main window activates or the user selects a module from navigation.
3. `NavigateWithDI` resolves the requested module root view.
4. The module root resolves only the active child screen or active workflow step.
5. Later screens and steps are created only when the user navigates into them, with optional reuse from a small lazy cache.

**User Experience Change:**

- The first screen for each module appears faster without requiring risky late registration changes to the DI container.
- The app feels lighter and more responsive because hidden module screens are no longer constructed immediately.
- Users stop paying the cost for inactive receiving steps, inactive admin pages, inactive history views, and other branches they may never open during that session, even though the modules remain registered and available for later navigation.

### S2-3: Batch and Cache Receiving Settings Reads for the Initial Receiving Screen

**Priority:** High  
**Effort:** Medium

**Affected Files**

- `Module_Receiving/Services/Service_ReceivingSettings.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_EditMode.cs`
- `Module_Settings.Core/Services/Service_SettingsCoreFacade.cs`

**Problem:**  
Receiving startup triggers many sequential `GetStringAsync` calls for UI text and behavior settings. Because the workflow page eagerly creates multiple step ViewModels, the application can fan out into dozens of individual settings reads before the user has interacted with anything. Even if cache hits help after warm-up, first-run and cold-cache startup still pay the round-trip overhead repeatedly.

**Recommended Alternatives:**

1. Preferred: add a bulk settings API that retrieves multiple keys in one call and returns a dictionary or immutable snapshot.
2. Hydrate a per-module `ReceivingSettingsSnapshot` once per session and serve later lookups from memory.
3. Treat UI text defaults as in-memory resources first, and only overlay database-backed overrides if they exist.
4. Collapse repeated requests for the same keys so constructor-time initialization does not issue redundant lookups.

**Expected Outcome:**  
The receiving workflow will stop generating a startup burst of small settings calls, reducing both time-to-first-screen and settings-store load.

**Workflow Before:**

1. Receiving workflow and child ViewModels request many settings one key at a time.
2. Each request goes through the settings facade and query handler independently.
3. Cold-start cache misses generate repeated database lookups and default handling.
4. Startup latency grows with the number of text and behavior keys requested.

**Workflow After:**

1. Receiving startup requests a grouped settings snapshot or bulk key set.
2. The snapshot is resolved once per session or once per module activation.
3. ViewModels read needed values from memory instead of issuing their own isolated calls.
4. Only changed or missing values trigger further persistence work.

**User Experience Change:**

- The receiving screen becomes ready sooner.
- Text, labels, and behavior settings appear consistently instead of filling in piecemeal.
- Users experience fewer small stalls during the first seconds after navigation.

### S2-4: Remove Blocking Startup Writes from Authentication, Workstation Detection, and Session Logging

**Priority:** High  
**Effort:** Medium

**Affected Files**

- `Module_Core/Services/Authentication/Service_Authentication.cs`
- `Module_Core/Services/Authentication/Service_UserSessionManager.cs`
- `Module_Core/Data/Authentication/Dao_User.cs`
- `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`

**Problem:**  
Personal-workstation startup currently awaits audit/config writes that are not required to let the user into the application. `AuthenticateByWindowsUsernameAsync` logs successful login activity before returning. `DetectWorkstationTypeAsync` also awaits `UpsertWorkstationConfigAsync` on every startup. These writes turn diagnostics and workstation bookkeeping into critical-path latency.

**Recommended Alternatives:**

1. Preferred: move login activity and workstation upsert onto a background queue that runs after the session is established.
2. Coalesce login-success and session-start logging into one event written after startup completes.
3. Make workstation persistence periodic or change-driven rather than a required write on every launch.
4. If any of these writes fail, keep them best-effort and log only to application telemetry rather than delaying UI readiness.

**Expected Outcome:**  
Personal-workstation startup will be more read-heavy and less write-bound, reducing avoidable database latency and improving reliability when the database is slow but still reachable.

**Workflow Before:**

1. Windows authentication succeeds.
2. Login activity is written before startup continues.
3. Workstation type is detected.
4. Workstation configuration is upserted on the startup path.
5. Session creation and UI readiness wait for optional writes to finish.

**Workflow After:**

1. Windows authentication succeeds.
2. Required read-only startup checks complete.
3. Session is created and the UI is shown.
4. Login audit and workstation bookkeeping are queued or executed as best-effort background work.
5. Failures in optional writes do not delay the user.

**User Experience Change:**

- Returning users reach the main app faster.
- Slow or busy databases are less likely to make startup feel sticky.
- Users benefit from a smoother startup even when telemetry or bookkeeping systems are degraded.

### S2-5: Collapse Duplicate Role Bootstrap Queries and Assign the Default Role Only Once

**Priority:** Medium  
**Effort:** Medium

**Affected Files**

- `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`
- `Module_Core/Services/Authentication/Service_UserLoginCoordinator.cs`
- `Module_Core/Services/Authentication/Service_UserPrivileges.cs`
- `Module_Settings.Core/Data/Dao_SettingsCoreRoles.cs`
- `Module_Settings.Core/Data/Dao_SettingsCoreUserRoles.cs`

**Problem:**  
The startup flow performs role work in more than one place. `AssignDefaultRoleIfMissingAsync` runs during app startup, and then `InitializePrivilegesAsync` re-queries the role catalog and the user-role mapping again. In the new-user path the default-role check can run twice before the session is even fully ready. This is unnecessary I/O and adds complexity to an already busy startup path.

**Recommended Alternatives:**

1. Preferred: centralize role bootstrap into a single service that both assigns the default role if needed and returns the effective role set.
2. Cache the role catalog in memory for the process lifetime because role definitions rarely change.
3. Skip the second default-role check if the user was just created and already assigned a role in the same startup sequence.
4. Make `InitializePrivilegesAsync` accept pre-fetched role data so the startup flow does not need to hit the database twice.

**Expected Outcome:**  
Startup will perform fewer role-related round-trips, the new-user path will avoid duplicate work, and privilege initialization will be simpler to reason about.

**Workflow Before:**

1. Startup checks whether the user already has roles.
2. Startup may assign the default role.
3. Privilege initialization re-loads roles and user-role mappings again.
4. New-user flow can repeat portions of the same work in one launch.

**Workflow After:**

1. Startup asks a single role-bootstrap service for effective roles.
2. The service assigns the default role only if needed.
3. The same call returns the current effective privilege set.
4. Later consumers reuse already-resolved role data.

**User Experience Change:**

- Login becomes faster, especially on first run or for newly created accounts.
- Permission-based UI arrives with less delay.
- Users are less likely to hit odd timing issues where roles are still settling during startup.

### S2-6: Make Stored Procedure Retry Policy Startup-Aware and Operation-Specific

**Priority:** Medium  
**Effort:** Medium

**Affected Files**

- `Module_Core/Helpers/Database/Helper_Database_StoredProcedure.cs`
- `Module_Core/Data/Authentication/Dao_User.cs`
- `Module_Core/Services/Authentication/Service_UserLoginCoordinator.cs`
- `Module_Settings.Core/Services/Service_SettingsCoreFacade.cs`

**Problem:**  
The shared stored-procedure helper applies the same retry/backoff policy to all calls, using up to three attempts with 100 ms, 200 ms, and 400 ms delays. That is reasonable for isolated writes, but during startup it can amplify latency across many small reads and default-seeding operations. When settings hydration or role loading fans out into many calls, even transient failures can turn into multi-second startup inflation.

**Recommended Alternatives:**

1. Preferred: add per-call retry options so startup-critical reads can use a lighter retry policy than writes or background operations.
2. Disable or sharply reduce retries for high-volume lookup paths such as settings reads during login.
3. Add fast-fail behavior for optional startup phases and reserve full retry/backoff only for required commits.
4. Emit retry counts in structured logs so startup slowness caused by transient faults is obvious.

**Expected Outcome:**  
Startup latency will stay bounded under transient database instability instead of multiplying small retry waits across many calls.

**Workflow Before:**

1. Startup triggers many small stored-procedure calls.
2. Any transient fault can cause each call to retry with the full shared backoff policy.
3. Repeated retries stack across settings, role, and auth-related operations.
4. A short database wobble becomes a long startup pause.

**Workflow After:**

1. Startup-critical reads use a lighter or specialized retry policy.
2. Optional calls can fail fast or degrade gracefully.
3. Retry-heavy policies remain reserved for the operations that truly need them.
4. Logs show which startup phases slowed down because of retries.

**User Experience Change:**

- Startup stays more consistent under light infrastructure instability.
- Users see fewer long pauses caused by automatic retry storms.
- Intermittent database hiccups feel like brief slowdowns instead of full startup hangs.

### S2-7: Remove Artificial Splash and Dialog Delays from the Critical Path

**Priority:** Medium  
**Effort:** Low

**Affected Files**

- `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`
- `Module_Shared/Views/View_Shared_NewUserSetupDialog.xaml.cs`
- `Module_Shared/Views/View_Shared_SharedTerminalLoginDialog.xaml.cs`

**Problem:**  
The startup flow contains fixed `Task.Delay` calls of 100 ms, 100 ms, 300 ms, and 500 ms before the application is considered ready. The new-user dialog then adds a forced 2-second success pause, and the shared-terminal lockout path adds a forced 5-second delay before closing. These waits do not represent real work and can make a healthy startup feel slow.

**Recommended Alternatives:**

1. Preferred: remove the delays entirely unless they are guarding a real UI readiness event that can be awaited directly.
2. Replace cosmetic completion waits with skippable animations or a feature flag used only for demos.
3. For the lockout path, display the message and let the caller close immediately after state is recorded.
4. If any delay must remain for UX reasons, make it cancellable and never put it on the core startup path.

**Expected Outcome:**  
The application will stop burning 1 to 3+ seconds on artificial pauses, making startup feel significantly snappier even before deeper architectural fixes land.

**Workflow Before:**

1. Splash screen starts.
2. Fixed delays are inserted between startup stages.
3. Completion waits delay the final window activation.
4. Exceptional dialogs add extra forced pauses before closing or continuing.

**Workflow After:**

1. Splash screen reflects actual progress only.
2. Startup moves forward as soon as each real dependency is ready.
3. Success and failure branches close immediately unless a real user decision is required.
4. Any cosmetic waiting is optional and outside the critical path.

**User Experience Change:**

- Startup feels more honest and immediate.
- Users stop waiting for decorative pauses that look like slowness.
- Exceptional flows become less frustrating because the app reacts right away.

## Section 3: Startup Resilience and Edge Cases

### S3-1: Replace Constructor-Launched Async Initialization with Explicit Awaited Lifecycle Hooks

**Priority:** High  
**Effort:** Medium

**Affected Files**

- `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_EditMode.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_LoadEntry.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_Review.cs`

**Problem:**  
Several startup-created ViewModels trigger asynchronous work from their constructors using fire-and-forget patterns such as `_ = LoadUITextAsync()` or direct `InitializeAsync()` calls. This makes initialization ordering non-deterministic, hides exceptions, and allows the UI to render while critical state is still loading or mutating in the background.

**Recommended Alternatives:**

1. Preferred: expose explicit `InitializeAsync` methods and await them from the page or window lifecycle where sequencing can be controlled.
2. Add an `IsInitialized` state so each view can avoid acting on partially loaded data.
3. Route startup initialization through a coordinator or page activation hook rather than constructors.
4. Centralize init-task exception handling so failures are observable and recoverable instead of becoming silent background faults.

**Expected Outcome:**  
Startup and first-render behavior will become deterministic, easier to debug, and less prone to partial UI state or swallowed initialization failures.

**Workflow Before:**

1. ViewModels are constructed.
2. Constructors kick off background async work with no central coordination.
3. The UI can render while initialization is still running.
4. Exceptions can be hard to observe and ordering can vary by machine speed.

**Workflow After:**

1. ViewModels are constructed in a cheap, synchronous state.
2. Explicit initialization hooks are awaited from page or workflow activation.
3. Each screen becomes interactive only after required setup completes.
4. Initialization failures are surfaced through one visible, controlled path.

**User Experience Change:**

- Screens open in a more stable and predictable state.
- Users see fewer partially loaded controls or sudden state flips after render.
- Startup issues become easier to understand because failures are surfaced cleanly.

### S3-2: Add Timeouts and Degraded-Mode Fallbacks for Startup Database Calls

**Priority:** High  
**Effort:** Medium

**Affected Files**

- `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`
- `Module_Core/Services/Authentication/Service_Authentication.cs`
- `Module_Core/Services/Authentication/Service_UserLoginCoordinator.cs`
- `Module_Settings.Core/Services/GetSettingQueryHandler.cs`
- `Module_Core/Helpers/Database/Helper_Database_StoredProcedure.cs`

**Problem:**  
If authentication, role loading, settings retrieval, or department lookup stalls, the splash screen can appear frozen with no bounded recovery path. Some branches already have safe defaults, but the startup orchestration still awaits several remote or database operations with no phase timeout, no degraded mode, and no user-facing retry decision.

**Recommended Alternatives:**

1. Preferred: add per-phase timeouts and classify startup calls as required or optional.
2. Continue with in-memory defaults for optional phases such as settings hydration, workstation bookkeeping, and non-critical text overrides.
3. Show a retry or degraded-mode decision on the splash screen instead of waiting indefinitely.
4. Cache last-known-good non-sensitive startup values locally so startup can proceed when the database is slow but not completely unavailable.

**Expected Outcome:**  
Startup will fail fast when it truly must fail, degrade gracefully when it can, and stop trapping the user behind an indefinite splash screen.

**Workflow Before:**

1. Startup enters a database-backed phase.
2. A slow dependency stalls the awaited call.
3. Splash messaging may update, but there is no bounded timeout or retry decision for the user.
4. Startup can appear frozen for an unpredictable amount of time.

**Workflow After:**

1. Each startup phase has an explicit timeout and importance level.
2. Required phases fail clearly when their budget is exceeded.
3. Optional phases fall back to defaults, retry in the background, or offer a user decision.
4. The splash screen can communicate whether startup is retrying, degrading, or aborting.

**User Experience Change:**

- Users are no longer trapped behind an indefinite loading window.
- Startup problems become understandable instead of mysterious.
- The app can still open in a limited mode when the failing subsystem is non-critical.

### S3-3: Make New User and Shared Terminal Dialog Bootstrapping Non-Blocking

**Priority:** Medium  
**Effort:** Low

**Affected Files**

- `Module_Shared/Views/View_Shared_NewUserSetupDialog.xaml.cs`
- `Module_Shared/ViewModels/ViewModel_Shared_NewUserSetup.cs`
- `Module_Shared/Views/View_Shared_SharedTerminalLoginDialog.xaml.cs`
- `Module_Shared/ViewModels/ViewModel_Shared_SharedTerminalLogin.cs`

**Problem:**  
The new-user dialog waits for department loading during `Loaded` before continuing its normal interaction flow, and the shared-terminal dialog deliberately sleeps for 5 seconds on lockout before closing. These branches magnify the feeling of a frozen or sluggish startup, especially when they are already being shown because the user hit an exceptional startup path.

**Recommended Alternatives:**

1. Preferred: render the dialog immediately, then populate optional dropdown data asynchronously with a local loading indicator.
2. Allow manual department entry even while department data is still loading.
3. Keep only the dependent controls disabled instead of treating the whole dialog as blocked.
4. Let the startup coordinator decide whether any post-lockout message needs to remain visible instead of forcing a timer inside the dialog itself.

**Expected Outcome:**  
Exceptional startup paths will feel responsive instead of stalled, and users will be able to act sooner even if supplemental lookup data is still loading.

**Workflow Before:**

1. User reaches the new-user or shared-terminal dialog path.
2. The dialog waits for supporting work such as department loading or lockout delay timers.
3. The whole flow feels blocked even when only one piece of data is pending.

**Workflow After:**

1. Dialog appears immediately.
2. Optional data loads asynchronously while the dialog remains usable.
3. Only dependent controls are temporarily disabled if needed.
4. Lockout and cancel behavior return control to startup without forced internal pauses.

**User Experience Change:**

- Exceptional startup paths feel interactive instead of frozen.
- New users can start filling out the dialog immediately.
- Shared-terminal failures return a quick answer instead of making the user wait through a timer.

### S3-4: Fail Loudly When the Settings Manifest Is Missing, Invalid, or Not Deployed

**Priority:** Medium  
**Effort:** Low

**Affected Files**

- `Module_Settings.Core/Services/Service_SettingsManifestProvider.cs`
- `Module_Settings.Core/Services/Service_SettingsMetadataRegistry.cs`
- `Module_Settings.Core/Services/Service_SettingsCoreFacade.cs`
- `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`

**Problem:**  
The settings manifest provider reads `settings.manifest.json` from disk and silently returns an empty definition list if the file is missing. That means deployment or packaging mistakes can degrade the entire settings system without an explicit failure. Startup will still continue, but settings behavior becomes an inconsistent mix of module hard-coded fallbacks and unknown-setting failures that are much harder to diagnose.

**Recommended Alternatives:**

1. Preferred: validate manifest availability and parse success during host startup, then fail fast with a clear error if a required manifest is missing.
2. Distinguish between `file missing`, `invalid JSON`, and `missing settings node` in the logs and error messages.
3. Add a build or publish validation step that ensures the manifest is copied to the output used by the packaged app.
4. If degraded startup is ever allowed, surface a visible warning and diagnostic event instead of silently returning an empty registry.

**Expected Outcome:**  
Settings deployment issues will be obvious immediately, preventing silent partial startup behavior and reducing time spent debugging environment-specific failures.

**Workflow Before:**

1. App starts and loads the settings manifest from disk.
2. If the file is missing or unusable, the provider returns an empty list.
3. Startup continues with partial defaults and hidden configuration gaps.
4. Problems surface later as inconsistent behavior instead of a clear startup failure.

**Workflow After:**

1. App starts and validates the manifest resource explicitly.
2. Missing or invalid manifest states are classified and logged clearly.
3. Startup either fails fast with a useful error or enters a clearly marked degraded mode.
4. Deployment and packaging mistakes are visible immediately.

**User Experience Change:**

- Broken deployments fail clearly instead of behaving strangely.
- Users and support staff get a concrete explanation instead of random settings issues.
- Environment-specific bugs become easier to triage because startup points at the actual root cause.

### S3-5: Stop PO Entry Mock Auto-Load from Running Before the PO Step Is Active

**Priority:** Medium  
**Effort:** Low

**Affected Files**

- `Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs`
- `Module_Receiving/Views/View_Receiving_Workflow.xaml.cs`
- `Module_Receiving/Services/Service_ReceivingWorkflow.cs`

**Problem:**  
`ViewModel_Receiving_POEntry` starts `InitializeAsync()` from its constructor, and in mock-data mode it waits 500 ms and then auto-loads a PO. Because the workflow page currently constructs every step view during startup, that mock-only behavior can run even when the user has not entered the PO step yet. In development and test environments this adds avoidable background startup work and can mutate workflow state from an inactive screen.

**Recommended Alternatives:**

1. Preferred: trigger mock PO autofill only when the PO entry step becomes active.
2. Replace the constructor-launched `async void` path with an explicit awaited initialization hook.
3. Make mock auto-load opt-in from the step UI instead of automatic.
4. Remove the fixed 500 ms wait and bind the behavior to actual page readiness if it must remain.

**Expected Outcome:**  
Mock-data startup will be predictable, inactive workflow steps will stop doing hidden work, and debug-only conveniences will no longer distort startup behavior.

**Workflow Before:**

1. Receiving workflow creates the PO-entry ViewModel even when PO entry is not the active step.
2. The constructor launches mock-data initialization.
3. A delayed auto-load runs in the background and can mutate workflow state from an inactive screen.

**Workflow After:**

1. PO-entry ViewModel stays idle until the PO step becomes active.
2. Mock-data autofill runs only when the user is actually entering the PO flow or explicitly requests it.
3. Debug convenience logic no longer changes startup behavior for unrelated steps.

**User Experience Change:**

- Developer and test environments get more predictable startup behavior.
- Users do not encounter surprise state changes caused by hidden mock routines.
- Debug-only helpers stop distorting perceived startup performance.

## Section 4: Diagnostics, Telemetry, and Validation

### S4-1: Add Structured Startup Phase Telemetry and Timing

**Priority:** Medium  
**Effort:** Low

**Affected Files**

- `App.xaml.cs`
- `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`
- `Module_Core/Services/Authentication/Service_UserLoginCoordinator.cs`
- `Module_Core/Services/Authentication/Service_Authentication.cs`

**Problem:**  
Startup regressions are currently hard to pinpoint. The code logs many events, but it does not emit a concise timing breakdown for each startup phase and each startup path. When startup is slow, there is no single source of truth showing whether the cost came from auth, roles, settings, workflow construction, or post-login view initialization.

**Recommended Alternatives:**

1. Preferred: time each phase with a stopwatch and emit structured logs containing phase name, elapsed milliseconds, and startup path.
2. Emit one startup summary event after the main window becomes interactive.
3. Add warning thresholds for slow phases so regressions surface in normal logs.
4. Include path metadata such as `new_user`, `personal_workstation`, `shared_terminal`, and `manual_switch_user`.

**Expected Outcome:**  
Future startup investigations will be evidence-driven, and performance work will be easier to prioritize and validate.

**Workflow Before:**

1. Startup runs through several phases.
2. Logs record individual events but not a unified timing story.
3. Slow startup reports require manual reconstruction from scattered messages.

**Workflow After:**

1. Startup phases are timed explicitly.
2. Each branch records structured phase durations and outcomes.
3. A summary event shows the full startup path and where time was spent.

**User Experience Change:**

- Fixes will land faster because startup bottlenecks are easier to prove.
- Regressions are more likely to be caught before users experience them for long.
- Support and development teams can explain startup issues with evidence instead of guesswork.

### S4-2: Add a Dedicated Startup Benchmark and Regression Test Harness

**Priority:** Medium  
**Effort:** Medium

**Affected Files**

- `MTM_Receiving_Application.Tests/`
- `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`
- `Module_Core/Services/Authentication/Service_UserLoginCoordinator.cs`
- `Module_Receiving/Views/View_Receiving_Workflow.xaml.cs`

**Problem:**  
Startup regressions are likely to return unless the main branches are testable in isolation. Right now there is no automated harness that simulates existing-user login, new-user creation, or shared-terminal login and asserts that the number of critical-path calls or elapsed budgets stay within reasonable bounds.

**Recommended Alternatives:**

1. Preferred: create startup orchestration tests with mocked auth, role, and settings services for each major branch.
2. Add a diagnostic mode that counts settings queries, role queries, and startup writes.
3. Establish baseline budgets for the critical path and fail tests when those budgets are exceeded.
4. Provide a developer-only benchmark command that measures cold startup and first-screen readiness locally.

**Expected Outcome:**  
Startup performance improvements will stay protected over time, and future changes will be able to prove they did not reintroduce critical-path bloat.

**Workflow Before:**

1. Startup changes are made manually.
2. Performance regressions are noticed only after users complain or after ad hoc investigation.
3. There is no repeatable guardrail for critical-path query counts or elapsed time.

**Workflow After:**

1. Startup branches are exercised by focused tests and benchmarks.
2. Expected budgets and call counts are checked automatically.
3. Regressions are detected during development instead of after release.

**User Experience Change:**

- Startup improvements are more likely to persist across future releases.
- Users benefit from steadier performance over time, not just one round of fixes.
- New startup slowdowns are caught earlier, reducing the chance that degraded behavior reaches production users.

## Section 5: Recommended Implementation Order

- [ ] `S2-1` - Rework new-user settings initialization first because it is the largest known first-login blocker and influences how later settings changes should be designed.
- [ ] `S2-4` - Remove blocking startup writes next so startup becomes mostly read-only before more invasive UI and workflow changes are made.
- [ ] `S2-5` - Collapse duplicate role bootstrap work after that to reduce redundant database traffic on every login path.
- [ ] `S2-2` - Lazy-load module root views and workflow step trees once the core login path is leaner, because this changes how the first screen of every module is composed.
- [ ] `S2-3` - Batch and cache receiving settings reads immediately after lazy loading so the first visible receiving step no longer causes a settings-query burst.
- [ ] `S2-6` - Make retry behavior startup-aware before tuning the remaining hot path so transient faults stop inflating every small settings and auth call.
- [ ] `S2-7` - Remove artificial splash and dialog delays after the real critical-path work has been reduced so the user can feel the gains immediately.
- [ ] `S3-1` - Replace constructor-fired async initialization next so the lazy-loading and settings changes execute deterministically.
- [ ] `S3-5` - Move PO-entry mock auto-load behind an explicit active-step initialization so mock-mode startup stops doing hidden work.
- [ ] `S3-3` - Make new-user and shared-terminal dialog bootstrapping non-blocking to clean up the slower exceptional startup branches.
- [ ] `S3-2` - Add timeouts and degraded-mode fallbacks once the startup phases are clearly separated and easier to classify as required versus optional.
- [ ] `S3-4` - Add manifest validation before telemetry and benchmarking so missing settings resources fail clearly instead of skewing later measurements.
- [ ] `S4-1` - Add startup phase telemetry after the main fixes are in place so baseline timings reflect the improved design instead of the current inflated path.
- [ ] `S4-2` - Finish with a benchmark and regression harness so the improved startup behavior stays protected.
