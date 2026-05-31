# Shutdown And Connection Cleanup Audit

Date: 2026-03-26

Scope: read-only audit of application shutdown paths, `IHost` teardown, SQL Server Infor Visual connection usage, MySQL connection usage, timers, event subscriptions, and UI automation shutdown behavior.

Method:
- Read all `.serena/memories/*` files before analysis, with emphasis on `forbidden_practices` and `architectural_patterns`.
- Read relevant repo instructions and lifecycle/database files.
- Searched the repo for all exit mechanisms, `IHost` lifecycle calls, timer hooks, `SqlConnection`, `MySqlConnection`, `IDisposable`, and UI automation entry points.
- Cross-checked shutdown semantics against official Microsoft docs for `Environment.Exit`, WinUI `Application.Exit`, Generic Host shutdown, and .NET finalization behavior.

Key external runtime facts used in this audit:
- `Environment.Exit` terminates the process immediately, does not wait for other threads, and does not execute `finally` blocks in the current `try`/`catch` scope.
- .NET 5+ does not guarantee finalizers at application termination.
- WinUI 3 guidance says main window `Closed` is the place to clean up managed resources.
- Generic Host shutdown is graceful only when `StopAsync` is called; host-managed teardown requires explicit stop/dispose behavior.

## 1. Shutdown Path Inventory

| Trigger | File | Method | Exit Mechanism | `IHost.StopAsync()` Called? | `EndSessionAsync()` Called? | `StopTimeoutMonitoring()` Called? | Open Connection Risk |
| --- | --- | --- | --- | --- | --- | --- | --- |
| User clicks `[X]` on main window | `MainWindow.xaml.cs`, `App.xaml.cs` | `MainWindow_Closed`, `OnMainWindowClosed` | `Application.Current.Exit()` from `MainWindow_Closed` after `Window.Closed` fires | No | Yes, via `App.OnMainWindowClosed`, but it is `async void` and not awaited | Only if `EndSessionAsync("manual_close")` completes | Medium: normal path attempts cleanup, but `Application.Exit` is triggered in a sibling closed handler and host teardown never runs |
| Session timeout | `Service_UserSessionManager.cs`, `App.xaml.cs`, `MainWindow.xaml.cs` | `OnTimerTick`, `OnSessionTimedOut`, `MainWindow_Closed`, `OnMainWindowClosed` | `SessionTimedOut` event -> `MainWindow?.Close()` -> `Application.Current.Exit()` | No | Yes, same as normal close | Yes, `OnTimerTick` calls `StopTimeoutMonitoring()` after raising timeout | Medium: timer is stopped before close, but session end logging still depends on an `async void` handler finishing during shutdown |
| New-user setup canceled | `Service_OnStartup_AppLifecycle.cs` | `StartAsync` | Programmatically closes splash, closes main window, then calls `Application.Current.Exit()` | No | No | No | Medium: no active session yet, but startup work is not canceled and host disposal is skipped |
| Shared terminal login max attempts exceeded | `Service_OnStartup_AppLifecycle.cs` | `StartAsync` | `AppInstance.GetCurrent().UnregisterKey()` + `Environment.Exit(0)` | No | No | No | High: immediate process termination skips coordinated shutdown entirely |
| Shared terminal login canceled | `Service_OnStartup_AppLifecycle.cs` | `StartAsync` | `Environment.Exit(0)` | No | No | No | High: immediate termination skips host/service cleanup |
| Shared terminal dialog closes unexpectedly | `Service_OnStartup_AppLifecycle.cs` | `StartAsync` | `Environment.Exit(0)` | No | No | No | High: immediate termination skips host/service cleanup |
| Authenticated user unexpectedly null at startup end | `Service_OnStartup_AppLifecycle.cs` | `StartAsync` | `Environment.Exit(0)` | No | No | No | High: immediate termination skips host/service cleanup |
| Startup exception | `Service_OnStartup_AppLifecycle.cs` | `StartAsync` catch block | Splash is closed programmatically, then `Environment.Exit(1)` | No | No | No | Critical: if the exception occurs after `_sessionManager.StartTimeoutMonitoring()` at line 295, the timer and session end path are skipped |
| Splash screen manually closed during startup, including Alt+F4 | `View_Shared_SplashScreenWindow.xaml.cs` | `SplashScreenWindow_Closed` | `Application.Current.Exit()` when `IsProgrammaticClose == false` | No | No | No | High: startup tasks are not canceled; any in-flight DB work has no coordinated shutdown path |

