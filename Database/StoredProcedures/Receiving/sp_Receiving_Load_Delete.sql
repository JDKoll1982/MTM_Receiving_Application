-- Stored Procedure: sp_Receiving_Load_Delete
-- Description: Deletes a receiving load record
-- Parameters: p_LoadID

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_Load_Delete`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_Load_Delete`(
    IN p_LoadID VARCHAR(36)
)
BEGIN
    DELETE FROM receiving_history
    WHERE LoadID = p_LoadID;
END$$

DELIMITER ;
