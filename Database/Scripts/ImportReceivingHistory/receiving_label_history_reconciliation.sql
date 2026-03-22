-- ============================================================================
-- Script: receiving_label_history_reconciliation
-- Module: Receiving
-- Purpose: Reconcile active receiving label queue against archived receiving history.
-- Usage:
--   Run after clear-to-history operations to verify no orphaned or missing rows.
-- ============================================================================

USE mtm_receiving_application;

SELECT COUNT(*) AS active_queue_count
FROM receiving_label_data;

SELECT
    DATE(COALESCE(transaction_date, created_at)) AS archive_date,
    COUNT(*) AS rows_archived,
    COUNT(CASE WHEN is_non_po_item = 1 THEN 1 END) AS non_po_rows,
    COUNT(CASE WHEN po_number IS NULL OR po_number = '' THEN 1 END) AS missing_po_rows
FROM receiving_history
GROUP BY DATE(COALESCE(transaction_date, created_at))
ORDER BY archive_date DESC
LIMIT 10;

SELECT COUNT(*) AS missing_part_id
FROM receiving_history
WHERE part_id IS NULL OR part_id = '';

SELECT COUNT(*) AS missing_transaction_date
FROM receiving_history
WHERE transaction_date IS NULL;

SELECT COUNT(*) AS missing_po_line_number
FROM receiving_history
WHERE po_number IS NOT NULL
  AND po_number <> ''
  AND (po_line_number IS NULL OR po_line_number = '');

SELECT
    part_id,
    po_number,
    po_line_number,
    COUNT(*) AS duplicate_count
FROM receiving_history
GROUP BY part_id, po_number, po_line_number, transaction_date, label_number
HAVING COUNT(*) > 1
ORDER BY duplicate_count DESC, part_id
LIMIT 50;

SELECT
    id,
    part_id,
    po_number,
    po_line_number,
    quantity,
    heat,
    initial_location,
    transaction_date,
    employee_number,
    vendor_name,
    created_at
FROM receiving_history
ORDER BY created_at DESC, id DESC
LIMIT 100;
