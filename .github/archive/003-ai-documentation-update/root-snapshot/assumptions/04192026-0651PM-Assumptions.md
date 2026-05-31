# Volvo Workflow Refactor Assumptions

Date: 2026-04-19 06:51 PM

This file documents the major assumptions that would be required to implement the requested Volvo workflow refactor. Work should not proceed until these are confirmed, corrected, or clarified.

## 1. Active Queue Scope

Assumption:
The new Volvo entry screen should load and manage all active rows from `volvo_line_data` across the current active queue, not just the current user's pending shipment.

Why this assumption is needed:
The requested Step 1 says to query `volvo_line_data` and return all rows with data, which is broader than the current shipment-based flow centered on a single pending `volvo_label_data` header.

Potential impact if wrong:
If the intended scope is only the current pending shipment, implementing a global active-queue editor would mix data across shipments and break the current shipment model.

Alternative interpretations considered:

- Load only the current pending shipment's lines.
- Load all non-archived rows for all active Volvo shipments.
- Load rows only for the current user's shipment/session.

## 2. Shipment Header Model After Removing Save/Generate/Clear Buttons

Assumption:
`volvo_label_data` remains the shipment header source of truth, and row-level auto-save updates both the line row and its parent shipment/header as needed.

Why this assumption is needed:
R4 removes `Generate Labels`, `Clear Label Data`, and `Save as Pending`, but the current model, completion flow, and history all depend on `volvo_label_data` header records still existing.

Potential impact if wrong:
If the intent is to stop using shipment headers entirely, the current completion, history, and auto-numbering design would need a deeper redesign than a UI refactor.

Alternative interpretations considered:

- Keep the existing header/line schema and auto-save into it.
- Replace the current header-first workflow with a line-only workflow.
- Keep headers but stop exposing them directly in the UI.

## 3. Header Fields Persistence

Assumption:
The top-level `Shipment Date`, `Shipment Number`, and `Order Notes` fields should remain on the page and should auto-save when changed, even though R4 only explicitly mentions row add/update/discrepancy changes.

Why this assumption is needed:
Those header fields still exist on the current Volvo page, but the requested refactor does not explicitly state whether they remain editable, auto-save, or become read-only.

Potential impact if wrong:
If those fields should not auto-save, implementing auto-save could create unintended writes or inconsistent completion behavior.

Alternative interpretations considered:

- Keep and auto-save them.
- Keep them but save only during completion.
- Remove them from the page entirely.

## 4. `po_status` Default Behavior

Assumption:
The new `po_status` column on `volvo_line_data` should default to `Pending` for new rows and switch to `Received` through a later business step or external update.

Why this assumption is needed:
R1 defines allowed values `Pending` and `Received`, while EC-5 mentions falling back to a MySQL default of `false / 0`, which conflicts with the declared string values.

Potential impact if wrong:
Using the wrong data type or default could break styling, persistence, filters, or later business logic.

Alternative interpretations considered:

- `VARCHAR` with default `Pending`.
- `ENUM('Pending','Received')` with default `Pending`.
- Boolean/integer status with UI mapping to display strings.

## 5. Duplicate Part Policy

Assumption:
Duplicate part numbers within the current active shipment/list should remain blocked.

Why this assumption is needed:
EC-7 explicitly says to decide whether duplicates should be blocked or allowed. The current implementation already blocks them.

Potential impact if wrong:
If duplicates are supposed to be allowed and merged later, continuing to block them would reject valid user input.

Alternative interpretations considered:

- Block duplicates and require editing the existing row.
- Allow duplicates as separate cards.
- Merge duplicates automatically into the existing card.

## 6. Employee Number Output Scope

Assumption:
R5 means any user identity written during Volvo persistence/archive operations should use the authenticated employee number instead of the Windows username, including archive metadata such as `archived_by`.

Why this assumption is needed:
The request says output going to `volvo_label_data` and `volvo_label_history` should use employee number, but the currently visible issue also affects archive metadata and related history writes.

Potential impact if wrong:
If only `employee_number` should change but `archived_by` should remain Windows username, broadening the change could alter audit semantics unexpectedly.

Alternative interpretations considered:

- Change only `employee_number` fields.
- Change all persisted actor identity fields, including archive metadata.
- Resolve employee number for headers only, not line/history audit records.

## 7. Current History/Edit Dialog Reuse vs New Card-First UI

Assumption:
The existing shipment history and edit dialog should remain intact unless directly required by the new card-based main entry experience.

Why this assumption is needed:
The requested changes target the main Volvo entry workflow, but the current module also has a separate history page and edit dialog with overlapping grid behavior.

Potential impact if wrong:
If history/edit must also move to the new card workflow, only refactoring the entry page would leave the module inconsistent.

Alternative interpretations considered:

- Refactor only the main entry page for now.
- Refactor both entry and history/edit experiences together.
- Keep history grid-based and entry card-based.

## 8. Auto-Save Trigger Timing and Retry UX

Assumption:
Auto-save should happen immediately after a successful user edit/add/remove/discrepancy action completes, using existing non-blocking error handling patterns from shared services.

Why this assumption is needed:
EC-8, EC-20, and EC-27 require async-safe auto-save behavior, but the spec does not define exact debounce, queueing, or retry mechanics.

Potential impact if wrong:
Choosing the wrong auto-save timing could introduce race conditions, stale writes, or a frustrating UI.

Alternative interpretations considered:

- Immediate save on every edit commit.
- Debounced save after short idle.
- Queue-based serialized saves per row or shipment.

## Request For Confirmation

Please confirm, correct, or clarify the assumptions above before implementation continues.

Recommended minimum decisions to unblock coding:

1. Should the page load all active `volvo_line_data` rows, or only the current pending shipment's rows?
2. Should duplicate part numbers be blocked, allowed, or auto-merged?
3. What is the exact storage type/default for `po_status`?
4. Should `archived_by` also switch from Windows username to employee number?
5. Should the top header fields remain and auto-save, or should the refactor remove/redefine them?
