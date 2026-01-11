-- =============================================
-- Stored Procedure: sp_PackageTypeMappings_GetByPrefix
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get Package Type for Part Prefix
-- =============================================
DROP PROCEDURE IF EXISTS sp_PackageTypeMappings_GetByPrefix$$
CREATE PROCEDURE sp_PackageTypeMappings_GetByPrefix(
    IN p_part_prefix VARCHAR(10)
)
BEGIN
    DECLARE v_package_type VARCHAR(50);

    -- Try exact match first
    SELECT package_type
    INTO v_package_type
    FROM receiving_package_type_mapping
    WHERE part_prefix = p_part_prefix
      AND is_active = TRUE
    LIMIT 1;

    -- If no match, get default
    IF v_package_type IS NULL THEN
        SELECT package_type
        INTO v_package_type
        FROM receiving_package_type_mapping
        WHERE is_default = TRUE
          AND is_active = TRUE
        LIMIT 1;
    END IF;

    SELECT v_package_type AS package_type;
END$$



DELIMITER ;
