# Dunnage UI Test Checklist

Last Updated: 2026-09-04

Manual, UI-facing checks only. Database/deployment behavior was verified separately.

## Add New Dunnage Type

- [ ] Add a **Text** spec: a **Default Value** box appears. Leave it blank and add the spec.
- [ ] Add a **Number** spec with no default typed: the saved spec shows `Default: 0`.
- [ ] Add a **Number** spec with `12.5` typed as the default: the saved spec shows `Default: 12.5`.
- [ ] Add a **Boolean** spec with no default typed: the saved spec shows `Default: false`.
- [ ] Add a **Boolean** spec with `true` typed: the saved spec shows `Default: true`.
- [ ] Add a **Choices** spec (e.g., Maintenance, Die Shop, Fab. Department): the editor shows the note
      "The first choice is used as the default value for new parts and labels."
- [ ] After adding that Choices spec, the spec row summary shows `Default: <first choice>`.
- [ ] Adding an 11th spec shows the "at most 10 specification fields" message and does not add it.
- [ ] Inline spec-editor errors appear in red **between the "Required Field" checkbox and the
      "Add Field" button** (not just near the type name). Confirm each case below:
- [ ] Field Name left blank → inline error "Field Name is required." and nothing is added.
- [ ] Duplicate Field Name → inline error "'<name>' is already defined for this type." and nothing is added.
- [ ] Number spec with Min Value greater than Max Value → inline error
      "Min Value cannot be greater than Max Value." and nothing is added.
- [ ] Number spec whose Default Value is above Max Value → inline error
      "Default Value cannot be greater than Max Value (…)." and nothing is added.
- [ ] Number spec whose Default Value is below Min Value → inline error
      "Default Value cannot be less than Min Value (…)." and nothing is added.
- [ ] Number spec with a non-numeric Default Value (e.g., "abc") → inline error
      "Default Value must be a number." and nothing is added.
- [ ] Number spec with a blank Default Value and Min/Max set: the spec is added with a default inside
      the range (0, or Min Value when Min > 0) — it can never be stored below Min or above Max.
- [ ] After an inline error, editing the field name/type/default clears the message; a successful
      Add Field also clears it.
- [ ] Save the type, close the dialog, and reopen **Edit Type** from the type grid:
      all added specs are still listed with their types and defaults intact.

## Edit Dunnage Type

- [ ] Open **Edit Type** for a type that has a Choices field: its choices are still shown in the spec row
      summary (not emptied).
- [ ] Change a spec's default value, then Save, then reopen: the new default is shown.
- [ ] Add a brand-new spec to an existing type, Save, then reopen: the new spec line is present.
- [ ] Add a new spec with a default to a type that already has parts/history rows saved; after Save,
      create a label for one of those parts and confirm the new field is pre-filled with the default.

## Add / Edit Dunnage Part (Type Specs step)

- [ ] Add Part for a type with a Choices field: the field is a dropdown that lists all available choices
      (e.g., Department shows Maintenance / Die Shop / Fab. Department).
- [ ] Select a choice, finish the wizard, and Save. The part keeps the selected value.
- [ ] Edit that part: the dropdown lists the choices again and is pre-selected to the saved value.
- [ ] Use "Copy From Existing Part": the copied details fill the spec fields (including choices) correctly.
- [ ] Confirm required Choices fields must be answered before Add/Edit Part can be completed.

## Default Location / Home Location edit (Enter Details propagation)

- [ ] Edit a part's **Home Location** to a new value. The confirm dialog mentions
      "default location from X to Y".
- [ ] After confirming, open Enter Details for that part: the Location field pre-fills with the new
      home location.
- [ ] For a row where the location was manually changed to something else, confirm that value is
      NOT overwritten (only default rows change).

## Change Dunnage Type (transfer a part to another type)

- [ ] In Part Selection, with a part selected, a **Change Type** button appears to the left of the
      image/dropdown toggle and is enabled.
- [ ] Clicking **Change Type** opens the modal dialog showing the part number, its current type, and
      a type dropdown that excludes the current type.
- [ ] With no target type chosen, the confirm button is disabled.
- [ ] Choosing a target type whose specs are all shared with the source type: the dialog shows
      "no extra fields required", confirm is enabled, and the transfer proceeds.
- [ ] Choosing a target type with a **required** field the source type does not have: the dialog
      prompts you to enter a value for that required field before enabling confirm.
- [ ] Leaving that required field blank keeps confirm disabled (the part cannot be transferred).
- [ ] Choosing a target type with a required field and entering a value, then confirming: the warning
      dialog lists how many current label data entries and history entries will be rewritten to the
      new type, and offers "Move Part".
- [ ] After confirming the move, the part disappears from the current type's part list and appears
      under the target type in Part Selection.
- [ ] Open Enter Details for the moved part: its location/spec values are preserved, and any
      required target-type fields show the value you entered.
- [ ] Source-type-only specs that the target type lacks are copied onto the target type as
      **optional** fields (shown in a note on the confirm step), and the part's saved values for them
      are retained.
- [ ] If the target type already has 10 specs and the source has an extra spec, the transfer is
      blocked with a message about no open spec slots, and nothing changes.
- [ ] Cancel at either the modal or the confirm step leaves the part's type unchanged.

## Delete Dunnage Part

- [ ] Delete a part that has saved label data and/or history: the warning lists how many current label
      data entries, history entries, and inventory records will be removed.
- [ ] The warning text states that all existing label data and history entries containing this part
      number will be removed.
- [ ] Cancel keeps everything; Delete removes the part and it no longer appears in Part Selection.
- [ ] Delete a part with no saved references: warning says none reference it and deletion succeeds.

## Delete Dunnage Type

- [ ] Delete a type that has parts and saved history: the warning lists the number of parts,
      current label data entries, and history entries that will be removed.
- [ ] The warning text states that all existing label data and history entries containing this type
      or its parts will be removed.
- [ ] Cancel keeps everything; Delete removes the type, its parts, and its saved rows, and the type no
      longer appears in Type Selection.
- [ ] Deleting a type with no references still shows the "nothing references this type" wording and
      succeeds.

## General sanity

- [ ] Normal label flow still works: pick type → pick part → Enter Details (choice defaults pre-fill) →
      Review → Save.
- [ ] "Clear All Labels" still archives rows to history and the history/reprint views still show the rows.
- [ ] Type Selection sorting (Times Used / Last Used / Recently Updated) still behaves as before.
