# 05-StartupFix-Startup-RemoveArtificialDelays

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement only this fix slice. Keep the change set focused, complete it end-to-end, and stop
only after the targeted validation passes.

## Feature

Remove all artificial Task.Delay calls from the startup path

## Implementation Order

05

## Why This Runs Fifth

The theme ordering fix (slice 04) must be in place first so this slice does not accidentally
re-expose the white-flash issue by changing the timing of the startup sequence. With the theme
applied correctly before the window is composited, the artificial delays are now safe to remove
without reintroducing the UX regression they were originally masking.

## Confirmed Decisions

- All `Task.Delay` calls introduced as workarounds inside the startup lifecycle must be removed.
- Three calls totalling 1,000 ms of mandatory artificial delay exist in the startup path.
- None of the delays are guarding real async operations — they are sleeps added to "let the UI
  settle" or work around initialization timing issues that no longer exist.
- If a delay was originally masking a race condition, identify the real fix and apply it instead
  of keeping the delay.
- Do not add new delays anywhere in the startup path.

## Current Repo State

- `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs` contains the following delays:
  - Line 98: `await Task.Delay(200)` — inserted after window content load, reason: "let UI thread settle"
  - Line 167: `await Task.Delay(300)` — inserted after authentication check, reason unknown
  - Line 289: `await Task.Delay(500)` — inserted before workstation configuration upsert
  - Total mandatory delay: 1,000 ms on every startup regardless of machine speed or state.
- The delays are not conditional — they always execute even when the operations they follow
  complete in under 10 ms.
- The theme flash fix is now handled by slice 04, so the 200 ms delay after content load is
  no longer needed as a visual stabilization workaround.

## Required Outcome

Remove all three `Task.Delay` calls from `Service_OnStartup_AppLifecycle.cs`. The application must
start at least 1 second faster than the current baseline. If removal of any delay exposes a
genuine race condition, fix the race condition rather than restoring the delay.

## Primary Change Areas

- `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs` — three `Task.Delay` call sites.

## Files That Must Change

- `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`

## Implementation Steps

1. Open `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`.
2. Search for all `Task.Delay` calls.
3. For each call:
   a. Read the surrounding comment (if any) explaining why the delay was added.
   b. Identify whether the delay was masking a genuine race condition or was simply added as
      a blanket wait for an operation that is now properly awaited.
   c. If it was masking a race condition, identify the real fix (e.g., await the missing
      operation, use a `TaskCompletionSource`, or subscribe to the correct ready event).
   d. If it was a blanket workaround with no real underlying issue, remove it directly.
4. Remove all three delay calls.
5. Build the solution and confirm zero errors.
6. Start the application three times and verify it reaches the main screen without any visible
   race-condition symptoms (blank panels, premature navigation, missing data on first render).
7. Measure startup time improvement (optional but recommended — compare task-manager process
   lifetime from launch to main window ready).

## Task Checklist

- [ ] Locate all `Task.Delay` calls in `Service_OnStartup_AppLifecycle.cs`.
- [ ] Review the comment/context for each delay to understand its original purpose.
- [ ] Remove the 200 ms post-content-load delay.
- [ ] Remove the 300 ms post-authentication delay.
- [ ] Remove the 500 ms pre-workstation-upsert delay.
- [ ] If any delay was masking a real race, apply the proper fix before removing the delay.
- [ ] `dotnet build` succeeds with zero errors.
- [ ] Application starts to the main screen on three consecutive runs without visible issues.

## Validation

- Search for `Task.Delay` in `Service_OnStartup_AppLifecycle.cs` — must return zero results.
- Start the application three consecutive times and confirm it reaches the main screen cleanly.
- Confirm the following do not regress after delay removal:
  - User authentication and login flow
  - Workstation configuration load
  - Main navigation pane population
  - Theme application (must remain flash-free from slice 04)
- `dotnet build` must succeed with no errors.

## Guardrails

- Do not restore any removed delay, even temporarily.
- Do not replace delays with `Thread.Sleep` — that is the same problem on the UI thread.
- If a genuine race condition is found, fix it properly (proper await, ready event, or
  initialization fence) rather than adding a new delay.
- Do not change any business logic beyond the delay removal and any required race-condition fixes.

## Completion Criteria

- Zero `Task.Delay` calls remain in `Service_OnStartup_AppLifecycle.cs`.
- The application starts at least 1,000 ms faster than the pre-fix baseline.
- No race conditions or visual defects are introduced by the removal.
- The solution builds cleanly.
