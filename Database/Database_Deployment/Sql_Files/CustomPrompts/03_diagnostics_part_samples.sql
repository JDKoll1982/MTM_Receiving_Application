USE mtm_receiving_application;

-- Total rows
SELECT COUNT(*) AS receiving_history_rows FROM receiving_history;

-- Distinct part descriptions (lowercase) to inspect common tokens
SELECT DISTINCT LOWER(part_description) AS desc_sample
FROM receiving_history
WHERE part_description IS NOT NULL
LIMIT 50;

-- Distinct part ids to inspect patterns
SELECT DISTINCT part_id
FROM receiving_history
LIMIT 50;

-- A sample of recent rows
SELECT part_id, part_description, quantity, coils_on_skid, packages_per_load, transaction_date
FROM receiving_history
ORDER BY transaction_date DESC
LIMIT 20;
