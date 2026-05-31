# 03-StartupFix-DunnageSync-FireAndForgetErrorHandling

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement only this fix slice. Keep the change set focused, complete it end-to-end, and stop
only after the targeted validation passes.

## Feature

Surface errors from the fire-and-forget dunnage image sync on startup

## Implementation Order

03

## Why This Runs Third

The structured Serilog logger replacement (slice 02) must be complete before this slice so that
the error continuation added here has a working, production-observable logger to write to.
This fix is low-risk and high-diagnostic-value: it requires adding only a `ContinueWith` chain
to one existing fire-and-forget call.

## Confirmed Decisions

- The dunnage image sync must remain fire-and-forget — it must not block the startup critical path.
- Any exception that escapes `SyncLocalCacheAsync` must be logged as a warning, not silently dropped.
- The logger used in the continuation must be the injected Serilog logger, not a static fallback.
- Do not change the behavior or internals of `SyncLocalCacheAsync` in this slice.

## Current Repo State

- `App.xaml.cs` line 77 fires the sync with the result discarded:
  ```csharp
  _ = dunnageImageStorage.SyncLocalCacheAsync();
  ```
- Any exception escaping `SyncLocalCacheAsync` becomes an unobserved task exception.
- In .NET 10 the CLR no longer rethrows unobserved task exceptions via the finalizer thread,
  so failures are completely silent.
- `IService_LoggingUtility` or `IService_ErrorHandler` is resolvable from `_host.Services`
  at the point `OnLaunched` runs, since `_host.StartAsync()` has already been awaited.
- `Module_Dunnage/Services/Service_DunnageImageStorage.cs` contains `SyncLocalCacheAsync`.

## Required Outcome

Wrap the fire-and-forget `SyncLocalCacheAsync` call with a `ContinueWith` continuation so that any
unhandled exception is logged as a warning via the structured logger rather than silently discarded.
The sync must still run without blocking startup.

## Primary Change Areas

- `App.xaml.cs` — the single `_ = dunnageImageStorage.SyncLocalCacheAsync();` line.

## Files That Must Change

- `App.xaml.cs`

## Recommended Target Shape

Replace the bare fire-and-forget discard with a logged continuation:

```csharp
_ = dunnageImageStorage.SyncLocalCacheAsync()
    .ContinueWith(
        t => _logger.LogWarning(
            $"Dunnage image sync failed on startup: {t.Exception?.GetBaseException().Message}"),
        TaskContinuationOptions.OnlyOnFaulted
    );
```

Where `_logger` is the `IService_LoggingUtility` instance resolved from `_host.Services` (or
injected into `App` if available). If `IService_LoggingUtility` is not yet a field on `App`,
resolve it inline:

```csharp
var logger = _host.Services.GetRequiredService<IService_LoggingUtility>();
_ = dunnageImageStorage.SyncLocalCacheAsync()
    .ContinueWith(
        t => logger.LogWarning(
            $"Dunnage image sync failed on startup: {t.Exception?.GetBaseException().Message}"),
        TaskContinuationOptions.OnlyOnFaulted
    );
```

## Implementation Steps

1. Open `App.xaml.cs` and locate the `_ = dunnageImageStorage.SyncLocalCacheAsync();` line.
2. Determine whether `IService_LoggingUtility` is already available as a field or local variable
   at this call site. If not, resolve it from `_host.Services`.
3. Replace the discard statement with a `ContinueWith` continuation that logs a warning on fault.
4. Use `TaskContinuationOptions.OnlyOnFaulted` so the continuation does not run on success paths.
5. Build the solution and confirm zero errors.
6. Manually test by temporarily pointing the dunnage image root to an inaccessible path and
   verifying a warning appears in `logs/app-*.txt`.

## Task Checklist

- [ ] Locate the fire-and-forget `SyncLocalCacheAsync` discard in `App.xaml.cs`.
- [ ] Resolve or confirm access to `IService_LoggingUtility` at the call site.
- [ ] Add `ContinueWith` with `TaskContinuationOptions.OnlyOnFaulted` logging a warning.
- [ ] `dotnet build` succeeds with zero errors.
- [ ] Verify the warning appears in the Serilog log file when the sync path fails.

## Validation

- Temporarily configure the Dunnage image root to an invalid/inaccessible path.
- Start the application and open `logs/app-*.txt`.
- A warning log entry containing the sync failure message must appear.
- The application must still complete startup normally — sync failure must not block the critical path.
- `dotnet build` must succeed with no errors.

## Guardrails

- Do not await `SyncLocalCacheAsync` — it must remain fire-and-forget.
- Do not change any logic inside `Service_DunnageImageStorage.SyncLocalCacheAsync`.
- Do not convert the warning to a user-visible error dialog in this slice.
- Do not add retry logic in this slice.

## Completion Criteria

- `SyncLocalCacheAsync` failures are logged as structured warnings in the Serilog log file.
- Startup is not blocked by sync failure.
- The solution builds cleanly.
