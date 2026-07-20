DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Loads_Delete`$$

CREATE PROCEDURE `sp_Dunnage_Loads_Delete`(
    IN p_load_uuid CHAR(36)
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
    DELETE FROM dunnage_history
    WHERE load_uuid = p_load_uuid;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
