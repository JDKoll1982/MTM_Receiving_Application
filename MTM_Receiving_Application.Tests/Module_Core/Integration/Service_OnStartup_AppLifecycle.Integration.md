# Service_OnStartup_AppLifecycle Integration Notes

Last Updated: 2025-01-19

## Scope
Startup workflow relies on WinUI windows, dialogs, and service provider resolution. Integration tests should run in a UI-capable harness.

## Proposed Tests
- Verify splash screen lifecycle and progress updates
- Validate login flow branches (existing user, new user, shared terminal)
- Validate session creation and window activation
