# Module Volvo — Database Replacement Plan

Last Updated: 2026-02-18

## Goal
Implement a two-table lifecycle for Volvo where workflow completion writes to active label queue tables consumed by LabelView2022, and `Clear Label Data` archives rows to history tables.

## Required Lifecycle (Authoritative)
1. User completes workflow
2. Save data to Volvo active label queue tables
3. User clicks `Clear Label Data`
4. Move queue rows to history tables
5. Delete moved rows from active queue tables

## Master Branch Label Output (Source of Truth)
From `master` `Helper_VolvoShipmentCalculations.GenerateLabelCsvAsync`, label rows written for printing were:
- Material
- Quantity
- Employee
- Date
- Time
- Receiver
- Notes

Workflow line/header data available throughout process includes:
- Shipment header: shipment_date, shipment_number, po_number, receiver_number, employee_number, notes, status
- Shipment line: part_number, quantity_per_skid, received_skid_count, calculated_piece_count, has_discrepancy, expected_skid_count, discrepancy_note

## Current Branch DB State
- Active tables exist:
  - `volvo_label_data` (header)
  - `volvo_line_data` (line)
- No dedicated archive tables exist; reporting views currently read active tables.

## Gap Analysis
### Active queue tables
- `volvo_label_data` + `volvo_line_data` already cover the workflow line/header fields needed to derive label rows.
- Ensure queue-to-print view exposes all LabelView2022-required columns (`Material/Quantity/Employee/Date/Time/Receiver/Notes`).

### Missing archive layer
- Need history tables mirroring active schema:
  - `volvo_label_history`
  - `volvo_line_history`
- Need linkage/audit metadata:
  - `archived_at`
  - `archived_by`
  - `archive_batch_id`

## Implementation Plan
1. **Schema additions (history layer)**
   - Create `volvo_label_history` with all business columns from `volvo_label_data` + archive metadata.
   - Create `volvo_line_history` with all business columns from `volvo_line_data` + archive metadata.
2. **Stored procedure additions**
   - Add `sp_Volvo_LabelData_ClearToHistory`:
     - copy header + lines to history tables
     - maintain referential mapping
     - delete moved rows from active tables
     - transactionally commit/rollback
3. **DAO/service updates**
   - Keep workflow save writes targeting active queue tables.
   - Wire `Clear Label Data` action to clear-to-history SP.
   - Return moved row counts and errors to UI.
4. **LabelView2022 compatibility contract**
   - Add/verify queue projection view for print app with required columns:
     - `Material` (part_number)
     - `Quantity` (calculated_piece_count or agreed print quantity)
     - `Employee` (employee_number)
     - `Date`, `Time`
     - `Receiver` (receiver_number)
     - `Notes`
   - Document definitive mapping in module docs.
5. **Reporting update**
   - Point history/report views to archive tables for historical reporting.
   - Keep active views only for current print queue.
6. **Verification**
   - Integration test: complete workflow writes to active queue tables.
   - Integration test: clear operation moves all related header/line rows and empties queue.
   - Integration test: no orphan lines/headers on failure paths.

## Deliverables
- Volvo history schema scripts (`volvo_label_history`, `volvo_line_history`).
- Transactional clear-to-history stored procedure.
- DAO/service wiring for active queue save + clear lifecycle.
- LabelView2022 field mapping documentation and tests.
