USE mtm_receiving_application_test;

-- Direct aggregate for 'MMC' parts (no prepared statements)
SET @part_filter = '%MMC%';

SELECT
  part_id AS part_number,
  part_description AS description,
  SUM(quantity) AS total_qty,
  SUM(CEIL(quantity / NULLIF(packages_per_load, 0))) AS est_skids
FROM receiving_history
WHERE LOWER(COALESCE(part_description, part_id)) LIKE LOWER(@part_filter)
GROUP BY part_id, part_description
ORDER BY total_qty DESC
LIMIT 25;
