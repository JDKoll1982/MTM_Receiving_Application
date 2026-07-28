# Scanner View

Last Updated: 2026-07-28

This view is the user execution screen for preparing and sending items to inventory.
The page must resize with the main window, and only the item list region should scroll.

## View Purpose

- allow fast item composition and ordering
- provide clear send controls and real-time feedback
- prevent accidental hidden sends or ambiguous state

## Required Regions

- Header region: session identity, selected profile, application status
- Items region: ordered items with status badges and a clear entry point to open the manage-items dialog
- Send Control region: Check All, Send Next, Send All, Stop After This
- Status region: inline issues, current message, progress counters (Sent, Failed, Waiting)
- Footer region: save for later state and last persistence result
- Item details region: visible from/to warehouse and location values, with warehouse values shown as settings-controlled and read-only
- Resize behavior: the main page stretches with the window; the items list owns its own scrolling area and the page itself does not scroll

## Textual Mockup

Top Bar:

- Scanner title
- Session selector
- Profile selector
- System status chip showing Ready or Not Ready

Main Body Left:

- Items table with columns for #, Item, From Warehouse, From Location, To Warehouse, To Location, Quantity, Status
- Manage Items button to open the batch-edit dialog
- item actions for Add, Remove, Move Up, Move Down are handled inside the manage-items dialog

Main Body Right:

- current item details panel showing part, from/to warehouse, from/to location, and quantity
- issues panel
- progress panel with Sent, Failed, Waiting counts

Bottom Action Strip:

- Check All
- Send Next
- Send All
- Stop After This
- Clear History
- Export

## Interaction Notes

- Send controls are available for the active draft session and update based on the current item counters and validation state.
- Stop control is available for the current batch and is applied as a stop request rather than interrupting an in-progress item.
- Row state changes are immediate and visually distinct.
- Sent items remain visible and read-only in the current session context.
- From and to warehouse values are populated from settings/profile defaults and are not editable per item.
- Users edit line-level values through the staged draft workflow rather than typing directly into the workbench grid.
- After a new item is added, the workbench validates part, source location, destination location, and available source quantity against Infor Visual.
- Invalid items remain in the batch with visible status and notes so the user can correct them.
- After data entry completes, the user manually validates what the ERP received and performs save/commit actions; no `Alt+S` or auto-finalize behavior is part of this screen.
- The workbench currently presents a compact item list with the item details panel on the right side so the operator can focus on the part number, locations, and status without the extra notes/warehouse/ID columns.
- The workbench layout must keep the red-box items region as the only scrollable section so the rest of the page stays anchored while the window resizes.
- Scanner pages now use a shared adaptive layout service that applies width breakpoints and row-targeted list viewport sizing to keep item scrolling consistent at different window sizes and display scales.

## Binding And UI Rules

- use x:Bind for all state and command bindings
- keep code-behind limited to view plumbing only
- route all business actions through view-model commands

## Use Or Modify Guidance

Use existing:

- shared MTM styling and status display patterns
- existing InfoBar or equivalent notification surfaces already used by shared base view-model

Modify or extend:

- receiving navigation host to expose this view
- shared style resources only if scanner-specific visual states need new semantic badges

Do not modify:

- global shared control templates unless required by multiple modules
