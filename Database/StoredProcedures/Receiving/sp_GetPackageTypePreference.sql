DELIMITER //

DROP PROCEDURE IF EXISTS sp_GetPackageTypePreference //

CREATE PROCEDURE sp_GetPackageTypePreference(
    IN p_PartID VARCHAR(50)
)
BEGIN
    SELECT * FROM receiving_package_types WHERE PartID = p_PartID;
END //

DELIMITER ;
