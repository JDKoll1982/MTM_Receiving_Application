DELIMITER //

DROP PROCEDURE IF EXISTS `sp_Receiving_PackageTypePreference_Delete` //

CREATE PROCEDURE `sp_Receiving_PackageTypePreference_Delete`(
    IN p_PartID VARCHAR(50)
)
BEGIN
    DELETE FROM receiving_package_types WHERE PartID = p_PartID;
END //

DELIMITER ;