Observations:
- `App` builds `_host` in `App.xaml.cs` but there is no `_host.StartAsync()`, `_host.StopAsync()`, or `_host.Dispose()` anywhere in the repo.
- `App.OnLaunched` subscribes to `SessionTimedOut` and `MainWindow.Closed` only after `await startupService.StartAsync()`. Any exit path taken during startup runs before those `App`-level shutdown subscriptions are attached.

## 2. Infor Visual Connection Audit

### Infor Visual DAO methods that open `SqlConnection`

| DAO | Method | Line | `await using` connection? | `await using` command/reader? | Cancellation-aware? | `ApplicationIntent=ReadOnly` enforced? | Exit risk |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `Dao_InforVisualConnection` | `TestConnectionAsync` | 40 | Yes | n/a | No | Indirect only via supplied connection string; this DAO does not validate in constructor | Medium |
| `Dao_InforVisualConnection` | `GetPOWithPartsAsync` | 70 | Yes | Yes | No | Indirect only | Medium |
| `Dao_InforVisualConnection` | `ValidatePoNumberAsync` | 129 | Yes | Yes | No | Indirect only | Medium |
| `Dao_InforVisualConnection` | `GetPartByNumberAsync` | 166 | Yes | Yes | No | Indirect only | Medium |
| `Dao_InforVisualConnection` | `SearchPartsByDescriptionAsync` | 204 | Yes | Yes | No | Indirect only | Medium |
| `Dao_InforVisualConnection` | `GetOutsideServiceHistoryByPartAsync` | 290 | Yes | Yes | No | Indirect only | Medium |
| `Dao_InforVisualConnection` | `FuzzySearchPartsByIdAsync` | 356 | Yes | Yes | No | Indirect only | Medium |
| `Dao_InforVisualConnection` | `GetPurchaseOrdersByPartAsync` | 409 | Yes | Yes | No | Indirect only | Medium |
| `Dao_InforVisualConnection` | `FuzzySearchVendorsByNameAsync` | 468 | Yes | Yes | No | Indirect only | Medium |
| `Dao_InforVisualConnection` | `GetOutsideServiceHistoryByVendorAsync` | 531 | Yes | Yes | No | Indirect only | Medium |
| `Dao_InforVisualConnection` | `GetPartsByVendorAsync` | 599 | Yes | Yes | No | Indirect only | Medium |
| `Dao_InforVisualConnection` | `GetOutsideServiceHistoryByVendorAndPartAsync` | 659 | Yes | Yes | No | Indirect only | Medium |
| `Dao_InforVisualConnection` | `FuzzySearchLocationsByWarehouseAsync` | 732 | Yes | Yes | No | Indirect only | Medium |
| `Dao_InforVisualConnection` | `PartExistsAsync` | 800 | Yes | Yes | No | Indirect only | Medium |
| `Dao_InforVisualConnection` | `LocationExistsAsync` | 834 | Yes | Yes | No | Indirect only | Medium |
| `Dao_InforVisualPart` | `GetByPartNumberAsync` | 57 | Yes | Yes | No | Yes, constructor validates `ApplicationIntent=ReadOnly` | Medium |
| `Dao_InforVisualPart` | `SearchPartsByDescriptionAsync` | 95 | Yes | Yes | No | Yes | Medium |
| `Dao_InforVisualPO` | `GetByPoNumberAsync` | 57 | Yes | Yes | No | Yes, constructor validates `ApplicationIntent=ReadOnly` | Medium |
| `Dao_InforVisualPO` | `ValidatePoNumberAsync` | 112 | Yes | Yes | No | Yes | Medium |

