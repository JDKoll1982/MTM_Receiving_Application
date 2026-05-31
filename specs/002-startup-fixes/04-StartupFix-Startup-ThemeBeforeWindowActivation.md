# 04-StartupFix-Startup-ThemeBeforeWindowActivation

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement only this fix slice. Keep the change set focused, complete it end-to-end, and stop
only after the targeted validation passes.

## Feature

Apply user theme before the main window is activated to eliminate white-frame flash on startup

## Implementation Order

04

## Why This Runs Fourth

The credential and logging fixes (slices 01–03) are complete and do not touch the window-activation
sequence. This slice reorders two lines in `App.xaml.cs` / the startup lifecycle. It is isolated,
low-risk, and produces an immediately visible UX improvement that does not depend on the delay
removal (slice 05) or the parallel query changes (slice 06).

## Confirmed Decisions

- The user-preference theme must be applied before `window.Activate()` is called.
- The theme should be applied to `MainWindow` or `App.Current.RequestedTheme` before activation.
- The current behavior is: window activates with the default light theme, then theme is applied
  asynchronously — causing a white-frame flash on dark-themed setups.
- Do not move the theme-read database call to a blocking pre-startup position; only reorder the
  window-visible moment relative to when the already-loaded theme value is applied.

## Current Repo State

- `App.xaml.cs` `OnLaunched` activates the main window before the theme is applied:
  ```
  1. _mainWindow.Activate()          ← window becomes visible with default theme
  2. await _appLifecycle.RunAsync()  ← theme is applied somewhere inside here
  ```
- The startup lifecycle reads the saved theme preference from database during `RunAsync`.
- `MainWindow.xaml.cs` or a startup step inside `Service_OnStartup_AppLifecycle` calls
  `WindowHelper` or `ThemeHelper` to apply `ElementTheme` to the window root.
- The white flash is visible on machines with dark mode configured as the default preference.
- Theme preference is stored as a user or workstation setting read during startup.

## Required Outcome

Ensure the user theme is applied to the main window root element before the window is made visible
so the user never sees the default light frame before the configured theme takes effect.

## Primary Change Areas

- `App.xaml.cs` `OnLaunched` — the ordering of `_mainWindow.Activate()` relative to the theme
  application step.
- If theme application requires the window content tree to be loaded, ensure the window is
  loaded but not yet activated (visible) before the theme call.

## Files That Must Change

- `App.xaml.cs`
- `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs` — if theme application
  is currently embedded inside `RunAsync` and must be extracted as a pre-activate step.

## Recommended Target Shape

The correct activation sequence is:

```
1. _mainWindow = _host.Services.GetRequiredService<MainWindow>()
2. _mainWindow.Activate()          // must happen so the XAML tree is initialized
3. ApplyTheme(_mainWindow)         // theme applied before window is visible to user
   OR
   await _appLifecycle.ApplyInitialThemeAsync(_mainWindow)
4. await _appLifecycle.RunAsync()  // remaining startup work
```

WinUI 3 requires `Activate()` to be called before the XAML content tree is accessible, but the
window can be repositioned or theme-applied immediately after. The window is not necessarily
visible in a perceptible flash if `Activate()` is followed immediately (synchronously) by the
theme call before the message loop yields to the OS compositor.

If the theme preference requires an async database read, the minimal approach is to read the
theme preference earlier in the startup pipeline (before `Activate`) and store the resolved value
so it can be applied synchronously right after `Activate()`.

## Implementation Steps

1. Locate the theme application logic currently inside `Service_OnStartup_AppLifecycle.RunAsync`.
2. Determine whether the theme preference value is available synchronously by the time `Activate()`
   runs, or whether it must be read from the database during startup.
3. If the value is available synchronously (e.g., from local settings or the last-read app
   settings), apply the theme immediately after `Activate()` before the first async yield.
4. If the value requires a database read, extract a minimal `GetInitialThemeAsync()` step that
   runs before `Activate()` so the theme is known when the window first appears.
5. Apply the resolved theme to the main window root element immediately after `Activate()`.
6. Confirm the remaining startup steps in `RunAsync` still execute in the correct order.
7. Build the solution and confirm zero errors.
8. Start the application on a dark-themed configuration and confirm no white-frame flash.

## Task Checklist

- [ ] Locate theme application call site inside the startup lifecycle.
- [ ] Determine if theme value is available synchronously before `Activate()`.
- [ ] Apply theme to window root element immediately after `Activate()` before any async yield.
- [ ] Confirm remaining `RunAsync` startup steps are unaffected.
- [ ] `dotnet build` succeeds with zero errors.
- [ ] Start on dark-mode configuration — no white-frame flash visible at startup.
- [ ] Start on light-mode configuration — correct theme applied, no regression.

## Validation

- Configure a dark theme preference in the application settings.
- Start the application and observe the window at the moment it becomes visible.
- No white/default-theme frame should appear before the dark theme takes effect.
- The application must complete startup normally in both light and dark theme configurations.
- `dotnet build` must succeed with no errors.

## Guardrails

- Do not move heavy database reads to a blocking pre-startup position to accommodate this change.
- Do not remove `_mainWindow.Activate()` — WinUI 3 requires it for the XAML tree to initialize.
- Do not change the theme storage, theme read path, or theme settings UI in this slice.
- Do not introduce a splash screen or loading overlay in this slice.

## Completion Criteria

- The main window displays with the user-configured theme from the first visible frame.
- No white-frame flash is observable on dark-theme configurations.
- The solution builds cleanly.
- Light-theme configurations are unaffected.
