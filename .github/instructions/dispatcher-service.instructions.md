# Dispatcher Service Standards

**Category**: Threading / UI
**Last Updated**: December 26, 2025
**Applies To**: `IDispatcherService`, `DispatcherService`

## Overview

WinUI 3 enforces thread affinity. UI elements can only be accessed from the UI thread. Background tasks (e.g., database operations, timers) must marshal calls back to the UI thread to update the interface.

## IDispatcherService

This service abstracts the `DispatcherQueue` to allow for unit testing and cleaner code.

### Methods

- `TryEnqueue(Action action)`: Schedules the action to run on the UI thread.

## Usage

### In ViewModels

When a background task completes and needs to update an `ObservableProperty`:

```csharp
await Task.Run(async () => 
{
    var data = await _service.GetDataAsync();
    
    _dispatcherService.TryEnqueue(() => 
    {
        // Update UI-bound property
        this.Items = data;
        this.IsBusy = false;
    });
});
```

### In Services

Services generally should return data and let the ViewModel handle UI updates. However, for global events (like Session Timeout) that trigger UI navigation, the service might need the dispatcher.

```csharp
private void OnTimeout()
{
    _dispatcherService.TryEnqueue(() => 
    {
        // Navigate to Login
        _navigationService.NavigateTo(typeof(LoginView));
    });
}
```

## Testing
Mock `IDispatcherService` in unit tests to execute the action immediately, avoiding threading issues in tests.

```csharp
_mockDispatcher.Setup(d => d.TryEnqueue(It.IsAny<Action>()))
               .Callback<Action>(a => a());
```
