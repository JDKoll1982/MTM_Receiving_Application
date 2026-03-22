DELIMITER //

DROP PROCEDURE IF EXISTS `sp_Receiving_PackageTypePreference_Save` //

CREATE PROCEDURE `sp_Receiving_PackageTypePreference_Save`(
    IN p_PartID VARCHAR(50),
    IN p_PackageTypeName VARCHAR(50),
    IN p_CustomTypeName VARCHAR(50),
    IN p_LastModified DATETIME
)
BEGIN
    INSERT INTO receiving_package_types (PartID, PackageTypeName, CustomTypeName, LastModified)
    VALUES (p_PartID, p_PackageTypeName, p_CustomTypeName, p_LastModified)
    ON DUPLICATE KEY UPDATE
        PackageTypeName = p_PackageTypeName,
        CustomTypeName = p_CustomTypeName,
        LastModified = p_LastModified;
END //

DELIMITER ;
