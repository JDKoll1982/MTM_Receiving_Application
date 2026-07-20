-- =============================================================================
-- APP_01: Receiving app PO receipt summary (MySQL side of reconciliation)
-- Server  : localhost (MySQL / MAMP)
-- Database: mtm_receiving_application
-- Run in  : phpMyAdmin or MAMP mysql client
-- Purpose : Summarises what the receiving app recorded per PO number and part
--           for MMC coil parts. Run alongside VISUAL_02 in SSMS to compare
--           Visual-recorded totals vs app-recorded totals.
--           Differences may indicate:
--             - Labels printed in the app but not yet posted in Visual
--             - Receiving events done in Visual only (bypassing the app)
--             - Data-entry errors in either system
-- =============================================================================
SELECT
    po_number,
    part_id AS part_number,
    COALESCE(part_description, part_id) AS description,
    COUNT(*) AS label_events,
    SUM(quantity) AS qty_received_app,
    SUM(
        COALESCE(packages_per_load, 1)
    ) AS total_skids,
    MIN(transaction_date) AS first_received,
    MAX(transaction_date) AS last_received
FROM receiving_history
WHERE (
        LOWER(part_id) LIKE '%mmc%'
        OR LOWER(part_description) LIKE '%mmc%'
    )
    AND po_number IS NOT NULL
GROUP BY
    po_number,
    part_id,
    part_description
ORDER BY po_number, part_id;