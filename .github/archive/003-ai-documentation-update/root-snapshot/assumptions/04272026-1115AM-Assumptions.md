Last Updated: 2026-04-27

# Assumptions For Receiving Guided Non-PO Save Behavior

## Confirmed Requirements Summary

1. Module_Receiving gets its own reusable non-PO reference catalog and its own per-part default mapping store.
2. Receiving should prompt before entering Review so the chosen non-PO reference is already visible in the review PO field.
3. The chosen non-PO reference continues to flow through the existing PO field/path for both modules.
4. Dunnage keeps its reusable saved-entry workflow and also gains a per-part remembered default non-PO reference.
5. Receiving should mirror Dunnage's reusable saved-entry workflow and also remember a per-part default non-PO reference.
6. When a part already has a saved non-PO default, the dialog should prefill that value automatically.
7. When the user confirms a different value for a part that already has a saved default, the user should be asked whether to overwrite the saved per-part default.

The requested feature spans UI workflow timing, persistence, and database schema. Before implementation, these assumptions need confirmation.

1. Assumption: Module_Receiving should get a Receiving-specific clone of the Dunnage reusable non-PO reference flow.
Why this assumption is needed: Dunnage currently uses its own dedicated dialog, DAO/service methods, table, and stored procedures for saved non-PO references. Receiving has no equivalent structure yet.
Potential impact if wrong: If you intended a shared cross-module non-PO reference store instead of a Receiving-specific one, creating separate Receiving artifacts would duplicate data and diverge behavior.
Alternative interpretations considered: Reuse Dunnage's existing `dunnage_non_po_entries` storage across both modules; create a shared module-agnostic `non_po_entries` store; create Receiving-only artifacts that mirror Dunnage.

USER ANSWER:
YES BUT ALSO SEE ASSUMPTION 5'S ANSWER

2. Assumption: The new Receiving database artifacts should mirror Dunnage's schema and behavior closely, using Receiving-specific names such as `receiving_non_po_entries`, `sp_Receiving_NonPO_GetAll`, `sp_Receiving_NonPO_Upsert`, and `sp_Receiving_NonPO_Delete`.
Why this assumption is needed: The request says to create a new modal window, database table, and stored procedures and to mimic Dunnage "to a tee," but it does not specify the exact Receiving naming convention to use.
Potential impact if wrong: Wrong naming will create migration churn, mismatch deployment expectations, or conflict with an intended existing naming standard.
Alternative interpretations considered: Use exact Dunnage naming patterns with a Receiving prefix; add the logic to an existing Receiving table; build a shared stored-procedure set with neutral names.

USER ANSWER:
YES BUT ALSO SEE ASSUMPTION 5'S ANSWER

3. Assumption: The Receiving prompt should occur exactly when the user clicks Save on the Guided Review page if the entry is Non-PO and no PO/reference value is present, instead of at the Details step where Dunnage currently prompts.
Why this assumption is needed: Your request explicitly says "when the user reaches the review page and clicks save," while Dunnage currently prompts earlier during the transition out of Details Entry.
Potential impact if wrong: Prompt timing changes the operator experience and could affect validation order, review-page expectations, and whether the chosen non-PO reference is visible before the final save.
Alternative interpretations considered: Match Dunnage's timing exactly by prompting before Review; prompt only on Review Save as requested; prompt both before Review and on Save if still blank.

USER ANSWER:
MATCH DUNNAGE ON THIS, DO THIS BEFORE ENTERING THE REVIEW PAGE SO THE REVIEW PAGE SHOWS WHAT THE USER ENTERED IN THE PO FIELD, ALSO REVIEW ASSUMPTION 5'S ANSWER

4. Assumption: The chosen non-PO reference should be written into the same PO-number field/path currently used by Receiving Guided Non-PO entries, just as Dunnage writes the chosen value into `CurrentSession.PONumber`.
Why this assumption is needed: Dunnage stores the chosen free-form non-PO value in the workflow session's PO field rather than in a separate dedicated "reason" field. Receiving likely needs the same save-path behavior to avoid invasive downstream schema changes.
Potential impact if wrong: If you expect Receiving to preserve a blank PO field and store the non-PO reason separately, writing into the PO field would make reports, exports, and review displays behave differently than intended.
Alternative interpretations considered: Reuse the existing Receiving PO field as Dunnage does; add a separate non-PO reference field to Receiving label/history tables; store both a displayed reason and a raw blank PO state.

USER ANSWER:
YES BUT ALSO SEE ASSUMPTION 5'S ANSWER

5. Assumption: The reusable modal behavior should match Dunnage feature-for-feature, including showing saved references, allowing delete, and optionally saving the current free-form value for future reuse.
Why this assumption is needed: "Mimic to a tee" implies not just collecting a one-time value, but reproducing the saved-entry workflow used in Dunnage.
Potential impact if wrong: Implementing only a simple text prompt would satisfy the save requirement but miss the reusable-reference workflow the operators currently have in Dunnage.
Alternative interpretations considered: Simple one-off prompt only; full Dunnage-style saved-reference dialog; partial clone without delete support.

USER ANSWER:
I WANT BOTH THE DUNNAGE AND RECEIVING WORKFLOWS TO ALSO ADD SAVING ON A PER PART BASIS, SO IF THE USER PREVIOUSLY SAVED A NON-PO ENTRY FOR A GIVEN RECEIVING OR DUNNAGE PART ID IT REMEMBERS WHAT THE USER SELECTED AND AUTO SELECTS / FILLS THE TEXT BOX IN THE MODAL WINDOW THAT POPS UP.  THE USER CAN STILL SELECT SOMETHING ELSE FOR THAT PART ID THOUGH, IN THAT CASE THE USER SHOULD BE ASKED IF THEY WISH TO OVERWRITE THE CURRENTLY SAVED DATA.
