# Customer Pull n' Pack Mockup Package 01

Last Updated: 2026-05-20

## Purpose

This document is the first structured mockup package outline for the **Customer Pull n' Pack** feature described in [VolvoMackPullPackReport_ReviewSpec.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/Module_ShipRec_Tools/docs/VolvoMackPullPackReport_ReviewSpec.md).

It is based on the guidance in [CustomerPullPackWaitlist_MockupPrompt.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/Module_ShipRec_Tools/docs/CustomerPullPackWaitlist_MockupPrompt.md) and is intended to guide mockup creation that stays close to the current MTM desktop application.

This is not implementation code. It is the first UI package definition for mockups.

---

## Flow Order

The mockups should be produced in this order:

1. Ship/Rec Tools tool selection
2. Customer Pull n' Pack report page
3. Create/Edit waitlist entry from report row
4. Dedicated waitlist work queue page
5. Waitlist detail / work state
6. Feature settings page
7. No-open-demand empty state

---

## Screen 01

### Screen name

Ship/Rec Tools Tool Selection With Customer Pull n' Pack

### Purpose

Show how the new Customer Pull n' Pack tool appears within the existing Ship/Rec Tools landing experience without breaking the current module structure.

### Where it lives in app navigation

Main application shell -> Ship/Rec Tools -> Tool Selection

### Key visible controls

- existing MTM app header/title shell
- Ship/Rec Tools page title
- existing tool category grouping pattern
- existing tool cards surface
- new tool card for `Customer Pull n' Pack`
- back or home navigation that matches the current app flow

### Key user actions

- open Ship/Rec Tools
- scan available tools
- choose Customer Pull n' Pack

### Short rationale

This screen should look almost identical to the current Ship/Rec tool selection experience. The only meaningful change is the addition of the new tool card in the correct category and with a title/summary that fits the existing card style.

### Mockup notes

- Use the current Ship/Rec Tools layout patterns instead of inventing a new landing page.
- Card styling should stay consistent with the current app's existing utility/tool surfaces.
- Do not turn this into a dashboard home page.

---

## Screen 02

### Screen name

Customer Pull n' Pack Report Page

### Purpose

Show the main reporting screen where users select a customer, review live demand, identify shortages, and create or update waitlist entries from report rows.

### Where it lives in app navigation

Main application shell -> Ship/Rec Tools -> Customer Pull n' Pack

### Key visible controls

- existing app header/title area
- page title and Ship/Rec context
- customer selector using WinUI-friendly search/select pattern
- date range controls
- shortage and state filters
- command area for refresh and print
- dense main report grid using a realistic DataGrid-style surface
- row-level action to create or update waitlist entry
- visible waitlist-linked indicator on rows that already have a related waitlist record
- obvious shortage visual treatment matching the current report intent

### Key user actions

- select customer
- apply filters
- review shortage rows
- sort by pull date, shortage, or order
- create a waitlist entry from a row
- update an existing linked waitlist entry
- print current report view

### Short rationale

This page should feel like the current Crystal report translated into an interactive MTM desktop page. It should preserve the recognizable grouping and shortage emphasis from the screenshots while using practical WinUI data surfaces.

### Mockup notes

- Prefer `DataGrid`, `ComboBox`, `AutoSuggestBox`, `InfoBar`, `CommandBar`, and `Expander` style solutions where appropriate.
- Keep the page dense and practical, not airy or consumer-app-like.
- Maintain a clear distinction between report data from Visual and waitlist workflow actions from MTM.

---

## Screen 03

### Screen name

Create/Edit Waitlist Entry From Report Row

### Purpose

Show the entry surface launched from the report page when a user creates a new waitlist request or edits an existing one.

### Where it lives in app navigation

Main application shell -> Ship/Rec Tools -> Customer Pull n' Pack -> Waitlist Entry Editor

### Key visible controls

- modal or side-panel edit surface that feels native to WinUI
- read-only context block showing customer, order, part, and source row information
- requested location field
- requested quantity field
- requester name prefilled from current app user
- handler notes field if editing existing record
- status selector when editing existing record
- save and cancel actions

### Key user actions

- create new waitlist entry from report row
- update linked waitlist entry
- save changes
- cancel without saving

### Short rationale

This screen should feel like an operational edit surface, not a form builder. The user needs enough source context to trust what they are editing without leaving the report page.

### Mockup notes

- `ContentDialog` or a task-focused side panel would both be valid directions.
- The design should make the source report context obvious.
- Internal keys do not need to dominate the UI, but the layout should imply reliable record linkage.

---

## Screen 04

### Screen name

Dedicated Waitlist Page

### Purpose

Show the material handler's primary working screen for open waitlist items.

### Where it lives in app navigation

Main application shell -> Ship/Rec Tools -> Customer Pull n' Pack -> Waitlist Page

### Key visible controls

