-- Report: Recent coil activity by location (last 90 days)
-- Description: Shows which locations have been actively receiving coils recently.
--              Locations with no recent activity may be stale/empty or reassigned.
--              Locations with heavy recent activity may need expanded space.
-- Usage: Paste directly into phpMyAdmin or pipe into MAMP mysql client.
--        Edit the LIKE literals in the WHERE clause to change the coil filter.
--        Edit the INTERVAL value to adjust the lookback window (default: 90 days).

SELECT
  COALESCE(initial_location, '** NO LOCATION **')          AS location,
  part_id                                                   AS part_number,
  COALESCE(part_description, part_id)                       AS description,
  COUNT(*)                                                  AS receiving_events,
  SUM(COALESCE(packages_per_load, 1))                       AS total_skids,
  SUM(quantity)                                             AS total_qty,
  MIN(transaction_date)                                     AS first_in_window,
  MAX(transaction_date)                                     AS last_received
FROM receiving_history
WHERE (LOWER(part_id)          LIKE '%mmc%'
    OR LOWER(part_description) LIKE '%mmc%')
  AND transaction_date >= DATE_SUB(CURDATE(), INTERVAL 90 DAY)
GROUP BY initial_location, part_id, part_description
ORDER BY location ASC, last_received DESC
LIMIT 200;

-- End of file
