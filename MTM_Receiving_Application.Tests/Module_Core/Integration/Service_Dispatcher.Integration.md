# Service_Dispatcher Integration Notes

Last Updated: 2025-01-19

## Scope
`Service_Dispatcher` depends on `DispatcherQueue` which requires a UI thread. Full behavior tests should use a WinUI integration test harness or dispatcher queue controller.

## Proposed Tests
- Create `DispatcherQueueController` on a dedicated thread
- Verify `TryEnqueue` executes callback
- Verify `CreateTimer` returns a running timer and `Tick` events flow through wrapper
