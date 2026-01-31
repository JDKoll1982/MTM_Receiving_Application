# View_Receiving_EditMode - Dependency & Navigation Review

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
  subgraph WEdit_DI["Current Dependency setup"]
    WEdit_DI_Start([Screen starts]) --> WEdit_DI_Lookup[Pulls needed parts from app-wide container]
    WEdit_DI_Lookup --> WEdit_DI_Ready([Screen ready])
  end
  subgraph WEdit_NAV["Current Navigation"]
    WEdit_NAV_Start([Workflow shows this step]) --> WEdit_NAV_Show[Screen stays until workflow changes]
    WEdit_NAV_Show --> WEdit_NAV_Next([Workflow moves to next step])
  end
```
