# Connected Services And Assets: Module Shared

Last Updated: 2026-07-21

This document defines Module_Shared artifacts that should be reused for scanner feature consistency.

## Reuse As-Is

- ViewModel_Shared_Base
Reason: standardized busy state, status fields, and notification integration for scanner view-models.

- shared help dialog patterns
Reason: scanner functionality can expose contextual help and safety reminders through existing shared help workflows.

- shared startup and main window view-model conventions
Reason: scanner entry points should follow existing shell and navigation composition patterns.

## Reuse With Targeted Extension

- shared notification surfaces
Extension intent: scanner-specific status messaging vocabulary such as item sent, stop initiated, and incomplete send detected.
Boundary: keep generic shared status infrastructure unchanged; only extend message usage patterns.

- shared route and shell integration points
Extension intent: expose scanner view and history navigation from existing receiving shell composition.
Boundary: avoid hard-coding scanner logic into generic shared shell services.

## Shared UX Conventions To Follow

- status severity mapping aligned with existing InfoBar semantics
- consistent command enablement cues and disabled-state behavior
- consistent page title and module-label patterns

## Not Recommended For Shared Changes

- do not add scanner-specific data models to Module_Shared
- do not place scanner business orchestration inside shared dialogs or code-behind
- do not alter shared base view-model contract for one module functionality if local view-model composition can solve it

## Ownership Summary

- scanner feature business ownership remains in Module_Scanner
- shared module provides compositional primitives, not feature domain ownership
