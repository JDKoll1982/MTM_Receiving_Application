-- =============================================
-- Stored Procedure: sp_Settings_WeldedCoils_Update
-- Purpose: Renames the part number on an existing welded-coil row.
--          Blocks changing to a partid that already exists on another row.
-- Parameters:
--   IN p_id     INT         - id of the row to update
--   IN p_partid VARCHAR(11) - new part number
-- Errors: SIGNAL '45000' when the target partid exists on a different row.
-- =============================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Settings_WeldedCoils_Update`$$
CREATE PROCEDURE `sp_Settings_WeldedCoils_Update`(
    IN p_id     INT,
    IN p_partid VARCHAR(11)
)
BEGIN
    IF EXISTS (SELECT 1 FROM settings_weldedcoils WHERE partid = p_partid AND id <> p_id) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Part already exists in welded coils';
    END IF;

    UPDATE settings_weldedcoils
    SET partid = p_partid
    WHERE id = p_id;
END $$

DELIMITER ;