### Infor Visual service/model findings

- `Service_InforVisualConnect` is service-layer only in the inspected code paths. It delegates to `Dao_InforVisualConnection` and does not directly open `SqlConnection`.
- `Model_InforVisualConnection.GetConnectionString()` includes `ApplicationIntent=ReadOnly`.
- `appsettings.json` and `Helper_Database_Variables.GetInforVisualConnectionString()` also include `ApplicationIntent=ReadOnly`.

### Infor Visual risk conclusions

- All current Infor Visual query methods use `await using` consistently for `SqlConnection`, `SqlCommand`, and `SqlDataReader`.
- None of the Infor Visual query methods pass a `CancellationToken` into `OpenAsync`, `ExecuteReaderAsync`, `ExecuteScalarAsync`, or polling logic. There is no app-lifetime cancellation source that can tell in-flight queries to stop before exit.
- `Dao_InforVisualConnection` does not perform the same constructor-level read-only enforcement that `Dao_InforVisualPO` and `Dao_InforVisualPart` do. Today it is safe by convention because the configured connection strings include the flag, but the guard is missing in the DAO that is used by `IService_InforVisual`.
- If `Environment.Exit` is called while one of these methods is executing on another thread, the normal `await using` unwind is not guaranteed to complete. The process termination will end the connection, but not through orderly application cleanup.

## 3. MySQL Connection Audit

### Direct `MySqlConnection` instantiations

| File | Method(s) / operation(s) | Direct `new MySqlConnection`? | `await using`? | Cancellation-aware? | Shutdown notes |
| --- | --- | --- | --- | --- | --- |
| `Module_Core/Data/Authentication/Dao_User.cs` | `CreateNewUserAsync`, `UpdateAsync`, `UpdateVisualCredentialsAsync`, `DeactivateAsync` | Yes | Yes | No | Orderly disposal on successful completion; abrupt `Environment.Exit` can still cut work off |
| `Module_Receiving/Data/Dao_ReceivingLoad.cs` | `SaveLoadsAsync`, `UpdateLoadsAsync`, `DeleteLoadsAsync`, `ClearLabelDataToHistoryAsync` | Yes | Yes | No | Direct opens are wrapped correctly; no shutdown-linked cancellation |
| `Module_Receiving/Data/Dao_ReceivingLabelData.cs` | `SaveLoadsAsync`, `ClearLabelDataToHistoryAsync`, `UpdateCurrentLabelDataAsync` | Yes | Yes | No | Direct opens are wrapped correctly; no shutdown-linked cancellation |
| `Module_Dunnage/Data/Dao_DunnageLabelData.cs` | `InsertBatchAsync`, `ClearToHistoryAsync` | Yes | Yes | No | Direct opens are wrapped correctly; no shutdown-linked cancellation |
| `Module_Volvo/Data/Dao_VolvoShipment.cs` | `InsertAsync` | Yes | Yes | No | Direct open is wrapped correctly |
| `Module_Volvo/Data/Dao_VolvoPart.cs` | `DeactivateAsync` | Yes | Yes | No | Direct open is wrapped correctly |
| `Module_Volvo/Data/Dao_VolvoLabelHistory.cs` | `ClearToHistoryAsync` | Yes | Yes | No | Direct open is wrapped correctly |
| `Module_Core/Helpers/Database/Helper_Database_StoredProcedure.cs` | `ExecuteAsync`, `ExecuteNonQueryAsync`, `ExecuteSingleAsync`, `ExecuteListAsync`, `ExecuteDataTableAsync` | Yes | Yes | No | This helper centralizes most MySQL work and disposes correctly on normal completion |
| `Module_Core/Helpers/Database/Helper_Database_StoredProcedure.cs` | `ExecuteInTransactionAsync` | Uses existing connection, no new connection | Command only uses `await using` | No | Depends on caller to own transaction/connection lifetime |
| `Module_Settings.DeveloperTools/Services/RunSettingsDbTestCommandHandler.cs` | `Handle` | Yes | Yes | Yes, `OpenAsync(cancellationToken)` | This is the only inspected MySQL open that is cancellation-aware |
| `Module_Volvo/Services/Service_Volvo.cs` | `SaveShipmentAsync` | Yes | Yes | No | Opens connection and transaction inside service layer, not DAO layer; disposal is normal only if method completes |

