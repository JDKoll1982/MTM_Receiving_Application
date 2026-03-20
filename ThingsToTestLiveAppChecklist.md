# Live App Test Checklist

Last Updated: 2026-03-19

## Purpose

Use this checklist to manually verify the completed changes listed in `ThingsToChangeChecklist.md` in the running application.

## Test Setup

- [ ] Launch the app successfully.
- [ ] Sign in with a user that can access Settings and the Receiving workflow.
- [ ] Confirm the app opens without startup errors or missing-page navigation failures.
- [ ] Confirm test data or mock/live ERP connectivity is available for Receiving scenarios.

## Settings Window Navigation

- [ ] Open Settings and verify the window loads normally.
- [ ] Navigate into Receiving settings and confirm the back-to-hub button appears on child pages.
- [ ] Click the back-to-hub button from a Receiving child page and confirm it returns to the correct hub.
- [ ] Confirm the back-to-hub button is hidden on hub/root pages.

## Receiving Settings Navigation Cleanup

- [ ] Open Receiving settings and confirm `Overview` is no longer present.
- [ ] Confirm `ERP Integration` is no longer present.
- [ ] Confirm `Preferences` now appears as `Part Number Auto Padding`.
- [ ] Confirm `Business Rules` now appears as `Workflow Options`.
- [ ] Confirm the Receiving settings hub only shows the expected remaining cards/pages.

## Receiving Defaults Page

- [ ] Open `Receiving > Defaults`.
- [ ] Confirm there is a configurable default location field with value `RECV` unless user/system settings override it.
- [ ] Change the default location, save, leave the page, return, and confirm the saved value persists.
- [ ] Reset defaults if supported and confirm the default location returns to `RECV`.
- [ ] Confirm `Label Database Table Save Location` no longer appears on the Defaults page.

## Receiving Validation Settings Page

- [ ] Open `Receiving > Validation`.
- [ ] Confirm `Require Part ID` is no longer present.
- [ ] Confirm the remaining validation toggles and numeric limit fields still render correctly.
- [ ] Save changes to validation settings and confirm the page does not throw errors.
- [ ] Reopen the page and confirm saved values persist.

## Receiving Workflow Options Page

- [ ] Open `Receiving > Workflow Options`.
- [ ] Confirm `Default Mode On Startup` is a combo box, not a free-text field.
- [ ] Confirm the combo box options are:
- [ ] `Mode Selection`
- [ ] `Guided Wizard`
- [ ] `Manual Entry`
- [ ] `Edit Mode`
- [ ] Confirm the old storage card/toggles are no longer present.
- [ ] Toggle and save each of the following successfully:
- [ ] `Remember Last Mode`
- [ ] `Confirm Mode Change`
- [ ] `Auto-Fill Heat/Lot`
- [ ] `Save Package Type As Default`
- [ ] `Show Review Table By Default`
- [ ] `Allow Edit After Save`

## Guided Wizard Startup Behavior

- [ ] Set `Default Mode On Startup` to `Mode Selection`, restart the app, and confirm Receiving opens at mode selection.
- [ ] Set `Default Mode On Startup` to `Guided Wizard`, restart the app, and confirm Receiving opens directly in guided flow.
- [ ] Set `Default Mode On Startup` to `Manual Entry`, restart the app, and confirm Receiving opens directly in manual entry.
- [ ] Set `Default Mode On Startup` to `Edit Mode`, restart the app, and confirm Receiving opens directly in edit mode.

## Remember Last Mode

- [ ] Enable `Remember Last Mode`.
- [ ] Select `Guided Wizard`, leave Receiving, return to Receiving, and confirm the last mode is reused when no explicit user default overrides it.
- [ ] Repeat with `Manual Entry`.
- [ ] Repeat with `Edit Mode`.
- [ ] Disable `Remember Last Mode`, change modes, restart or re-enter Receiving, and confirm the app falls back to the configured startup default instead of the most recently used mode.

## Confirm Mode Change

- [ ] Enable `Confirm Mode Change`.
- [ ] Enter some in-progress Receiving data, switch modes, and confirm a confirmation dialog appears.
- [ ] Cancel the dialog and confirm the current mode/data remains intact.
- [ ] Accept the dialog and confirm the mode changes.
- [ ] Disable `Confirm Mode Change`.
- [ ] Repeat a mode switch with in-progress data and confirm no confirmation dialog appears.

## Guided Load Entry Location Behavior

