-- =============================================
-- Stored Procedure: sp_PackageType_Delete
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Delete Package Type
-- =============================================
DROP PROCEDURE IF EXISTS sp_PackageType_Delete$$
CREATE PROCEDURE sp_PackageType_Delete(
    IN p_id INT
)
BEGIN
    DECLARE v_usage_count INT DEFAULT 0;
    
    -- Check if package type is in use (referenced by package_type_mappings)
    SELECT COUNT(*) INTO v_usage_count
    FROM package_type_mappings
    WHERE package_type = (SELECT name FROM package_types WHERE id = p_id)
      AND is_active = TRUE;
    
    IF v_usage_count > 0 THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Cannot delete package type that is in use by mappings';
    END IF;
    
    -- Soft delete
    UPDATE package_types
    SET is_active = FALSE,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_id;
    
    SELECT ROW_COUNT() AS affected_rows;
END$$



DELIMITER ;
