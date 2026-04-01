# Dunnage Part Modal Assumptions

Last Updated: 2026-04-01

1. Inventory Type in the Edit Part ID and New Part ID dialogs should map to the existing inventoried-parts list, not be stored inside `dunnage_parts.spec_values`.
   Why this assumption is needed: the current codebase stores inventory method in `dunnage_requires_inventory` via `Model_InventoriedDunnage`, while the part record only stores `part_id`, `type_id`, `spec_values`, and `home_location`.
   Potential impact if wrong: the UI could save inventory type to the wrong data store, causing the workflow's inventory checks to ignore the user's selection.
   Alternative interpretations considered: store inventory type as a spec value; add a new column directly to `dunnage_parts`; keep inventory type out of the part dialogs entirely.

2. The new inventory-type dropdown should support `Not Inventoried`, `Adjust In`, `Receive In`, and `Both`, where `Not Inventoried` means no inventoried-list row should exist for that part.
   Why this assumption is needed: existing dialogs only expose `Adjust In`, `Receive In`, and `Both`, but the new part dialogs also need a way to represent the current non-inventoried state.
   Potential impact if wrong: users may be forced into inventory tracking unintentionally, or lose the ability to remove inventory tracking from a part.
   Alternative interpretations considered: only allow the three existing inventory methods; use a checkbox plus a separate method dropdown; infer non-inventoried from a blank selection.

3. Editing the Part ID should persist by updating `dunnage_parts.part_id`, and if the part is inventoried the inventoried-list entry should be migrated to the new Part ID by replacing the old row with a new row carrying the same inventory method and notes.
   Why this assumption is needed: the current update stored procedure does not support changing `part_id`, and the inventoried-list DAO only supports insert, update-by-id, lookup-by-part, and delete-by-id.
   Potential impact if wrong: renaming a part could leave inventory tracking tied to the old Part ID or fail to save at all.
   Alternative interpretations considered: prohibit renaming when inventory tracking exists; add a dedicated stored procedure to rename both records atomically; update only the part record and require manual inventory-list cleanup later.

4. The quick-add dialog should stop auto-generating the Part ID as the authoritative value and instead use spec changes only to suggest or prefill a user-editable Part ID field.
   Why this assumption is needed: the request explicitly asks to let the user edit the Part ID name instead of treating it as auto-generated, but the current quick-add dialog rebuilds the field on every spec change.
   Potential impact if wrong: a user's typed Part ID could keep getting overwritten, or the dialog could stop providing helpful defaults when specs change.
   Alternative interpretations considered: remove auto-generation entirely; keep a separate read-only preview and a manual Part ID field; only autofill the field until the user edits it once.

Please confirm, correct, or clarify these assumptions before I continue with the part modal persistence changes. I can continue with the non-ambiguous page layout and workflow changes separately, but I should not implement the modal save-path changes until these assumptions are approved.
