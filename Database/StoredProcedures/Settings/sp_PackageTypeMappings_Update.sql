-- =============================================
-- Stored Procedure: sp_PackageTypeMappings_Update
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Update Package Type Mapping
-- =============================================
DROP PROCEDURE IF EXISTS sp_PackageTypeMappings_Update$$
CREATE PROCEDURE sp_PackageTypeMappings_Update(
    IN p_id INT,
    IN p_package_type VARCHAR(50),
    IN p_is_default BOOLEAN,
    IN p_display_order INT
)
BEGIN
    UPDATE package_type_mappings
    SET package_type = p_package_type,
        is_default = p_is_default,
        display_order = p_display_order,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_id;
    
    SELECT ROW_COUNT() AS affected_rows;
END$$



DELIMITER ;
