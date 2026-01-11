DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Loads_Delete`$$

CREATE PROCEDURE `sp_Dunnage_Loads_Delete`(
    IN p_load_uuid CHAR(36)
)
BEGIN
    DELETE FROM dunnage_history
    WHERE load_uuid = p_load_uuid;
END$$

DELIMITER ;
