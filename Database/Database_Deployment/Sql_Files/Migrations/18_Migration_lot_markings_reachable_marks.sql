-- =====================================================================
-- Migration: 18_Migration_lot_markings_reachable_marks
-- Module:    ShipRec Tools (Heat / Lot Management)
-- Purpose:
--   Brings markings the ledger already holds back within reach of the receiving lines they belong
--   to, so the Heat / Lot list reads the recorded marking rather than the codes the lines carry, and
--   so a user can change or lock the marking again.
--
-- Background:
--   A marking is keyed on (normalized lot number, supplier group). The supplier group is the official
--   Infor Visual vendor id when that lookup returns exactly one match, and the normalized vendor name
--   otherwise. One vendor can therefore be keyed two ways, and a receipt saved on the id side leaves
--   a marking that no receiving line can reach, because every line carries the name.
--
--   The receipts saved on 2026-11-05 around 13:03 hit exactly that. Three rows are affected:
--     117  842T30290  supplier group VID-001080-01, lines carry MST STEEL CORPORATION
--     118  346732     supplier group VID-001080-01, a second marking for a lot row 81 already holds
--     119  T46733     supplier group VID-001080-01, and the lines carry the number as 346733
--   Row 119 is unreachable twice over: the number and the supplier are both different from the lines.
--
--   The application no longer hides this. The lot list is read from the receiving lines, so material
--   that came in is listed even when its marking is unreachable; such a lot shows the shape and color
--   its own lines carry. This file is what puts the recorded marking back behind those lines.
--
-- Lifecycle: Run AFTER 17_Migration_lot_markings_supplier_group_width. Safe to re-run at any time.
--
-- Superseded: the resolver now reads the ledger under the supplier id *and* under the supplier name the
--             receipt carries before it creates a marking, so a marking no line can reach is no longer
--             created by saving a receipt. This file stays for a database repaired before that change,
--             where the rows described above can still exist. Where Update all lots has already been run,
--             the rows it names are gone and both statements below change nothing.
--
-- Safety:    Read this file before running it.
--            1. Back up first, from a shell, with the credentials you already use:
--                 mysqldump --single-transaction --routines mtm_receiving_application > before_18.sql
--            2. Run the DIAGNOSTICS section and read the output. It writes nothing.
--            3. Run the REPAIR section inside a transaction, then check VERIFICATION before COMMIT.
--               ROLLBACK undoes everything the repair did.
--            4. Every statement is idempotent and carries its own guard, so a second run changes
--               nothing and reports fewer rows.
--            No row is deleted by the REPAIR section. The OPTIONAL section at the end removes one row
--            a person has to choose to remove, and stays commented out for that reason.
-- =====================================================================

USE mtm_receiving_application;

-- ---------------------------------------------------------------------
-- DIAGNOSTICS - read only. Expect no rows after the repair has run.
-- ---------------------------------------------------------------------

-- 1. Markings no receiving line can reach. The lot number is on lines, but the supplier group the
--    ledger keyed the marking on is on none of them, so the marker is invisible to every reader that
--    matches lines to markings.
SELECT
    lm.`id`,
    lm.`heat_number` AS `lot_number_on_marking`,
    lm.`supplier_id` AS `supplier_group_on_marking`,
    lm.`color_code`,
    line_groups.`supplier_groups_on_lines`,
    line_groups.`line_count`
FROM `lot_markings` AS lm
JOIN (
    SELECT
        g.`heat` AS `heat_text`,
        GROUP_CONCAT(DISTINCT g.`supplier_group` ORDER BY g.`supplier_group` SEPARATOR ' | ')
            AS `supplier_groups_on_lines`,
        SUM(g.`line_count`) AS `line_count`
    FROM (
        SELECT
            `heat`,
            UPPER(TRIM(COALESCE(NULLIF(TRIM(`po_vendor`), ''), NULLIF(TRIM(`vendor_name`), ''), '')))
                AS `supplier_group`,
            COUNT(*) AS `line_count`
        FROM `receiving_label_data`
        WHERE `heat` IS NOT NULL AND TRIM(`heat`) <> ''
        GROUP BY `heat`, `supplier_group`

        UNION ALL

        SELECT
            `heat`,
            UPPER(TRIM(COALESCE(NULLIF(TRIM(`po_vendor`), ''), NULLIF(TRIM(`vendor_name`), ''), ''))),
            COUNT(*)
        FROM `receiving_history`
        WHERE `heat` IS NOT NULL AND TRIM(`heat`) <> ''
        GROUP BY `heat`, 2
    ) AS g
    GROUP BY g.`heat`
) AS line_groups ON line_groups.`heat_text` = lm.`heat_number`
WHERE NOT EXISTS (
        SELECT 1
        FROM `receiving_label_data` AS rld
        WHERE rld.`heat` = lm.`heat_number`
          AND UPPER(TRIM(COALESCE(NULLIF(TRIM(rld.`po_vendor`), ''), NULLIF(TRIM(rld.`vendor_name`), ''), '')))
              = lm.`supplier_id`
    )
  AND NOT EXISTS (
        SELECT 1
        FROM `receiving_history` AS rh
        WHERE rh.`heat` = lm.`heat_number`
          AND UPPER(TRIM(COALESCE(NULLIF(TRIM(rh.`po_vendor`), ''), NULLIF(TRIM(rh.`vendor_name`), ''), '')))
              = lm.`supplier_id`
    )
