# List Of Needed Changes

Last Updated: 2026-03-31

This folder breaks [ListOfNeededChanges.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/ListOfNeededChanges.md) into one implementation instruction file per requested change.

- [TOC-Continuance-Prompt-Checklist.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/TOC-Continuance-Prompt-Checklist.md)

## Dunnage Edit Mode

- [01-dunnage-edit-mode-quantity-whole-number.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/01-dunnage-edit-mode-quantity-whole-number.md)
- [02-dunnage-edit-mode-quantity-resets-on-edit.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/02-dunnage-edit-mode-quantity-resets-on-edit.md)
- [03-dunnage-edit-mode-type-button-modal.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/03-dunnage-edit-mode-type-button-modal.md)
- [04-dunnage-edit-mode-part-id-button-modal.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/04-dunnage-edit-mode-part-id-button-modal.md)
- [05-dunnage-edit-mode-load-number-showing-zeros.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/05-dunnage-edit-mode-load-number-showing-zeros.md)
- [06-dunnage-edit-mode-po-auto-format.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/06-dunnage-edit-mode-po-auto-format.md)
- [07-dunnage-edit-mode-location-button-modal.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/07-dunnage-edit-mode-location-button-modal.md)
- [08-dunnage-edit-mode-modal-fuzzy-search.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/08-dunnage-edit-mode-modal-fuzzy-search.md)
- [09-dunnage-edit-mode-created-date-date-only.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/09-dunnage-edit-mode-created-date-date-only.md)
- [10-dunnage-edit-mode-created-time-column.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/10-dunnage-edit-mode-created-time-column.md)
- [11-dunnage-edit-mode-filter-buttons-clipped.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/11-dunnage-edit-mode-filter-buttons-clipped.md)
- [12-dunnage-edit-mode-remove-columns-help-button.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/12-dunnage-edit-mode-remove-columns-help-button.md)
- [13-dunnage-edit-mode-select-all-not-working.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/13-dunnage-edit-mode-select-all-not-working.md)
- [14-dunnage-edit-mode-save-changes-not-working.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/14-dunnage-edit-mode-save-changes-not-working.md)
- [15-dunnage-edit-mode-remove-selected-not-working.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/15-dunnage-edit-mode-remove-selected-not-working.md)
- [16-dunnage-edit-mode-filtered-searchbar.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/16-dunnage-edit-mode-filtered-searchbar.md)
- [17-dunnage-remove-manual-mode.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/17-dunnage-remove-manual-mode.md)
- [18-dunnage-add-new-part-modal-specs.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/18-dunnage-add-new-part-modal-specs.md)
- [19-dunnage-enter-details-location-behavior.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/19-dunnage-enter-details-location-behavior.md)

## Receiving

- [20-receiving-enter-load-location-case-normalization.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/20-receiving-enter-load-location-case-normalization.md)
- [21-receiving-enter-package-type-autofill-button.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/21-receiving-enter-package-type-autofill-button.md)

## Volvo

- [22-volvo-add-part-location-textbox.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/22-volvo-add-part-location-textbox.md)
- [23-volvo-shipment-history-details-modal.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/docs/ListOfNeededChanges/23-volvo-shipment-history-details-modal.md)

## Shared Guidance

- Reuse existing selection and fuzzy-search patterns before introducing new dialogs.
- Keep MVVM separation intact: View -> ViewModel -> Service -> DAO.
- Any Infor Visual lookup remains read-only.
- After implementation, validate the impacted workflow end to end and update affected module metadata if behavior or screens changed.