### MySQL pool findings

- No calls to `MySqlConnection.ClearAllPools()` or `MySqlConnection.ClearPool()` were found anywhere in the repo.
- Connector/NET pooling is enabled by default, and connection disposal returns connections to the pool rather than physically destroying every native connection immediately.
- On normal method completion, `await using` returns pooled connections correctly.
- On `Environment.Exit`, there is no repo-level attempt to flush or clear MySQL pools before process termination.

### MySQL risk conclusions

- Normal-case disposal is generally strong: direct opens consistently use `await using`.
- Abrupt process termination remains a risk because almost all operations are non-cancellable and no pool-clear hook exists on shutdown.
- `Service_Volvo.SaveShipmentAsync` opens a `MySqlConnection` and transaction directly in a service. That is an architectural deviation from the project’s DAO pattern, and it also means shutdown behavior for that transaction is not isolated to DAO cleanup conventions.

### Additional MySQL closure edge cases

- Connector/NET pooling keeps the native server connection alive after `MySqlConnection` disposal, so a logical close is not an immediate socket teardown. This matters during app exit because the client-side `Dispose` may finish before the server-side pooled connection is actually reclaimed.
- Oracle’s Connector/NET documentation states that pooled idle connections are cleaned up by a background job every three minutes. That means resource release after shutdown is not immediate unless the application explicitly clears pools.
- Each unique MySQL connection string creates a separate connection pool. If helper methods, app settings, and any hardcoded strings drift apart, shutdown can leave multiple independent pools to be reclaimed separately.
- General `DbConnection.Close` semantics roll back pending local transactions before the connection is returned to the pool. That protects database integrity on orderly disposal, but abrupt process termination still prevents the app from observing or reporting that rollback path cleanly.
- Connections that are not explicitly disposed are not guaranteed to return to the pool promptly. Under exception-heavy exit paths, that can temporarily exhaust the pool and turn shutdown timing bugs into follow-on connection timeout failures on the next run.
- Because pooled connections are reused rather than fully disconnected, server-side login/logout audit behavior does not map one-to-one with logical open/close calls. During shutdown analysis, a missing logout event is not proof that the application leaked a logical connection.

## 4. `IHost` Lifecycle Audit

### What the app does today

- `App` builds `_host` in `App.xaml.cs`.
- No `_host.StartAsync()` call exists.
- No `_host.StopAsync()` call exists.
- No `_host.Dispose()` call exists.
- No `IHostApplicationLifetime`, `ApplicationStopping`, `ProcessExit`, or linked application shutdown token infrastructure exists anywhere in repo code.
- No project `IHostedService` implementations were found in production code.

### Consequences

- Because `StopAsync` is never called, there is no explicit Generic Host shutdown phase.
- Because `_host.Dispose()` is never called, the DI root service provider is not explicitly disposed.
- The current codebase does not define app-owned singleton services implementing `IDisposable` or `IHostedService`; therefore there is no project-defined `Dispose`/`StopAsync` implementation currently being skipped.
- Even so, missing `_host.Dispose()` is still a lifecycle gap because it leaves teardown of the host/service-provider graph implicit and process-driven instead of explicit and ordered.

