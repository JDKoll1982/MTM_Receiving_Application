-- =============================================
-- Stored Procedure: sp_Receiving_PackageTypeMappings_GetByPrefix
-- Purpose: Return package_type by matching the provided part identifier
--          (accepts either a prefix or a full part number; falls back to DEFAULT)
-- =============================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Receiving_PackageTypeMappings_GetByPrefix$$
CREATE PROCEDURE sp_Receiving_PackageTypeMappings_GetByPrefix(
    IN p_part_identifier VARCHAR(100)
)
sp_end: BEGIN
    DECLARE v_package_type VARCHAR(50) DEFAULT NULL;
    DECLARE v_input VARCHAR(100) DEFAULT NULL;

    SET v_input = TRIM(IFNULL(p_part_identifier, ''));

    -- If input empty, return default mapping (if any)
    IF v_input = '' THEN
        SELECT package_type
        INTO v_package_type
        FROM receiving_package_type_mapping
        WHERE is_default = TRUE
          AND is_active = TRUE
        LIMIT 1;

        SELECT v_package_type AS package_type;
        LEAVE sp_end;
    END IF;

    -- Try to find the most specific mapping where the stored part_prefix
    -- is a prefix of the provided identifier. Prefer longer prefixes (more specific),
    -- then use display_order for tie-breaker.
    SELECT package_type
    INTO v_package_type
    FROM receiving_package_type_mapping
    WHERE is_active = TRUE
      AND v_input LIKE CONCAT(part_prefix, '%')
    ORDER BY CHAR_LENGTH(part_prefix) DESC, display_order ASC
    LIMIT 1;

    -- Fallback to default mapping if nothing found
    IF v_package_type IS NULL THEN
        SELECT package_type
        INTO v_package_type
        FROM receiving_package_type_mapping
        WHERE is_default = TRUE
          AND is_active = TRUE
        LIMIT 1;
    END IF;

    SELECT v_package_type AS package_type;

END sp_end$$

DELIMITER ;
