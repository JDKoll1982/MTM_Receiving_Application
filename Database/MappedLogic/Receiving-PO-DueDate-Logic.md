# Receiving PO Due Date Logic

Last Updated: 2026-03-27

## Purpose

This file documents the approved source-selection logic for `po_due_date` in receiving workflows.
It exists so future features can reuse the same Infor Visual date-source reasoning without re-deriving it from task notes.

## Approved Source Rules

### Line-Level Due Date

- Purchasing can set a PO line due date earlier or later than the overall PO due date.
- That line-level due date is a valid source and must remain mapped for both current and future features.

### Header-Level Promise Or Desired Receive Date

- Header-level promise date or desired receive date is also a valid source and must remain mapped for future features.
- The current receiving flow does not use the header-level date as the primary stored value when a usable line-level date exists.

## Current Storage Rule

### Guided Mode

- The user selects one PO line row.
- If the selected line-level due date is not blank or `NULL`, store that value as `po_due_date`.
- Otherwise, store the header-level promise date.

### Manual PO-Assisted Entry

- The user selects one PO line row.
- If the selected line-level due date is not blank or `NULL`, store that value as `po_due_date`.
- Otherwise, store the header-level promise date.

## Blanket Orders

- Blanket-order date sources should still be mapped for future features.
- For the current feature set, blanket orders do not use a separate storage rule.
- Unless later business rules say otherwise, use the same source-selection order:
  1. selected line-level due date when present
  2. header-level promise date when the selected line-level date is blank or `NULL`

## Implementation Notes

- Keep the SQL and service-model mapping for both line-level and header-level date sources.
- Do not let guided mode and manual PO-assisted entry invent separate rules.
- Keep the persisted target field name as `po_due_date`.
- Guided mode now carries the selected line due date through `Model_InforVisualPart.DueDate`; if that is blank, it falls back to `Model_InforVisualPO.HeaderPromiseDate`.
- Manual PO-assisted entry uses the same selection order in `ApplySelectedPoPart(...)` and now copies `po_due_date` forward when rows inherit a selected PO part.
