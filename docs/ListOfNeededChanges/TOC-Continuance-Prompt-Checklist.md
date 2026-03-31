# TOC Continuance Prompt Checklist

Last Updated: 2026-03-31

## Purpose

Use this file as the single restart point for continuing implementation work from [ListOfNeededChanges.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/ListOfNeededChanges.md).

This file is intended to act as all of the following:

- Table of contents
- Continuance prompt
- Implementation checklist
- Persona holder for future coding sessions

## Persona Holder

You are continuing work in the MTM Receiving Application.

Operate as a careful WinUI 3 and MVVM implementation agent for this repository.

Non-negotiable rules while using this checklist:

- Preserve MVVM flow: View -> ViewModel -> Service -> DAO -> Database.
- Do not let ViewModels call DAOs directly.
- Use `x:Bind`, not runtime `Binding`.
- Keep Infor Visual access read-only.
- Prefer reusing existing dialogs, selectors, and search patterns over creating duplicates.
- When working any location/type/part modal listed below, also apply [08-dunnage-edit-mode-modal-fuzzy-search.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/08-dunnage-edit-mode-modal-fuzzy-search.md).
- When working Dunnage location behavior, compare against the Receiving location flow before changing code.
- When removing Dunnage Manual Mode, use [17-dunnage-remove-manual-mode.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/17-dunnage-remove-manual-mode.md) as a decommission checklist, not just a UI cleanup note.

## How To Use This File

1. Start at the top of the checklist unless the user explicitly chooses a different item.
2. Open the linked instruction file for the current checklist item.
3. Implement the change in code.
4. Mark the item complete here after implementation is verified.
5. If one item depends on another, complete the dependency first or implement them together.

## Dependency Notes

- Items 03, 04, 07, and 19 should use item 08 when a modal search experience is involved.
- Item 17 is a broader cleanup task and should be treated as a decommission effort with dead-code validation.
- Item 20 is the behavioral reference for case-normalized location handling.

## Checklist In Original Requested Order

### Dunnage Edit Mode

- [ ] 1.  Quantity should be whole number
      File: [01-dunnage-edit-mode-quantity-whole-number.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/01-dunnage-edit-mode-quantity-whole-number.md)

- [ ] 2.  Quantity will not stay edited and resets to original value
      File: [02-dunnage-edit-mode-quantity-resets-on-edit.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/02-dunnage-edit-mode-quantity-resets-on-edit.md)

- [ ] 3.  Type should be a button cell that opens a Dunnage Type modal
      File: [03-dunnage-edit-mode-type-button-modal.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/03-dunnage-edit-mode-type-button-modal.md)
      Also apply: [08-dunnage-edit-mode-modal-fuzzy-search.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/08-dunnage-edit-mode-modal-fuzzy-search.md)

- [ ] 4.  Part ID should be a button cell that opens a Part ID modal for the selected Type
      File: [04-dunnage-edit-mode-part-id-button-modal.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/04-dunnage-edit-mode-part-id-button-modal.md)
      Also apply: [08-dunnage-edit-mode-modal-fuzzy-search.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/08-dunnage-edit-mode-modal-fuzzy-search.md)

- [ ] 5.  Load number is only showing zeros
      File: [05-dunnage-edit-mode-load-number-showing-zeros.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/05-dunnage-edit-mode-load-number-showing-zeros.md)

- [ ] 6.  PO Number should auto-format when the input is numeric
      File: [06-dunnage-edit-mode-po-auto-format.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/06-dunnage-edit-mode-po-auto-format.md)

- [ ] 7.  Location should be a button cell that opens an Infor Visual location modal
      File: [07-dunnage-edit-mode-location-button-modal.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/07-dunnage-edit-mode-location-button-modal.md)
      Also apply: [08-dunnage-edit-mode-modal-fuzzy-search.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/08-dunnage-edit-mode-modal-fuzzy-search.md)

- [ ] 8.  Modal searching for Type, Part ID, and Location
      File: [08-dunnage-edit-mode-modal-fuzzy-search.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/08-dunnage-edit-mode-modal-fuzzy-search.md)

- [ ] 9.  Created Date should show only the date
      File: [09-dunnage-edit-mode-created-date-date-only.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/09-dunnage-edit-mode-created-date-date-only.md)

- [ ] 10. Add Created Time column sourced from Created Date
      File: [10-dunnage-edit-mode-created-time-column.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/10-dunnage-edit-mode-created-time-column.md)

- [ ] 11. Filter buttons are clipped and need shorter text
      File: [11-dunnage-edit-mode-filter-buttons-clipped.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/11-dunnage-edit-mode-filter-buttons-clipped.md)

- [ ] 12. Remove the duplicate Columns help button
      File: [12-dunnage-edit-mode-remove-columns-help-button.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/12-dunnage-edit-mode-remove-columns-help-button.md)

- [ ] 13. Select All button is not working
      File: [13-dunnage-edit-mode-select-all-not-working.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/13-dunnage-edit-mode-select-all-not-working.md)

- [ ] 14. Save Changes button is not working
      File: [14-dunnage-edit-mode-save-changes-not-working.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/14-dunnage-edit-mode-save-changes-not-working.md)

- [ ] 15. Remove Selected is not working
      File: [15-dunnage-edit-mode-remove-selected-not-working.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/15-dunnage-edit-mode-remove-selected-not-working.md)

- [ ] 16. Add the same filtered search bar as Receiving Edit Mode
      File: [16-dunnage-edit-mode-filtered-searchbar.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/16-dunnage-edit-mode-filtered-searchbar.md)

### Dunnage Workflow / Other Dunnage Screens

- [ ] 17. Completely remove Manual Mode
      File: [17-dunnage-remove-manual-mode.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/17-dunnage-remove-manual-mode.md)

- [ ] 18. Add New Dunnage Part modal should only show specs included with the selected Type
      File: [18-dunnage-add-new-part-modal-specs.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/18-dunnage-add-new-part-modal-specs.md)

- [ ] 19. Dunnage Enter Details location should behave like Receiving Enter Load Information
      File: [19-dunnage-enter-details-location-behavior.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/19-dunnage-enter-details-location-behavior.md)
      Also apply if a modal is used: [08-dunnage-edit-mode-modal-fuzzy-search.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/08-dunnage-edit-mode-modal-fuzzy-search.md)

### Receiving

- [ ] 20. Normalize matched location casing to the Infor Visual database value
      File: [20-receiving-enter-load-location-case-normalization.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/20-receiving-enter-load-location-case-normalization.md)

- [ ] 21. Add Auto-Fill button inside the Package Type card
      File: [21-receiving-enter-package-type-autofill-button.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/21-receiving-enter-package-type-autofill-button.md)

### Volvo

- [ ] 22. Volvo Add Part modal location textbox should work like the other module location textboxes
      File: [22-volvo-add-part-location-textbox.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/22-volvo-add-part-location-textbox.md)

- [ ] 23. Volvo Shipment History details should be a designed modal window
      File: [23-volvo-shipment-history-details-modal.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/23-volvo-shipment-history-details-modal.md)

## Session Notes

Use this section to capture the current stopping point before ending a work session.

- Current item in progress:
- Blockers:
- Files already changed:
- Tests/build already run:
- Next concrete action:
