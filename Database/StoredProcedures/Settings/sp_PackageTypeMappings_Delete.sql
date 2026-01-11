-- =============================================
-- Stored Procedure: sp_PackageTypeMappings_Delete
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Delete Package Type Mapping (soft delete)
-- =============================================
DROP PROCEDURE IF EXISTS sp_PackageTypeMappings_Delete$$
CREATE PROCEDURE sp_PackageTypeMappings_Delete(
    IN p_id INT
)
BEGIN
    UPDATE receiving_package_type_mapping
    SET is_active = FALSE,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_id;

    SELECT ROW_COUNT() AS affected_rows;
END$$



DELIMITER ;
