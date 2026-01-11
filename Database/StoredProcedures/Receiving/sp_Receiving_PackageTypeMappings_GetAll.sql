DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Receiving_PackageTypeMappings_GetAll$$
CREATE PROCEDURE sp_Receiving_PackageTypeMappings_GetAll(IN p_includeInactive BOOLEAN)
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
    FROM mtm_receiving_application.receiving_package_type_mapping
    WHERE (is_active = TRUE) OR (p_includeInactive = TRUE)
    ORDER BY display_order, part_prefix;
END$$

DELIMITER ;
