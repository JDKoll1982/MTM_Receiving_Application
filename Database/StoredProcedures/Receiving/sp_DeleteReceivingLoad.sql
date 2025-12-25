-- Stored Procedure: sp_DeleteReceivingLoad
-- Description: Deletes a receiving load record
-- Parameters: p_LoadID

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_DeleteReceivingLoad;

DELIMITER $$

CREATE PROCEDURE sp_DeleteReceivingLoad(
    IN p_LoadID VARCHAR(36)
)
BEGIN
    DELETE FROM receiving_loads 
    WHERE LoadID = p_LoadID;
END$$

DELIMITER ;
