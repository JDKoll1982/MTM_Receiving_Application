-- Stored Procedure: sp_Receiving_HeatLotKeys_GetAll
-- Purpose:  Lists every distinct heat and supplier pair that any receiving row carries, in the queue
--           and in history, together with the marking those rows hold and how many rows there are.
--           The application uses it two ways: to find the rows whose marking no longer matches the
--           lot number they carry, and to find the lots in the ledger whose number no longer appears
--           on any row.
--           Read only. This procedure writes nothing.
-- Parameters: none.
-- Returns:  one row per (source, heat, supplier_name, shape_code, color_code, part_id) with a row
--           count and the date of the most recent line in that group.
--           source is 'queue' for receiving_label_data or 'history' for receiving_history.
--           shape_code and color_code are null when those rows carry no marking.
--           part_id is the part number those rows carry, empty when they carry none.
--           last_line_date is the newest receipt date in the group, falling back to when the line
--           was written when it carries no receipt date, so it is never null for a real group.
-- Notes:    The heat is returned exactly as captured, because normalization belongs to the
--           application: two spellings that normalize to one lot are reconciled there, not here.
--           A pair is returned whether or not it carries a marking, so a lot number that is still in
--           use is never mistaken for one that has gone.
--           Grouping by the marking as well as by the pair is what keeps the result small, since a
--           lot's rows collapse into a handful of entries rather than one per row.
--           Grouping by the part as well means one lot appears once for each part that carries it.
--           That is deliberate: a lot is not owned by one part, it is carried by a set of them, and
--           the set is what tells the application whether the lot belongs to material that is
--           approved. The count and the date are per group and are summed by lot in the application,
--           so a lot carried by two parts still reports its own lines once and no line is counted
--           twice.
--           The row count and the date are what let the tool say how many lines a lot covers and
--           when it was last seen. Both are read here rather than counted separately so that one
--           read answers both questions and the two answers can never disagree.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_HeatLotKeys_GetAll`;

DELIMITER $$
CREATE PROCEDURE `sp_Receiving_HeatLotKeys_GetAll`()
BEGIN
    SELECT
        'queue' AS `source`,
        COALESCE(rld.`heat`, '') AS `heat`,
        COALESCE(
            NULLIF(TRIM(rld.`po_vendor`), ''),
            NULLIF(TRIM(rld.`vendor_name`), ''),
            ''
        ) AS `supplier_name`,
        COALESCE(rld.`part_id`, '') AS `part_id`,
        rld.`heat_shape_code` AS `shape_code`,
        rld.`heat_color_code` AS `color_code`,
        COUNT(*) AS `row_count`,
        MAX(COALESCE(rld.`received_date`, rld.`created_at`)) AS `last_line_date`
    FROM `receiving_label_data` AS rld
    GROUP BY `source`, `heat`, `supplier_name`, rld.`part_id`, rld.`heat_shape_code`, rld.`heat_color_code`

    UNION ALL

    SELECT
        'history' AS `source`,
        COALESCE(rh.`heat`, '') AS `heat`,
        COALESCE(
            NULLIF(TRIM(rh.`po_vendor`), ''),
            NULLIF(TRIM(rh.`vendor_name`), ''),
            ''
        ) AS `supplier_name`,
        COALESCE(rh.`part_id`, '') AS `part_id`,
        rh.`heat_shape_code` AS `shape_code`,
        rh.`heat_color_code` AS `color_code`,
        COUNT(*) AS `row_count`,
        MAX(COALESCE(rh.`received_date`, rh.`created_at`)) AS `last_line_date`
    FROM `receiving_history` AS rh
    GROUP BY `source`, `heat`, `supplier_name`, rh.`part_id`, rh.`heat_shape_code`, rh.`heat_color_code`;
END $$
DELIMITER ;
