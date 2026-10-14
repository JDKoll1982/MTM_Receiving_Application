-- Stored Procedure: sp_LotMarkingPartScope_Delete
-- Purpose:  Removes a row from the enabled-part list outright. The tool prefers
--           sp_LotMarkingPartScope_SetEnabled so the list keeps its history; this exists for
--           correcting a part number that was added by mistake.
-- Parameters:
--   p_id INT - Id of the list row.
-- Returns:  rows_deleted.
-- Errors:   none. A row that is already gone reports zero rows deleted.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_LotMarkingPartScope_Delete`;

DELIMITER $$
CREATE PROCEDURE `sp_LotMarkingPartScope_Delete`(IN p_id INT)
BEGIN
    DELETE FROM `lot_marking_part_scope`
    WHERE `id` = p_id;

    SELECT ROW_COUNT() AS rows_deleted;
END $$
DELIMITER ;
