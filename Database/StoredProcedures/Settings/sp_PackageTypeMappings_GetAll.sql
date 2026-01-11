-- =============================================
-- Stored Procedure: sp_PackageTypeMappings_GetAll
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get Package Type Mappings
-- =============================================
DROP PROCEDURE IF EXISTS sp_PackageTypeMappings_GetAll$$
CREATE PROCEDURE sp_PackageTypeMappings_GetAll()
BEGIN
    SELECT
        id,
        part_prefix,
        package_type,
        is_default,
        display_order,
        is_active,
        created_at,
        updated_at,
        created_by
    FROM receiving_package_type_mapping
    WHERE is_active = TRUE
    ORDER BY display_order, part_prefix;
END$$



DELIMITER ;
