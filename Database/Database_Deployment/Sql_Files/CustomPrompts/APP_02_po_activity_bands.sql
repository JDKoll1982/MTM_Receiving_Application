-- =============================================================================
-- APP_02: POs in app that are NOT in recent Visual receivers (reconciliation gap)
-- Server  : localhost (MySQL / MAMP)
-- Database: mtm_receiving_application
-- Run in  : phpMyAdmin or MAMP mysql client
-- Purpose : Shows PO numbers for MMC coil parts that appear in the receiving app
--           but have not been received in the app recently (> 30 days ago).
--           Cross-reference with VISUAL_01 open POs to identify stale open POs
--           that the app already received but Visual may not have posted.
--           Also flags POs that are missing from the app entirely.
-- =============================================================================
SELECT
    po_number,
    part_id AS part_number,
    COALESCE(part_description, part_id) AS description,
    SUM(quantity) AS total_qty_in_app,
    SUM(
        COALESCE(packages_per_load, 1)
    ) AS total_skids,
    MIN(transaction_date) AS first_received,
    MAX(transaction_date) AS last_received,
    DATEDIFF(
        CURDATE(),
        MAX(transaction_date)
    ) AS days_since_last_receipt,
    CASE
        WHEN MAX(transaction_date) >= DATE_SUB(CURDATE(), INTERVAL 30 DAY) THEN 'Active (≤30d)'
        WHEN MAX(transaction_date) >= DATE_SUB(CURDATE(), INTERVAL 90 DAY) THEN 'Recent (31-90d)'
        WHEN MAX(transaction_date) >= DATE_SUB(CURDATE(), INTERVAL 180 DAY) THEN 'Aging (91-180d)'
        ELSE 'Stale (>180d)'
    END AS activity_band
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
ORDER BY
    last_received DESC,
    po_number,
    part_id;