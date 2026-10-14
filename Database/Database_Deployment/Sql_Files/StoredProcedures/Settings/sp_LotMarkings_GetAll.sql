-- Stored Procedure: sp_LotMarkings_GetAll
-- Purpose:  Lists every recorded heat/lot marking, newest first.
-- Parameters: none.
-- Returns:  id, heat_number, heat_number_normalized, supplier_id, supplier_name,
--           shape_code, color_code, marking_source, is_locked, first_seen_part_number,
--           change_reason, resolved_by, resolved_at, notes, created_at.
-- Errors:   none.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_LotMarkings_GetAll`;

DELIMITER $$
CREATE PROCEDURE `sp_LotMarkings_GetAll`()
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
    ORDER BY `resolved_at` DESC, `heat_number` ASC;
END $$
DELIMITER ;
