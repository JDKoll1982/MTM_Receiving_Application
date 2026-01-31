# Module-Agnostic Fix Plan: Dependency Injection & Navigation

Last Updated: 2026-01-30

## Purpose
Provide a consistent, module-agnostic plan to fix dependency setup and navigation issues identified in per-view review files. This plan is written so you can request work using the pattern:

Implement {FixFileName} on Module_{Name}

---

## How to Request Work
Use any of the following commands:
- Implement DI-Setup-Standardization.md on Module_Receiving
- Implement Navigation-Ownership.md on Module_Dunnage
- Implement Arrival-Actions.md on Module_Routing

---

## Fix Files (Reusable Across Modules)

### DI-Setup-Standardization.md
**Goal**: Remove runtime service lookups from screens and move setup to explicit handoff.

**Checklist**
- Replace global lookups in view code-behind with constructor-based setup.
- Ensure dependencies are provided by the owning workflow or page creation path.
- Eliminate mixed setup paths (no fallback lookups).
- Confirm all required parts are registered in the dependency container.

**Validation**
- Screens load without global lookups.
- Dependencies are visible and consistent across all usage paths.

---

### Navigation-Ownership.md
**Goal**: Ensure a single, visible owner controls navigation for the workflow.

**Checklist**
- Move navigation decisions out of individual screens when they are not the workflow owner.
- Keep all step-jump logic in the workflow or navigation service.
- Make entry and exit rules for each step explicit.

**Validation**
- Navigation behavior is consistent and traceable.
- Screens no longer choose their own next step.

---

### Arrival-Actions.md
**Goal**: Run “arrival” actions when navigation confirms the step, not just on screen load.

**Checklist**
- Identify actions triggered on screen load (e.g., setup calls, refreshes).
- Relocate those actions to the navigation/step activation event.
- Keep UI load limited to display concerns.

**Validation**
- Arrival actions run exactly once per step activation.
- Timing is consistent across devices and usage paths.

---

### Service-Lookup-Removal.md
**Goal**: Remove deprecated service lookup usage from UI screens.

**Checklist**
- Replace deprecated global lookup usage with explicit dependency handoff.
- Consolidate usage patterns so the same screen behaves the same across all creation paths.

**Validation**
- No deprecated lookup calls remain in the target screens.
- Behavior is stable and consistent.

---

## Module Application Pattern
When applying any fix file to a module, the work should follow this order:
1. Identify the screens in the target module that map to the fix file scope.
2. Align each screen’s setup and navigation to the fix checklist.
3. Update the module’s documentation log with a plain-language summary.
4. Provide a short before/after summary for review.
5. Add any pitfals to the Copilot Section at the bottom of this file.

---

## Request Template
Copy and fill one line per request:

Implement {FixFileName} on Module_{Name}

**Examples**
- Implement DI-Setup-Standardization.md on Module_Receiving
- Implement Navigation-Ownership.md on Module_Dunnage
- Implement Arrival-Actions.md on Module_Settings

## Copilot Section - Place what you learn though implementations of this file here so you do not have repeated pitfalls during later implementations

- For XAML-instantiated step views, replace inline view tags with ContentControl hosts and inject the views through the parent page constructor.
- Ensure the parent page is created via DI (use NavigateWithDI) before removing parameterless constructors.
- Move any step entry logic from view load events into workflow/viewmodel step-change handlers.
- Keep view constructors limited to dependency wiring and UI setup; avoid service lookups in code-behind.
- When removing Loaded event handlers from code-behind, also remove matching Loaded="..." attributes from the XAML to avoid generated build errors.
- Removing Loaded handlers can shift layout; correct spacing, widths, and alignment directly in XAML and avoid compensating with code-behind.
- UI layout changes should live in XAML only unless there is a clear, unavoidable runtime dependency.
