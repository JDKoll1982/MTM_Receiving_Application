# View_Receiving_ManualEntry - Dependency & Navigation Review

Last Updated: 2026-01-30

## Summary
- This screen pulls its parts and warning helper from a global app lookup (deprecated).
- Movement into and out of this screen is controlled elsewhere.

## Dependency setup issues
- Uses a global lookup instead of a clear handoff from the workflow.
- The warning helper is a hidden dependency that can fail silently.
- If the lookup fails, the screen can load without expected safety checks.

## Navigation issues
- The screen does not define entry or exit actions.
- Navigation is handled outside the screen, making the flow harder to trace.

## Impact
- Hidden dependencies make it harder to diagnose safety or data-entry issues.

## Recommended direction (plain language)
- Have the workflow provide all required parts up front.
- Keep navigation decisions centralized so the path is clear.

```mermaid
flowchart TD
  subgraph WManual_DI["Dependency setup (current)"]
    WManual_DI_Start([Screen starts]) --> WManual_DI_Lookup[Pulls needed parts from app-wide container]
    WManual_DI_Lookup --> WManual_DI_Ready([Screen ready])
  end
  subgraph WManual_NAV["Navigation (current)"]
    WManual_NAV_Start([Workflow shows this step]) --> WManual_NAV_Show[Screen stays until workflow changes]
    WManual_NAV_Show --> WManual_NAV_Next([Workflow moves to next step])
  end
```
