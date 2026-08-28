-- =============================================
-- Stored Procedure: sp_Settings_WeldedCoils_Delete
-- Purpose: Removes a welded-coil row.
-- Parameters:
--   IN p_id INT - id of the row to delete
-- =============================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Settings_WeldedCoils_Delete`$$
CREATE PROCEDURE `sp_Settings_WeldedCoils_Delete`(
    IN p_id INT
)
BEGIN
    DELETE FROM settings_weldedcoils
    WHERE id = p_id;
END $$

DELIMITER ;
