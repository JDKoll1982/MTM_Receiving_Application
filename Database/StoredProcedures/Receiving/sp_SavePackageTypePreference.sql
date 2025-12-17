DELIMITER //

DROP PROCEDURE IF EXISTS sp_SavePackageTypePreference //

CREATE PROCEDURE sp_SavePackageTypePreference(
    IN p_PartID VARCHAR(50),
    IN p_PackageTypeName VARCHAR(50),
    IN p_CustomTypeName VARCHAR(50),
    IN p_LastModified DATETIME
)
BEGIN
    INSERT INTO package_type_preferences (PartID, PackageTypeName, CustomTypeName, LastModified)
    VALUES (p_PartID, p_PackageTypeName, p_CustomTypeName, p_LastModified)
    ON DUPLICATE KEY UPDATE 
        PackageTypeName = p_PackageTypeName,
        CustomTypeName = p_CustomTypeName,
        LastModified = p_LastModified;
END //

DELIMITER ;
