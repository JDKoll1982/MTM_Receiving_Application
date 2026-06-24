-- Report: Coil parts received without a location assignment
-- Description: Lists coil parts where initial_location was not recorded.
--              These are blind spots in your warehouse map. High skid counts here
--              mean material sitting somewhere with no tracking.
-- Usage: Paste directly into phpMyAdmin or pipe into MAMP mysql client.
--        Edit the LIKE literals in the WHERE clause to change the coil filter.

SELECT
  part_id                                                   AS part_number,
  COALESCE(part_description, part_id)                       AS description,
  COUNT(*)                                                  AS events_without_location,
  SUM(COALESCE(packages_per_load, 1))                       AS untracked_skids,
  SUM(quantity)                                             AS untracked_qty,
  MIN(transaction_date)                                     AS first_untracked,
  MAX(transaction_date)                                     AS last_untracked
FROM receiving_history
WHERE (LOWER(part_id)          LIKE '%mmc%'
    OR LOWER(part_description) LIKE '%mmc%')
  AND (initial_location IS NULL OR TRIM(initial_location) = '')
GROUP BY part_id, part_description
ORDER BY untracked_skids DESC
LIMIT 50;

-- End of file
