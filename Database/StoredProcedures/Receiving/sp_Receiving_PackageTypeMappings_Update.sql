-- =============================================
-- Stored Procedure: sp_Receiving_PackageTypeMappings_Update
-- Purpose: Update a receiving_package_type_mapping row.
-- - Enforces unique part_prefix (fails if duplicate exists on another row)
-- - If p_is_default = TRUE, clears other defaults
-- - Updates part_prefix, package_type, is_default, display_order, is_active, updated_at
-- Returns: affected_rows INT, error_message TEXT (NULL on success)
-- =============================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Receiving_PackageTypeMappings_Update$$
CREATE PROCEDURE sp_Receiving_PackageTypeMappings_Update(
    IN p_id INT,
    IN p_part_prefix VARCHAR(10),
    IN p_package_type VARCHAR(50),
    IN p_is_default BOOLEAN,
    IN p_display_order INT,
    IN p_is_active BOOLEAN
)
BEGIN
    DECLARE v_conflict_id INT DEFAULT NULL;

    -- Check for duplicate part_prefix on a different row
    SELECT id INTO v_conflict_id
    FROM receiving_package_type_mapping
    WHERE part_prefix = p_part_prefix
      AND id <> p_id
    LIMIT 1;

    IF v_conflict_id IS NOT NULL THEN
        SELECT 0 AS affected_rows, CONCAT('Duplicate part_prefix found with id=', v_conflict_id) AS error_message;
    ELSE
        -- If marking this row default, clear other defaults
        IF p_is_default THEN
            UPDATE receiving_package_type_mapping
            SET is_default = FALSE
            WHERE is_default = TRUE
              AND id <> p_id;
        END IF;

        -- Perform the update
        UPDATE receiving_package_type_mapping
        SET part_prefix = p_part_prefix,
            package_type = p_package_type,
            is_default = p_is_default,
            display_order = p_display_order,
            is_active = p_is_active,
            updated_at = CURRENT_TIMESTAMP
        WHERE id = p_id;

        SELECT ROW_COUNT() AS affected_rows, NULL AS error_message;
    END IF;
END$$

DELIMITER ;
