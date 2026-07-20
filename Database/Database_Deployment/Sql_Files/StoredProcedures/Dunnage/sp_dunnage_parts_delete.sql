DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Parts_Delete`$$

CREATE PROCEDURE `sp_Dunnage_Parts_Delete`(
    IN p_id INT
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
    DELETE FROM dunnage_parts
    WHERE id = p_id;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
