-- Fixed: sp_Receiving_PackageTypeMappings_Insert
-- Aligns with receiving_package_type_mapping schema, normalizes prefix,
-- ensures single default, and upserts on duplicate part_prefix returning id.

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Receiving_PackageTypeMappings_Insert$$
CREATE PROCEDURE sp_Receiving_PackageTypeMappings_Insert(
    IN p_part_prefix VARCHAR(10),
    IN p_package_type VARCHAR(50),
    IN p_is_default TINYINT(1),
    IN p_display_order INT,
    IN p_created_by INT
)
BEGIN
    DECLARE v_prefix VARCHAR(10);

    SET v_prefix = UPPER(TRIM(p_part_prefix));

    -- If this mapping is marked default, clear any existing default flags
    IF p_is_default = 1 THEN
        UPDATE receiving_package_type_mapping
        SET is_default = FALSE
        WHERE is_default = TRUE;
    END IF;

    -- Insert or update existing mapping (part_prefix is UNIQUE).
    -- Use LAST_INSERT_ID(id) trick so we can return the row id for both insert & update.
    INSERT INTO receiving_package_type_mapping (
        part_prefix,
        package_type,
        is_default,
        display_order,
        is_active,
        created_by
    ) VALUES (
        v_prefix,
        p_package_type,
        p_is_default,
        p_display_order,
        TRUE,
        p_created_by
    )
    ON DUPLICATE KEY UPDATE
        package_type = VALUES(package_type),
        is_default = VALUES(is_default),
        display_order = VALUES(display_order),
        is_active = TRUE,
        created_by = IFNULL(VALUES(created_by), created_by),
        updated_at = CURRENT_TIMESTAMP,
        id = LAST_INSERT_ID(id);

    SELECT LAST_INSERT_ID() AS id;
END$$

DELIMITER ;
