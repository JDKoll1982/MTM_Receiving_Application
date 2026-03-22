DELIMITER //

DROP PROCEDURE IF EXISTS `sp_Receiving_PackageTypePreference_Get` //

CREATE PROCEDURE `sp_Receiving_PackageTypePreference_Get`(
    IN p_PartID VARCHAR(50)
)
BEGIN
    SELECT * FROM receiving_package_types WHERE PartID = p_PartID;
END //

DELIMITER ;
