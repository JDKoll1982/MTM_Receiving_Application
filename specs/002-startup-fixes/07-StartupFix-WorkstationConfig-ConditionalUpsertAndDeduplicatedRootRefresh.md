# 07-StartupFix-WorkstationConfig-ConditionalUpsertAndDeduplicatedRootRefresh

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement only this fix slice. Keep the change set focused, complete it end-to-end, and stop
only after the targeted validation passes.

## Feature

Conditional workstation config upsert on change and deduplicated root folder refresh on startup

## Implementation Order

07

## Why This Runs Last

The parallel query refactor (slice 06) provides the `workstationConfig` result used in this fix.
This slice makes two surgical optimizations inside specific service methods. Running it last
ensures these fine-grained changes are evaluated against the final, clean startup sequence rather
than an intermediate state with delays or sequential queries still in place.

## Confirmed Decisions

- The workstation configuration upsert must only execute when the loaded config differs from the
  current stored values. It must not write on every startup unconditionally.
- The comparison must be a shallow equality check on the fields the upsert writes — it does not
  need to be a full deep-equals of the entire config model.
- `RefreshConfiguredRootFolderAsync` is called exactly twice on startup. One of those calls is
  redundant and must be removed.
- The surviving `RefreshConfiguredRootFolderAsync` call must be the one that runs after the
  workstation config is confirmed current (not the earlier one that runs before config is loaded).
- Do not change the behavior of `RefreshConfiguredRootFolderAsync` itself — only remove the
  extra call at the call site.

## Current Repo State

- Workstation config upsert location:
  - `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs` calls
    `UpsertWorkstationConfigAsync(config)` unconditionally after reading the current config.
  - The upsert executes on every startup regardless of whether any value changed.
  - The upsert is a MySQL stored-procedure write round trip (~300 ms) on every run.
- Duplicate root folder refresh locations:
  - First call: early in `Service_OnStartup_AppLifecycle.RunAsync`, before the parallel
    query block — line approximately 85.
  - Second call: after workstation config is loaded and confirmed — line approximately 310.
  - The second call is the correct one (runs with current workstation data); the first is
    the redundant one added during an earlier development phase.
- `Module_Core/Services/Authentication/Service_Authentication.cs` may contain
  workstation-upsert or change-check logic if it was partially moved there.

## Required Outcome

1. Workstation configuration is only written to MySQL when at least one field differs from the
   current stored value, eliminating the unconditional write on every startup.
2. `RefreshConfiguredRootFolderAsync` is called exactly once per startup (the post-config call),
   eliminating the redundant early call.

## Primary Change Areas

- `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`:
  - The `UpsertWorkstationConfigAsync` call site — add a change check guard.
  - The first (early) `RefreshConfiguredRootFolderAsync` call — remove it.

## Files That Must Change

- `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`
- `Module_Core/Services/Authentication/Service_Authentication.cs` — if the workstation
  upsert or change-detection logic is partially defined there.

## Recommended Target Shape

**Conditional upsert pattern:**

```csharp
// Before (unconditional write every startup)
var currentConfig = await _workstationService.GetWorkstationConfigAsync(machineName);
await _workstationService.UpsertWorkstationConfigAsync(currentConfig);

// After (write only when changed)
var currentConfig = await _workstationService.GetWorkstationConfigAsync(machineName);
var resolvedConfig = BuildExpectedWorkstationConfig(machineName);

if (!WorkstationConfigEquals(currentConfig, resolvedConfig))
{
    _logger.LogInfo("Workstation config changed — persisting update.");
    await _workstationService.UpsertWorkstationConfigAsync(resolvedConfig);
}
else
{
    _logger.LogInfo("Workstation config unchanged — skipping upsert.");
}
```

`WorkstationConfigEquals` must compare only the fields the upsert actually writes
(machine name, IP address, workstation type flags, and version). It must not compare
auto-generated or audit fields (created-date, updated-date, primary key).

**Deduplicated root folder refresh:**

Remove the early call entirely. Keep only the post-config call. Add a structured log
entry at the kept call site so the removal is observable in the startup log.

## Implementation Steps

1. Open `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`.
2. Locate the `UpsertWorkstationConfigAsync` call site.
3. Identify the fields that the upsert writes — these are the fields to compare.
4. Build or locate a `BuildExpectedWorkstationConfig` helper that produces the expected
   config for the current machine state (this may already exist as the input to the upsert).
5. Add a `WorkstationConfigEquals` comparison covering the write-relevant fields only.
6. Guard the upsert call behind the comparison.
7. Add a log entry for both the "changed" and "unchanged" branches.
8. Locate the two `RefreshConfiguredRootFolderAsync` calls in the same file.
9. Remove the first (early) call.
10. Add a log entry at the kept call site for startup observability.
11. Build the solution and confirm zero errors.
12. Start the application twice:
    - First run: config should write (new or first run after schema change).
    - Second run: config should be detected as unchanged and the upsert skipped.
    Verify in the log file.
13. Confirm `RefreshConfiguredRootFolderAsync` appears exactly once per startup in the log.

## Task Checklist

- [ ] Locate `UpsertWorkstationConfigAsync` call site.
- [ ] Identify the fields the upsert writes (the comparison scope).
- [ ] Add `WorkstationConfigEquals` or equivalent inline comparison.
- [ ] Guard the upsert behind the comparison.
- [ ] Add log entries for both changed and unchanged branches.
- [ ] Locate both `RefreshConfiguredRootFolderAsync` call sites.
- [ ] Remove the first (early) call.
- [ ] Add a log entry at the kept call.
- [ ] `dotnet build` succeeds with zero errors.
- [ ] Second startup logs "Workstation config unchanged — skipping upsert."
- [ ] `RefreshConfiguredRootFolderAsync` appears exactly once in the startup log.

## Validation

- Start the application twice consecutively without changing any machine settings.
- Open `logs/app-*.txt` after the second run.
- Confirm the log shows "skipping upsert" on the second run — the unconditional write is gone.
- Confirm `RefreshConfiguredRootFolderAsync` log entry appears exactly once per startup run.
- Confirm workstation configuration is still correctly loaded and available for downstream
  startup steps after this change.
- `dotnet build` must succeed with no errors.

## Guardrails

- Do not change the schema or stored procedure for the workstation upsert.
- Do not perform a deep-equals on fields that change on every read (e.g., timestamps).
- Do not remove the post-config `RefreshConfiguredRootFolderAsync` call — only the early one.
- Do not change the behavior of `RefreshConfiguredRootFolderAsync` itself.
- Do not change any workstation config field definitions or the workstation model.

## Completion Criteria

- The workstation config upsert MySQL write does not execute on startup when no config fields
  have changed since the last persisted values.
- `RefreshConfiguredRootFolderAsync` executes exactly once per startup.
- Both optimizations are visible as structured log entries.
- The solution builds cleanly.
- Workstation configuration is correct and available to all downstream startup consumers.
