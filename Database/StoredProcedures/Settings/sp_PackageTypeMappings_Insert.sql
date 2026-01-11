-- =============================================
-- Stored Procedure: sp_PackageTypeMappings_Insert
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Insert Package Type Mapping
-- =============================================
DROP PROCEDURE IF EXISTS sp_PackageTypeMappings_Insert$$
CREATE PROCEDURE sp_PackageTypeMappings_Insert(
    IN p_part_prefix VARCHAR(10),
    IN p_package_type VARCHAR(50),
    IN p_is_default BOOLEAN,
    IN p_display_order INT,
    IN p_created_by INT
)
BEGIN
    INSERT INTO receiving_package_type_mapping (
        part_prefix,
        package_type,
        is_default,
        display_order,
        is_active,
        created_by
    ) VALUES (
        p_part_prefix,
        p_package_type,
        p_is_default,
        p_display_order,
        TRUE,
        p_created_by
    );

    SELECT LAST_INSERT_ID() AS id;
END$$



DELIMITER ;