### App-defined singleton services with lifecycle-sensitive state but no `IDisposable`

These are not skipped because of missing `Dispose` interfaces, but they still rely on orderly shutdown rather than host-managed teardown:

- `IService_UserSessionManager` / `Service_UserSessionManager`: owns `_timeoutTimer` and session state.
- `IService_DunnageWorkflow` / `Service_DunnageWorkflow`: subscribes to `SessionTimedOut`.
- `IService_UIAutomation` / `Service_UIAutomation`: would need coordinated cancellation if ever used in long-running automation.
- `IService_SettingsWindowHost` / `Service_SettingsWindowHost`: tracks owned window references.
- `MainWindow`: registered as singleton.

## 5. Timer And Event Handler Audit

### `_timeoutTimer`

- `_timeoutTimer` is created in `Service_UserSessionManager.StartTimeoutMonitoring()`.
- Normal manual close path: timer stops only if `App.OnMainWindowClosed` finishes `EndSessionAsync()`.
- Session-timeout path: timer is stopped inside `OnTimerTick()` immediately after raising `SessionTimedOut`, before the window closes.
- Startup-exception path after line 295 in `Service_OnStartup_AppLifecycle.StartAsync`: timer can remain active until the process is terminated by `Environment.Exit(1)` because neither `EndSessionAsync()` nor `StopTimeoutMonitoring()` is called.

### Other timer findings

- `ViewModel_Dunnage_AddTypeDialog` creates `_validationTimer` and reuses it, but there is no explicit teardown path or unsubscribe from `Tick`.
- `VolvoShipmentEditDialog` creates a short-lived local `DispatcherTimer` for a 3-second success message and stops it in its own tick handler. This is low risk.

### Event subscription findings

- `App.OnLaunched` subscribes to `sessionManager.SessionTimedOut += OnSessionTimedOut` and `MainWindow.Closed += OnMainWindowClosed`. No unsubscribe exists.
- `Service_DunnageWorkflow` subscribes to `_sessionManager.SessionTimedOut += OnSessionTimedOut` in its singleton constructor. No unsubscribe exists.
- `ViewModel_Dunnage_Review` subscribes to `_workflowService.StepChanged += OnWorkflowStepChanged` and has a `Dispose()` that unsubscribes, but no call site for `Dispose()` was found anywhere in production code.
- `View_Dunnage_ReviewView` has a parameterless constructor that resolves `ViewModel_Dunnage_Review` via `App.GetService<ViewModel_Dunnage_Review>()`, but the view does not dispose the ViewModel on unload/close.
- Receiving transient ViewModels subscribe to singleton workflow events with no matching unsubscribe found anywhere:
  - `ViewModel_Receiving_Workflow`
  - `ViewModel_Receiving_LoadEntry`
  - `ViewModel_Receiving_HeatLot`
  - `ViewModel_Receiving_WeightQuantity`
  - `ViewModel_Receiving_PackageType`
  - `ViewModel_Receiving_ManualEntry`
  - `ViewModel_Receiving_Review`
  - `ViewModel_Receiving_EditMode`
- Additional Dunnage transient ViewModels also subscribe to singleton workflow events with no unsubscribe found in the inspected code.
- `Service_ViewModelRegistry` is not the cause of those leaks. It stores weak references only.

### Timer/event cleanup conclusion

- `_timeoutTimer` is handled correctly only on the happy path and on session timeout.
- The app currently has several event-subscription leaks in transient ViewModels, and `ViewModel_Dunnage_Review.Dispose()` appears to be dead code in production.

## 6. UI Automation Audit

### Current code state

- `Service_UIAutomation` is registered as a singleton.
- It exposes cancellable methods such as `FindWindowAsync`, `WaitForWindowToCloseAsync`, `DismissPopupIfPresentAsync`, `FillFieldAsync`, and `ClearAndFillFieldAsync`.
- Each polling method checks `CancellationToken` and uses cancellable `Task.Delay`.
- No call sites to `IService_UIAutomation` or `Service_UIAutomation` were found outside the service/interface/DI registration itself.