- app header/title shell
- page title showing this is the waitlist work area
- queue/grid of open waitlist entries
- filters for status, location, customer, and requester
- default status filter behavior for open work
- command area for refresh, print pull list, and open selected item
- problem-state visibility and notes indicator

### Key user actions

- view open queue
- filter to active work
- open an individual waitlist item
- print a pull-focused list

### Short rationale

This page should feel more operational than analytical. It is where handlers actually work the queue, so clarity, density, and status scanning matter more than report-style grouping.

### Mockup notes

- `ListDetailsView` or a left-grid/right-detail pattern is a strong candidate here.
- This page should not look like the report page with a different title.
- Make the status scanning faster than on the report page.

---

## Screen 05

### Screen name

Waitlist Detail Work State

### Purpose

Show the focused handler workflow for one selected waitlist item, including status changes, notes, and completion/problem handling.

### Where it lives in app navigation

Main application shell -> Ship/Rec Tools -> Customer Pull n' Pack -> Waitlist Page -> Waitlist Detail

### Key visible controls

- selected waitlist summary header
- customer/order/part/location context
- requested quantity and current related quantity context
- status actions for `Accepted`, `Completed`, `Cancelled`, and `Problem`
- notes editor
- save/update action
- audit info for last updated by / last updated time
- optional inline notification surface for save or problem feedback

### Key user actions

- accept work
- mark completed
- mark cancelled
- mark problem
- enter/update notes
- save updates

### Short rationale

This screen needs to feel reliable and operational. A handler should be able to understand the request quickly, act, and move on to the next item without extra navigation friction.

### Mockup notes

- When status is `Problem`, the mockup should make note entry feel mandatory or strongly encouraged.
- The action set should feel like desktop task execution, not social/workflow chat UI.

---

## Screen 06

### Screen name

Customer Pull n' Pack Settings Page

### Purpose

Show the feature-level defaults page for users who need to control customer, filtering, print, and waitlist default behavior.

### Where it lives in app navigation

Main application shell -> Settings path appropriate to Ship/Rec Tools feature or tool-level settings entry from Customer Pull n' Pack

### Key visible controls

- grouped settings sections
- default customer selector
- default date range type and length
- default sort order
- default shortage filter toggle
- default unpulled filter toggle
- default print preset selector
- auto refresh option
- waitlist default status filter
- favorite customers quick-select management area
- save and cancel commands

### Key user actions

- edit feature defaults
- save settings
- reset or cancel changes

### Short rationale

This page should feel like an existing MTM settings form. It should be simpler and more form-driven than the report or waitlist work screens.

### Mockup notes

- Use realistic WinUI form controls and grouped sections.
- Do not style this like a web admin portal.

---

## Screen 07

### Screen name

No Open Demand Empty State

### Purpose

Show what the user sees when a valid customer is selected but there is no open demand to report.

### Where it lives in app navigation

Main application shell -> Ship/Rec Tools -> Customer Pull n' Pack

### Key visible controls

- customer selector still visible
- date range controls still visible
- empty-state message
- clear guidance on what happened
- action to choose another customer or adjust date range
- optional refresh action

### Key user actions

- understand why no report is shown
- pick another customer
- change filters/date range
- refresh

### Short rationale

This prevents the tool from feeling broken. The user should immediately understand that the selected customer currently has no open demand rather than assuming the page failed to load.

### Mockup notes

- Keep the shell and filter bar visible so the state still feels like the normal tool.
- Use a native-feeling empty-state panel, not an oversized illustration-first landing page.

---

## Navigation Preservation Notes

- Every screen should visually sit inside the current MTM application shell.
- Ship/Rec Tools should remain an identifiable module context on every feature page.
- The tool selection view should continue to feel like the user's entry point.
- The report page should clearly lead into the waitlist flow without replacing the existing Ship/Rec navigation model.
- The dedicated waitlist page should still feel like part of the same feature, not a different subsystem.

---

## Waitlist Separation Notes

- The report page should show report data first and waitlist actions second.
- The waitlist page should show workflow state first and report linkage second.
- The mockups should make it visually clear that report rows can create or update waitlist records, but that the waitlist itself is stored and worked separately.
- The create/edit waitlist surface should visibly carry source-row context so users trust the connection between the two surfaces.

---

## Control Guidance Notes

The first mockup pass should stay close to controls that are already used or strongly aligned with the current app:

- `DataGrid`
- `InfoBar`
- `Expander`
- `AutoSuggestBox`
- `ComboBox`
- `ToggleSwitch`
- `CommandBar`
- `ContentDialog`
- `ListDetailsView` if a split queue/detail view is helpful

More specialized CommunityToolkit controls should only appear when they clearly improve the workflow.

---

## Next Step

Use this package as the structure for the first mockup round.

If needed, the next iteration can split into:

1. report-focused mockups
2. waitlist work-queue mockups
3. settings-focused mockups