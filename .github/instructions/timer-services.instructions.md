# Timer Service Standards

**Category**: Core Services
**Last Updated**: December 26, 2025
**Applies To**: `ITimer`, `DispatcherTimerWrapper`

## Overview

The `DispatcherTimerWrapper` wraps the WinUI 3 `DispatcherQueueTimer` to make it testable and injectable. It allows ViewModels to use timers without direct dependency on UI threads or specific UI frameworks.

## Responsibilities

1.  **Abstraction**: Hide `DispatcherQueueTimer` implementation details.
2.  **Testability**: Allow mocking of timer events in unit tests.

## Implementation Pattern

The wrapper exposes standard timer properties and events.

```csharp
public class DispatcherTimerWrapper : IDispatcherTimer
{
    private readonly DispatcherQueueTimer _timer;
    // ... implementation delegating to _timer
}
```

## Usage

Inject `ITimer` (or a factory that produces `ITimer`) into ViewModels.

```csharp
public MyViewModel(ITimer timer)
{
    _timer = timer;
    _timer.Interval = TimeSpan.FromSeconds(1);
    _timer.Tick += OnTick;
    _timer.Start();
}
```

## Registration
- Register as **Transient**. Each usage typically requires a distinct timer instance.
- **Note**: Since `DispatcherQueueTimer` requires a `DispatcherQueue`, the registration might need a factory delegate or the service needs to be created on the UI thread. Ensure the DI container can resolve it correctly in the context of the UI thread.
