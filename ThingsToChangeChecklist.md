# Things To Change Checklist

Last Updated: 2026-03-19

## Completed

- [x] Create checklist artifact from ThingsToChange.md
- [x] Remove Receiving Settings Overview page from navigation and DI
- [x] Remove Receiving ERP Integration settings page from navigation and DI
- [x] Rename Receiving navigation card `Preferences` to `Part Number Auto Padding`
- [x] Rename Receiving `Business Rules` page to `Workflow Options`
- [x] Remove Receiving validation `Require Part ID` setting from UI, ViewModel, keys, defaults, and manifest scripts
- [x] Remove Receiving defaults `Label Database Table Save Location` setting from UI, ViewModel, keys, defaults, and manifest scripts
- [x] Remove Receiving workflow storage card settings from UI, ViewModel, keys, defaults, and manifest scripts
- [x] Add Receiving default location setting to strongly typed keys/defaults and expose it on the Receiving Defaults page
- [x] Fix Settings window back-to-hub button visibility for nested settings pages
- [x] Change Receiving `Default Mode On Startup` editor from free text to constrained combo box
- [x] Apply default location `RECV` when guided receiving advances with blank location
- [x] Add lost-focus location validation and fuzzy suggestions to Receiving Load Entry
- [x] Wire major Receiving Validation settings into runtime validation service
- [x] Wire concrete workflow option settings into runtime behavior (`ConfirmModeChange`, `AutoFillHeatLotEnabled`, `SavePackageTypeAsDefault`, `ShowReviewTableByDefault`, startup default mode)
- [x] Wire `RememberLastMode` into startup and mode selection
- [x] Wire `AllowEditAfterSave` into the completion flow and Edit Mode handoff
- [x] Update Weight/Quantity blank-state behavior and add auto-fill
- [x] Build and validate the first implementation slice

## In Progress

- [ ] None

## Pending

- [ ] None