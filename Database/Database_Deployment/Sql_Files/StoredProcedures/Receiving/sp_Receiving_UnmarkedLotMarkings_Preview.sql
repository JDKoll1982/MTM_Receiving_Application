-- Stored Procedure: sp_Receiving_UnmarkedLotMarkings_Preview
-- Purpose:  Lists the unmarked receiving rows of one part, grouped by the heat value as captured and
--           the supplier, so the application can report how many rows and how many distinct lots a
--           fill-in would cover before anything is written.
--           Read only. This procedure writes nothing.
-- Parameters:
--   p_part_id VARCHAR(50) - Part to inspect, matched exactly against part_id.
-- Returns:  one row per (source, heat, supplier_name) with the number of unmarked rows it covers.
--           source is 'queue' for receiving_label_data or 'history' for receiving_history.
-- Notes:    A row counts as unmarked when heat_shape_code is null.
--           The heat value is returned exactly as captured, because normalization belongs to the
--           application: two spellings that normalize to one lot are grouped there, not here.
--           Rows with no heat, or with the value the receiving flow substitutes for blank, are
--           returned like any other and skipped by the caller, so the blank rule lives in one place.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_UnmarkedLotMarkings_Preview`;

DELIMITER $$
CREATE PROCEDURE `sp_Receiving_UnmarkedLotMarkings_Preview`(
    IN p_part_id VARCHAR(50)
)
BEGIN
    SELECT
        'queue' AS `source`,
        COALESCE(rld.`heat`, '') AS `heat`,
        COALESCE(
            NULLIF(TRIM(rld.`po_vendor`), ''),
            NULLIF(TRIM(rld.`vendor_name`), ''),
            ''
        ) AS `supplier_name`,
        COUNT(*) AS `row_count`
    FROM `receiving_label_data` AS rld
    WHERE rld.`part_id` = p_part_id
      AND rld.`heat_shape_code` IS NULL
    GROUP BY `source`, `heat`, `supplier_name`

    UNION ALL

    SELECT
        'history' AS `source`,
        COALESCE(rh.`heat`, '') AS `heat`,
        COALESCE(
            NULLIF(TRIM(rh.`po_vendor`), ''),
            NULLIF(TRIM(rh.`vendor_name`), ''),
            ''
        ) AS `supplier_name`,
        COUNT(*) AS `row_count`
    FROM `receiving_history` AS rh
    WHERE rh.`part_id` = p_part_id
      AND rh.`heat_shape_code` IS NULL
    GROUP BY `source`, `heat`, `supplier_name`;
END $$
DELIMITER ;
