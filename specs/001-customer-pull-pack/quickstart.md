# Quickstart: Customer Pull n' Pack Tool

Date: 2026-05-25
Branch: 001-customer-pull-pack

## Purpose
This guide describes the fastest path to verify the Customer Pull n' Pack feature once implementation begins.

## Preconditions
- Build the solution successfully.
- Test MySQL waitlist persistence is available.
- Read-only Visual demand data is available.
- If the embedded crystal-style report surface still shows sample rows or a hardcoded run timestamp, treat the walkthrough as a preview-only checkpoint and do not sign off the report implementation yet.
- Seed data includes:
  - one customer with open demand and no linked waitlist work
  - one source line with existing open waitlist work
  - one line with no selectable part locations
  - one line already owned by a handler and moved to Problem
  - one accepted waitlist line where the requester can still edit requester-facing notes but not handler-owned fields
  - one completed waitlist line whose source demand or location data has changed and should surface a requester-facing recheck indicator
  - one user profile with saved favorite customers, print preset, and waitlist default status filter

## Build And Test Commands
```powershell
dotnet build MTM_Receiving_Application.slnx
dotnet test MTM_Receiving_Application.Tests\MTM_Receiving_Application.Tests.csproj
```

## Validation Walkthrough

### 0. Confirm The Crystal-Style Report Is No Longer Using Preview-Only Sample Data
1. Open Ship/Rec Tools and launch Customer Pull n' Pack.
2. Confirm the main window widens for the report surface.
3. Confirm the filter card starts collapsed but the clear, refresh, and waitlist actions remain visible in the header.
4. Confirm the report header uses the MTM logo, not a placeholder badge.
5. Confirm the visible parent parts, request-line values, quantity totals, and sub-part rows match the live or seeded Customer Pull n' Pack dataset for the selected customer rather than hardcoded sample rows.
6. Leave Customer Pull n' Pack and confirm the main window returns to the standard shell size.

### 1. Review Customer Demand
1. Open Ship/Rec Tools and launch Customer Pull n' Pack.
2. Select a customer with open demand.
3. Confirm the report is scoped to one customer.
4. Confirm the crystal-style grouped surface shows request-line headers directly above the actionable rows.
5. Confirm shortage cues and linked waitlist indicators are visible.
6. Confirm the main window shared status/info bar shows Customer Pull n' Pack status instead of a report-local or Ship/Rec-host-local banner.
7. Measure elapsed time and confirm actionable demand or the no-demand state appears within 30 seconds.

### 2. Create Waitlist Work From Report Selections
1. Select compatible source lines for the same customer and parent part.
2. Confirm the line rows use row-highlight selection instead of checkbox-only selection affordances.
3. Click one or more SUB PARTS ON HAND rows on the main display.
4. Confirm the selectable location rows also use row-highlight selection.
5. Open the waitlist editor.
6. Save.
7. Confirm one waitlist record exists per selected source line.

### 3. Confirm New Requests Start Unselected
1. Select a line with available locations but no existing linked waitlist item.
2. Confirm no location rows are preselected.
3. Confirm only clicked locations carry into the waitlist editor.

### 4. Confirm Duplicate Open Requests Are Blocked
1. Select a source line that already has open waitlist work.
2. Attempt to start another create flow.
3. Confirm the feature blocks the duplicate.
4. Confirm the user sees the duplicate warning in the main window shared info bar and can open the existing waitlist item from that global action.

### 5. Confirm No-Location Exception Path
1. Select a line with no selectable part locations.
2. Open the waitlist editor.
3. Try to save without a note and confirm validation blocks the save.
4. Add a note and save again.
5. Confirm the new waitlist item is flagged for location review.

### 6. Work The Queue
1. Open the dedicated waitlist page.
2. Confirm the default queue shows Requested, Accepted, and Problem.
3. Select a Requested item and mark it Accepted.
4. Confirm ownership is assigned on acceptance.

### 7. Save Problem Status
1. Select an owned queue item.
2. Change it to Problem.
3. Confirm a preset reason or freeform note is required.
4. Save the Problem state.
5. Confirm the same owner remains on the item until explicit unassign.

### 8. Unassign Ownership
1. Open an Accepted or Problem item owned by the current handler.
2. Use the unassign action.
3. Confirm the item returns to an unowned state.

### 9. Confirm Post-Acceptance Requester Edit Boundaries
1. Open an Accepted waitlist item as the requester who does not currently own the line.
2. Edit requester-facing notes or context and save.
3. Confirm the save succeeds and the current owner does not change.
4. Attempt to edit handler-owned fields such as accepted locations, handler status, or ownership.
5. Confirm the feature blocks those edits.

### 10. Confirm Completed-Line Recheck Indicator
1. Open a report line whose linked waitlist item is Completed.
2. Change the underlying source demand or location picture in the test data.
3. Refresh the report.
4. Confirm the linked waitlist item remains Completed.
5. Confirm the report shows a requester-facing recheck indicator outside the normal waitlist status display.

### 11. Restore Saved Defaults And Favorites
1. Save defaults including favorite customers, sort order, shortage/open-work filters, print preset, and waitlist default status filter.
2. Reopen the tool.
3. Confirm the last good customer/date-range context is restored.
4. Confirm saved favorite customers and the saved filter/preset state are available without re-entry.

### 12. Print Current Work Variants
1. Restore defaults or reopen the tool with saved context.
2. Trigger current-view print.
3. Trigger shortage-only print.
4. Trigger waitlist-only print.
5. Trigger floor copy print.
6. Trigger pull-list print.
7. Trigger a selected part or customer-order print context.
8. Confirm each print output includes filters, title/run context, and chosen locations where applicable.
9. Measure elapsed time for at least one representative print action and confirm output is available within 60 seconds.

## Recommended Test Coverage
- Query handler tests for demand report filters and empty state.
- Grouped report projection tests that prove the embedded crystal-style control is bound from live report data rather than control-local sample rows.
- Command handler tests for batch waitlist save, duplicate conflict handling, and no-location exception validation.
- Requester permission tests for post-acceptance note edits, handler-field lock enforcement, and owner preservation.
- Report-surface selection tests that prove row-highlight selection drives the same report-side selection state used by waitlist creation.
- Queue command tests for Accepted, Problem, Completed, Cancelled, and unassign transitions.
- Report tests for completed-line recheck indicator behavior.
- Defaults persistence tests for favorite customers, saved filters, print presets, and waitlist default status filters.
- Print read-model tests for current-view, shortage-only, waitlist-only, floor-copy, pull-list, and selected part/customer-order output.
