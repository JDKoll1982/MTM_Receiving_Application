-- Stored Procedure: sp_LotMarkingPartScope_GetAll
-- Purpose:  Lists the part numbers that receive heat/lot markings, enabled ones first.
-- Parameters: none.
-- Returns:  id, part_id, is_enabled, added_by, added_at.
-- Errors:   none.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_LotMarkingPartScope_GetAll`;

DELIMITER $$
CREATE PROCEDURE `sp_LotMarkingPartScope_GetAll`()
BEGIN
    SELECT
        `id`,
        `part_id`,
        `is_enabled`,
        `added_by`,
        `added_at`
    FROM `lot_marking_part_scope`
    ORDER BY `is_enabled` DESC, `part_id` ASC;
END $$
DELIMITER ;
