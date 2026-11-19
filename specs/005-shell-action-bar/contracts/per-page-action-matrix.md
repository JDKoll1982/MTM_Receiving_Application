# Per-Page Action Matrix

**Feature**: [spec.md](../spec.md) | **Plan**: [plan.md](../plan.md)

This is the UI contract a test asserts without reopening the spec. It fixes the strip's layout
order and, per Receiving page, exactly which actions are offered. A page shows exactly these
actions and nothing else (FR-012); a page that offers none collapses the bar and takes the full
remaining height (FR-003).

## Strip Layout

One strip pinned directly beneath the page content, on the side of the window opposite the
navigation pane. It sizes to its content and wraps or scrolls horizontally at compact widths
without hiding any action the current page offers.

- Left group, in order, with a divider between each: `Guided`, `Manual`, `Edit`, `Overlay`.
- Right group, in order: `Back`, `Next`, a divider, `Clear Label Data`, `Help`.
- `Guided`, `Manual`, and `Edit` each carry an unlabelled default checkbox with tooltip
  `make Guided my default`, `make Manual my default`, or `make Edit my default`.
- The left group is absent on every non-Receiving page.

## Receiving Page Matrix

A page-specific verb may occupy the Back or Next slot instead of the default action. The slot owner
is named under each page.

| Page | Actions offered | Slot note |
| --- | --- | --- |
| `Enter PO Number` | `Guided`, `Manual`, `Edit`, `Overlay`, `Next`, `Clear Label Data`, `Help` | no Back |
| `Select Part` | `Guided`, `Manual`, `Edit`, `Overlay`, `Back`, `Next`, `Clear Label Data`, `Help` | |
| `Enter Load Information` | `Guided`, `Manual`, `Edit`, `Overlay`, `Back`, `Next`, `Clear Label Data`, `Help` | |
| `Enter Weight & Quantity` | `Guided`, `Manual`, `Edit`, `Overlay`, `Back`, `Next`, `Clear Label Data`, `Help` | |
| `Enter Heat & Lot` | `Guided`, `Manual`, `Edit`, `Overlay`, `Back`, `Next`, `Clear Label Data`, `Help` | |
| `Enter Package Type` | `Guided`, `Manual`, `Edit`, `Overlay`, `Back`, `Next`, `Clear Label Data`, `Help` | |
| `Manual Entry` | `Guided`, `Manual`, `Edit`, `Overlay`, `Clear Label Data`, `Help`, `Save & Finish` | `Save & Finish` in the Next slot |
| `Edit Mode` | `Guided`, `Manual`, `Edit`, `Overlay`, `Clear Label Data`, `Help`, `Save & Finish` | `Save & Finish` in the Next slot |
| `Review & Save` | `Guided`, `Manual`, `Edit`, `Overlay`, `Back`, `Clear Label Data`, `Help`, `Save to Database` | `Save to Database` in the Next slot |
| `Result` | `Guided`, `Manual`, `Edit`, `Overlay`, `Clear Label Data`, `Help`, `Start New Entry` | `Start New Entry` in the Back slot; no Next |

## Non-Receiving Pages

A non-Receiving page shows only what that page's own contract offers, and nothing when it offers
nothing. It never shows `Guided`, `Manual`, `Edit`, or `Overlay`, and its default checkboxes never
appear. The strip is absent and the page content occupies the full remaining height when the page
offers no actions (FR-028).

## Slot Verb Rules

- `Manual Entry` and `Edit Mode` offer `Save & Finish` in the Next slot and keep no Save & Finish
  control of their own; their editing toolbars (add row, add multiple rows, remove row, auto fill,
  and file, paging, and filter controls) stay on the page (FR-020, FR-023).
- `Review & Save` offers `Save to Database` in the Next slot and offers no `Add another Part/PO`
  action anywhere (FR-018).
- `Result` offers `Start New Entry` in the Back slot and includes `Overlay` (FR-019).
- `Overlay` opens `View_Receiving_LabelOverlayWindow` and must not open it twice while it is already
  open.
- Back appears only on pages that support going back; Next appears only on pages that support
  advancing and is disabled until that page's own conditions are met (FR-009).

## Labels That Must Not Appear

`Mode Selection` and `Add another Part/PO` must not appear anywhere in the application's
navigation, settings, help, or the strip after the change.

## Removed Surfaces

Every action the strip owns is deleted from the page that previously drew it, leaving no duplicate
control and no second binding of the same command:

- `ReceivingWorkflowNavigationGrid` in `Module_Receiving/Views/View_Receiving_Workflow.xaml`.
- `Module_Receiving/Views/View_Receiving_ModeSelection.xaml` and
  `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs`, the `ModeSelection` workflow
  step, its step-bar entry, and `ReturnToModeSelectionCommand`.
