-- =============================================
-- Stored Procedure: sp_Settings_WeldedCoils_SetActive
-- Purpose: Flips a welded-coil row between Active (1) and Inactive (0).
-- Parameters:
--   IN p_id        INT       - id of the row to update
--   IN p_is_active TINYINT(1) - 1 = active, 0 = inactive
-- =============================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Settings_WeldedCoils_SetActive`$$
CREATE PROCEDURE `sp_Settings_WeldedCoils_SetActive`(
    IN p_id        INT,
    IN p_is_active TINYINT(1)
)
BEGIN
    UPDATE settings_weldedcoils
    SET isActive = p_is_active
    WHERE id = p_id;
END $$

DELIMITER ;
