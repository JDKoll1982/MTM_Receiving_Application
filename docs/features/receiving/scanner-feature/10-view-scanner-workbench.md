# Scanner View

Last Updated: 2026-07-21

This view is the user execution screen for preparing and sending items to inventory.

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

- Send controls disabled when validation fails
- Stop control enabled only while running
- row state changes are immediate and visually distinct
- sent rows remain visible and read-only
- from and to warehouse values are populated from settings/profile defaults and are not editable per item
- users edit line-level values through the manage-items workflow rather than typing directly into the workbench grid
- after a new item is added, the manage-items workflow validates part, source location, destination location, and available source quantity against Infor Visual
- invalid items remain in the batch with visible status and notes so the user can correct them
- after data entry completes, the user manually validates what the ERP received and performs save/commit actions; no `Alt+S` or auto-finalize behavior is part of this screen

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
