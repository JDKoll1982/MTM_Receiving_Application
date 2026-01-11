DELIMITER //

DROP PROCEDURE IF EXISTS sp_DeletePackageTypePreference //

CREATE PROCEDURE sp_DeletePackageTypePreference(
    IN p_PartID VARCHAR(50)
)
BEGIN
    DELETE FROM receiving_package_types WHERE PartID = p_PartID;
END //

DELIMITER ;
