# Service_UserSessionManager Integration Notes

Last Updated: 2025-01-19

## Scope
Timer behavior depends on UI thread dispatcher timers. Full verification should use a dispatcher queue controller.

## Proposed Tests
- Use a dispatcher queue timer and verify `SessionTimedOut` event fires
- Validate timer stops after timeout
