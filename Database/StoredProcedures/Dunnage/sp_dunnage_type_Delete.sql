-- =============================================
-- Stored Procedure: sp_dunnage_type_Delete
-- =============================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_type_Delete$$
CREATE PROCEDURE sp_dunnage_type_Delete(
    IN p_id INT
)
BEGIN
    DECLARE v_usage_count INT DEFAULT 0;

    -- Check if package type is in use (referenced by receiving_package_type_mapping)
    -- mapping should reference the package type by id (package_type_id)
    SELECT COUNT(*) INTO v_usage_count
    FROM receiving_package_type_mapping
    WHERE package_type_id = p_id
      AND is_active = TRUE;

    IF v_usage_count > 0 THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Cannot delete package type that is in use by mappings';
    END IF;

    -- Soft delete on the receiving_package_types table
    UPDATE receiving_package_types
    SET is_active = FALSE,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_id;

    SELECT ROW_COUNT() AS affected_rows;
END$$

DELIMITER ;
