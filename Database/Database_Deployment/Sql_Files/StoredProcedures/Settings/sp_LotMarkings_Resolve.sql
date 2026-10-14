-- Stored Procedure: sp_LotMarkings_Resolve
-- Purpose:  Reads the marking already recorded for one lot. A row returned here is
--           authoritative: the caller must use it and must not compute a new marking.
-- Parameters:
--   p_heat_number_normalized VARCHAR(100) - The matching key produced by the normalizer.
--   p_supplier_id            VARCHAR(15)  - Supplier group, or 'NONPO'.
-- Returns:  The same column list as sp_LotMarkings_GetAll, or no row when the lot is new.
-- Errors:   none.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_LotMarkings_Resolve`;

DELIMITER $$
CREATE PROCEDURE `sp_LotMarkings_Resolve`(
    IN p_heat_number_normalized VARCHAR(100),
    IN p_supplier_id VARCHAR(15)
)
BEGIN
    SELECT
        `id`,
        `heat_number`,
        `heat_number_normalized`,
        `supplier_id`,
        `supplier_name`,
        `shape_code`,
        `color_code`,
        `marking_source`,
        `is_locked`,
        `first_seen_part_number`,
        `change_reason`,
        `resolved_by`,
        `resolved_at`,
        `notes`,
        `created_at`
    FROM `lot_markings`
    WHERE `heat_number_normalized` = p_heat_number_normalized
      AND `supplier_id` = p_supplier_id
    LIMIT 1;
END $$
DELIMITER ;