### Shutdown implications

- There is no current production path in the repo where an active UI automation workflow is definitely running during app shutdown, because no consumer code was found.
- However, there is also no application-lifetime `CancellationTokenSource`, no linked cancellation with app shutdown, and no `IHostApplicationLifetime.ApplicationStopping` subscription.
- If this service is used in future with default tokens, application shutdown would not signal cancellation into its loops. A process-level exit could therefore interrupt automation after focus changes or keystroke sends but before post-action verification.

### UI automation conclusion

- Current active shutdown risk: low, because the service appears dormant in this repo snapshot.
- Design risk if activated later: medium, because cancellation is opt-in per call and not linked to app shutdown.

## 7. Gaps And Risk Summary

### 🔴 Critical

1. `_host` is never explicitly stopped or disposed.
   - Evidence: `_host` is built in `App.xaml.cs`; no `_host.StopAsync()` or `_host.Dispose()` calls exist anywhere.
   - Impact: shutdown order is process-driven, not host-driven; the DI root is never explicitly torn down.

2. `Environment.Exit(...)` is used in multiple startup paths and in the startup exception handler.
   - Evidence: `Service_OnStartup_AppLifecycle.cs` lines 261, 273, 285, 323, 354.
   - Impact: abrupt termination bypasses coordinated cleanup; if failure occurs after `_sessionManager.StartTimeoutMonitoring()` at line 295, timer/session cleanup is skipped.

3. `ViewModel_Dunnage_Review.Dispose()` is never called.
   - Evidence: `Dispose()` exists, but no production call site was found.
   - Impact: `_workflowService.StepChanged` subscription is retained, leaking the transient ViewModel.

### 🟡 Warning

1. Main-window close cleanup depends on `async void` event handling.
   - Evidence: `MainWindow_Closed` calls `Application.Current.Exit()` while `App.OnMainWindowClosed` separately performs `await sessionManager.EndSessionAsync("manual_close")`.
   - Impact: session end logging and timer stop are attempted, but not awaited by the framework as an orderly shutdown phase.

2. Splash-screen manual close has no startup cancellation path.
   - Evidence: `View_Shared_SplashScreenWindow.SplashScreenWindow_Closed` calls `Application.Current.Exit()` directly.
   - Impact: in-flight startup/auth/database operations are not canceled through a shared token.

3. `Dao_InforVisualConnection` does not validate `ApplicationIntent=ReadOnly` in its constructor.
   - Evidence: constructor at line 23 accepts any string; unlike `Dao_InforVisualPO` and `Dao_InforVisualPart`, no `SqlConnectionStringBuilder` enforcement exists.
   - Impact: current safety depends on configuration discipline rather than DAO-level guardrails.

4. Most DB work is not cancellation-aware.
   - Evidence: Infor Visual DAO methods and `Helper_Database_StoredProcedure` use parameterless async open/execute methods.
   - Impact: shutdown cannot ask in-flight operations to stop gracefully before app exit.

5. MySQL pools are never explicitly cleared on shutdown.
   - Evidence: no `ClearAllPools`/`ClearPool` calls found.
   - Impact: normal disposal is fine, but abrupt exit has no explicit pool flush/cleanup step.

6. Many transient ViewModels subscribe to singleton workflow events without unsubscribe paths.
   - Evidence: receiving and dunnage ViewModels subscribe to `StepChanged`; only `ViewModel_Dunnage_Review` even defines a `Dispose` method.
   - Impact: memory/event-handler leaks during runtime, independent of final app exit.

7. `Service_Volvo.SaveShipmentAsync` opens MySQL connection/transaction directly in service layer.
   - Evidence: direct `new MySqlConnection(...)` at line 576.
   - Impact: architectural drift and one more shutdown-sensitive connection site outside DAO conventions.

