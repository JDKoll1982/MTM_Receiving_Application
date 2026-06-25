-- BEGIN SCRIPT
USE mtm_receiving_application;

-- =========================
-- 1) Example: common schema pattern
-- Tables:
-- receiver_line (part_id, received_qty, skid_count) OR
-- receiver_line (part_id, received_qty) + part table (id, part_number, description)
-- Adjust names below to match your schema.
-- =========================

-- Example A: If you store qty per line and have a separate part table:
SELECT
p.part_number AS part_number,
p.description AS description,
SUM(rl.received_qty) AS total_received_qty,
SUM(COALESCE(rl.skid_count, CEIL(rl.received_qty / NULLIF(p.quantity_per_skid,0)))) AS total_skid_estimate
FROM receiver_line rl
LEFT JOIN part p ON p.id = rl.part_id
WHERE LOWER(COALESCE(p.description, p.part_number)) LIKE '%coil%'
GROUP BY p.part_number, p.description
ORDER BY total_received_qty DESC
LIMIT 25;

-- Example B: If receiver_line already contains a skid_count column:
SELECT
rl.part_number AS part_number,
rl.part_description AS description,
SUM(rl.received_qty) AS total_received_qty,
SUM(rl.skid_count) AS total_skid_count
FROM receiver_line rl
WHERE LOWER(COALESCE(rl.part_description, rl.part_number)) LIKE '%coil%'
GROUP BY rl.part_number, rl.part_description
ORDER BY total_received_qty DESC
LIMIT 25;

-- =========================
-- 2) Top by skid count specifically (if skid_count exists)
-- If you only have quantity-per-skid, it estimates skids as CEIL(qty / qty_per_skid).
-- =========================

-- Using explicit skid_count column:
SELECT
rl.part_number,
rl.part_description,
SUM(rl.skid_count) AS total_skids,
SUM(rl.received_qty) AS total_qty
FROM receiver_line rl
WHERE LOWER(COALESCE(rl.part_description, rl.part_number)) LIKE '%coil%'
GROUP BY rl.part_number, rl.part_description
ORDER BY total_skids DESC
LIMIT 25;

-- Estimating skids from quantity_per_skid:
SELECT
p.part_number,
p.description,
SUM(rl.received_qty) AS total_qty,
SUM(CEIL(rl.received_qty / NULLIF(p.quantity_per_skid,0))) AS estimated_total_skids
FROM receiver_line rl
LEFT JOIN part p ON p.id = rl.part_id
WHERE LOWER(COALESCE(p.description, p.part_number)) LIKE '%coil%'
GROUP BY p.part_number, p.description
ORDER BY estimated_total_skids DESC
LIMIT 25;

-- =========================
-- 3) If your "coil" ID is stored in a part-type column rather than description,
-- change the WHERE clause to match that column (e.g., p.part_type = 'Coil').
-- =========================

-- Example: part_type approach
SELECT
p.part_number,
p.description,
SUM(rl.received_qty) AS total_qty,
SUM(CEIL(rl.received_qty / NULLIF(p.quantity_per_skid,0))) AS estimated_total_skids
FROM receiver_line rl
LEFT JOIN part p ON p.id = rl.part_id
WHERE p.part_type = 'Coil'
GROUP BY p.part_number, p.description
ORDER BY total_qty DESC
LIMIT 25;

-- END SCRIPT