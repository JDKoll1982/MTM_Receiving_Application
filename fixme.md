# Startup Performance Fix List

Identified startup performance issues to be planned and implemented. Each fix is documented with the affected file(s), the problem, and the expected outcome. No code snippets are included — the AI model will plan and implement each fix.

---

## Fix 1 — Move `Host.Build()` Off the UI Thread

**Priority:** High  
**Effort:** Medium  
**File:** `App.xaml.cs`

**Problem:**  
`Host.CreateDefaultBuilder().Build()` is called synchronously inside the `App()` constructor. This runs before the WinUI framework is ready and blocks the UI thread while loading all configuration files, registering and validating every service across all 8 modules, and resolving the Serilog pipeline. The app window appears frozen until `Build()` completes.

**Expected Outcome:**  
The host build is kicked off on a background thread immediately when the app starts, and awaited in `OnLaunched` before proceeding. The UI thread is never blocked by DI container construction.

---

## Fix 2 — Remove Artificial `Task.Delay` Calls From the Startup Sequence

**Priority:** Critical  
**Effort:** Low  
**File:** `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`

**Problem:**  
Four hardcoded `Task.Delay` calls are baked into the startup sequence adding a guaranteed minimum of 1,000ms of artificial wait time on every launch regardless of how fast the machine or network is. The delays were added to "give the UI thread time to initialize" and for a cosmetic "Ready!" pause before showing the main window. These are unnecessary.

**Expected Outcome:**  
All artificial delays are removed. UI thread yielding is replaced with a proper non-blocking yield mechanism. The cosmetic "Ready!" pause is eliminated and the main window is shown immediately after startup completes.

---

## Fix 3 — Lazy-Register Settings Views Instead of Upfront Transient Registration

**Priority:** Medium  
**Effort:** Medium  
**File:** `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`

**Problem:**  
Over 60 View and ViewModel types across all Settings modules are registered as `Transient` at startup. The DI container validates every registration including recursive constructor dependency checks during `.Build()`. Most of these views are never visited during a typical session, so the registration cost is paid unconditionally at every launch.

**Expected Outcome:**  
Settings Views and ViewModels are registered lazily or via a factory pattern so the DI container does not validate their full dependency graph at build time. Views are only resolved when actually navigated to.

---

## Fix 4 — Replace Reflection-Based Service Locator in Settings Views With Constructor Injection

**Priority:** High  
**Effort:** Low  
**Files:**  
- `Module_Settings.Dunnage/Views/View_Settings_Dunnage_WorkflowHub.xaml.cs`  
- `Module_Settings.Receiving/Views/View_Settings_Receiving_WorkflowHub.xaml.cs`

**Problem:**  
Both views resolve `IService_LoggingUtility` by using `System.Reflection` to access the private `_host` field on `App` and then retrieve the `Services` property. This reflection-based service locator pattern runs on every view instantiation and adds unnecessary overhead on every navigation to these settings views. It also bypasses the DI container's constructor injection design.

**Expected Outcome:**  
Both views receive `IService_LoggingUtility` (and any other needed services) directly via constructor injection. The reflection-based lookup is removed entirely.

---

## Fix 5 — Eliminate Redundant Role Check DB Round-Trip on Every Startup

**Priority:** Medium  
**Effort:** Low  
**File:** `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`

**Problem:**  
`AssignDefaultRoleIfMissingAsync` is called on every startup for every authenticated user. It always performs a DB query to check whether the user already has roles assigned. For the overwhelming majority of users who already have roles, this is a wasted round-trip on every single launch. The method was intended as a one-time backwards-compatibility migration aid.

**Expected Outcome:**  
The role assignment check is either folded into the existing authentication query so no extra DB call is needed, or the result is cached so the check only runs once per user account rather than on every launch. The DB round-trip is eliminated for existing users with roles already assigned.

---

## Fix 6 — Parallelize User Authentication and Workstation Detection

**Priority:** Medium  
**Effort:** Low  
**File:** `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`

**Problem:**  
`AuthenticateByWindowsUsernameAsync` and `DetectWorkstationTypeAsync` are called sequentially during startup. These two operations are completely independent of each other — workstation detection does not depend on the user auth result. Running them serially wastes one full DB and/or system call latency worth of startup time.

**Expected Outcome:**  
Both calls are fired concurrently and awaited together. The total time for both operations equals the duration of the slower one rather than the sum of both.

---

## Fix 7 — Show Splash Screen Before `_host.StartAsync()`

**Priority:** Medium  
**Effort:** Low  
**File:** `App.xaml.cs`

**Problem:**  
In `OnLaunched`, `_host.StartAsync()` is awaited before the startup lifecycle service runs and before the splash screen is displayed. During this time the user sees a blank window with no visual feedback. The splash screen only appears after all hosted background services have started.

**Expected Outcome:**  
A minimal splash window is shown at the very beginning of `OnLaunched` before `_host.StartAsync()` is called. The user receives immediate visual feedback that the application is loading. The splash screen is then handed off to the lifecycle service for progress updates as before.