### 🟢 OK

1. Infor Visual query methods consistently use `await using` for `SqlConnection`, `SqlCommand`, and `SqlDataReader` on the normal completion path.
2. Direct MySQL opens inspected in DAOs/helpers also consistently use `await using` on the normal completion path.
3. Session-timeout path stops `_timeoutTimer` before closing the window.
4. `Service_ViewModelRegistry` uses weak references, so it is not the source of ViewModel retention.
5. `Service_UIAutomation` supports cancellable polling APIs, even though app-lifetime cancellation is not yet wired in.

## 8. Recommended Fixes (Do Not Implement Yet)

1. Add an application shutdown coordinator in `App.xaml.cs` that performs graceful teardown before final exit.
   - High-level fix: centralize shutdown into a single async path that calls `IService_UserSessionManager.EndSessionAsync(...)`, then `_host.StopAsync(...)`, then `_host.Dispose()`.
   - Rationale: removes duplicated exit behavior and creates one ordered shutdown sequence.

2. Replace direct `Environment.Exit(...)` usage in startup service with a host-aware shutdown request.
   - High-level fix: have `Service_OnStartup_AppLifecycle` signal a shutdown service or callback instead of killing the process directly.
   - Rationale: preserves MVVM/service-layer architecture while allowing app-level cleanup to run.

3. Make `MainWindow` close flow defer `Application.Current.Exit()` until async cleanup completes.
   - High-level fix: route `MainWindow_Closed` / app exit through one app-level shutdown routine instead of calling `Application.Current.Exit()` from the window closed handler immediately.
   - Rationale: avoids racing `async void` cleanup against app termination.

4. Add constructor-level read-only validation to `Dao_InforVisualConnection`.
   - High-level fix: mirror the `SqlConnectionStringBuilder` + `ApplicationIntent=ReadOnly` check already used in `Dao_InforVisualPO` and `Dao_InforVisualPart`.
   - Rationale: makes the primary Infor Visual DAO safe by construction.

5. Introduce an application-lifetime `CancellationTokenSource` for shutdown-sensitive async work.
   - High-level fix: expose a shared shutdown token from the app/service layer and pass it into long-running DB or automation operations where supported.
   - Rationale: allows graceful cancellation instead of relying on process termination.

6. Add deterministic unsubscribe/dispose handling for transient ViewModels that subscribe to singleton events.
   - High-level fix: either make those ViewModels disposable and ensure views dispose them, or move subscriptions into services with app lifetime.
   - Special note: `ViewModel_Dunnage_Review.Dispose()` is already present; it just needs a real disposal path.

7. Add explicit timer cleanup for transient dialog ViewModels.
   - High-level fix: stop/unsubscribe transient timers when the owning view or ViewModel is torn down.
   - Rationale: prevents UI-lifetime leaks even if the process stays alive for a long session.

8. Decide whether MySQL pool clearing is required for your operational model.
   - High-level fix: if desired, call `MySqlConnection.ClearAllPools()` during the centralized shutdown path after DB work has completed.
   - Rationale: not required for correctness in the normal path, but it makes shutdown intent explicit.

9. Move `Service_Volvo.SaveShipmentAsync` transaction ownership behind DAO abstractions.
   - High-level fix: keep orchestration in the service layer, but push direct connection/transaction management into DAO-level methods or a DAO transaction façade.
   - Rationale: aligns with repo architecture and keeps connection lifecycle rules in the data layer.

## Bottom Line

The application is generally careful about normal-case connection disposal inside DAO/helper methods, but shutdown is not coordinated. The biggest weaknesses are abrupt `Environment.Exit(...)` paths, the lack of any `_host.StopAsync()` / `_host.Dispose()` call, and transient ViewModel event subscriptions that never get torn down. Infor Visual connections are read-only by configuration today, but the main DAO used by `IService_InforVisual` is still missing the constructor-level guard that the smaller specialized DAOs already have.