USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Dunnage_LabelData_Delete`;

DELIMITER $$

CREATE PROCEDURE `sp_Dunnage_LabelData_Delete`(
    IN p_load_uuid CHAR(36)
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
    DELETE FROM dunnage_label_data
    WHERE load_uuid = p_load_uuid;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
