DELIMITER //

DROP PROCEDURE IF EXISTS sp_GetPackageTypePreference //

CREATE PROCEDURE sp_GetPackageTypePreference(
    IN p_PartID VARCHAR(50)
)
BEGIN
    SELECT * FROM package_type_preferences WHERE PartID = p_PartID;
END //

DELIMITER ;
