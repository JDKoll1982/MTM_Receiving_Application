-- Stored Procedure: sp_Receiving_Load_Delete
-- Description: Deletes a receiving load record by GUID.
--              Parameter name p_LoadID matches Dao_ReceivingLoad.DeleteLoadsAsync,
--              which sends { "p_LoadID", load.LoadID.ToString() } — the DAO helper
--              preserves the existing p_ prefix, so the parameter arrives as @p_LoadID.

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Receiving_Load_Delete` $$

CREATE PROCEDURE `sp_Receiving_Load_Delete`(
    IN p_LoadID CHAR(36)
)
BEGIN
    DELETE FROM receiving_history
    WHERE load_guid = p_LoadID;
END$$

DELIMITER ;