ORDER BY lm.`id`;

-- 2. Markings whose lot number appears on no line at all. These are lots that were corrected away or
--    entered by hand. The tool's own Update all lots removes the ones that carry no locking, so this
--    list is for reading, not for this file to act on.
SELECT
    lm.`id`,
    lm.`heat_number`,
    lm.`supplier_id` AS `supplier_group`,
    lm.`color_code`,
    lm.`is_locked`,
    lm.`resolved_at`
FROM `lot_markings` AS lm
WHERE NOT EXISTS (
        SELECT 1 FROM `receiving_label_data` AS rld WHERE rld.`heat` = lm.`heat_number`
    )
  AND NOT EXISTS (SELECT 1 FROM `receiving_history` AS rh WHERE rh.`heat` = lm.`heat_number`)
ORDER BY lm.`id`;

-- ---------------------------------------------------------------------
-- REPAIR - run inside a transaction so it can be rolled back.
-- ---------------------------------------------------------------------
START TRANSACTION;

-- 3. The lot recorded as 'T46733' whose receipts carry '346733'. The number was captured one
--    character off when the marking was first recorded, so no line has ever matched it.
--    Guarded on: the wrong number still being there, a line actually carrying the corrected number,
--    and no other marking already holding that number for the same supplier.
UPDATE `lot_markings` AS lm
SET lm.`heat_number` = '346733',
    lm.`heat_number_normalized` = '346733'
WHERE lm.`heat_number_normalized` = 'T46733'
  AND (
        EXISTS (SELECT 1 FROM `receiving_label_data` AS rld WHERE rld.`heat` = '346733')
        OR EXISTS (SELECT 1 FROM `receiving_history` AS rh WHERE rh.`heat` = '346733')
      )
  AND NOT EXISTS (
        SELECT 1
        FROM (SELECT `heat_number_normalized`, `supplier_id` FROM `lot_markings`) AS other
        WHERE other.`heat_number_normalized` = '346733'
          AND other.`supplier_id` = lm.`supplier_id`
    );

-- 4. Markings whose supplier group no line carries, when every line for that number carries one and
--    the same other group and no marking already holds that pair. This is the vendor-keyed-two-ways
--    case: the marking was recorded against the official supplier id while the receipts carry the
--    name. Re-keying it puts the recorded marking back behind its lines and keeps its shape and
--    color, so a handler sees the same marking the receipt already showed.
--    Guarded so it never merges two markings: a row is left alone when a marking for the target
--    pair already exists. Row 118 is left alone exactly that way, because row 81 already holds
--    (346732, MST STEEL CORPORATION), and which of the two markings should win is a decision for a
--    person, not for a migration.
UPDATE `lot_markings` AS lm
JOIN (
    SELECT
        g.`heat` AS `heat_text`,
        MIN(g.`supplier_group`) AS `supplier_group`,
        COUNT(DISTINCT g.`supplier_group`) AS `group_count`
    FROM (
        SELECT
            `heat`,
            UPPER(TRIM(COALESCE(NULLIF(TRIM(`po_vendor`), ''), NULLIF(TRIM(`vendor_name`), ''), '')))
                AS `supplier_group`
        FROM `receiving_label_data`
        WHERE `heat` IS NOT NULL AND TRIM(`heat`) <> ''
        GROUP BY `heat`, `supplier_group`

        UNION ALL

        SELECT
            `heat`,
            UPPER(TRIM(COALESCE(NULLIF(TRIM(`po_vendor`), ''), NULLIF(TRIM(`vendor_name`), ''), '')))
        FROM `receiving_history`
        WHERE `heat` IS NOT NULL AND TRIM(`heat`) <> ''
        GROUP BY `heat`, 2
    ) AS g
    GROUP BY g.`heat`
) AS line_groups ON line_groups.`heat_text` = lm.`heat_number`
SET lm.`supplier_id` = line_groups.`supplier_group`,
    lm.`supplier_name` = line_groups.`supplier_group`
