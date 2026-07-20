USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Dunnage_LabelData_Delete`;

DELIMITER $$

CREATE PROCEDURE `sp_Dunnage_LabelData_Delete`(
    IN p_load_uuid CHAR(36)
)
BEGIN
    SET FOREIGN_KEY_CHECKS = 0;
    DELETE FROM dunnage_label_data
    WHERE load_uuid = p_load_uuid;
    SET FOREIGN_KEY_CHECKS = 1;
END $$

DELIMITER ;
