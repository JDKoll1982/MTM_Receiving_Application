-- Stored Procedure: sp_Receiving_Load_Delete
-- Description: Deletes a receiving load record by GUID

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Receiving_Load_Delete` $$

CREATE PROCEDURE `sp_Receiving_Load_Delete`(
    IN p_LoadGuid CHAR(36)
)
BEGIN
    DELETE FROM receiving_history
    WHERE load_guid = p_LoadGuid;
END$$

DELIMITER ;
