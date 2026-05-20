# Customer Pull n' Pack Handler Workflow Visual Direction 01

Last Updated: 2026-05-20

## Purpose

This document adds tighter visual direction to [CustomerPullPackWaitlist_HandlerWorkflow_MockupPackage_01.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/Module_ShipRec_Tools/docs/ReportMockups/CustomerPullPackWaitlist_HandlerWorkflow_MockupPackage_01.md).

It should guide the material-handler-specific mockups toward a fast, warehouse-friendly desktop workflow that still feels like MTM.

---

## Handler Experience Priorities

The handler workflow should optimize for:

- speed
- scanning
- low ambiguity
- fast status changes
- reliable note capture when problems happen
- easy movement back to the queue

This workflow should feel more operational than the report page.

---

## Layout Direction

### Queue page

- Use a dense queue-first layout.
- The default focal point should be the open work list, not large summary cards.
- A split queue/detail layout is strongly encouraged if it improves speed.
- `ListDetailsView`, `DataGrid`, or a left-grid/right-detail pattern are strong candidates.

### Detail pages/states

- The selected item summary should stay near the top.
- Status actions should remain close to the work context.
- Notes should be easy to find and edit.
- Audit metadata should be visible but secondary.

### Empty states

- Keep queue filters visible.
- Use concise explanations.
- Avoid oversized illustrations or dead-space-heavy layouts.

---

## Control Direction

Use realistic controls that support warehouse execution:

- `DataGrid` for queue views
- `InfoBar` for status or save feedback
- `CommandBar` for queue actions
- `ComboBox` and `ToggleSwitch` for filtering
- `ContentDialog` only when a focused confirmation is truly needed
- `InAppNotification` when non-blocking completion feedback is enough
- `Expander` only if it helps compress secondary context

Prefer stable desktop patterns over novelty.

---

## Status Emphasis Direction

### Accepted

- Should feel active but neutral.
- Enough emphasis to stand out in queue scans.

### Completed

- Should feel resolved and stable.
- Confirmation should be visible without overwhelming the operator.

### Cancelled

- Should feel clearly closed.
- Distinguish it from completed.

### Problem

- Should have the strongest attention behavior after shortages.
- The UI should make notes feel required or strongly expected.
- The notes area should become visually prominent in this state.

---

## Visual Tone

- industrial
- practical
- calm but serious
- efficient
- desktop-first

Do not make the handler workflow prettier at the cost of speed or clarity.

---

## Print Direction

The pull-list print mode should feel like a warehouse tool:

- strong table structure
- sub-part grouping obvious at a glance
- location lines easy to scan quickly
- minimal decorative styling
- strong print readability on paper

---

## What To Avoid

- do not turn the handler workflow into a report clone
- do not overuse card layouts where tables are faster
- do not bury status actions behind too many clicks
- do not treat notes as secondary when the item is in Problem state
- do not introduce web-style kanban or chat-task metaphors

---

## Success Check

The handler workflow visual direction is successful if:

- a material handler could quickly understand what to work next
- changing status feels fast and obvious
- problem handling is explicit and note-driven
- the pages still feel like part of the MTM application and Ship/Rec Tools module
