# Customer Pull n' Pack Mockup Visual Direction 01

Last Updated: 2026-05-20

## Purpose

This document adds tighter visual direction to [CustomerPullPackWaitlist_MockupPackage_01.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/Module_ShipRec_Tools/docs/ReportMockups/CustomerPullPackWaitlist_MockupPackage_01.md).

Use it to guide the first mockup pass so the screens feel like the current MTM Receiving Application instead of a generic dashboard or web prototype.

---

## Global Visual Rules

### App shell

- Keep the existing MTM desktop shell feeling intact.
- Page titles should sit where current MTM tool pages expect them.
- Preserve the sense that the user is still inside Ship/Rec Tools.
- Avoid introducing a brand-new left sidebar, floating global filters, or web-style dashboard chrome.

### Density and spacing

- Favor dense, readable desktop spacing over spacious marketing-page spacing.
- Tables and queues should prioritize operational scanning.
- Detail panels can breathe slightly more than the data grids, but should still feel efficient.

### Control style

- Prefer realistic WinUI 3 + CommunityToolkit controls.
- Safe defaults for the first pass:
  - `DataGrid`
  - `InfoBar`
  - `Expander`
  - `AutoSuggestBox`
  - `ComboBox`
  - `ToggleSwitch`
  - `CommandBar`
  - `ContentDialog`
- Use `ListDetailsView` only when the split master/detail layout clearly improves the handler workflow.

### Color and emphasis

- Use restrained enterprise colors, not saturated consumer-app colors.
- Red should remain the strongest shortage indicator.
- Yellow should remain the Late Order indicator.
- Waitlist-related states should be readable and operationally clear, not decorative.

### Typography

- Use standard WinUI-style hierarchy.
- Titles should be clear and practical.
- Grid text should stay readable at dense desktop sizes.
- Avoid oversized display text or stylized headings that would not fit the current app.

### Icons

- Use existing app-friendly iconography and `Material.Icons.WinUI3` style thinking.
- Icons should support scanning, not dominate the layout.

---

## Visual Intent By Screen

## Screen 01

### Ship/Rec Tools Tool Selection

- Keep the current tool-selection layout almost unchanged.
- The new card should look like it was always part of the module.
- Use the same category grouping logic and card sizing as other Ship/Rec tools.
- The Customer Pull n' Pack card should read as operational/reporting functionality, not a separate subsystem.

## Screen 02

### Customer Pull n' Pack Report Page

- This should be the most report-like screen in the package.
- The filter bar should feel native and efficient.
- The main results surface should feel like a serious operational grid, not a card wall.
- Preserve visual echoes of the provided screenshots:
  - strong row grouping
  - visible shortage emphasis
  - clear pull date and quantity emphasis
  - visible relationship between parent part, FG on hand, and sub-parts
- Waitlist actions should be clearly available but visually secondary to the report itself.

## Screen 03

### Create/Edit Waitlist Entry

- This should feel like a focused task surface.
- A `ContentDialog`-style composition is the safest first direction.
- Keep the source report context visible and trustworthy.
- Internal keys should remain hidden or secondary.
- Primary actions should be clear: save, cancel, update existing entry.

## Screen 04

### Dedicated Waitlist Page

- This should feel more like a work queue than a report.
- The queue should be visually scannable first, detailed second.
- Favor a `ListDetailsView` or split queue/detail pattern if the mockup benefits from it.
- Filters should prioritize active handling states.
- Print pull-list access should be visible but not dominant.

## Screen 05

### Waitlist Detail Work State

- This is the most task-driven screen in the package.
- Status actions should be visually strong and unambiguous.
- `Problem` should visually force attention to notes.
- The detail state should reduce ambiguity about location, quantity, and what the handler is expected to do next.

## Screen 06

### Settings Page

- This should feel like a standard MTM settings form.
- Group settings into practical sections:
  - customer defaults
  - report defaults
  - print defaults
  - waitlist defaults
- Keep save/cancel placement conventional and calm.
- Do not over-style this page.

## Screen 07

### No Open Demand Empty State

- This should look like a valid tool state, not an error page.
- Keep the page shell and filters visible.
- Make the empty-state message explicit and brief.
- Give the user obvious next actions: change customer, change date range, refresh.

---

## Status Language Direction

Use these states consistently in the mockups:

- Accepted
- Completed
- Cancelled
- Problem

The report page may still show a waitlist indicator, but the handler page should be the authoritative working surface for these states.

---

## Print Visual Direction

- Print-oriented surfaces should stay visually close to the provided screenshots.
- The primary print layout should assume landscape 8.5 x 11.
- `Floor Copy` should feel like a polished operational printout.
- `Pull List` should feel like a warehouse action sheet, with one table per unique sub-part.

---

## What To Avoid

- do not redesign the entire app shell
- do not replace dense grids with oversized cards everywhere
- do not use mobile-first page structures
- do not use flashy modern-dashboard motifs
- do not make the waitlist page look like a separate product

---

## Success Check

The visual direction is successful if:

- a current MTM user would believe the screens belong to the existing app
- the report still feels like the current Crystal report evolved into an interactive tool
- the waitlist clearly feels separate from the report, but tightly related to it
- the handler workflow feels faster and more task-oriented than the report workflow
