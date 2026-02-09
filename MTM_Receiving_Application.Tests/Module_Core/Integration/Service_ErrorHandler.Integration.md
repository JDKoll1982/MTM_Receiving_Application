# Service_ErrorHandler Integration Notes

Last Updated: 2025-01-19

## Scope
`ShowErrorDialogAsync` uses `ContentDialog` and requires a valid `XamlRoot`. UI tests should validate dialog rendering.

## Proposed Tests
- Provide a real `XamlRoot` via `IService_Window`
- Verify dialog shows correct title and content