WHERE line_groups.`group_count` = 1
  AND lm.`supplier_id` <> line_groups.`supplier_group`
  AND NOT EXISTS (
        SELECT 1
        FROM `receiving_label_data` AS rld
        WHERE rld.`heat` = lm.`heat_number`
          AND UPPER(TRIM(COALESCE(NULLIF(TRIM(rld.`po_vendor`), ''), NULLIF(TRIM(rld.`vendor_name`), ''), '')))
              = lm.`supplier_id`
    )
  AND NOT EXISTS (
        SELECT 1
        FROM `receiving_history` AS rh
        WHERE rh.`heat` = lm.`heat_number`
          AND UPPER(TRIM(COALESCE(NULLIF(TRIM(rh.`po_vendor`), ''), NULLIF(TRIM(rh.`vendor_name`), ''), '')))
              = lm.`supplier_id`
    )
  AND NOT EXISTS (
        SELECT 1
        FROM (SELECT `heat_number_normalized`, `supplier_id` FROM `lot_markings`) AS other
        WHERE other.`heat_number_normalized` = lm.`heat_number_normalized`
          AND other.`supplier_id` = line_groups.`supplier_group`
    );

-- The two statements above change 0 rows on a second run. Check that before committing:

-- ---------------------------------------------------------------------
-- VERIFICATION
-- ---------------------------------------------------------------------

-- 5. Every marking held for part MMC0000848 whose lines were received between 2026-10-06 and
--    2026-11-06, with whether its supplier group is one the lines actually carry. After the repair
--    every row should read 'reachable'. Row 118 is expected to read 'duplicate marking' - it is the
--    second marking for lot 346732 and is left for a person to decide on.
SELECT
    lm.`id`,
    lm.`heat_number` AS `lot_number`,
    lm.`supplier_id` AS `supplier_group`,
    lm.`color_code`,
    CASE
        WHEN EXISTS (
            SELECT 1
            FROM (
                SELECT `heat`, `po_vendor`, `vendor_name`
                FROM `receiving_label_data`
                WHERE COALESCE(`received_date`, `created_at`) >= '2026-10-06'
                  AND COALESCE(`received_date`, `created_at`) < '2026-11-07'
                UNION ALL
                SELECT `heat`, `po_vendor`, `vendor_name`
                FROM `receiving_history`
                WHERE COALESCE(`received_date`, `created_at`) >= '2026-10-06'
                  AND COALESCE(`received_date`, `created_at`) < '2026-11-07'
            ) AS r
            WHERE r.`heat` = lm.`heat_number`
              AND UPPER(TRIM(COALESCE(NULLIF(TRIM(r.`po_vendor`), ''), NULLIF(TRIM(r.`vendor_name`), ''), '')))
                  = lm.`supplier_id`
        ) THEN 'reachable'
        WHEN EXISTS (
            SELECT 1
            FROM (SELECT `id`, `heat_number_normalized` FROM `lot_markings`) AS other
            WHERE other.`heat_number_normalized` = lm.`heat_number_normalized`
              AND other.`id` <> lm.`id`
        ) THEN 'duplicate marking'
        ELSE 'unreachable'
    END AS `state`
FROM `lot_markings` AS lm
WHERE lm.`heat_number` IN ('344934', '345121', '346732', '346733', '842T30290')
ORDER BY lm.`heat_number`, lm.`id`;

-- 6. Re-run DIAGNOSTICS 1. After the repair it should return row 118 only, which is the duplicate
--    left for a person. Any other row it returns is a marking this file could not repair on its own.
--    The repair is still uncommitted at this point: nothing is kept until you commit.

-- ---------------------------------------------------------------------
-- OPTIONAL - nothing below runs while it stays commented out.
-- ---------------------------------------------------------------------

-- 7. Remove the second marking the 2026-11-05 receipt created for lot 346732, keeping the marking
--    the lot already had (row 81, TEAL). Guarded to that row, to that lot number, and to the lot
--    still being on receiving lines, so it does nothing on a re-run.
--    Run Update all lots in the Heat / Lot Management tool afterwards. That writes the surviving
--    marking onto the lines that were saved with the other one, and removes the lots no line carries.
--
-- DELETE FROM `lot_markings`
-- WHERE `id` = 118
--   AND `heat_number` = '346732'
--   AND (
--         EXISTS (SELECT 1 FROM `receiving_label_data` AS rld WHERE rld.`heat` = '346732')
--         OR EXISTS (SELECT 1 FROM `receiving_history` AS rh WHERE rh.`heat` = '346732')
--       );

-- ---------------------------------------------------------------------
-- Finish: run exactly one of these two lines yourself.
-- ---------------------------------------------------------------------
-- COMMIT;    -- keeps the repair
-- ROLLBACK;  -- undoes everything since START TRANSACTION