- [ ] In guided mode, navigate to `Enter Load Information`.
- [ ] Enter a valid live location, tab out, and confirm no warning appears.
- [ ] Enter an invalid live location, tab out, and confirm validation triggers.
- [ ] Confirm invalid-location feedback includes fuzzy suggestions when matches exist.
- [ ] Confirm focus returns to the Location field after invalid entry.
- [ ] Leave Location blank, advance to the next step, and confirm the configured default location is applied.
- [ ] Change the configured default location away from `RECV`, repeat the blank-location advance, and confirm the new default is used.

## Validation Settings Runtime Behavior

- [ ] Disable `Require PO Number` and confirm a blank PO can pass the relevant validation path if the workflow allows it.
- [ ] Enable `Require Heat/Lot` and confirm blank heat/lot entry is blocked.
- [ ] Disable `Require Heat/Lot` and confirm blank heat/lot entry is allowed.
- [ ] Disable `Warn On Same Day Receiving` and confirm same-day receiving no longer raises a warning banner/dialog in Weight/Quantity.
- [ ] Enable `Warn On Same Day Receiving` and confirm the warning returns.
- [ ] Disable `Warn On Quantity Exceeds PO` and confirm exceeding PO quantity no longer raises the warning.
- [ ] Enable `Warn On Quantity Exceeds PO` and confirm the warning returns.
- [ ] Set `Min Load Count` and `Max Load Count` to non-default values and confirm Load Entry validation enforces them.
- [ ] Set `Min Quantity` and `Max Quantity` to non-default values and confirm Weight/Quantity validation enforces them.
- [ ] Enable `Allow Negative Quantity` and confirm negative quantity is allowed but zero is still rejected.
- [ ] Disable `Allow Negative Quantity` and confirm negative quantity is rejected.

## Heat/Lot Behavior

- [ ] Enable `Auto-Fill Heat/Lot`.
- [ ] Enter a heat/lot value in the first row, move to the Heat/Lot step, and confirm blank rows are auto-filled from prior populated rows.
- [ ] Disable `Auto-Fill Heat/Lot` and confirm rows are no longer auto-filled automatically on entry.
- [ ] Use the explicit `Auto-Fill` button and confirm it still fills downward.

## Package Type Behavior

- [ ] Enable `Save Package Type As Default`.
- [ ] Choose a package type for a part, leave the page, return for the same part, and confirm the selected type is reused.
- [ ] Disable `Save Package Type As Default`.
- [ ] Repeat the scenario and confirm package type is no longer auto-saved as the new default.

## Review Screen Behavior

- [ ] Enable `Show Review Table By Default` and confirm the Review step opens in table view first.
- [ ] Disable `Show Review Table By Default` and confirm the Review step opens in single-entry view first.
- [ ] Confirm switching between single view and table view still works in both settings states.

## Weight / Quantity Screen

- [ ] Open the Weight / Quantity step with fresh loads and confirm fields start visually blank instead of showing `1` automatically.
- [ ] Enter a value only in the first populated row and leave later rows blank.
- [ ] Click `Auto Fill` and confirm the first populated value fills downward into blank rows only.
- [ ] Confirm existing non-blank rows are not overwritten by `Auto Fill`.

## Completion Screen And Post-Save Editing

- [ ] Complete and save a receiving session successfully.
- [ ] Confirm the completion screen still shows the normal success details.
- [ ] With `Allow Edit After Save` enabled, confirm an `Edit Saved Entries` button is visible.
- [ ] Click `Edit Saved Entries` and confirm the app navigates into Edit Mode.
- [ ] Confirm Edit Mode loads the `Current Labels` queue rather than empty memory.
- [ ] Make an edit and save it successfully from Edit Mode.
- [ ] Disable `Allow Edit After Save`, save another session, and confirm `Edit Saved Entries` no longer appears on the completion screen.

## Removed-Feature Regression Checks

- [ ] Confirm no navigation path still references the removed `Overview` page.
- [ ] Confirm no navigation path still references the removed `ERP Integration` page.
- [ ] Confirm no orphaned header text or stale labels still say `Business Rules` where the user should see `Workflow Options`.
- [ ] Confirm no stale labels still say `Preferences` where the user should see `Part Number Auto Padding`.

## Final Smoke Checks

- [ ] Close and reopen the app after changing Receiving settings and confirm the app still starts cleanly.
- [ ] Run one full Guided Wizard happy-path scenario from mode selection through completion.
- [ ] Run one Manual Entry happy-path scenario.
- [ ] Run one Edit Mode scenario against current labels or history.
- [ ] Confirm no unexpected dialogs, crashes, or missing-view navigation errors occur during these flows.