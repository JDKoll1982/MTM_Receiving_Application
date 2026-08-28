-- =============================================
-- Stored Procedure: sp_Settings_WeldedCoils_GetAll
-- Purpose: Returns all welded-coil rows, active first then by part number.
-- Returns: result set of (id, partid, isActive)
-- =============================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Settings_WeldedCoils_GetAll`$$
CREATE PROCEDURE `sp_Settings_WeldedCoils_GetAll`()
BEGIN
    SELECT
        id,
        partid,
        isActive
    FROM settings_weldedcoils
    ORDER BY isActive DESC, partid;
END $$

DELIMITER ;
