# Per-Page Action Matrix: Non-Inventoried Labels

**Feature**: [spec.md](../spec.md) | **Plan**: [plan.md](../plan.md)

This is the UI contract a test asserts without reopening the spec. It fixes the strip's layout order and,
per Non-Inventoried Labels page, exactly which actions are offered. A page shows exactly these actions
and nothing else (FR-017).

## Strip Layout

One strip, the same one that serves Receiving, pinned directly beneath the page content on the side of
the window opposite the navigation pane. It sizes to its content and wraps or scrolls horizontally at
compact widths without hiding any action the current page offers (FR-001, FR-003).

- Left group, in order: `Guided`, `Edit`, `Image Search`.
- Right group, in order: `Back`, `Next`, a divider, `Clear All Labels`, `Help`.
- Each mode chip is a button. Pressing it enters that mode. Holding Ctrl and pressing it stores that mode
  as the signed-in user's default without changing the page, and the word `Default` appears beneath the
  chip that holds it (FR-006, FR-007).
- The mode in force draws success-green text and a success-green border; the others draw neither
  (FR-025).
- `Back` and its icon draw in the danger colour, the save verb draws in the success colour, and both
  follow the active theme (FR-023).
- `Clear All Labels` and `Help` are icon-only, each keeping its tooltip and its screen-reader name
  (FR-024).
- `Manual` never appears. No mode chip, no badge, and no default marker for it exists anywhere in the
  application (FR-011).

## Non-Inventoried Labels Page Matrix

A page-specific verb may occupy the Back or Next position instead of the default action. The position
owner is named under each page.

| Page | Actions offered | Slot note |
| --- | --- | --- |
| Select Type | `Guided`, `Edit`, `Image Search`, `Next`, `Clear All Labels`, `Help` | no Back; Next unavailable until a type is selected |
| Select Part | `Guided`, `Edit`, `Image Search`, `Back`, `Next`, `Clear All Labels`, `Help` | Next unavailable until a part is selected |
| Enter Loads | `Guided`, `Edit`, `Image Search`, `Back`, `Next`, `Clear All Labels`, `Help` | Next unavailable until the entry is valid |
| Enter Details | `Guided`, `Edit`, `Image Search`, `Back`, `Review`, `Clear All Labels`, `Help` | `Review` in the Next position |
| Review & Save | `Guided`, `Edit`, `Image Search`, `Back`, `Save`, `Clear All Labels`, `Help` | `Save` in the Next position |
| Edit Mode | `Guided`, `Edit`, `Image Search`, `Save`, `Clear All Labels`, `Help` | `Save` in the Next position; no Back |
| Image Search | `Guided`, `Edit`, `Image Search`, `Back`, `Clear All Labels`, `Help` | no Next |

## Pages Outside Both Modules

A page outside Receiving and Non-Inventoried Labels shows only what that page's own contract offers, and
nothing when it offers nothing. It never shows `Guided`, `Edit`, or `Image Search`, and its default
marker never appears. The strip is absent and the page content fills the full remaining height when the
page offers no actions (FR-002, FR-004, FR-035).

## Slot Verb Rules

- `Enter Details` offers `Review` in the Next position and draws no Review control of its own (FR-018).
- `Review & Save` offers `Save` in the Next position and draws no Save control of its own (FR-018).
- `Edit Mode` offers `Save` in the Next position, draws no Save control of its own, and keeps its editing
  toolbar on the page: load selection, select all, remove, column chooser, filters, and paging. None of
  those appear in the strip (FR-019).
- `Image Search` offers `Back`, which returns to the guided step the operator was on when they entered
  Image Search, and to Select Type when Image Search was the mode the module opened in. Choosing a part
  hands it to Select Part (FR-017).
- `Clear All Labels` archives the active non-inventoried label queue under the existing rules, and takes
  the wider clear when activated with Shift held. It is unavailable rather than failing when there is
  nothing to clear (FR-015).
- `Help` opens help for the current page (FR-016).
- Next appears only on pages that support advancing and is unavailable until that page's own condition is
  met. Back appears only on pages that support going back, and is removed rather than disabled where it
  is not supported (FR-014).

## Keyboard Routes

Existing Back, Next, and save shortcuts keep working and obey exactly the visibility and enablement rules
the strip's controls obey, so no shortcut can reach an action the strip has hidden or disabled (FR-021).
The mode shortcut that used to open the mode-selection screen is removed with that screen.

## Labels That Must Not Appear

`Mode Selection`, `Manual`, `Manual Entry`, `Save All`, and `Save Changes` must not appear anywhere in the
application's navigation, settings, help, or strip after the change. The save verb is `Save`.

## Removed Surfaces

Every action the strip owns is deleted from the page that previously drew it, leaving no duplicate
control and no second binding of the same command (FR-018):

- The whole navigation row in `Module_Dunnage/Views/View_Dunnage_WorkflowView.xaml`: the seven
  `Mode Selection` buttons, the four `Back` buttons, the two `Next` buttons, the `Review` button, both
  `Clear All Labels` buttons, and the `Help` icon.
- `Save All` in `Module_Dunnage/Views/View_Dunnage_ReviewView.xaml`.
- `Save Changes` in `Module_Dunnage/Views/View_Dunnage_EditModeView.xaml`.
- `Module_Dunnage/Views/View_Dunnage_ModeSelectionView.xaml` and
  `Module_Dunnage/ViewModels/ViewModel_Dunnage_ModeSelectionViewModel.cs`, the `ModeSelection` step, its
  step-bar entry, and `ReturnToModeSelectionCommand`.
- `Module_Dunnage/Views/View_Dunnage_ManualEntryView.xaml` and
  `Module_Dunnage/ViewModels/ViewModel_Dunnage_ManualEntryViewModel.cs`, the `ManualEntry` step, and its
  `Save & Finish` panel.
