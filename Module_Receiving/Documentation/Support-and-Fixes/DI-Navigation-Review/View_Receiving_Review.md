# View_Receiving_Review - Dependency & Navigation Review

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
- Navigation remains outside this screen with no single visible owner.

## Impact
- Inconsistent arrival timing can lead to confusing user experiences.

## Recommended direction (plain language)
- Let the workflow provide this screen what it needs directly.
- Run arrival actions when navigation says the step is active, not just when it loads.

```mermaid
flowchart TD
  subgraph WReview_DI["Dependency setup (current)"]
    WReview_DI_Start([Screen starts]) --> WReview_DI_Lookup[Pulls needed parts from app-wide container]
    WReview_DI_Lookup --> WReview_DI_Ready([Screen ready])
  end
  subgraph WReview_NAV["Navigation (current)"]
    WReview_NAV_Start([Workflow shows this step]) --> WReview_NAV_Load[Arrival actions run when screen loads]
    WReview_NAV_Load --> WReview_NAV_Next([Workflow moves to next step])
  end
```
