-- =============================================
-- Stored Procedure: sp_Settings_WeldedCoils_Insert
-- Purpose: Adds a new welded-coil part (defaults isActive = 1).
--          Blocks duplicate part numbers at the SP layer because the live
--          table has no unique key on partid.
-- Parameters:
--   IN  p_partid  VARCHAR(11) - part number to add
--   OUT p_new_id  INT         - id of the new row
-- Errors: SIGNAL '45000' when the part already exists.
-- =============================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Settings_WeldedCoils_Insert`$$
CREATE PROCEDURE `sp_Settings_WeldedCoils_Insert`(
    IN  p_partid  VARCHAR(11),
    OUT p_new_id  INT
)
BEGIN
    IF EXISTS (SELECT 1 FROM settings_weldedcoils WHERE partid = p_partid) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Part already exists in welded coils';
    END IF;

    INSERT INTO settings_weldedcoils (partid, isActive)
    VALUES (p_partid, 1);

    SET p_new_id = LAST_INSERT_ID();
END $$

DELIMITER ;
