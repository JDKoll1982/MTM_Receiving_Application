# 06-StartupFix-Authentication-ParallelStartupQueries

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement only this fix slice. Keep the change set focused, complete it end-to-end, and stop
only after the targeted validation passes.

## Feature

Run independent authentication and workstation startup queries in parallel

## Implementation Order

06

## Why This Runs Sixth

The artificial delays are now removed (slice 05), so the startup sequence is running at its true
sequential speed. This slice adds the next layer of improvement by running independent queries
concurrently. It must come after the delay removal to avoid masking the parallelism gain with
artificial serial waits.

## Confirmed Decisions

- Independent startup queries that do not depend on each other's results may run in parallel
  using `Task.WhenAll`.
- The following startup queries were confirmed independent and safe to parallelize:
  - `AuthenticateUserAsync` (MySQL — reads user record)
  - `GetWorkstationConfigAsync` (MySQL — reads workstation record)
  - `CheckSoftwareVersionAsync` (MySQL — reads version manifest)
- These three queries currently run sequentially and each takes 150–400 ms independently,
  totalling up to 1,200 ms in sequence. Running them in parallel reduces the wall-clock cost
  to approximately the slowest single query.
- Do not parallelize any step that depends on the result of another startup step.
- Do not parallelize any step that writes to shared mutable state that later steps read.

## Current Repo State

- Sequential startup query order in `Service_OnStartup_AppLifecycle.cs`:
  ```
  await AuthenticateUserAsync()          // ~200 ms
  await GetWorkstationConfigAsync()      // ~350 ms
  await CheckSoftwareVersionAsync()      // ~150 ms
  ```
- Each of these calls is currently `await`ed individually in sequence.
- All three return independent result objects consumed separately after the awaits.
- Authentication result is used to determine role assignment (a later step).
- Workstation config result is used during workstation upsert (a later step, not in this query group).
- Software version result is used for the update-available notification (a later step).
- All three queries target MySQL via stored procedures.
- Relevant services:
  - `Module_Core/Services/Authentication/Service_Authentication.cs`
  - `Module_Core/Services/Authentication/Service_UserLoginCoordinator.cs`
  - `Module_Core/Services/Versioning/Service_SoftwareVersionMonitor.cs`

## Required Outcome

Replace the three sequential await calls with a single `Task.WhenAll` so all three queries
start at the same moment and the startup path continues when the slowest one completes.

## Primary Change Areas

- `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs` — the sequential
  authentication, workstation-config, and version-check await block.

## Files That Must Change

- `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`

## Recommended Target Shape

Replace the sequential awaits with a `Task.WhenAll` pattern:

```csharp
// Before (sequential — up to ~700 ms)
var authResult = await _authService.AuthenticateUserAsync(credentials);
var workstationConfig = await _workstationService.GetWorkstationConfigAsync(machineName);
var versionStatus = await _versionMonitor.CheckSoftwareVersionAsync();

// After (parallel — wall-clock cost ≈ slowest single query, ~350 ms)
var authTask = _authService.AuthenticateUserAsync(credentials);
var workstationTask = _workstationService.GetWorkstationConfigAsync(machineName);
var versionTask = _versionMonitor.CheckSoftwareVersionAsync();

await Task.WhenAll(authTask, workstationTask, versionTask);

var authResult = authTask.Result;
var workstationConfig = workstationTask.Result;
var versionStatus = versionTask.Result;
```

Use `.Result` to unwrap after `WhenAll` — it is safe here because `WhenAll` already completed
all tasks before we reach the unwrap. Do not use `await taskName` after `WhenAll` as that
re-awaits an already-completed task unnecessarily.

## Implementation Steps

1. Open `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`.
2. Locate the sequential block where `AuthenticateUserAsync`, `GetWorkstationConfigAsync`,
   and `CheckSoftwareVersionAsync` are awaited in sequence.
3. Verify the three queries are truly independent at this point in the sequence:
   - None of them reads state written by another in this same block.
   - None of them requires the caller to be on a specific thread or scheduler.
4. Refactor to the `Task.WhenAll` pattern shown in `## Recommended Target Shape`.
5. Confirm the result variables (`authResult`, `workstationConfig`, `versionStatus`) are
   used in the same downstream steps as before.
6. Check whether exception propagation is acceptable: `Task.WhenAll` will throw an
   `AggregateException` if any task faults. Ensure the caller's try-catch handles
   `AggregateException` or unwraps it with `.InnerException`.
7. Build the solution and confirm zero errors.
8. Start the application and confirm all three startup results are correctly consumed:
   - User is authenticated with the correct role.
   - Workstation config is loaded.
   - Version check notification fires if a new version is available.

## Task Checklist

- [ ] Locate the three sequential startup query awaits in `Service_OnStartup_AppLifecycle.cs`.
- [ ] Verify the three queries are independent (no shared mutable state between them).
- [ ] Refactor to `Task.WhenAll` pattern.
- [ ] Confirm result variables are correctly unwrapped after `WhenAll`.
- [ ] Confirm exception handling covers `AggregateException` from `WhenAll`.
- [ ] `dotnet build` succeeds with zero errors.
- [ ] Authentication, workstation config, and version check all function correctly after refactor.
- [ ] Confirm no other startup step between these three and the next consumer was silently
  depending on the sequential ordering.

## Validation

- Start the application and confirm:
  - User is authenticated and role is assigned correctly.
  - Workstation configuration is available for the subsequent upsert step.
  - Software version check result is used correctly (notification shown if applicable).
- Measure the elapsed time of the parallel block and confirm it is materially faster than
  the sequential sum (should be close to the slowest single query, not the total).
- `dotnet build` must succeed with no errors.
- Run three consecutive startups and confirm no race conditions or missing-data symptoms.

## Guardrails

- Do not parallelize any step that writes to shared application state read by another parallel step.
- Do not parallelize any step that depends on the result of another step in the same `WhenAll`.
- Do not use `ConfigureAwait(false)` without confirming it is safe for every task in the group.
- Do not change the downstream consumers of `authResult`, `workstationConfig`, or `versionStatus`.
- Do not parallelize the role-assignment or workstation-upsert steps — those depend on the
  results from this parallel block and must remain sequential after the `WhenAll`.

## Completion Criteria

- The three independent startup queries run concurrently via `Task.WhenAll`.
- Wall-clock startup time is reduced by the gap between the sequential total and the slowest
  single query (approximately 350–500 ms saved).
- Authentication, workstation config, and version checks all produce correct results.
- The solution builds cleanly.
