-- ============================================================================
-- Stored Procedure: sp_Receiving_PackageTypes_Delete
-- Description: Soft delete a receiving package type
-- Parameters:
--   @p_id: The ID of the package type to delete
-- Feature: Receiving Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_PackageTypes_Delete`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_PackageTypes_Delete`(
    IN p_id INT
)
BEGIN
    -- Hard delete from receiving_package_types table
    -- This table (PreferenceID, PartID, PackageTypeName, CustomTypeName, LastModified)
    -- stores user preferences for package types per part, not the mapping table
    DELETE FROM receiving_package_types
    WHERE PreferenceID = p_id;

    SELECT ROW_COUNT() AS affected_rows;
END $$

DELIMITER ;
