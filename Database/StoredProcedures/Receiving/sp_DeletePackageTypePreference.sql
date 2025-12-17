DELIMITER //

DROP PROCEDURE IF EXISTS sp_DeletePackageTypePreference //

CREATE PROCEDURE sp_DeletePackageTypePreference(
    IN p_PartID VARCHAR(50)
)
BEGIN
    DELETE FROM package_type_preferences WHERE PartID = p_PartID;
END //

DELIMITER ;
