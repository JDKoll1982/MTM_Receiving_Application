# View_Receiving_HeatLot - Dependency & Navigation Review

Last Updated: 2026-01-30

## Summary
- This screen pulls what it needs from a global app lookup at runtime (deprecated).
- Arrival actions run when the screen loads, not when navigation is explicitly confirmed.

## Dependency setup issues
- Uses a global lookup instead of being clearly provided what it needs.
- If the lookup fails, the screen can appear without expected behavior.
- The setup is harder to test or replace.

## Navigation issues
- Arrival actions are tied to screen load timing, which can be inconsistent.
- The navigation path is controlled outside this screen, not in a single visible place.

## Impact
- Inconsistent arrival timing can lead to confusing user experiences.

## Recommended direction (plain language)
- Let the workflow provide this screen what it needs directly.
- Run arrival actions when navigation says the step is active, not just when it loads.

```mermaid
flowchart TD
  subgraph WHeat_DI["Dependency setup (current)"]
    WHeat_DI_Start([Screen starts]) --> WHeat_DI_Lookup[Pulls needed parts from app-wide container]
    WHeat_DI_Lookup --> WHeat_DI_Ready([Screen ready])
  end
  subgraph WHeat_NAV["Navigation (current)"]
    WHeat_NAV_Start([Workflow shows this step]) --> WHeat_NAV_Load[Arrival actions run when screen loads]
    WHeat_NAV_Load --> WHeat_NAV_Next([Workflow moves to next step])
  end
```
