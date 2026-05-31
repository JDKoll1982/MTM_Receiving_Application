# 02-StartupFix-Logging-ReplaceDebugWriteLine

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement only this fix slice. Keep the change set focused, complete it end-to-end, and stop
only after the targeted validation passes.

## Feature

Replace all `Debug.WriteLine` calls in the startup path with structured Serilog logging.

## Implementation Order

**Order:** 02

## Why This Runs Second

Structured logging must be in place before any subsequent startup changes land. The fire-and-forget
fix (slice 03), the parallel query refactor (slice 06), and the workstation upsert change (slice 07)
all rely on diagnostic output being visible in the Serilog log files. Replacing `Debug.WriteLine` now
ensures that every later change produces observable production-grade output from day one.

## Confirmed Decisions

- All `System.Diagnostics.Debug.WriteLine` calls in the startup path must be replaced with the
  project's structured Serilog logger.
- The `IService_LoggingUtility` or `IService_ErrorHandler` injected instances are the correct
  logging surfaces — not bare `Log.Logger` statics.
- Stack-trace fragments that are currently written via `Debug.WriteLine` must either be surfaced as
  structured properties on a warning/error log entry or dropped if they are diagnostic scaffolding
  with no production value.
- No `Debug.WriteLine` calls should remain in `Service_OnStartup_AppLifecycle.cs` after this slice.
- `System.Diagnostics.Debug` should not be imported in the startup service after this change.

## Current Repo State

- 16 `System.Diagnostics.Debug.WriteLine` calls exist in
  `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`:
  - Lines 203–210: workstation type detection trace output.
  - Lines 446–509: role assignment per-step trace output including stack fragments.
- Serilog is fully configured at startup in `Infrastructure/Logging/SerilogConfiguration.cs`.
- `IService_ErrorHandler` is injected into `Service_OnStartup_AppLifecycle` and supports
  `HandleErrorAsync` and `HandleException`.
- `IService_LoggingUtility` is registered as a singleton and provides structured log methods.
- Log files are written to `logs/app-*.txt` via the Serilog rolling file sink.
- `Debug.WriteLine` output is invisible in production — it requires a debugger-attached process.

## Required Outcome

Replace every `System.Diagnostics.Debug.WriteLine` in `Service_OnStartup_AppLifecycle.cs` with the
appropriate structured Serilog logger call so that workstation detection and role assignment events
are observable in the production log file without a debugger attached.

## Primary Change Areas

- `Service_OnStartup_AppLifecycle.cs` — workstation detection `Debug.WriteLine` block (lines 203–210).
- `Service_OnStartup_AppLifecycle.cs` — role assignment `Debug.WriteLine` block (lines 446–509).
- Remove the `using System.Diagnostics;` import once all calls are removed, unless `Debug` is
  used elsewhere in the same file for a legitimate purpose.

## Files That Must Change

- `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`

## Recommended Target Shape

Map each `Debug.WriteLine` category to a structured log level:

| Current `Debug.WriteLine` site | Replacement log level | Rationale |
|---|---|---|
| Workstation type detected | `LogInfo` / Information | Normal startup milestone |
| Workstation shared-terminal flag | `LogInfo` / Information | Normal startup milestone |
| Role assignment step traces | `LogInfo` / Information | Normal startup steps |
| Role assignment failure or missing-role | `LogWarning` / Warning | Potential configuration issue |
| Stack trace fragments | Drop or `LogDebug` | Only needed during active debugging |

Use the injected `IService_LoggingUtility` for all replacements. Use `_errorHandler.HandleErrorAsync`
only for entries that represent an actionable warning or error visible to the user.

## Implementation Steps

1. Open `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`.
2. Search for all `Debug.WriteLine` calls (approximately 16).
3. For each call, determine the correct Serilog log level based on the table above.
4. Replace each `Debug.WriteLine(...)` with the corresponding `_logger.LogInfo(...)`,
   `_logger.LogWarning(...)`, or equivalent structured call using the injected logger.
5. For stack-trace fragments that have no production diagnostic value, remove the call entirely.
6. Remove the `using System.Diagnostics;` import if `Debug` is no longer referenced.
7. Build the solution and confirm zero errors.
8. Do a final search for `Debug.WriteLine` in the startup service to confirm none remain.

## Task Checklist

- [ ] Replace all `Debug.WriteLine` calls in the workstation detection block with structured log calls.
- [ ] Replace all `Debug.WriteLine` calls in the role assignment block with structured log calls.
- [ ] Remove or drop any stack-trace fragment lines that have no production diagnostic value.
- [ ] Remove `using System.Diagnostics;` if no longer needed.
- [ ] `dotnet build` succeeds with zero errors.
- [ ] Search for `Debug.WriteLine` in the file confirms zero remaining instances.

## Validation

- `grep -n "Debug.WriteLine" Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`
  must return zero results after the change.
- `dotnet build` must succeed with no new errors or warnings.
- Start the application locally and open `logs/app-*.txt` — workstation detection and role
  assignment events must appear in the log file.
- Confirm no `using System.Diagnostics;` import remains in the file unless `Debugger` or
  `Stopwatch` is in active use elsewhere in the same file.

## Guardrails

- Do not change the business logic of workstation detection or role assignment — only replace
  the logging mechanism.
- Do not introduce a new logging abstraction or logging helper in this slice.
- Do not change the Serilog configuration or sink setup.
- Do not remove any informational content that has genuine diagnostic value in production.
- Do not use `Log.Logger` static calls — always use the injected `IService_LoggingUtility` or
  `IService_ErrorHandler` instance.

## Completion Criteria

- Zero `Debug.WriteLine` calls remain in `Service_OnStartup_AppLifecycle.cs`.
- Workstation detection and role assignment milestones appear in the Serilog log file on a
  normal application startup.
- The solution builds cleanly.
