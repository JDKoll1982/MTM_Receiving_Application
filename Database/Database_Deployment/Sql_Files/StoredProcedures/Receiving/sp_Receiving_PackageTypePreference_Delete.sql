DELIMITER //

DROP PROCEDURE IF EXISTS `sp_Receiving_PackageTypePreference_Delete` //

CREATE PROCEDURE `sp_Receiving_PackageTypePreference_Delete`(
    IN p_PartID VARCHAR(50)
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
    DELETE FROM receiving_package_types WHERE PartID = p_PartID;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END //

DELIMITER ;
