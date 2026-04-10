# Assumptions For Dunnage Part Specific Specs Behavior

Last Updated: 2026-04-09

1. Assumption: "Work exactly like specs work for Dunnage Types" means the Part Specific Specs page in both Add Part ID and Edit Part ID should switch from the current simple key/value editor to the same spec-definition editor used by Dunnage Types.
   Why this assumption is needed: the current part dialogs store extra part-specific entries as direct values, while the Dunnage Type dialog stores spec definitions with field name, data type, required flag, units, numeric bounds, and choice lists.
   Potential impact if wrong: I could either overbuild the feature by changing the data contract and downstream runtime behavior, or underbuild it by only restyling the page while leaving the old key/value model in place.
   Alternative interpretations considered:

- Rebuild the Part Specific Specs page as a true spec-definition editor identical to Dunnage Types.

2. Assumption: If the page becomes a true spec-definition editor, those part-specific definitions should be persisted in a way that downstream Dunnage workflows can understand, not just displayed in the add/edit dialogs.
   Why this assumption is needed: the current `Model_DunnagePart.SpecValues` JSON is consumed throughout the Dunnage workflow as actual saved values, not field definitions.
   Potential impact if wrong: changing the page only would create a UI that saves data the rest of the Dunnage workflow cannot use correctly.
   Alternative interpretations considered:

3. Assumption: Existing saved part-specific key/value entries either need to remain supported as-is or need an explicit migration rule.
   Why this assumption is needed: current part records already store part-specific specs as plain values in `spec_values`, and Edit Part preloads and rewrites those entries today.
   Potential impact if wrong: existing Dunnage parts could lose data, become unreadable in Edit Part, or stop populating spec values in the guided/manual workflows.
   Alternative interpretations considered:

- Preserve backward compatibility with current key/value part-specific entries.
- Migrate existing entries to the new definition-based format during edit/save.

4. Assumption: The existing "Choose Existing Specs" flow should also carry over any new part-specific spec behavior, not just the configured type-spec values and notes.
   Why this assumption is needed: the current picker copies saved part values into the draft. If part-specific specs become definition-based, the reuse flow likely needs to copy those as well to stay consistent.
   Potential impact if wrong: users could see different part-specific specs depending on whether they typed them manually or reused an existing part as a template.
   Alternative interpretations considered:

- Continue copying only configured values and notes, leaving part-specific definitions behind.

5. Assumption: This change affects more than just the Add/Edit Part dialogs if you want true parity with Dunnage Type specs.
   Why this assumption is needed: type specs are definitions that later drive runtime inputs. Matching that behavior for part-specific specs would require follow-on updates in the Dunnage workflow, not just the dialog page.
   Potential impact if wrong: implementing only the dialog change would look correct visually but behave inconsistently during actual Dunnage receiving.
   Alternative interpretations considered:

- Limit the change to the add/edit dialogs only.

## User Confirmation Received 2026-04-09

1. Rebuild the Part Specific Specs page as a true spec-definition editor identical to Dunnage Types.
2. On save, combine configured type-spec values and part-specific spec definitions into one JSON payload in the part `spec_values` column.
3. Apply the new definition-based part-specific spec behavior only to newly created parts. Existing parts remain supported as legacy value-based records.
4. Rework the existing-spec template picker so copying from another part in the same type carries the new mixed spec payload correctly.
5. Update downstream details, manual, and review flows so part-specific specs participate in runtime behavior instead of being treated as plain static key/value text.
